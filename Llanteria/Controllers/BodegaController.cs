using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class BodegaController : Controller
{
    private readonly BodegaService ser;

    // Inyectamos el servicio de bodega
    public BodegaController(BodegaService bodegaService)
    {
        ser = bodegaService;
    }

    // Listado de todas las bodegas o puntos de almacenamiento
    public IActionResult Index()
    {
        var lista = ser.GetBodegas();
        return View(lista);
    }

    // Vista para registrar una nueva ubicación física
    public IActionResult Create()
    {
        return View();
    }

    // Procesa el registro de la nueva bodega
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Bodega b)
    {
        if (ModelState.IsValid)
        {
            ser.AddBodega(b);
            return RedirectToAction(nameof(Index));
        }
        return View(b);
    }

    // Vista para editar el nombre o detalles de la bodega
    public IActionResult Edit(int id)
    {
        var b = ser.GetBodega(id);
        if (b == null) return NotFound();
        return View(b);
    }

    // Actualiza la información de la bodega
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Bodega b)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateBodega(b);
            return RedirectToAction(nameof(Index));
        }
        return View(b);
    }

    // Elimina una bodega del sistema
    public IActionResult Delete(int id)
    {
        ser.DeleteBodega(id);
        return RedirectToAction(nameof(Index));
    }
}