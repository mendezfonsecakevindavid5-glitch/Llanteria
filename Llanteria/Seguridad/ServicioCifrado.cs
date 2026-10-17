using System.Security.Cryptography;
using System.Text;

namespace Llanteria.Seguridad;

/// <summary>
/// Cifrado de datos en reposo con AES-256-GCM (cifrado autenticado: si alguien modifica el dato en la base,
/// el descifrado falla en lugar de devolver basura). La clave maestra NO está en el código ni en el proyecto:
/// se lee de la configuración privada (carpeta Llanteria_Privado o variable de entorno Cifrado__Clave).
/// De la clave maestra se deriva (HKDF-SHA256) la clave real de cifrado, y cada valor usa un nonce aleatorio propio.
/// El "contexto" (p. ej. "Cliente.Foto") se autentica junto con el dato, así un valor copiado de una columna a otra no descifra.
/// </summary>
public sealed class ServicioCifrado
{
    private const string PrefijoTexto = "enc1:";
    private static readonly byte[] PrefijoBytes = Encoding.ASCII.GetBytes("ENC1");
    private const int TamNonce = 12;
    private const int TamTag = 16;

    private readonly byte[] _clave;

    /// <summary>Instancia única usada por los convertidores de EF Core (el modelo se construye una sola vez).</summary>
    public static ServicioCifrado Actual { get; private set; } = null!;

    public ServicioCifrado(string claveBase64)
    {
        if (string.IsNullOrWhiteSpace(claveBase64))
            throw new InvalidOperationException("Falta \"Cifrado:Clave\" en la configuración privada (Llanteria_Privado/appsettings.json).");

        byte[] maestra;
        try { maestra = Convert.FromBase64String(claveBase64.Trim()); }
        catch (FormatException) { throw new InvalidOperationException("\"Cifrado:Clave\" debe estar en Base64."); }
        if (maestra.Length < 32)
            throw new InvalidOperationException("\"Cifrado:Clave\" debe tener al menos 32 bytes (256 bits) en Base64.");

        _clave = HKDF.DeriveKey(HashAlgorithmName.SHA256, maestra, 32, salt: null, info: Encoding.UTF8.GetBytes("Llanteria.Datos.v1"));
        CryptographicOperations.ZeroMemory(maestra);
    }

    public static void Inicializar(ServicioCifrado servicio) => Actual = servicio;

    // ---------- Texto ----------
    public bool EsTextoCifrado(string? valor) => valor != null && valor.StartsWith(PrefijoTexto, StringComparison.Ordinal);

    public string? Cifrar(string? texto, string contexto)
    {
        if (texto == null || EsTextoCifrado(texto)) return texto;
        return PrefijoTexto + Convert.ToBase64String(Sellar(Encoding.UTF8.GetBytes(texto), contexto));
    }

    /// <summary>Descifra; los datos antiguos (sin prefijo) se devuelven tal cual y quedan cifrados la próxima vez que se guarden.</summary>
    public string? Descifrar(string? valor, string contexto)
    {
        if (valor == null || !EsTextoCifrado(valor)) return valor;
        return Encoding.UTF8.GetString(Abrir(Convert.FromBase64String(valor[PrefijoTexto.Length..]), contexto));
    }

    // ---------- Bytes (fotos) ----------
    public bool EsBytesCifrados(byte[]? datos)
        => datos != null && datos.Length > PrefijoBytes.Length + TamNonce + TamTag
           && datos.AsSpan(0, PrefijoBytes.Length).SequenceEqual(PrefijoBytes);

    public byte[]? Cifrar(byte[]? datos, string contexto)
    {
        if (datos == null || EsBytesCifrados(datos)) return datos;
        var sellado = Sellar(datos, contexto);
        var salida = new byte[PrefijoBytes.Length + sellado.Length];
        PrefijoBytes.CopyTo(salida, 0);
        sellado.CopyTo(salida, PrefijoBytes.Length);
        return salida;
    }

    public byte[]? Descifrar(byte[]? datos, string contexto)
    {
        if (datos == null || !EsBytesCifrados(datos)) return datos;
        return Abrir(datos.AsSpan(PrefijoBytes.Length).ToArray(), contexto);
    }

    // ---------- Núcleo ----------
    private byte[] Sellar(byte[] claro, string contexto)
    {
        var nonce = RandomNumberGenerator.GetBytes(TamNonce);
        var cifrado = new byte[claro.Length];
        var tag = new byte[TamTag];
        using var aes = new AesGcm(_clave, TamTag);
        aes.Encrypt(nonce, claro, cifrado, tag, Encoding.UTF8.GetBytes(contexto));

        var salida = new byte[TamNonce + TamTag + cifrado.Length];
        nonce.CopyTo(salida, 0);
        tag.CopyTo(salida, TamNonce);
        cifrado.CopyTo(salida, TamNonce + TamTag);
        return salida;
    }

    private byte[] Abrir(byte[] sellado, string contexto)
    {
        if (sellado.Length < TamNonce + TamTag)
            throw new CryptographicException("Dato cifrado incompleto.");

        var nonce = sellado.AsSpan(0, TamNonce);
        var tag = sellado.AsSpan(TamNonce, TamTag);
        var cifrado = sellado.AsSpan(TamNonce + TamTag);
        var claro = new byte[cifrado.Length];
        using var aes = new AesGcm(_clave, TamTag);
        aes.Decrypt(nonce, cifrado, tag, claro, Encoding.UTF8.GetBytes(contexto)); // lanza si fue alterado
        return claro;
    }
}
