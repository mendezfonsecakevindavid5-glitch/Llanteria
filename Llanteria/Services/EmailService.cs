using System.Net;
using System.Text;
using System.Text.Json;

namespace Llanteria.Services
{
    public class EmailService
    {
        private static readonly HttpClient Http = new();
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CorreoDestino => _configuration["Contacto:CorreoDestino"] ?? "";

        public async Task<bool> EnviarCorreoAsync(string destinatarioEmail, string destinatarioNombre,
            string asunto, string cuerpoHtml, string? responderA = null)
        {
            try
            {
                var apiKey = _configuration["Brevo:ApiKey"];
                var fromEmail = _configuration["Brevo:FromEmail"];
                var fromName = _configuration["Brevo:FromName"] ?? "Llandupar";

                if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(destinatarioEmail))
                {
                    Console.WriteLine("EmailService: falta configurar Brevo:ApiKey, Brevo:FromEmail o el destinatario.");
                    return false;
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

                var response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var detalle = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Brevo ({response.StatusCode}): {detalle}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
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
        {
            var cuerpo = $"<p style='font-family:Arial,sans-serif;font-size:15px;color:#000;margin:0'>Código de verificación: <strong>{WebUtility.HtmlEncode(otp)}</strong>. No compartir.</p>";
            return await EnviarCorreoAsync(destinatarioEmail, "Usuario", "Código de verificación - Llandupar", cuerpo);
        }
    }
}