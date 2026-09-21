using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Llanteria.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<bool> EnviarCorreoAsync(string destinatarioEmail, string destinatarioNombre, string asunto, string cuerpoHtml)
        {
            try
            {
                var apiKey = _configuration["Brevo:ApiKey"] ?? _configuration["MailerSend:ApiKey"];
                var fromEmail = _configuration["Brevo:FromEmail"] ?? _configuration["MailerSend:FromEmail"];
                var fromName = _configuration["Brevo:FromName"] ?? "Sistema Llantería";
                var requestUri = "https://api.brevo.com/v3/smtp/email";

                // Estructura JSON requerida por la API v3 de Brevo
                var payload = new
                {
                    sender = new { name = fromName, email = fromEmail },
                    to = new[] { new { email = destinatarioEmail, name = destinatarioNombre } },
                    subject = asunto,
                    htmlContent = cuerpoHtml
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Brevo ({response.StatusCode}): {errorResponse}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarTokenRecuperacionAsync(string destinatarioEmail, string otp)
        {
            string asunto = "Código de verificación de Llantería";

            string cuerpoHtml = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{ font-family: Arial, sans-serif; background-color: #ffffff; margin: 0; padding: 20px; color: #202124; }}
                .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }}
                .header-box {{ background-color: #C92B0A; padding: 30px; text-align: left; }}
                .header-title {{ color: #ffffff; font-size: 24px; margin: 0; font-weight: normal; }}
                .content-box {{ padding: 40px 30px; }}
                .text-body {{ font-size: 16px; line-height: 1.5; color: #3c4043; margin-bottom: 30px; }}
                .otp-code {{ font-size: 36px; font-weight: bold; color: #000000; text-align: center; margin-bottom: 30px; letter-spacing: 2px; }}
                .footer-text {{ font-size: 14px; color: #5f6368; line-height: 1.4; border-top: 1px solid #e0e0e0; padding-top: 20px; }}
                .bottom-footer {{ margin-top: 20px; text-align: left; font-size: 12px; color: #70757a; }}
            </style>
        </head>
        <body>
            <div style='margin-bottom: 20px;'>
                <strong style='font-size: 24px; color: #C92B0A;'>Llantería Valledupar</strong>
            </div>
            <div class='container'>
                <div class='header-box'>
                    <h2 class='header-title'>Código de verificación de Llantería</h2>
                </div>
                <div class='content-box'>
                    <p class='text-body'>
                        Llantería recibió una solicitud para usar esta dirección de correo electrónico como método de recuperación para la cuenta <strong>{destinatarioEmail}</strong>.
                    </p>
                    
                    <div class='otp-code'>{otp}</div>
                    
                    <p class='text-body'>
                        Cuando se te solicite, ingresa el código para demostrar que Llantería puede comunicarse contigo a través de esta dirección de correo electrónico.
                    </p>
                    
                    <p class='footer-text'>
                        Si no reconoces <strong>{destinatarioEmail}</strong>, es probable que alguien haya dado tu dirección de correo electrónico por error. Puedes ignorar este mensaje.<br><br>
                        Atentamente.<br>
                        El equipo de Llantería Valledupar
                    </p>
                </div>
            </div>
            <div class='bottom-footer'>
                No se pueden enviar respuestas a esta dirección de correo electrónico.
            </div>
        </body>
        </html>";

            return await EnviarCorreoAsync(destinatarioEmail, "Usuario", asunto, cuerpoHtml);
        }
    }
}