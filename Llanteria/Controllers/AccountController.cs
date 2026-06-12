using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters; // 👈 Necesario para el filtro
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace Llanteria.Controllers
{
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

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        // Se registra el intento de login exitoso mediante el filtro
        // Nota: Solo se registrará si el método retorna un resultado exitoso (RedirectToAction)
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Inicio de sesión exitoso", "Usuarios" })]
        public IActionResult Login(string username, string password)
        {
            var user = _userSer.GetUsuarios()
                .FirstOrDefault(u => u.Username == username);

            if (user != null && user.PasswordHash == password && user.Estado == "Activo")
            {
                // Aquí deberías crear la sesión (Cookie de autenticación/Claims)
                // Es vital que aquí guardes el ID del usuario en los Claims 
                // para que el filtro de Log funcione correctamente.
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Credenciales inválidas o cuenta inactiva.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Registro de nuevo cliente", "Clientes" })]
        public IActionResult Register(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
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
}