using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class UsuarioController : Controller
{
    private readonly UsuarioService ser;

    // Inyección del servicio de usuarios
    public UsuarioController(UsuarioService usuarioService)
    {
        ser = usuarioService;
    }

    // --- SECCIÓN DE ACCESO (LOGIN) ---

    // Muestra la pantalla de inicio de sesión
    public IActionResult Login()
    {
        return View();
    }

    // Procesa las credenciales de acceso
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        // Buscamos el usuario que coincida con el Username y PasswordHash
        var user = ser.GetUsuarios()
            .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

        if (user != null)
        {
            // Validamos que el estado sea 'Activo'
            if (user.Estado == "Activo")
            {
                // Actualizamos la fecha de última conexión (opcional)
                user.UltimaConexion = DateTime.Now;
                ser.UpdateUsuario(user);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Su cuenta se encuentra: " + user.Estado;
            return View();
        }

        ViewBag.Error = "Nombre de usuario o contraseña incorrectos";
        return View();
    }

    // --- SECCIÓN DE GESTIÓN (CRUD) ---

    // Lista todos los usuarios registrados
    public IActionResult Index()
    {
        return View(ser.GetUsuarios());
    }

    // Vista para el registro de nuevos usuarios
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Usuario u)
    {
        if (ModelState.IsValid)
        {
            ser.AddUsuario(u);
            return RedirectToAction(nameof(Index));
        }
        return View(u);
    }

    // Vista para editar un usuario existente
    public IActionResult Edit(int id)
    {
        var u = ser.GetUsuario(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Usuario u)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateUsuario(u);
            return RedirectToAction(nameof(Index));
        }
        return View(u);
    }

    // Acción para eliminar un usuario
    public IActionResult Delete(int id)
    {
        ser.DeleteUsuario(id);
        return RedirectToAction(nameof(Index));
    }
}