using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class MarcaController : Controller
{
    private readonly MarcaService ser;

    // Inyección del servicio para gestionar las marcas de llantas y repuestos
    public MarcaController(MarcaService marcaService)
    {
        ser = marcaService;
    }

    // Listado de todas las marcas registradas
    public IActionResult Index()
    {
        var lista = ser.GetMarcas();
        return View(lista);
    }

    // Vista para agregar una marca nueva al catálogo
    public IActionResult Create()
    {
        return View();
    }

    // Procesa el registro de la nueva marca
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Marca m)
    {
        if (ModelState.IsValid)
        {
            ser.AddMarca(m);
            return RedirectToAction(nameof(Index));
        }
        return View(m);
    }

    // Vista para editar el nombre de una marca existente
    public IActionResult Edit(int id)
    {
        var m = ser.GetMarca(id);
        if (m == null) return NotFound();
        return View(m);
    }

    // Actualiza los datos de la marca
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Marca m)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateMarca(m);
            return RedirectToAction(nameof(Index));
        }
        return View(m);
    }

    // Elimina una marca (solo si no tiene productos asociados)
    public IActionResult Delete(int id)
    {
        ser.DeleteMarca(id);
        return RedirectToAction(nameof(Index));
    }
}