using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Llanteria.Controllers;

public class UsuarioController : Controller
{
    private static readonly string[] Estados = { "Activo", "Inactivo", "Baneado" };

    private readonly UsuarioService ser;
    private readonly EmpleadoService empSer;
    private readonly RoleService rolSer;
    private readonly LlanteriaDbContext _db;

    public UsuarioController(UsuarioService usuarioService, EmpleadoService empleadoService,
        RoleService roleService, LlanteriaDbContext db)
    public UsuarioController(UsuarioService usuarioService)
    {
        ser = usuarioService;
        empSer = empleadoService;
        rolSer = roleService;
        _db = db;
    }

    public IActionResult Index() => View(ser.GetUsuarios());

    public IActionResult Create()
    public IActionResult Login()
    {
        CargarCombos();
        return View(new Usuario { Estado = "Activo" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Usuario u)
    [HttpPost]
    public IActionResult IniciarAutenticacion(string username, string password)
    {
        if (_db.Usuarios.Any(x => x.Username == u.Username))
            ModelState.AddModelError(nameof(Usuario.Username), "Ese nombre de usuario ya existe.");
        if (_db.Usuarios.Any(x => x.IdEmpleado == u.IdEmpleado))
            ModelState.AddModelError(nameof(Usuario.IdEmpleado), "Ese empleado ya tiene un usuario.");
        if (!string.IsNullOrEmpty(u.PasswordHash) && u.PasswordHash.Length < 6)
            ModelState.AddModelError(nameof(Usuario.PasswordHash), "La contraseña debe tener al menos 6 caracteres.");
        if (!ModelState.IsValid)
        // Buscar el usuario solo por nombre o correo
        var user = ser.GetUsuarios()
        .FirstOrDefault(u => u.Username == username || u.Correo == username);

        // Verificar si existe y si la contraseña combinada coincide utilizando el Helper
        if (user == null || !Llanteria.Services.SecurityService.VerificarPassword(password, user.PasswordHash))
        {
            CargarCombos(u);
            return View(u);
        }
            return Json(new { success = false, message = "Nombre de usuario o contraseña incorrectos." });
        }

        if (user.Estado != "Activo")
        {
            return Json(new { success = false, message = "Su cuenta se encuentra: " + user.Estado });
        }

        // Verifica si el correo está vacío, nulo o contiene el correo por defecto de migración
        bool sinCorreoValido = string.IsNullOrWhiteSpace(user.Correo) || user.Correo == "sin_correo@llanteria.com";

        if (sinCorreoValido)
        {
            // PRIMERA VEZ: UltimaConexion es null -> Ingresa por única vez
            if (user.UltimaConexion == null)
            {
                user.UltimaConexion = DateTime.Now; // Se registra el ingreso para invalidar futuros intentos sin correo
                ser.UpdateUsuario(user);

                HttpContext.Session.SetString("UsuarioId", user.Id.ToString());

                return Json(new
                {
                    success = true,
                    esUsuarioLegacy = true,
                    redirectUrl = Url.Action("Index", "Home")
                });
            }

        u.PasswordHash = Claves.Hash(u.PasswordHash);
        u.Estado = "Activo";
        u.FechaCreacion = DateTime.Now;
        u.MotivoSancion = null;
        u.FechaFinBaneo = null;
        ser.AddUsuario(u);
        TempData["Exito"] = "Usuario creado.";
        return RedirectToAction(nameof(Index));
    }
            // SEGUNDA VEZ EN ADELANTE: UltimaConexion ya no es null -> Se bloquea el inicio de sesión
            return Json(new
            {
                success = false,
                message = "Acceso denegado. No tiene un correo electrónico registrado para completar la verificación en dos pasos (2FA). Contacte al administrador para vincular su correo."
            });
        }

    public IActionResult Edit(int id)
    {
        var u = ser.GetUsuario(id);
        if (u == null) return NotFound();
        CargarCombos(u);
        return View(u);
        // Flujo Estándar: Generación y envío del código 2FA
        string codigoVerificacion = new Random().Next(100000, 999999).ToString();
        HttpContext.Session.SetString($"2FA_Code_{username}", codigoVerificacion);
        HttpContext.Session.SetString($"2FA_UserPending", username);



        // Lógica de envío de correo omitida para brevedad de servicio SMTP



        return Json(new { success = true, esUsuarioLegacy = false });

    }


        if (!Estados.Contains(estado)) ModelState.AddModelError("Estado", "Estado no válido.");
        if (estado == "Baneado" && string.IsNullOrWhiteSpace(motivoSancion))
            ModelState.AddModelError("MotivoSancion", "Indica el motivo de la sanción.");

    [HttpPost]

    public IActionResult VerificarCodigo(string username, string code)

    {

        string? codigoGuardado = HttpContext.Session.GetString($"2FA_Code_{username}");



        if (!string.IsNullOrEmpty(codigoGuardado) && codigoGuardado == code)

        // No permitir que el administrador se bloquee a sí mismo.
        var propio = User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString();
        if (propio && estado != "Activo")
            ModelState.AddModelError("Estado", "No puedes desactivar tu propia cuenta.");
        {

            var user = ser.GetUsuarios().FirstOrDefault(u => u.Username == username);

            if (user != null)

        if (!ModelState.IsValid)
        {
            u.IdRol = idRol;
            u.Estado = estado;
            u.MotivoSancion = motivoSancion;
            u.FechaFinBaneo = fechaFinBaneo;
            CargarCombos(u);
            return View(u);
        }
            {

                user.UltimaConexion = DateTime.Now;

                ser.UpdateUsuario(user);

                HttpContext.Session.SetString("UsuarioId", user.Id.ToString());

            }



            HttpContext.Session.Remove($"2FA_Code_{username}");

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

        }

        u.IdRol = idRol;
        u.Estado = estado;
        u.MotivoSancion = estado == "Baneado" ? motivoSancion?.Trim() : null;
        u.FechaFinBaneo = estado == "Baneado" ? fechaFinBaneo : null;
        _db.SaveChanges();
        TempData["Exito"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));


        return Json(new { success = false, message = "El código ingresado es incorrecto o ha expirado." });

    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString())
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }
        ser.DeleteUsuario(id); // si tiene registros asociados, el filtro global avisa del error
        TempData["Exito"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));


    [HttpPost]

    public IActionResult ReenviarCodigo(string username)

    {

        string nuevoCodigo = new Random().Next(100000, 999999).ToString();

        HttpContext.Session.SetString($"2FA_Code_{username}", nuevoCodigo);



        // Lógica de reenvío por correo SMTP



        return Json(new { success = true });

    }

    private void CargarCombos(Usuario? u = null)
    {
        // Al crear, solo se ofrecen empleados que todavía no tienen usuario.
        var conUsuario = _db.Usuarios.Select(x => x.IdEmpleado).ToList();
        var libres = empSer.GetEmpleados()
            .Where(e => !conUsuario.Contains(e.Id))
            .Select(e => new { e.Id, Nombre = $"{e.Nombres} {e.Apellidos} ({e.NumeroDocumento})" })
            .ToList();
        ViewBag.IdEmpleado = new SelectList(libres, "Id", "Nombre", u?.IdEmpleado);
        ViewBag.IdRol = new SelectList(rolSer.GetRoles(), "Id", "NombreRol", u?.IdRol);
        ViewBag.Estados = new SelectList(Estados, u?.Estado ?? "Activo");
    }
}
}