using System.Net.Mail;
using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private const int MinutosVigenciaCodigo = 10;
    private const int SegundosEntreEnvios = 30;
    private const int IntentosMaximos = 5;
    private const int PuntosDeBienvenida = 50;

    // Claves de sesión: una por flujo, para que registro y recuperación no se mezclen.
    private const string Registro = "reg";
    private const string Recuperacion = "rec";

    private readonly LlanteriaDbContext _db;
    private readonly ClienteService _clienteSer;
    private readonly SexoService _sexoSer;
    private readonly TipoDocumentoService _docSer;
    private readonly EmailService _emailSer;
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

    public AccountController(LlanteriaDbContext db, ClienteService clienteSer, SexoService sexoSer,
        TipoDocumentoService docSer, EmailService emailSer)
    {
        _db = db;
        _clienteSer = clienteSer;
        _sexoSer = sexoSer;
        _docSer = docSer;
        _emailSer = emailSer;
    }
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

    // ---------- Inicio y cierre de sesión ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirigirSegunRol(User.IsInRole("Administrador") || User.IsInRole("Empleado"), returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
        [HttpGet]
        public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        username = (username ?? "").Trim();
        password ??= "";
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _userSer.GetUsuarios().FirstOrDefault(u => u.Username == username || u.Correo == username);

        // 1) Personal del taller (usuario o correo).
        var user = _db.Usuarios.Include(u => u.IdRolNavigation)
            .FirstOrDefault(u => u.Username == username || u.Correo == username);

        if (user != null)
        {
            if (!Claves.Verificar(user.PasswordHash, password, out var actualizar))
                return ErrorLogin();
            if (actualizar) user.PasswordHash = Claves.Hash(password);

            // Un baneo con fecha de fin se levanta solo al vencer.
            if (user.Estado == "Baneado" && user.FechaFinBaneo.HasValue && user.FechaFinBaneo.Value <= DateTime.Now)
            bool passwordValido = SecurityService.VerificarPassword(password, user.PasswordHash);
            if (!passwordValido)
            {
                user.Estado = "Activo";
                user.MotivoSancion = null;
                user.FechaFinBaneo = null;
            }

            if (user.Estado != "Activo")
            {
                ViewBag.Error = user.Estado == "Baneado"
                    ? "La cuenta está suspendida" + (user.FechaFinBaneo.HasValue ? $" hasta el {user.FechaFinBaneo:dd/MM/yyyy HH:mm}." : ".")
                    : "La cuenta está inactiva. Comunícate con el administrador.";
                return View();
            }

            user.UltimaConexion = DateTime.Now;
            _db.SaveChanges();

            var rol = user.IdRolNavigation?.NombreRol ?? "Empleado";
            await IniciarSesion(new Claim[]
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, rol)
            });
            return RedirigirSegunRol(rol is "Administrador" or "Empleado", returnUrl);
        }
            string nombreRol = user.IdRolNavigation?.NombreRol ?? "Cliente";
            var cliente = _clienteSer.GetClientes().FirstOrDefault(c => c.Correo == user.Username || c.NumeroDocumento == user.Username);
            string fotoPath = cliente?.Foto ?? "";

        // 2) Cliente del club de puntos (correo o número de documento).
        var cliente = _db.Clientes.FirstOrDefault(c => c.Correo == username || c.NumeroDocumento == username);
        if (cliente == null || !Claves.Verificar(cliente.PasswordHash, password, out var actualizarCliente))
            return ErrorLogin();
        if (actualizarCliente)
        {
            cliente.PasswordHash = Claves.Hash(password);
            _db.SaveChanges();
        }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("Foto", fotoPath)
            };

        // Sin NameIdentifier a propósito: ese claim es el id de un Usuario del panel.
        await IniciarSesion(new Claim[]
        {
            new(ClaimTypes.Name, cliente.Nombres),
            new("ClienteId", cliente.Id.ToString()),
            new(ClaimTypes.Role, "Cliente")
        });
        return RedirigirSegunRol(false, returnUrl, clienteId: cliente.Id);
    }

    // Mismo mensaje para cuenta inexistente y contraseña errónea: no se revela cuál falló.
    private IActionResult ErrorLogin()
    {
        ViewBag.Error = "Usuario o contraseña incorrectos.";
        return View("Login");
    }

    private Task IniciarSesion(IEnumerable<Claim> claims)
        => HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirigirSegunRol(bool esPersonal, string? returnUrl, int? clienteId = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        if (esPersonal) return RedirectToAction("Index", "Dashboard");
        return clienteId.HasValue || User.IsInRole("Cliente") ? RedirectToAction("Index", "MiCuenta") : RedirectToAction("Index", "Home");
    }

    // ---------- Registro en el club de puntos ----------

    [HttpGet]
    public IActionResult Register()
    {
        CargarCombos();
        return View(new Cliente());
    }
        [HttpGet]
        public IActionResult Register()
        {
            CargarCombosViewBag();
            return View();
        }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Register(Cliente cliente, string? contrasena, string? confirmacion)
    {
        cliente.Nombres = (cliente.Nombres ?? "").Trim();
        cliente.Apellidos = (cliente.Apellidos ?? "").Trim();
        cliente.Correo = (cliente.Correo ?? "").Trim();
        cliente.NumeroDocumento = (cliente.NumeroDocumento ?? "").Trim();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Cliente cliente, string Username, string Password, string? FotoBase64)
        {
            ModelState.Remove("IdDocumentoNavigation");
            ModelState.Remove("IdSexoNavigation");

        // El correo debe haberse verificado con el código enviado.
        if (!string.Equals(HttpContext.Session.GetString(Registro + ":ok"), cliente.Correo, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(Cliente.Correo), "Verifica tu correo con el código antes de crear la cuenta.");

        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 6)
            ModelState.AddModelError("contrasena", "La contraseña debe tener al menos 6 caracteres.");
        else if (contrasena != confirmacion)
            ModelState.AddModelError("confirmacion", "Las contraseñas no coinciden.");

        var correo = cliente.Correo.ToLower();
        if (_db.Clientes.AsNoTracking().Any(c => c.NumeroDocumento == cliente.NumeroDocumento))
            ModelState.AddModelError(nameof(Cliente.NumeroDocumento), "Ya hay una cuenta con ese documento.");
        if (_db.Clientes.AsNoTracking().Any(c => c.Correo.ToLower() == correo))
            ModelState.AddModelError(nameof(Cliente.Correo), "Ya hay una cuenta con ese correo.");

        if (!ModelState.IsValid)
        {
            CargarCombos(cliente);
            return View(cliente);
        }
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

        cliente.Id = 0;
        cliente.PuntosAcumulados = PuntosDeBienvenida;
        cliente.PasswordHash = Claves.Hash(contrasena!);
        _clienteSer.AddCliente(cliente);
        LimpiarCodigo(Registro);
        return RedirectToAction(nameof(Welcome));
    }
        private void CargarCombosViewBag()
        {
            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
            ViewBag.TiposDocumento = _docSer.GetTipoDocumentos().ToList();
            ViewBag.Paises = _paisSer.GetPaises().ToList();
        }

    public IActionResult Welcome() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigoOtp([FromBody] SolicitudOtpDto dto)
    {
        var correo = (dto?.Correo ?? "").Trim();
        if (!CorreoValido(correo)) return Json(new { success = false, message = "Escribe un correo válido." });
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

        var correoLower = correo.ToLower();
        if (_db.Clientes.AsNoTracking().Any(c => c.Correo.ToLower() == correoLower))
            return Json(new { success = false, message = "Ese correo ya tiene una cuenta." });

        return await EnviarCodigo(Registro, correo);
    }
            bool enviado = await _emailSer.EnviarTokenRecuperacionAsync(dto.Correo, otp);
            if (enviado) return Json(new { success = true });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult VerificarToken([FromBody] string tokenIngresado) => VerificarCodigo(Registro, tokenIngresado);

    // ---------- Recuperación de contraseña ----------

    [HttpGet]
    public IActionResult ReestablecerContra() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigoRecuperacion([FromBody] SolicitudOtpDto dto)
    {
        var correo = (dto?.Correo ?? "").Trim();
        if (!CorreoValido(correo)) return Json(new { success = false, message = "Escribe un correo válido." });
        [HttpPost]
        public IActionResult VerificarToken([FromBody] string tokenIngresado)
        {
            string tokenGuardado = HttpContext.Session.GetString("CodigoRegistro");
            if (string.IsNullOrEmpty(tokenGuardado))
                return Json(new { success = false, message = "El código expiró. Solicita uno nuevo." });

        // Respuesta idéntica exista o no la cuenta, para no revelar qué correos están registrados.
        var existe = _db.Usuarios.AsNoTracking().Any(u => u.Correo == correo)
                     || _db.Clientes.AsNoTracking().Any(c => c.Correo == correo);
        if (!existe)
        {
            HttpContext.Session.SetString(Recuperacion + ":correo", correo);
            HttpContext.Session.Remove(Recuperacion + ":code");
            return Json(new { success = true });
        }
        return await EnviarCodigo(Recuperacion, correo);
    }
            if (tokenGuardado == tokenIngresado)
                return Json(new { success = true });

            return Json(new { success = false, message = "El código es incorrecto." });
        }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult VerificarTokenRecuperacion([FromBody] string tokenIngresado) => VerificarCodigo(Recuperacion, tokenIngresado);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ProcesarReestablecimiento(string NuevaContrasena)
    {
        var correo = HttpContext.Session.GetString(Recuperacion + ":ok");
        if (string.IsNullOrEmpty(correo))
        [Authorize]
        [HttpGet]
        public IActionResult MiPerfil()
        {
            TempData["Error"] = "Primero verifica tu correo con el código que te enviamos.";
            return RedirectToAction(nameof(ReestablecerContra));
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var cliente = _clienteSer.GetClientes().FirstOrDefault(c => c.Id.ToString() == userId);
            if (cliente == null) return NotFound();

            ViewBag.Sexos = _sexoSer.GetSexos().ToList();
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", cliente.IdSexo);
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", cliente.IdDocumento);

            return View(cliente);
        }
        if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena.Length < 6)
        {
            TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
            return RedirectToAction(nameof(ReestablecerContra));
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == correo);
        var cliente = usuario == null ? _db.Clientes.FirstOrDefault(c => c.Correo == correo) : null;
        if (usuario == null && cliente == null)
        {
            TempData["Error"] = "No se pudo actualizar la contraseña.";
            return RedirectToAction(nameof(ReestablecerContra));
        }

        var hash = Claves.Hash(NuevaContrasena);
        if (usuario != null) usuario.PasswordHash = hash; else cliente!.PasswordHash = hash;
        _db.SaveChanges();
        LimpiarCodigo(Recuperacion);
        TempData["Exito"] = "Tu contraseña fue actualizada. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    // ---------- Códigos de un solo uso (sesión) ----------
        public IActionResult Welcome() => View();

        [HttpGet]
        public IActionResult ReestablecerContra() => View();

    private async Task<IActionResult> EnviarCodigo(string flujo, string correo)
    {
        var s = HttpContext.Session;
        if (long.TryParse(s.GetString(flujo + ":sent"), out var ultimo) &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ultimo < SegundosEntreEnvios)
            return Json(new { success = false, message = "Espera unos segundos antes de pedir otro código." });
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

        var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        if (!await _emailSer.EnviarTokenRecuperacionAsync(correo, codigo))
            return Json(new { success = false, message = "No se pudo enviar el correo. Inténtalo más tarde." });

        s.SetString(flujo + ":code", codigo);
        s.SetString(flujo + ":correo", correo);
        s.SetString(flujo + ":exp", DateTimeOffset.UtcNow.AddMinutes(MinutosVigenciaCodigo).ToUnixTimeSeconds().ToString());
        s.SetString(flujo + ":sent", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        s.SetInt32(flujo + ":intentos", 0);
        s.Remove(flujo + ":ok");
        return Json(new { success = true });
    }
            bool enviado = await _emailSer.EnviarTokenRecuperacionAsync(dto.Correo, otp);
            if (enviado) return Json(new { success = true });

    private IActionResult VerificarCodigo(string flujo, string? ingresado)
    {
        var s = HttpContext.Session;
        var guardado = s.GetString(flujo + ":code");
        if (string.IsNullOrEmpty(guardado) ||
            !long.TryParse(s.GetString(flujo + ":exp"), out var exp) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
            return Json(new { success = false, message = "El código no existe o venció. Solicita uno nuevo." });

        var intentos = (s.GetInt32(flujo + ":intentos") ?? 0) + 1;
        s.SetInt32(flujo + ":intentos", intentos);
        if (intentos > IntentosMaximos)
        {
            s.Remove(flujo + ":code");
            return Json(new { success = false, message = "Demasiados intentos. Solicita un código nuevo." });
        }

        if (!string.Equals(guardado, (ingresado ?? "").Trim(), StringComparison.Ordinal))
            return Json(new { success = false, message = "El código es incorrecto." });
            string tokenGuardado = HttpContext.Session.GetString("CodigoRecuperacion");
            if (string.IsNullOrEmpty(tokenGuardado))
                return Json(new { success = false, message = "El código expiró. Solicita uno nuevo." });

        s.SetString(flujo + ":ok", s.GetString(flujo + ":correo") ?? "");
        s.Remove(flujo + ":code");
        return Json(new { success = true });
    }
            if (tokenGuardado == tokenIngresado) return Json(new { success = true });

            return Json(new { success = false, message = "El código es incorrecto." });
        }

    private void LimpiarCodigo(string flujo)
    {
        foreach (var k in new[] { "code", "correo", "exp", "sent", "intentos", "ok" })
            HttpContext.Session.Remove(flujo + ":" + k);
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

    private static bool CorreoValido(string correo)
        => correo.Length is > 5 and <= 100 && MailAddress.TryCreate(correo, out var m) && m.Address == correo;
            HttpContext.Session.Remove("CodigoRecuperacion");
            HttpContext.Session.Remove("CorreoRecuperacion");

    private void CargarCombos(Cliente? c = null)
    {
        ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", c?.IdSexo);
        ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", c?.IdDocumento);
    }
}
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult BloqueoSeguridad() => View();
    }

public class SolicitudOtpDto
{
    public string? Correo { get; set; }
    public string? Nombre { get; set; }
}
