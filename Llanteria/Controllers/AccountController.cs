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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private const int MinutosVigenciaCodigo = 10;
    private const int SegundosEntreEnvios = 30;
    private const int IntentosMaximos = 5;
    private const int PuntosDeBienvenida = 50;

    private const string Registro = "reg";
    private const string Recuperacion = "rec";

    private readonly LlanteriaDbContext _db;
    private readonly UsuarioService _userSer;
    private readonly ClienteService _clienteSer;
    private readonly SexoService _sexoSer;
    private readonly TipoDocumentoService _docSer;
    private readonly EmailService _emailSer;
    private readonly PaisService _paisSer;

    public AccountController(
        LlanteriaDbContext db,
        UsuarioService userSer,
        ClienteService clienteSer,
        SexoService sexoSer,
        TipoDocumentoService docSer,
        EmailService emailSer,
        PaisService paisSer)
    {
        _db = db;
        _userSer = userSer;
        _clienteSer = clienteSer;
        _sexoSer = sexoSer;
        _docSer = docSer;
        _emailSer = emailSer;
        _paisSer = paisSer;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirigirSegunRol(RolesApp.EsPersonal(User), returnUrl, rol: RolesApp.De(User));
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        username = (username ?? "").Trim();
        password ??= "";

        var user = _db.Usuarios.Include(u => u.IdRolNavigation)
            .FirstOrDefault(u => u.Username == username || u.Correo == username);

        if (user != null)
        {
            if (!Claves.Verificar(user.PasswordHash, password, out var actualizar))
                return ErrorLogin();
            if (actualizar) user.PasswordHash = Claves.Hash(password);

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

            var rol = NormalizarRol(user.IdRolNavigation?.NombreRol);
            await IniciarSesion(ClaimsDe(user, rol));
            return RedirigirSegunRol(EsRolDePanel(rol), returnUrl, user.IdCliente, rol);
        }

        return ErrorLogin();
    }

    private IActionResult ErrorLogin()
    {
        ViewBag.Error = "Usuario o contraseña incorrectos.";
        return View("Login");
    }

    private Task IniciarSesion(IEnumerable<Claim> claims)
        => HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

    /// <summary>
    /// Datos de la sesión. La foto NO va en la cookie (una imagen no cabe): el avatar la pide a User/Foto.
    /// "Nombres" y "Apellidos" se guardan para dibujar las iniciales sin consultar la base en cada página.
    /// </summary>
    private Claim[] ClaimsDe(Usuario user, string rol)
    {
        string nombres = "", apellidos = "";
        if (user.IdCliente.HasValue)
        {
            var c = _db.Clientes.AsNoTracking().Where(x => x.Id == user.IdCliente.Value)
                .Select(x => new { x.Nombres, x.Apellidos }).FirstOrDefault();
            nombres = c?.Nombres ?? ""; apellidos = c?.Apellidos ?? "";
        }
        else if (user.IdEmpleado.HasValue)
        {
            var e = _db.Empleados.AsNoTracking().Where(x => x.Id == user.IdEmpleado.Value)
                .Select(x => new { x.Nombres, x.Apellidos }).FirstOrDefault();
            nombres = e?.Nombres ?? ""; apellidos = e?.Apellidos ?? "";
        }

        return new Claim[]
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, rol),
            new("ClienteId", user.IdCliente?.ToString() ?? ""),
            new("Nombres", nombres),
            new("Apellidos", apellidos)
        };
    }

    // GET se conserva porque otros controladores redirigen aquí; el menú del avatar usa POST con token.
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Roles que trabajan en el panel (AdminLayout + menú lateral) en lugar del sitio público.</summary>
    private static bool EsRolDePanel(string? rol)
        => rol is RolesApp.Administrador or RolesApp.Secretario or RolesApp.Empleado;

    /// <summary>Aunque el rol se haya guardado con otras mayúsculas ("administrador"), se normaliza para que IsInRole lo reconozca.</summary>
    private static string NormalizarRol(string? nombreRol)
    {
        var rol = (nombreRol ?? RolesApp.UsuarioRegistrado).Trim();
        foreach (var conocido in new[] { RolesApp.Administrador, RolesApp.Secretario, RolesApp.Empleado, RolesApp.Cliente, RolesApp.UsuarioRegistrado })
            if (string.Equals(rol, conocido, StringComparison.OrdinalIgnoreCase)) return conocido;
        return rol;
    }

    /// <summary>Misma lógica que RedirigirSegunRol pero devolviendo la URL (para el login por código, que responde JSON).</summary>
    private string DestinoSegunRol(string rol, string? returnUrl, int? clienteId)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return returnUrl;
        if (EsRolDePanel(rol)) return Url.Action("Index", "Dashboard")!;
        // El usuario registrado solo ve las vistas del sitio (HomeController); su foto en el header abre su menú.
        if (rol == RolesApp.UsuarioRegistrado) return Url.Action("Index", "Home")!;
        return clienteId.HasValue || rol == RolesApp.Cliente
            ? Url.Action("Index", "MiCuenta")! : Url.Action("Index", "Home")!;
    }

    private IActionResult RedirigirSegunRol(bool esPersonal, string? returnUrl, int? clienteId = null, string? rol = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        if (esPersonal) return RedirectToAction("Index", "Dashboard");
        // El usuario registrado solo ve las vistas del sitio (HomeController): vuelve al inicio con su foto en el header.
        if (rol == RolesApp.UsuarioRegistrado) return RedirectToAction("Index", "Home");
        return clienteId.HasValue || rol == RolesApp.Cliente || User.IsInRole(RolesApp.Cliente)
            ? RedirectToAction("Index", "MiCuenta") : RedirectToAction("Index", "Home");
    }

    // --- MÉTODOS NUEVOS PARA EL LOGIN POR FASES (OTP) ---
    [HttpPost]
    public async Task<IActionResult> IniciarAutenticacion(string username, string password)
    {
        username = (username ?? "").Trim();
        password ??= "";

        var user = _db.Usuarios.Include(u => u.IdRolNavigation)
            .FirstOrDefault(u => u.Username == username || u.Correo == username);

        if (user == null || !Claves.Verificar(user.PasswordHash, password, out var actualizar))
        {
            return Json(new { success = false, message = "Usuario o contraseña incorrectos." });
        }

        if (actualizar)
        {
            user.PasswordHash = Claves.Hash(password);
            _db.SaveChanges();
        }

        if (user.Estado != "Activo")
        {
            string msg = user.Estado == "Baneado"
                ? "La cuenta está suspendida" + (user.FechaFinBaneo.HasValue ? $" hasta el {user.FechaFinBaneo:dd/MM/yyyy HH:mm}." : ".")
                : "La cuenta está inactiva. Comunícate con el administrador.";
            return Json(new { success = false, message = msg });
        }

        var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        if (string.IsNullOrWhiteSpace(user.Correo))
            return Json(new { success = false, message = "Tu cuenta no tiene un correo registrado para enviarte el código. Comunícate con el administrador." });

        // Antes el envío se lanzaba "sin esperar" y el resultado se ignoraba: si Brevo lo rechazaba, el usuario pasaba a la
        // segunda etapa y esperaba un código que nunca iba a llegar. Ahora se espera el resultado y, si falla, se avisa en la primera etapa.
        var envio = await _emailSer.EnviarTokenAsync(user.Correo, codigo);
        if (!envio.Ok)
            return Json(new { success = false, message = MensajeFalloCorreo(envio) });

        HttpContext.Session.SetString("login:username", user.Username);
        HttpContext.Session.SetString("login:code", codigo);
        HttpContext.Session.SetString("login:exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString());

        string correoCensurado = user.Correo ?? user.Username;
        if (!string.IsNullOrEmpty(user.Correo) && user.Correo.Contains("@"))
        {
            var partes = user.Correo.Split('@');
            var nombre = partes[0];
            var dominio = partes[1];
            correoCensurado = nombre.Length <= 2 ? nombre + "***@" + dominio : nombre.Substring(0, 2) + new string('*', nombre.Length - 2) + "@" + dominio;
        }

        return Json(new { success = true, correoCensurado = correoCensurado });
    }

    [HttpPost]
    public async Task<IActionResult> VerificarCodigoLogin(string username, string code, string? returnUrl = null)
    {
        var s = HttpContext.Session;
        var guardado = s.GetString("login:code");
        var sesionUser = s.GetString("login:username");

        if (string.IsNullOrEmpty(guardado) || string.IsNullOrEmpty(sesionUser) || !string.Equals(sesionUser, username, StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { success = false, message = "El código expiró o no es válido." });
        }

        if (!string.Equals(guardado, (code ?? "").Trim(), StringComparison.Ordinal))
        {
            return Json(new { success = false, message = "El código es incorrecto." });
        }

        var user = _db.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefault(u => u.Username == username);
        if (user == null) return Json(new { success = false, message = "Usuario no encontrado." });

        user.UltimaConexion = DateTime.Now;
        _db.SaveChanges();

        var rol = NormalizarRol(user.IdRolNavigation?.NombreRol);
        await IniciarSesion(ClaimsDe(user, rol));

        s.Remove("login:code");
        s.Remove("login:username");
        s.Remove("login:exp");

        // El administrador (y el personal) entra al Dashboard con AdminLayout; el resto, a su espacio.
        return Json(new { success = true, redirectUrl = DestinoSegunRol(rol, returnUrl, user.IdCliente) });
    }

    [HttpPost]
    public async Task<IActionResult> ReenviarCodigoLogin(string username)
    {
        var user = _db.Usuarios.FirstOrDefault(u => u.Username == username);
        if (user == null || string.IsNullOrEmpty(user.Correo))
        {
            return Json(new { success = false, message = "No se pudo reenviar el código." });
        }

        // Solo se reenvía dentro de un intento de inicio de sesión que ya pasó la contraseña (evita usar este endpoint para enviar correos a cualquiera).
        if (!string.Equals(HttpContext.Session.GetString("login:username"), user.Username, StringComparison.OrdinalIgnoreCase))
            return Json(new { success = false, message = "La sesión de verificación venció. Vuelve a iniciar sesión." });

        var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var envio = await _emailSer.EnviarTokenAsync(user.Correo, codigo);
        if (!envio.Ok)
            return Json(new { success = false, message = MensajeFalloCorreo(envio) });

        HttpContext.Session.SetString("login:code", codigo);
        HttpContext.Session.SetString("login:exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString());

        return Json(new { success = true });
    }
    // ---------------------------------------------------

    [HttpGet]
    public IActionResult Register()
    {
        CargarCombos();
        return View(new Cliente());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Cliente cliente, string? Username, string? Password, string? FotoBase64, bool AceptaTerminos = false)
    {
        cliente.Nombres = (cliente.Nombres ?? "").Trim();
        cliente.Apellidos = (cliente.Apellidos ?? "").Trim();
        cliente.Correo = (cliente.Correo ?? "").Trim();
        cliente.NumeroDocumento = (cliente.NumeroDocumento ?? "").Trim();

        if (!string.Equals(HttpContext.Session.GetString(Registro + ":ok"), cliente.Correo, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(Cliente.Correo), "Verifica tu correo con el código antes de crear la cuenta.");

        if (string.IsNullOrEmpty(Password) || Password.Length < 6)
            ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres.");

        if (string.IsNullOrEmpty(Username))
            ModelState.AddModelError("Username", "El nombre de usuario es obligatorio.");

        if (!AceptaTerminos)
            ModelState.AddModelError("AceptaTerminos", "Debes aceptar los términos y autorizar el tratamiento de tus datos personales.");

        var correo = cliente.Correo.ToLower();
        if (_db.Clientes.AsNoTracking().Any(c => c.NumeroDocumento == cliente.NumeroDocumento))
            ModelState.AddModelError(nameof(Cliente.NumeroDocumento), "Ya hay una cuenta con ese documento.");
        if (_db.Clientes.AsNoTracking().Any(c => c.Correo.ToLower() == correo) || _db.Usuarios.AsNoTracking().Any(u => u.Correo.ToLower() == correo))
            ModelState.AddModelError(nameof(Cliente.Correo), "Ya hay una cuenta con ese correo.");
        if (_db.Usuarios.AsNoTracking().Any(u => u.Username == Username))
            ModelState.AddModelError("Username", "El nombre de usuario ya se encuentra en uso.");

        if ((cliente.Telefono ?? "").Length > 20)
            ModelState.AddModelError(nameof(Cliente.Telefono), "El teléfono (con el prefijo del país) no puede tener más de 20 caracteres.");

        if (!ModelState.IsValid)
        {
            CargarCombos(cliente);
            if (EsAjax())
            {
                var primero = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
                return Json(new { success = false, message = primero ?? "Por favor, corrige los errores en el formulario." });
            }
            return View(cliente);
        }

        if (!string.IsNullOrEmpty(FotoBase64))
            cliente.Foto = FotoBase64;

        cliente.Id = 0;
        cliente.PuntosAcumulados = PuntosDeBienvenida;
        cliente.PasswordHash = Claves.Hash(Password!);

        // Cliente y usuario se guardan en UNA sola operación: o quedan los dos o no queda ninguno.
        // Se crea fuera del try para poder usarlo después (iniciar sesión con él).
        var nuevoUsuario = new Usuario
        {
            Username = Username!,
            Correo = cliente.Correo,
            PasswordHash = cliente.PasswordHash,
            IdRol = IdRolRegistrado(),
            Estado = "Activo",
            FechaCreacion = DateTime.Now,
            IdClienteNavigation = cliente,
            IdTipoDocumento = cliente.IdDocumento
        };

        try
        {
            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _db.ChangeTracker.Clear();
            HttpContext.RequestServices.GetRequiredService<ILogger<AccountController>>()
                .LogError(ex, "No se pudo registrar al usuario {Username}.", Username);

            var mensaje = MensajeDeBaseDeDatos(ex);
            CargarCombos(cliente);
            if (EsAjax()) return Json(new { success = false, message = mensaje });
            ModelState.AddModelError(string.Empty, mensaje);
            return View(cliente);
        }

        LimpiarCodigo(Registro);
        await IniciarSesion(ClaimsDe(nuevoUsuario, RolesApp.UsuarioRegistrado));

        if (EsAjax())
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Welcome)) });

        return RedirectToAction(nameof(Welcome));
    }

    private bool EsAjax()
        => Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true;

    /// <summary>Traduce el error de SQL Server a un mensaje que el usuario entienda (el detalle técnico solo se muestra en desarrollo).</summary>
    private string MensajeDeBaseDeDatos(DbUpdateException ex)
    {
        var sql = ex.GetBaseException() as SqlException;
        var mensaje = sql?.Number switch
        {
            2601 or 2627 when (sql.Message.Contains("UQ__Usuarios__CE6D8B9F67398071", StringComparison.Ordinal))
                => "La base de datos tiene un índice antiguo que impide crear más usuarios sin empleado. Reinicia la aplicación para que se corrija automáticamente.",
            2601 or 2627 => "Ya existe una cuenta con ese documento, correo o nombre de usuario.",
            547 => "Falta un dato base en la base de datos (rol, tipo de documento o sexo). Reinicia la aplicación para que se creen de nuevo.",
            2628 or 8152 => "Alguno de los datos es más largo de lo que permite la base de datos. Revisa nombres, teléfono y documento.",
            207 or 208 => "La base de datos no está actualizada con la versión de la aplicación (falta una tabla o columna). Reinicia la aplicación.",
            _ => "No se pudo guardar el registro. Inténtalo de nuevo en un momento."
        };

        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment() && sql != null) mensaje += $" [SQL {sql.Number}: {sql.Message}]";
        return mensaje;
    }

    // Antes se fijaba IdRol = 2, que en la base es "Empleado": quien se registraba entraba al panel.
    // Si el rol aún no existe (base recién creada), se crea aquí para que el registro nunca falle por eso.
    private int IdRolRegistrado()
    {
        var id = _db.Roles.AsNoTracking().Where(r => r.NombreRol == RolesApp.UsuarioRegistrado).Select(r => r.Id).FirstOrDefault();
        if (id != 0) return id;

        var rol = new Role { NombreRol = RolesApp.UsuarioRegistrado };
        _db.Roles.Add(rol);
        _db.SaveChanges();
        return rol.Id;
    }

    public IActionResult Welcome() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigoOtp([FromBody] SolicitudOtpDto dto)
    {
        var correo = (dto?.Correo ?? "").Trim();
        if (!Validaciones.CorreoValido(correo)) return Json(new { success = false, message = "Escribe un correo válido." });

        var correoLower = correo.ToLower();
        if (_db.Clientes.AsNoTracking().Any(c => c.Correo.ToLower() == correoLower) ||
            _db.Usuarios.AsNoTracking().Any(u => u.Correo.ToLower() == correoLower))
        {
            return Json(new { success = false, message = "Ese correo ya tiene una cuenta." });
        }

        return await EnviarCodigo(Registro, correo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult VerificarToken([FromBody] string tokenIngresado) => VerificarCodigo(Registro, tokenIngresado);

    [HttpGet]
    public IActionResult ReestablecerContra() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCodigoRecuperacion([FromBody] SolicitudOtpDto dto)
    {
        var correo = (dto?.Correo ?? "").Trim();
        if (!Validaciones.CorreoValido(correo)) return Json(new { success = false, message = "Escribe un correo válido." });

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
        if (usuario != null) usuario.PasswordHash = hash;
        if (cliente != null) cliente.PasswordHash = hash;
        _db.SaveChanges();
        LimpiarCodigo(Recuperacion);
        TempData["Exito"] = "Tu contraseña fue actualizada. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    private async Task<IActionResult> EnviarCodigo(string flujo, string correo)
    {
        var s = HttpContext.Session;
        if (long.TryParse(s.GetString(flujo + ":sent"), out var ultimo) &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ultimo < SegundosEntreEnvios)
            return Json(new { success = false, message = "Espera unos segundos antes de pedir otro código." });

        var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var envio = await _emailSer.EnviarTokenAsync(correo, codigo);
        if (!envio.Ok)
            return Json(new { success = false, message = MensajeFalloCorreo(envio) });

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

    /// <summary>Mensaje para el usuario cuando no se pudo enviar el código. En desarrollo se agrega el detalle técnico (el mismo queda siempre en el log).</summary>
    private string MensajeFalloCorreo(ResultadoCorreo r)
    {
        var mensaje = "No se pudo enviar el código a tu correo. Inténtalo de nuevo en un momento.";
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment()) mensaje += $" [{r.Mensaje} | {r.Motivo}]";
        return mensaje;
    }

    private void LimpiarCodigo(string flujo)
    {
        foreach (var k in new[] { "code", "correo", "exp", "sent", "intentos", "ok" })
            HttpContext.Session.Remove(flujo + ":" + k);
    }

    private void CargarCombos(Cliente? c = null)
    {
        var sexos = _sexoSer.GetSexos().ToList();
        var documentos = _docSer.GetTipoDocumentos().ToList();
        var paises = _paisSer.GetPaises().ToList();

        ViewBag.Sexos = sexos;
        ViewBag.TiposDocumento = documentos;
        ViewBag.Paises = paises;

        ViewBag.IdSexo = new SelectList(sexos, "Id", "Nombre", c?.IdSexo);
        ViewBag.IdDocumento = new SelectList(documentos, "Id", "Nombre", c?.IdDocumento);
    }

    [HttpGet]
    public IActionResult BloqueoSeguridad() => View();
}

public class SolicitudOtpDto
{
    public string? Correo { get; set; }
    public string? Nombre { get; set; }
}