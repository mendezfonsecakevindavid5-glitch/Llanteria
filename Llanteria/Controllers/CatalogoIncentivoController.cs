using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class CatalogoIncentivoController : Controller
{
    private readonly CatalogoIncentivoService ser;

    // Inyección del servicio para gestionar el catálogo de premios/incentivos
    public CatalogoIncentivoController(CatalogoIncentivoService incentivoService)
    {
        ser = incentivoService;
    }

    // Listado de incentivos disponibles (Premios, Descuentos, etc.)
    public IActionResult Index()
    {
        var lista = ser.GetIncentivos();
        return View(lista);
    }

    // Vista para crear un nuevo incentivo o beneficio
    public IActionResult Create()
    {
        return View();
    }

    // Guarda el incentivo, definiendo cuántos puntos requiere el cliente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CatalogoIncentivo i)
    {
        if (ModelState.IsValid)
        {
            ser.AddIncentivo(i);
            return RedirectToAction(nameof(Index));
        }
        return View(i);
    }

    // Vista para editar un incentivo (ej. cambiar los puntos necesarios o la descripción)
    public IActionResult Edit(int id)
    {
        var i = ser.GetIncentivo(id);
        if (i == null) return NotFound();
        return View(i);
    }

    // Procesa la actualización del incentivo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CatalogoIncentivo i)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateIncentivo(i);
            return RedirectToAction(nameof(Index));
        }
        return View(i);
    }

    // Elimina un incentivo del catálogo
    public IActionResult Delete(int id)
    {
        ser.DeleteIncentivo(id);
        return RedirectToAction(nameof(Index));
    }
}