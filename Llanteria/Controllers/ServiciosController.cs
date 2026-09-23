using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Llanteria.Controllers
{
    public class ServiciosController : Controller
    {
        [HttpGet("Servicios/montaje-balanceo")]
        public IActionResult MontajeBalanceo()
        {
            return View("~/Views/Servicios/montaje-balanceo.cshtml");
        }

        [HttpGet("Servicios/alineacion-3d")]
        public IActionResult Alineacion3D()
        {
            return View("~/Views/Servicios/alineacion-3d.cshtml");
        }

        [HttpGet("Servicios/mantenimiento-express")]
        public IActionResult MantenimientoExpress()
        {
            return View("~/Views/Servicios/mantenimiento-express.cshtml");
        }

        [HttpGet("Servicios/baterias")]
        public IActionResult Baterias()
        {
            return View("~/Views/Servicios/baterias.cshtml");
        }

        [HttpGet("Servicios/suspension-amortiguadores")]
        public IActionResult SuspensionAmortiguadores()
        {
            return View("~/Views/Servicios/suspension-amortiguadores.cshtml");
        }

        [HttpGet("Servicios/serviteca-movil")]
        public IActionResult ServitecaMovil()
        {
            return View("~/Views/Servicios/serviteca-movil.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarContacto(string ServicioNombre, string Nombre, string Correo, string Telefono, string Mensaje)
        {
            try
            {
                var tuCorreoDestino = "mendezfonsecakevindavid5@gmail.com";
                var passwordAplicacion = "eodb uysa gfjq ojmo";

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(tuCorreoDestino, passwordAplicacion);
                    smtpClient.EnableSsl = true;

                    string cuerpoHtml = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #C92B0A; color: white; padding: 20px; text-align: center;'>
                            <h2 style='margin: 0; text-transform: uppercase;'>Nueva Solicitud de Servicio</h2>
                            <p style='margin: 5px 0 0 0; opacity: 0.8;'>Llantería System</p>
                        </div>
                        <div style='padding: 20px; background-color: #fbfbfb;'>
                            <p style='font-size: 16px; color: #333;'>Un cliente ha solicitado información sobre un servicio desde la página web.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                            
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555; width: 35%;'>Servicio Consultado:</td>
                                    <td style='padding: 8px 0; color: #C92B0A; font-weight: bold; font-size: 16px;'>{ServicioNombre}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Cliente:</td>
                                    <td style='padding: 8px 0; color: #333;'>{Nombre}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Correo:</td>
                                    <td style='padding: 8px 0; color: #000; font-weight: bold;'>{Correo}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Teléfono / WhatsApp:</td>
                                    <td style='padding: 8px 0; color: #333;'>{Telefono}</td>
                                </tr>
                            </table>

                            <div style='margin-top: 20px; padding: 15px; background-color: #fff; border-left: 4px solid #C92B0A; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);'>
                                <h4 style='margin: 0 0 10px 0; color: #555;'>Detalles / Vehículo:</h4>
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
                        mailMessage.Subject = $"[Solicitud de Servicio] {ServicioNombre} - {Nombre}";
                        mailMessage.Body = cuerpoHtml;
                        mailMessage.IsBodyHtml = true;

                        mailMessage.ReplyToList.Add(new MailAddress(Correo));
                        mailMessage.To.Add(tuCorreoDestino);

                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }

                TempData["MensajeEnviado"] = "true";
            }
            catch (Exception)
            {
                TempData["ErrorCorreo"] = "true";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}