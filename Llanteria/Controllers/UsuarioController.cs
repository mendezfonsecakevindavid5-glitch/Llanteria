using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
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

    public IActionResult Edit(int id)
    {
        var u = ser.GetUsuario(id);
        if (u == null) return NotFound();
        CargarCombos(u);
        return View(u);
    }

    [HttpPost, ValidateAntiForgeryToken]
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

    [HttpPost, ValidateAntiForgeryToken]
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

    private void CargarCombos(Usuario? u = null)
    {
        var conUsuario = _db.Usuarios.Select(x => x.IdEmpleado).ToList();
        var libres = empSer.GetEmpleados()
            .Where(e => !conUsuario.Contains(e.Id))
            .Select(e => new { e.Id, Nombre = $"{e.Nombres} {e.Apellidos} ({e.NumeroDocumento})" })
            .ToList();
        ViewBag.IdEmpleado = new SelectList(libres, "Id", "Nombre", u?.IdEmpleado);
        ViewBag.IdRol = new SelectList(rolSer.GetRoles().Where(r => r.NombreRol != "Cliente"), "Id", "NombreRol", u?.IdRol);
        ViewBag.Estados = new SelectList(Estados, u?.Estado ?? "Activo");
    }
}