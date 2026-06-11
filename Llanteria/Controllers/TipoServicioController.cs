using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class TipoServicioController : Controller
{
    private readonly TipoServicioService ser;

    // Inyección del servicio para gestionar el catálogo de servicios técnicos
    public TipoServicioController(TipoServicioService tipoServicioService)
    {
        ser = tipoServicioService;
    }

    // Listado de servicios ofrecidos en la llantería
    public IActionResult Index()
    {
        var lista = ser.GetTiposServicio();
        return View(lista);
    }

    // Vista para agregar un nuevo tipo de servicio (ej: Rectificación de rines)
    public IActionResult Create()
    {
        return View();
    }

    // Guarda el servicio y su costo base
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TipoServicio ts)
    {
        if (ModelState.IsValid)
        {
            ser.AddTipoServicio(ts);
            return RedirectToAction(nameof(Index));
        }
        return View(ts);
    }

    // Vista para editar el precio o nombre de un servicio
    public IActionResult Edit(int id)
    {
        var ts = ser.GetTipoServicio(id);
        if (ts == null) return NotFound();
        return View(ts);
    }

    // Procesa la actualización del servicio
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TipoServicio ts)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateTipoServicio(ts);
            return RedirectToAction(nameof(Index));
        }
        return View(ts);
    }

    // Elimina un tipo de servicio (si no tiene facturas asociadas)
    public IActionResult Delete(int id)
    {
        ser.DeleteTipoServicio(id);
        return RedirectToAction(nameof(Index));
    }
}