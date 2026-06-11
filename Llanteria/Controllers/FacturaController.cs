using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Data;
using System.Linq;

namespace Llanteria.Controllers;

public class FacturaController : Controller
{
    private readonly FacturaService ser;
    private readonly LlanteriaDbContext _context;

    public FacturaController(FacturaService facturaService, LlanteriaDbContext context)
    {
        ser = facturaService;
        _context = context;
    }

    public IActionResult Index()
    {
        var lista = ser.GetFacturas();
        return View(lista);
    }

    public IActionResult Create()
    {
        ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Factura f)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddFactura(f);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "No se pudo guardar la factura. Intente nuevamente.");
        }

        ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
        return View(f);
    }

    public IActionResult Edit(int id)
    {
        var f = ser.GetFactura(id);
        if (f == null)
        {
            return NotFound();
        }

        ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
        return View(f);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Factura f)
    {
        if (id != f.Id)
        {
            return NotFound();
        }

        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateFactura(f);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "No se pudo actualizar la factura. Intente nuevamente.");
        }

        ViewBag.IdCliente = new SelectList(_context.Clientes.Select(c => new { c.Id, NombreCompleto = c.Nombres + " " + c.Apellidos }), "Id", "NombreCompleto", f.IdCliente);
        return View(f);
    }

    public IActionResult Delete(int id)
    {
        ser.DeleteFactura(id);
        return RedirectToAction(nameof(Index));
    }
}