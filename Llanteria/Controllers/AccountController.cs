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
using Microsoft.AspNetCore.Http;
using System;

namespace Llanteria.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioService _userSer;
        private readonly ClienteService _clienteSer;
        private readonly SexoService _sexoSer;
        private readonly TipoDocumentoService _docSer;
        private readonly EmailService _emailSer;

        public AccountController(
            UsuarioService userSer,
            ClienteService clienteSer,
            SexoService sexoSer,
            TipoDocumentoService docSer,
            EmailService emailSer)
        {
            _userSer = userSer;
            _clienteSer = clienteSer;
            _sexoSer = sexoSer;
            _docSer = docSer;
            _emailSer = emailSer;
        }

        // --- ACCIONES DE LOGIN ---

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _userSer.GetUsuarios().FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "El usuario ingresado no existe";
                return View();
            }

            if (user.PasswordHash != password)
            {
                ViewBag.Error = "Esta contraseña no corresponde a este usuario";
                return View();
            }

            if (user.Estado != "Activo")
            {
                ViewBag.Error = "La cuenta se encuentra inactiva o baneada.";
                return View();
            }

            string nombreRol = user.IdRolNavigation?.NombreRol ?? "Cliente";

            // Buscar foto asignada al cliente/usuario si existe
            var cliente = _clienteSer.GetClientes().FirstOrDefault(c => c.Correo == user.Username || c.NumeroDocumento == user.Username);
            string fotoPath = cliente?.Foto ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("Foto", fotoPath) // ✅ Claim con la ruta de la foto
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
            // ✅ Carga de Sexos para la vista
            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
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

            // ✅ Recarga de Sexos en caso de error en POST
            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", cliente.IdSexo);
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", cliente.IdDocumento);
            ViewBag.TiposDocumento = _docSer.GetTipoDocumentos().ToList();
            return View(cliente);
        }

        // --- ENVIAR CÓDIGO OTP POR CORREO ---
        [HttpPost]
        public async Task<IActionResult> EnviarCodigoOtp([FromBody] SolicitudOtpDto dto)
        {
            if (string.IsNullOrEmpty(dto.Correo))
            {
                return Json(new { success = false, message = "El correo es requerido." });
            }

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            HttpContext.Session.SetString("CodigoRegistro", otp);

            bool enviado = await _emailSer.EnviarTokenVerificacionAsync(dto.Correo, dto.Nombre, otp);

            if (enviado)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "No se pudo enviar el correo de verificación." });
        }

        // --- LÓGICA VERIFICAR TOKEN --- 

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

            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
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

            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", cliente.IdSexo);
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", cliente.IdDocumento);
            return View(cliente);
        }

        public IActionResult Welcome() => View();
    }

    public class SolicitudOtpDto
    {
        public string Correo { get; set; }
        public string Nombre { get; set; }
    }
}