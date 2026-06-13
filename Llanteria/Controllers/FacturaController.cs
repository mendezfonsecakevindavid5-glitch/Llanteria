using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Data;
using Llanteria.Filters; // 👈 NECESARIO: Namespace de tu filtro
using System.Linq;
using System;

namespace Llanteria.Controllers
{
    public class FacturaController : Controller
    {
        private readonly FacturaService ser;
        private readonly ClienteService _clienteService;
        private readonly LlanteriaDbContext _context;

        public FacturaController(FacturaService facturaService, ClienteService clienteService, LlanteriaDbContext context)
        {
            ser = facturaService;
            _clienteService = clienteService;
            _context = context;
        }

        public IActionResult Index() => View(ser.GetFacturas());

        public IActionResult Create()
        {
            ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Aplica el filtro para auditar la creación de facturas
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Creó nueva factura", "Factura" })]
        public IActionResult Create(Factura f)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Guardar la factura
                    ser.AddFactura(f);

                    // 2. Lógica de Puntos
                    int puntosGanados = (int)(f.TotalPagar / 1000);
                    var cliente = _clienteService.GetCliente(f.IdCliente);
                    if (cliente != null)
                    {
                        int nuevosPuntos = cliente.PuntosAcumulados + puntosGanados;
                        _clienteService.ActualizarPuntos(cliente.Id, nuevosPuntos);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al procesar: " + ex.Message);
                }
            }

            ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
            return View(f);
        }

        public IActionResult Edit(int id)
        {
            var f = ser.GetFactura(id);
            if (f == null) return NotFound();

            ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
            return View(f);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Aplica el filtro para auditar la edición de facturas
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó una factura", "Factura" })]
        public IActionResult Edit(int id, Factura f)
        {
            if (id != f.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ser.UpdateFactura(f);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "No se pudo actualizar la factura.");
                }
            }
            ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
            return View(f);
        }

        // Aplica el filtro para auditar la eliminación de facturas
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó una factura", "Factura" })]
        public IActionResult Delete(int id)
        {
            ser.DeleteFactura(id);
            return RedirectToAction(nameof(Index));
        }


    }
}