using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Llanteria.Services;

/// <summary>Datos que el navegador necesita para dibujar el informe de Power BI dentro del panel.</summary>
public sealed record PowerBiEmbed(string EmbedUrl, string ReportId, string Token, DateTimeOffset Expira);

/// <summary>
/// Enlaza el panel con Power BI. Dos modos (config "PowerBI:Modo"):
///
///  "Iframe"    -> el informe se muestra dentro de un iframe con PowerBI:UrlInforme (Archivo > Insertar informe > "Sitio web o portal"
///                 si cada persona tiene su cuenta de Power BI con licencia, o "Publicar en la web" que es PÚBLICO: no lo uses con datos del negocio).
///  "Embebido"  -> "la aplicación es dueña de los datos": el servidor se identifica ante Microsoft con una entidad de servicio (Azure AD)
///                 y pide un token de solo lectura de 1 hora para ese informe. Quien usa el panel NO necesita cuenta de Power BI,
///                 y el secreto (PowerBI:ClientSecret) nunca sale del servidor. Requiere una capacidad (Fabric F, Power BI Embedded A o Premium P).
/// </summary>
public sealed class PowerBiService
{
    private const string ApiBase = "https://api.powerbi.com/v1.0/myorg";
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<PowerBiService> _log;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private string? _tokenAad;
    private DateTimeOffset _expiraAad;
    private PowerBiEmbed? _embed;

    public PowerBiService(IHttpClientFactory http, IConfiguration cfg, ILogger<PowerBiService> log)
    {
        _http = http; _cfg = cfg; _log = log;
    }

    public string Modo => (_cfg["PowerBI:Modo"] ?? "").Trim();

    /// <summary>Dirección del iframe (solo modo "Iframe"). Debe ser https y de un dominio de Microsoft Power BI.</summary>
    public string? UrlIframe
    {
        get
        {
            var url = _cfg["PowerBI:UrlInforme"]?.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u) || u.Scheme != Uri.UriSchemeHttps) return null;
            return u.Host.EndsWith(".powerbi.com", StringComparison.OrdinalIgnoreCase) || u.Host.EndsWith(".powerbigov.us", StringComparison.OrdinalIgnoreCase) ? u.ToString() : null;
        }
    }

    private string Valor(string clave) => (_cfg["PowerBI:" + clave] ?? "").Trim();

    private bool EmbebidoListo =>
        Guid.TryParse(Valor("TenantId"), out _) && Guid.TryParse(Valor("ClientId"), out _) && Valor("ClientSecret").Length > 0
        && Guid.TryParse(Valor("WorkspaceId"), out _) && Guid.TryParse(Valor("ReportId"), out _);

    public bool Configurado => Modo.Equals("Iframe", StringComparison.OrdinalIgnoreCase) ? UrlIframe != null
                             : Modo.Equals("Embebido", StringComparison.OrdinalIgnoreCase) && EmbebidoListo;
    public bool EsEmbebido => Modo.Equals("Embebido", StringComparison.OrdinalIgnoreCase);

    private async Task<string> TokenAadAsync(CancellationToken ct)
    {
        if (_tokenAad != null && DateTimeOffset.UtcNow < _expiraAad.AddMinutes(-5)) return _tokenAad;
        var cuerpo = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = Valor("ClientId"),
            ["client_secret"] = Valor("ClientSecret"),
            ["scope"] = "https://analysis.windows.net/powerbi/api/.default"
        });
        using var resp = await _http.CreateClient().PostAsync($"https://login.microsoftonline.com/{Valor("TenantId")}/oauth2/v2.0/token", cuerpo, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode) throw new InvalidOperationException($"Azure AD respondió {(int)resp.StatusCode}."); // el cuerpo no se registra: puede traer datos del tenant
        using var doc = JsonDocument.Parse(json);
        _tokenAad = doc.RootElement.GetProperty("access_token").GetString();
        _expiraAad = DateTimeOffset.UtcNow.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32());
        return _tokenAad!;
    }

    /// <summary>Token de incrustación del informe (modo "Embebido"). Devuelve null si falta configuración o Microsoft rechaza la petición.</summary>
    public async Task<PowerBiEmbed?> ObtenerAsync(CancellationToken ct)
    {
        if (!EsEmbebido || !EmbebidoListo) return null;
        await _cerrojo.WaitAsync(ct);
        try
        {
            if (_embed != null && DateTimeOffset.UtcNow < _embed.Expira.AddMinutes(-10)) return _embed;

            var cli = _http.CreateClient();
            cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAadAsync(ct));
            var grupo = Valor("WorkspaceId");
            var informe = Valor("ReportId");

            using var rInforme = await cli.GetAsync($"{ApiBase}/groups/{grupo}/reports/{informe}", ct);
            if (!rInforme.IsSuccessStatusCode) { _log.LogError("PowerBI: no se pudo leer el informe ({Codigo}). ¿La entidad de servicio es miembro del área de trabajo?", (int)rInforme.StatusCode); return null; }
            using var dInforme = JsonDocument.Parse(await rInforme.Content.ReadAsStringAsync(ct));
            var embedUrl = dInforme.RootElement.GetProperty("embedUrl").GetString()!;

            using var cuerpo = new StringContent(JsonSerializer.Serialize(new { accessLevel = "View" }), Encoding.UTF8, "application/json"); // solo lectura
            using var rToken = await cli.PostAsync($"{ApiBase}/groups/{grupo}/reports/{informe}/GenerateToken", cuerpo, ct);
            if (!rToken.IsSuccessStatusCode) { _log.LogError("PowerBI: no se pudo generar el token ({Codigo}). Revisa que el área de trabajo tenga capacidad asignada.", (int)rToken.StatusCode); return null; }
            using var dToken = JsonDocument.Parse(await rToken.Content.ReadAsStringAsync(ct));

            _embed = new PowerBiEmbed(embedUrl, informe, dToken.RootElement.GetProperty("token").GetString()!, dToken.RootElement.GetProperty("expiration").GetDateTimeOffset());
            return _embed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.LogError(ex, "PowerBI: error al preparar el informe.");
            return null;
        }
        finally { _cerrojo.Release(); }
    }
}
