using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
                var apiKey = _configuration["MailerSend:ApiKey"];
                var fromEmail = _configuration["MailerSend:FromEmail"] ?? "soporte@llanteria.com";
                var fromName = _configuration["MailerSend:FromName"] ?? "Sistema Llantería";
                var requestUri = "https://api.mailersend.com/v1/email";

                var payload = new
                {
                    from = new { email = fromEmail, name = fromName },
                    to = new[] { new { email = destinatarioEmail, name = destinatarioNombre } },
                    subject = asunto,
                    html = cuerpoHtml
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync(requestUri, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarTokenVerificacionAsync(string destinatarioEmail, string destinatarioNombre, string otp)
        {
            string asunto = "Código de Verificación - Llantería Valledupar";

            // Layout de correo breve con encabezado coincidente
            string cuerpoHtml = $@"
                <!DOCTYPE html>
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <link href='https://fonts.googleapis.com/css2?family=Orbitron:wght@900&family=Plus+Jakarta+Sans:wght@400;600;700;800&display=swap' rel='stylesheet'>
                </head>
                <body style='font-family: ""Plus Jakarta Sans"", sans-serif; background-color: #F8F9FA; margin:0; padding: 20px; color: #09090B;'>
                    <div style='max-width: 520px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.08); border: 1px solid #E5E7EB;'>
                        
                        <!-- Header igual a _Layout.cshtml -->
                        <div style='background-color: #C92B0A; padding: 22px; text-align: center;'>
                            <div style='display: inline-block; text-align: left;'>
                                <span style='font-family: ""Orbitron"", sans-serif; font-weight: 900; color: #FFFFFF; font-size: 22px; display: block; text-transform: uppercase;'>LLantería</span>
                                <span style='font-family: ""Plus Jakarta Sans"", sans-serif; color: rgba(255,255,255,0.9); font-size: 13px; font-weight: 600; display: block;'>Valledupar</span>
                            </div>
                        </div>

                        <!-- Texto conciso descriptivo del token -->
                        <div style='padding: 30px 25px; text-align: center;'>
                            <h2 style='color: #09090B; font-size: 20px; font-weight: 800; margin-top: 0; margin-bottom: 12px;'>Código de Verificación</h2>
                            <p style='font-size: 15px; color: #4B5563; margin-bottom: 24px; line-height: 1.5;'>
                                Hola <strong>{destinatarioNombre}</strong>, este es tu código para completar la verificación de tu registro en el sistema:
                            </p>
                            
                            <div style='background-color: #F3F4F6; border: 2px dashed #C92B0A; padding: 16px; border-radius: 12px; font-size: 32px; font-weight: 800; letter-spacing: 10px; color: #C92B0A; display: inline-block; margin-bottom: 20px;'>
                                {otp}
                            </div>
                            
                            <p style='color: #6B7280; font-size: 13px; margin: 0;'>
                                Si no solicitaste este registro, puedes ignorar este correo.
                            </p>
                        </div>

                        <div style='background-color: #09090B; padding: 12px; text-align: center;'>
                            <p style='color: #9CA3AF; font-size: 12px; margin: 0;'>&copy; 2026 Llantería Valledupar. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatarioEmail, destinatarioNombre, asunto, cuerpoHtml);
        }
    }
}