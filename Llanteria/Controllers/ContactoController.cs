using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class ContactoController : Controller
{
    // GET: /Contacto
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EnviarMensaje(string Nombre, string Correo, string Asunto, string Mensaje)
    {
        try
        {
            var tuCorreoDestino = "mendezfonsecakevindavid5@gmail.com";
            var passwordAplicacion = "eodb uysa gfjq ojmo"; // Reemplazar con una clave nueva si la anterior expiró

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.UseDefaultCredentials = false; // Requiere agregarse antes de Credentials
                smtpClient.Credentials = new NetworkCredential(tuCorreoDestino, passwordAplicacion);
                smtpClient.EnableSsl = true;

                string cuerpoHtml = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #C92B0A; color: white; padding: 20px; text-align: center;'>
                    <h2 style='margin: 0; text-transform: uppercase;'>Nuevo Mensaje de Contacto</h2>
                    <p style='margin: 5px 0 0 0; opacity: 0.8;'>Llantería System</p>
                </div>
                <div style='padding: 20px; background-color: #fbfbfb;'>
                    <p style='font-size: 16px; color: #333;'>Has recibido una nueva sugerencia o pregunta desde el sitio web.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px 0; font-weight: bold; color: #555; width: 30%;'>Cliente:</td>
                            <td style='padding: 8px 0; color: #333;'>{Nombre}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; font-weight: bold; color: #555;'>Correo:</td>
                            <td style='padding: 8px 0; color: #000; font-weight: bold;'>{Correo}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px 0; font-weight: bold; color: #555;'>Asunto:</td>
                            <td style='padding: 8px 0; color: #333;'>{Asunto}</td>
                        </tr>
                    </table>

                    <div style='margin-top: 20px; padding: 15px; background-color: #fff; border-left: 4px solid #C92B0A; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);'>
                        <h4 style='margin: 0 0 10px 0; color: #555;'>Mensaje / Pregunta:</h4>
                        <p style='margin: 0; color: #333; line-height: 1.5; white-space: pre-line;'>{Mensaje}</p>
                    </div>
                </div>
                <div style='background-color: #f1f1f1; padding: 15px; text-align: center; font-size: 12px; color: #777;'>
                    Este correo fue generado automáticamente por el portal web de Llantería Valledupar.
                </div>
            </div>";

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(tuCorreoDestino, "Portal Llantería");
                    mailMessage.Subject = $"[Web Contacto] {Asunto} - {Nombre}";
                    mailMessage.Body = cuerpoHtml;
                    mailMessage.IsBodyHtml = true;

                    mailMessage.ReplyToList.Add(new MailAddress(Correo));
                    mailMessage.To.Add(tuCorreoDestino);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            TempData["MensajeEnviado"] = "true";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Puedes poner un punto de interrupción aquí en Visual Studio para leer ex.Message
            TempData["ErrorCorreo"] = "true";
            return RedirectToAction(nameof(Index));
        }
    }
}