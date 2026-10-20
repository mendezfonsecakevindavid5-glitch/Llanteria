using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

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
    {
        ser = usuarioService;
        empSer = empleadoService;
        rolSer = roleService;
        _db = db;
    }

    public IActionResult Index() => View(ser.GetUsuarios());

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Usuario { Estado = "Activo" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Usuario u)
    {
        if (_db.Usuarios.Any(x => x.Username == u.Username))
            ModelState.AddModelError(nameof(Usuario.Username), "Ese nombre de usuario ya existe.");
        if (_db.Usuarios.Any(x => x.IdEmpleado == u.IdEmpleado))
            ModelState.AddModelError(nameof(Usuario.IdEmpleado), "Ese empleado ya tiene un usuario.");
        if (!string.IsNullOrEmpty(u.PasswordHash) && u.PasswordHash.Length < 6)
            ModelState.AddModelError(nameof(Usuario.PasswordHash), "La contraseña debe tener al menos 6 caracteres.");

        // El secretario puede crear usuarios, pero solo con rol "Empleado" (el administrador puede asignar cualquiera).
        // Se valida aquí también por si alguien manipula el formulario: el combo ya viene limitado en CargarCombos.
        if (!User.IsInRole(RolesApp.Administrador) && u.IdRol != IdRolEmpleado())
            ModelState.AddModelError(nameof(Usuario.IdRol), "Solo puedes crear usuarios con rol Empleado.");

        if (!ModelState.IsValid)
        {
            CargarCombos(u);
            return View(u);
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

    // Editar rol/estado de un usuario existente es solo del administrador: el secretario únicamente crea usuarios nuevos.
    [Authorize(Roles = RolesApp.Administrador)]
    public IActionResult Edit(int id)
    {
        var u = ser.GetUsuario(id);
        if (u == null) return NotFound();
        CargarCombos(u);
        return View(u);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesApp.Administrador)]
    public IActionResult Edit(int id, int idRol, string estado, string? motivoSancion, DateTime? fechaFinBaneo)
    {
        var u = ser.GetUsuario(id);
        if (u == null) return NotFound();

        if (!Estados.Contains(estado)) ModelState.AddModelError("Estado", "Estado no válido.");
        if (estado == "Baneado" && string.IsNullOrWhiteSpace(motivoSancion))
            ModelState.AddModelError("MotivoSancion", "Indica el motivo de la sanción.");

        var propio = User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString();
        if (propio && estado != "Activo")
            ModelState.AddModelError("Estado", "No puedes desactivar tu propia cuenta.");

        if (!ModelState.IsValid)
        {
            u.IdRol = idRol;
            u.Estado = estado;
            u.MotivoSancion = motivoSancion;
            u.FechaFinBaneo = fechaFinBaneo;
            CargarCombos(u);
            return View(u);
        }

        u.IdRol = idRol;
        u.Estado = estado;
        u.MotivoSancion = estado == "Baneado" ? motivoSancion?.Trim() : null;
        u.FechaFinBaneo = estado == "Baneado" ? fechaFinBaneo : null;
        _db.SaveChanges();
        TempData["Exito"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesApp.Administrador)]
    public IActionResult Delete(int id)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString())
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }
        ser.DeleteUsuario(id);
        TempData["Exito"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private int IdRolEmpleado() => rolSer.GetRoles().Where(r => r.NombreRol == RolesApp.Empleado).Select(r => r.Id).FirstOrDefault();

    private void CargarCombos(Usuario? u = null)
    {
        var conUsuario = _db.Usuarios.Select(x => x.IdEmpleado).ToList();
        var libres = empSer.GetEmpleados()
            .Where(e => !conUsuario.Contains(e.Id))
            .Select(e => new { e.Id, Nombre = $"{e.Nombres} {e.Apellidos} ({e.NumeroDocumento})" })
            .ToList();
        ViewBag.IdEmpleado = new SelectList(libres, "Id", "Nombre", u?.IdEmpleado);

        // El secretario solo ve y puede elegir el rol "Empleado"; el administrador ve todos (menos Cliente y Usuario registrado).
        var roles = User.IsInRole(RolesApp.Administrador)
            ? rolSer.GetRoles().Where(r => r.NombreRol != RolesApp.Cliente && r.NombreRol != RolesApp.UsuarioRegistrado)
            : rolSer.GetRoles().Where(r => r.NombreRol == RolesApp.Empleado);
        ViewBag.IdRol = new SelectList(roles, "Id", "NombreRol", u?.IdRol ?? IdRolEmpleado());
        ViewBag.Estados = new SelectList(Estados, u?.Estado ?? "Activo");
        ViewBag.EsSecretario = !User.IsInRole(RolesApp.Administrador);
    }
}