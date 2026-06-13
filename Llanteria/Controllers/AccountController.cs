using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Necesario para .Include si fuera necesario

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
        public async Task<IActionResult> Login(string username, string password)
        {
            // SOLUCIÓN: Buscamos en la lista directamente. 
            // Como ya es una lista, el Include no es necesario si la relación está cargada.
            var user = _userSer.GetUsuarios()
                .FirstOrDefault(u => u.Username == username);

            if (user != null && user.PasswordHash == password && user.Estado == "Activo")
            {
                // Si la navegación IdRolNavigation es null, es porque la lista no cargó la relación.
                // Accedemos a través de la propiedad que ya tienes en el modelo.
                string nombreRol = user.IdRolNavigation?.NombreRol ?? "Cliente";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, nombreRol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity), authProperties);

                if (nombreRol == "Administrador" || nombreRol == "Empleado")
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Home", "Index");
            }

            ViewBag.Error = "Credenciales inválidas o cuenta inactiva.";
            return View();
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
            ViewBag.IdSexo = new SelectList(_sexoSer.GetSexos(), "Id", "Nombre");
            ViewBag.IdDocumento = new SelectList(_docSer.GetTipoDocumentos(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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