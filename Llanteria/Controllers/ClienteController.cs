using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Llanteria.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteService ser;
        private readonly TipoDocumentoService docSer;
        private readonly SexoService sexoSer;

        public ClienteController(ClienteService clienteService, TipoDocumentoService documentoService, SexoService sexoService)
        {
            ser = clienteService;
            docSer = documentoService;
            sexoSer = sexoService;
        }

        // --- ACCIONES DE ADMINISTRACIÓN (SOLO ADMINS) ---
        [Authorize]
        public IActionResult Index() => View(ser.GetClientes());

        [Authorize]
        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente c)
        {
            if (ModelState.IsValid)
            {
                ser.AddCliente(c);
                return RedirectToAction(nameof(Index));
            }
            CargarCombos();
            return View(c);
        }

        // --- REGISTRO PÚBLICO (CLUB DE PUNTOS) ---
        [AllowAnonymous]
        public IActionResult Register()
        {
            CargarCombos();
            return View(); // Busca en Views/Cliente/Register.cshtml
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Cliente c)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(c);
            }

            try
            {
                ser.AddCliente(c);
                return RedirectToAction(nameof(ConfirmacionRegistro));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al registrarse: " + ex.Message);
                CargarCombos();
                return View(c);
            }
        }

        [AllowAnonymous]
        public IActionResult ConfirmacionRegistro() => View();

        // --- OTRAS ACCIONES ADMINISTRATIVAS ---
        [Authorize]
        public IActionResult Edit(int id)
        {
            var c = ser.GetCliente(id);
            if (c == null) return NotFound();
            CargarCombos();
            return View(c);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cliente c)
        {
            if (ModelState.IsValid)
            {
                ser.UpdateCliente(c);
                return RedirectToAction(nameof(Index));
            }
            CargarCombos();
            return View(c);
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            ser.DeleteCliente(id);
            return RedirectToAction(nameof(Index));
        }

        private void CargarCombos()
        {
            ViewBag.IdDocumento = new SelectList(docSer.GetTipoDocumentos() ?? new List<TipoDocumento>(), "Id", "Nombre");
            ViewBag.IdSexo = new SelectList(sexoSer.GetSexos() ?? new List<Sexo>(), "Id", "Nombre");
        }
    }
}