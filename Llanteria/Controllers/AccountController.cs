using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;

namespace Llanteria.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioService _userSer;
        private readonly ClienteService _clienteSer;
        private readonly SexoService _sexoSer;
        private readonly TipoDocumentoService _docSer;

        public AccountController(UsuarioService userSer, ClienteService clienteSer, SexoService sexoSer, TipoDocumentoService docSer)
        {
            _userSer = userSer;
            _clienteSer = clienteSer;
            _sexoSer = sexoSer;
            _docSer = docSer;
        }

        // --- ACCIONES DE LOGIN ---

        // ¡MÉTODO AGREGADO! Este es el encargado de abrir la página cuando haces clic en Ingresar
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Buscamos al usuario por su Username
            var user = _userSer.GetUsuarios().FirstOrDefault(u => u.Username == username);

            // 2. Validación: El usuario no existe en la base de datos
            if (user == null)
            {
                ViewBag.Error = "El usuario ingresado no existe";
                return View();
            }

            // 3. Validación: La contraseña no coincide
            if (user.PasswordHash != password)
            {
                ViewBag.Error = "Esta contraseña no corresponde a este usuario";
                return View();
            }

            // 4. Validación: El estado de la cuenta no es activo
            if (user.Estado != "Activo")
            {
                ViewBag.Error = "La cuenta se encuentra inactiva o baneada.";
                return View();
            }

            // 5. Autenticación exitosa
            string nombreRol = user.IdRolNavigation?.NombreRol ?? "Cliente";

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, nombreRol)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (nombreRol == "Administrador" || nombreRol == "Empleado")
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // --- ACCIONES DE REGISTRO ---

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.TiposDocumento = _docSer.GetTipoDocumentos().ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Cliente cliente)
        {
            ModelState.Remove("IdDocumentoNavigation");
            ModelState.Remove("IdSexoNavigation"); 

            if (ModelState.IsValid)
            {
                cliente.PuntosAcumulados = 50;
                _clienteSer.AddCliente(cliente);
                return RedirectToAction("Welcome");
            }

            ViewBag.TiposDocumento = _docSer.GetTipoDocumentos().ToList();
            return View(cliente);
        }

        // --- Lógica SMTP --- 

        [HttpPost]
        public IActionResult GenerarYEnviarToken([FromBody] string correoDestino)
        {
            try
            {
                // 1. Generar código numérico de 6 dígitos
                Random rnd = new Random();
                string token = rnd.Next(100000, 999999).ToString();

                // 2. Guardar en sesión
                HttpContext.Session.SetString("CodigoRegistro", token);
                HttpContext.Session.SetString("CorreoRegistro", correoDestino);

                // 3. Configurar el correo con MimeKit
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("TU_CORREO_EMPRESA@gmail.com")); // CAMBIA ESTO
                email.To.Add(MailboxAddress.Parse(correoDestino));
                email.Subject = "Código de Verificación - Llantería";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $"<h3>Tu código de seguridad es: <strong>{token}</strong></h3>"
                };

                // 4. Enviar mediante MailKit
                using var smtp = new SmtpClient();
                // Nota: Usa una contraseña de aplicación si es Gmail
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("TU_CORREO_EMPRESA@gmail.com", "TU_CONTRASEÑA_DE_APLICACION");
                smtp.Send(email);
                smtp.Disconnect(true);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error enviando correo: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult VerificarToken([FromBody] string tokenIngresado)
        {
            string tokenGuardado = HttpContext.Session.GetString("CodigoRegistro");

            if (string.IsNullOrEmpty(tokenGuardado))
            {
                return Json(new { success = false, message = "El código expiró. Solicita uno nuevo." });
            }

            if (tokenGuardado == tokenIngresado)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "El código es incorrecto." });
        }

        // --- FUNCIONES DE PERFIL ---

        [Authorize]
        [HttpGet]
        public IActionResult MiPerfil()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var cliente = _clienteSer.GetClientes().FirstOrDefault(c => c.Id.ToString() == userId);

            if (cliente == null) return NotFound();

            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", cliente.IdSexo);
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", cliente.IdDocumento);

            return View(cliente);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MiPerfil(Cliente cliente)
        {
            ModelState.Remove("IdDocumentoNavigation");
            ModelState.Remove("IdSexoNavigation");

            if (ModelState.IsValid)
            {
                _clienteSer.UpdateCliente(cliente);
                TempData["Success"] = "Perfil actualizado correctamente.";
                return RedirectToAction("MiPerfil");
            }

            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", cliente.IdSexo);
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", cliente.IdDocumento);
            return View(cliente);
        }

        public IActionResult Welcome() => View();
    }
}