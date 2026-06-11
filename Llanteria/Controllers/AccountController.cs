using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

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

    // --- LOGIN PARA EMPLEADOS Y ADMINS ---
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // 💡 CREDENCIALES TEMPORALES PARA PRUEBAS Y SUSTENTACIÓN
        if (username == "admin" && password == "12345")
        {
            // Redirige directamente al Panel de Control que creamos
            return RedirectToAction("Index", "Dashboard");
        }

        // Lógica original (si no es el admin de prueba, busca en la base de datos)
        var user = _userSer.GetUsuarios()
            .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

        if (user != null && user.Estado == "Activo")
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "Credenciales inválidas o cuenta inactiva.";
        return View();
    }

    // --- REGISTRO PARA CLIENTES (SISTEMA DE INCENTIVOS) ---
    public IActionResult Register()
    {
        ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
        ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
        return View();
    }

    [HttpPost]
    public IActionResult Register(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            // Al registrarse por primera vez, le damos 50 puntos de bienvenida
            cliente.PuntosAcumulados = 50;
            _clienteSer.AddCliente(cliente);
            return RedirectToAction("Welcome");
        }
        ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
        ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
        return View(cliente);
    }

    public IActionResult Welcome() => View();
}