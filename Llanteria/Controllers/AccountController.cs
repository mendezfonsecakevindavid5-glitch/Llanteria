using System.Net.Mail;
using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
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

    public AccountController(LlanteriaDbContext db, ClienteService clienteSer, SexoService sexoSer,
        TipoDocumentoService docSer, EmailService emailSer)
    {
        _db = db;
        _clienteSer = clienteSer;
        _sexoSer = sexoSer;
        _docSer = docSer;
        _emailSer = emailSer;
    }

    // ---------- Inicio y cierre de sesión ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirigirSegunRol(User.IsInRole("Administrador") || User.IsInRole("Empleado"), returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        username = (username ?? "").Trim();
        password ??= "";

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

        // 2) Cliente del club de puntos (correo o número de documento).
        var cliente = _db.Clientes.FirstOrDefault(c => c.Correo == username || c.NumeroDocumento == username);
        if (cliente == null || !Claves.Verificar(cliente.PasswordHash, password, out var actualizarCliente))
            return ErrorLogin();
        if (actualizarCliente)
        {
            cliente.PasswordHash = Claves.Hash(password);
            _db.SaveChanges();
        }

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

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Register(Cliente cliente, string? contrasena, string? confirmacion)
    {
        cliente.Nombres = (cliente.Nombres ?? "").Trim();
        cliente.Apellidos = (cliente.Apellidos ?? "").Trim();
        cliente.Correo = (cliente.Correo ?? "").Trim();
        cliente.NumeroDocumento = (cliente.NumeroDocumento ?? "").Trim();

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

        cliente.Id = 0;
        cliente.PuntosAcumulados = PuntosDeBienvenida;
        cliente.PasswordHash = Claves.Hash(contrasena!);
        _clienteSer.AddCliente(cliente);
        LimpiarCodigo(Registro);
        return RedirectToAction(nameof(Welcome));
    }

    public IActionResult Welcome() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigoOtp([FromBody] SolicitudOtpDto dto)
    {
        var correo = (dto?.Correo ?? "").Trim();
        if (!CorreoValido(correo)) return Json(new { success = false, message = "Escribe un correo válido." });

        var correoLower = correo.ToLower();
        if (_db.Clientes.AsNoTracking().Any(c => c.Correo.ToLower() == correoLower))
            return Json(new { success = false, message = "Ese correo ya tiene una cuenta." });

        return await EnviarCodigo(Registro, correo);
    }

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

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult VerificarTokenRecuperacion([FromBody] string tokenIngresado) => VerificarCodigo(Recuperacion, tokenIngresado);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ProcesarReestablecimiento(string NuevaContrasena)
    {
        var correo = HttpContext.Session.GetString(Recuperacion + ":ok");
        if (string.IsNullOrEmpty(correo))
        {
            TempData["Error"] = "Primero verifica tu correo con el código que te enviamos.";
            return RedirectToAction(nameof(ReestablecerContra));
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

    private async Task<IActionResult> EnviarCodigo(string flujo, string correo)
    {
        var s = HttpContext.Session;
        if (long.TryParse(s.GetString(flujo + ":sent"), out var ultimo) &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ultimo < SegundosEntreEnvios)
            return Json(new { success = false, message = "Espera unos segundos antes de pedir otro código." });

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

        s.SetString(flujo + ":ok", s.GetString(flujo + ":correo") ?? "");
        s.Remove(flujo + ":code");
        return Json(new { success = true });
    }

    private void LimpiarCodigo(string flujo)
    {
        foreach (var k in new[] { "code", "correo", "exp", "sent", "intentos", "ok" })
            HttpContext.Session.Remove(flujo + ":" + k);
    }

    private static bool CorreoValido(string correo)
        => correo.Length is > 5 and <= 100 && MailAddress.TryCreate(correo, out var m) && m.Address == correo;

    private void CargarCombos(Cliente? c = null)
    {
        ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre", c?.IdSexo);
        ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre", c?.IdDocumento);
    }
}

public class SolicitudOtpDto
{
    public string? Correo { get; set; }
    public string? Nombre { get; set; }
}
