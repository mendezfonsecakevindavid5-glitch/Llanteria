using System.Net;
using System.Text;
using System.Text.Json;

namespace Llanteria.Services
{
    /// <summary>Resultado de un envío: Motivo es el detalle técnico (para el log), Mensaje es el texto que se le puede mostrar al usuario.</summary>
    public record ResultadoCorreo(bool Ok, string? Motivo = null, string? Mensaje = null);

    public class EmailService
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _log;

        public EmailService(IConfiguration configuration, ILogger<EmailService> log)
        {
            _configuration = configuration;
            _log = log;
        }

        public string CorreoDestino => _configuration["Contacto:CorreoDestino"] ?? "";

        public async Task<bool> EnviarCorreoAsync(string destinatarioEmail, string destinatarioNombre,
            string asunto, string cuerpoHtml, string? responderA = null)
            => (await EnviarCorreoConResultadoAsync(destinatarioEmail, destinatarioNombre, asunto, cuerpoHtml, responderA)).Ok;

        /// <summary>
        /// Igual que <see cref="EnviarCorreoAsync"/> pero dice POR QUÉ falló (llave rechazada, remitente no verificado, sin red...).
        /// Cada fallo queda en el log con el detalle que devuelve Brevo.
        /// </summary>
        public async Task<ResultadoCorreo> EnviarCorreoConResultadoAsync(string destinatarioEmail, string destinatarioNombre,
            string asunto, string cuerpoHtml, string? responderA = null)
        {
            try
            {
                var apiKey = _configuration["Brevo:ApiKey"]?.Trim();
                var fromEmail = _configuration["Brevo:FromEmail"]?.Trim();
                var fromName = _configuration["Brevo:FromName"] ?? "Llandupar";

                if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(destinatarioEmail))
                {
                    const string faltan = "Falta configurar Brevo:ApiKey, Brevo:FromEmail o el destinatario (Privado/appsettings.json).";
                    _log.LogError("EmailService: {Motivo}", faltan);
                    return new ResultadoCorreo(false, faltan, "El envío de correos no está configurado.");
                }

                var payload = new Dictionary<string, object>
                {
                    ["sender"] = new { name = fromName, email = fromEmail },
                    ["to"] = new[] { new { email = destinatarioEmail, name = destinatarioNombre } },
                    ["subject"] = asunto,
                    ["htmlContent"] = cuerpoHtml
                };
                if (!string.IsNullOrWhiteSpace(responderA))
                    payload["replyTo"] = new { email = responderA };

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("api-key", apiKey);
                request.Headers.Add("accept", "application/json");

                using var response = await Http.SendAsync(request);
                if (response.IsSuccessStatusCode) return new ResultadoCorreo(true);

                var detalle = await response.Content.ReadAsStringAsync();
                var motivo = $"Brevo respondió {(int)response.StatusCode} {response.StatusCode}: {detalle}";
                _log.LogError("EmailService: no se pudo enviar a {Destino}. {Motivo}", destinatarioEmail, motivo);
                return new ResultadoCorreo(false, motivo, Explicar(response.StatusCode, detalle));
            }
            catch (TaskCanceledException ex)
            {
                _log.LogError(ex, "EmailService: Brevo no respondió en 20 segundos (¿sin salida a internet o bloqueado por firewall?).");
                return new ResultadoCorreo(false, "Brevo no respondió a tiempo (timeout de 20 s).", "El servicio de correo no respondió a tiempo.");
            }
            catch (HttpRequestException ex)
            {
                _log.LogError(ex, "EmailService: no se pudo conectar con api.brevo.com.");
                return new ResultadoCorreo(false, "No se pudo conectar con api.brevo.com: " + ex.Message, "No hay conexión con el servicio de correo.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "EmailService: error inesperado al enviar correo.");
                return new ResultadoCorreo(false, ex.Message, "No se pudo enviar el correo.");
            }
        }

        private static string Explicar(HttpStatusCode codigo, string detalle)
        {
            var d = (detalle ?? "").ToLowerInvariant();
            if (codigo == HttpStatusCode.Unauthorized)
                return d.Contains("ip") ? "Brevo bloqueó la IP de este servidor (Brevo > Seguridad > Direcciones IP autorizadas)." : "Brevo rechazó la llave de API (Brevo:ApiKey).";
            if (codigo == HttpStatusCode.Forbidden) return "La cuenta de Brevo no tiene permiso para enviar correos (¿cuenta sin activar?).";
            if (codigo == HttpStatusCode.TooManyRequests) return "Se alcanzó el límite de envíos de Brevo.";
            if (codigo == HttpStatusCode.BadRequest && d.Contains("sender")) return "El remitente (Brevo:FromEmail) no está verificado en Brevo.";
            return "El servicio de correo rechazó el envío.";
        }

        public static string PlantillaAviso(string titulo, IEnumerable<(string Etiqueta, string? Valor)> datos, string? mensaje = null)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:6px;overflow:hidden'>");
            sb.Append($"<div style='background:#C92B0A;color:#fff;padding:16px 20px'><h2 style='margin:0;font-size:18px'>{WebUtility.HtmlEncode(titulo)}</h2></div>");
            sb.Append("<div style='padding:20px'><table style='width:100%;border-collapse:collapse'>");
            foreach (var (etiqueta, valor) in datos)
                sb.Append($"<tr><td style='padding:6px 0;color:#555;width:35%'><strong>{WebUtility.HtmlEncode(etiqueta)}</strong></td><td style='padding:6px 0'>{WebUtility.HtmlEncode(valor ?? "-")}</td></tr>");
            sb.Append("</table>");
            if (!string.IsNullOrWhiteSpace(mensaje))
                sb.Append($"<div style='margin-top:16px;padding:12px;border-left:4px solid #C92B0A;background:#fafafa;white-space:pre-line'>{WebUtility.HtmlEncode(mensaje)}</div>");
            sb.Append("</div></div>");
            return sb.ToString();
        }

        /// <summary>Correo del código OTP (login, registro y recuperación de contraseña). Mensaje corto, en negro, sin adornos.</summary>
        public async Task<bool> EnviarTokenRecuperacionAsync(string destinatarioEmail, string otp)
            => (await EnviarTokenAsync(destinatarioEmail, otp)).Ok;

        /// <summary>Igual que <see cref="EnviarTokenRecuperacionAsync"/> pero con el motivo del fallo, para avisarle al usuario.</summary>
        public Task<ResultadoCorreo> EnviarTokenAsync(string destinatarioEmail, string otp)
        {
            var cuerpo = $"<p style='font-family:Arial,sans-serif;font-size:15px;color:#000;margin:0'>Código de verificación: <strong>{WebUtility.HtmlEncode(otp)}</strong>. No compartir.</p>";
            return EnviarCorreoConResultadoAsync(destinatarioEmail, "Usuario", "Código de verificación - Llandupar", cuerpo);
        }
    }
}