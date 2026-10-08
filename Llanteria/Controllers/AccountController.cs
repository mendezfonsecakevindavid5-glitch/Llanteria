using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
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
        private readonly PaisService _paisSer;
        private readonly CredentialGenerationService _credSer;

        public AccountController(
            UsuarioService userSer,
            ClienteService clienteSer,
            SexoService sexoSer,
            TipoDocumentoService docSer,
            EmailService emailSer,
            PaisService paisSer,
            CredentialGenerationService credSer)
        {
            _userSer = userSer;
            _clienteSer = clienteSer;
            _sexoSer = sexoSer;
            _docSer = docSer;
            _emailSer = emailSer;
            _paisSer = paisSer;
            _credSer = credSer;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _userSer.GetUsuarios().FirstOrDefault(u => u.Username == username || u.Correo == username);

            if (user == null)
            {
                ViewBag.Error = "El usuario ingresado no existe";
                return View();
            }

            bool passwordValido = SecurityService.VerificarPassword(password, user.PasswordHash);
            if (!passwordValido)
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
            var cliente = _clienteSer.GetClientes().FirstOrDefault(c => c.Correo == user.Username || c.NumeroDocumento == user.Username);
            string fotoPath = cliente?.Foto ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("Foto", fotoPath)
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

        [HttpGet]
        public IActionResult Register()
        {
            CargarCombosViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Cliente cliente, string Username, string Password, string? FotoBase64)
        {
            ModelState.Remove("IdDocumentoNavigation");
            ModelState.Remove("IdSexoNavigation");

            if (_clienteSer.GetClientes().Any(c => c.Correo == cliente.Correo) || _userSer.GetUsuarios().Any(u => u.Correo == cliente.Correo))
            {
                ModelState.AddModelError("Correo", "Ya existe una cuenta registrada con este correo electrónico.");
            }

            if (_userSer.GetUsuarios().Any(u => u.Username == Username))
            {
                ModelState.AddModelError("Username", "El nombre de usuario ya se encuentra en uso.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(FotoBase64))
                    {
                        cliente.Foto = FotoBase64;
                    }
                    else
                    {
                        cliente.Foto = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(cliente.Correo ?? Username)}&background=C92B0A&color=fff&rounded=true";
                    }

                    cliente.PuntosAcumulados = 50;
                    _clienteSer.AddCliente(cliente);

                    Usuario nuevoUsuario = new Usuario
                    {
                        Username = Username,
                        Correo = cliente.Correo,
                        PasswordHash = SecurityService.GenerarPasswordAleatorio(Password),
                        IdRol = 2,
                        IdEmpleado = null,
                        Estado = "Activo",
                        FechaCreacion = DateTime.Now
                    };

                    _userSer.AddUsuario(nuevoUsuario);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, nuevoUsuario.Username),
                        new Claim(ClaimTypes.NameIdentifier, nuevoUsuario.Id.ToString()),
                        new Claim(ClaimTypes.Role, "Cliente"),
                        new Claim("Foto", cliente.Foto)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }
                catch (Exception ex)
                {
                    string mensajeError = ex.InnerException?.Message ?? ex.Message;
                    return Json(new { success = false, message = "Error en base de datos: " + mensajeError });
                }
            }

            var primerError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault() ?? "Por favor, verifique los datos ingresados.";
            return Json(new { success = false, message = primerError });
        }

        private void CargarCombosViewBag()
        {
            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
            ViewBag.TiposDocumento = _docSer.GetTipoDocumentos().ToList();
            ViewBag.Paises = _paisSer.GetPaises().ToList();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarCodigoOtp([FromBody] SolicitudOtpDto dto)
        {
            if (string.IsNullOrEmpty(dto.Correo))
                return Json(new { success = false, message = "El correo es requerido." });

            if (_clienteSer.GetClientes().Any(c => c.Correo == dto.Correo) || _userSer.GetUsuarios().Any(u => u.Correo == dto.Correo))
                return Json(new { success = false, message = "Ya existe una cuenta asociada a este correo electrónico." });

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            HttpContext.Session.SetString("CodigoRegistro", otp);

            bool enviado = await _emailSer.EnviarTokenRecuperacionAsync(dto.Correo, otp);
            if (enviado) return Json(new { success = true });

            return Json(new { success = false, message = "No se pudo enviar el correo de verificación." });
        }

        [HttpPost]
        public IActionResult VerificarToken([FromBody] string tokenIngresado)
        {
            string tokenGuardado = HttpContext.Session.GetString("CodigoRegistro");
            if (string.IsNullOrEmpty(tokenGuardado))
                return Json(new { success = false, message = "El código expiró. Solicita uno nuevo." });

            if (tokenGuardado == tokenIngresado)
                return Json(new { success = true });

            return Json(new { success = false, message = "El código es incorrecto." });
        }

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

        [HttpGet]
        public IActionResult ReestablecerContra() => View();

        [HttpPost]
        public async Task<IActionResult> EnviarCodigoRecuperacion([FromBody] SolicitudOtpDto dto)
        {
            if (string.IsNullOrEmpty(dto.Correo))
                return Json(new { success = false, message = "El correo es requerido." });

            bool userExists = _userSer.GetUsuarios().Any(u => u.Correo == dto.Correo) || _clienteSer.GetClientes().Any(c => c.Correo == dto.Correo);
            if (!userExists)
                return Json(new { success = false, message = "Este correo no se encuentra registrado en ninguna cuenta." });

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            HttpContext.Session.SetString("CodigoRecuperacion", otp);
            HttpContext.Session.SetString("CorreoRecuperacion", dto.Correo);

            bool enviado = await _emailSer.EnviarTokenRecuperacionAsync(dto.Correo, otp);
            if (enviado) return Json(new { success = true });

            return Json(new { success = false, message = "Error al enviar el correo." });
        }

        [HttpPost]
        public IActionResult VerificarTokenRecuperacion([FromBody] string tokenIngresado)
        {
            string tokenGuardado = HttpContext.Session.GetString("CodigoRecuperacion");
            if (string.IsNullOrEmpty(tokenGuardado))
                return Json(new { success = false, message = "El código expiró. Solicita uno nuevo." });

            if (tokenGuardado == tokenIngresado) return Json(new { success = true });

            return Json(new { success = false, message = "El código es incorrecto." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcesarReestablecimiento(string NuevaContrasena)
        {
            string correoUsuario = HttpContext.Session.GetString("CorreoRecuperacion");
            if (string.IsNullOrEmpty(correoUsuario))
                return RedirectToAction("ReestablecerContra");

            var usuario = _userSer.GetUsuarios().FirstOrDefault(u => u.Correo == correoUsuario);
            if (usuario != null)
            {
                usuario.PasswordHash = SecurityService.GenerarPasswordAleatorio(NuevaContrasena);
                _userSer.UpdateUsuario(usuario);
            }

            HttpContext.Session.Remove("CodigoRecuperacion");
            HttpContext.Session.Remove("CorreoRecuperacion");

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult BloqueoSeguridad() => View();
    }

    public class SolicitudOtpDto
    {
        public string Correo { get; set; }
        public string Nombre { get; set; }
    }
}