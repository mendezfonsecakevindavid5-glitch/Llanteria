using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

public class ContactoController : Controller
{
    [HttpPost]
    public async Task<IActionResult> EnviarMensaje(string Nombre, string Correo, string Asunto, string Mensaje)
    {
        try
        {
            // Puedes usar un servicio profesional como SendGrid/Resend. 
            // Para arrancar rápido y profesional sin costo, usaremos el SMTP seguro de tu proveedor de correo (ej. Gmail/Outlook)
            var tuCorreoDestino = "mendezfonsecakevindavid5@gmail.com";
            var passwordAplicacion = "tuup ydch lglq rwwa"; // Clave de aplicación generada en tu cuenta

            var smtpClient = new SmtpClient("smtp.gmail.com") // Si usas Outlook: smtp.office365.com
            {
                Port = 587,
                Credentials = new NetworkCredential(tuCorreoDestino, passwordAplicacion),
                EnableSsl = true,
            };

            // Creamos el diseño profesional en HTML para el cuerpo del correo
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

            var mailMessage = new MailMessage
            {
                From = new MailAddress(tuCorreoDestino, "Portal Llantería"),
                Subject = $"[Web Contacto] {Asunto} - {Nombre}",
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };

            // Añadimos la cabecera ReplyTo para que cuando le des a "Responder" en tu correo, le responda directo al cliente
            mailMessage.ReplyToList.Add(new MailAddress(Correo));
            mailMessage.To.Add(tuCorreoDestino);

            await smtpClient.SendMailAsync(mailMessage);

            // Redireccionamos al Inicio mostrando un mensaje de éxito usando TempData
            TempData["MensajeEnviado"] = "true";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ViewBag.Error = "No se pudo enviar el mensaje: " + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}