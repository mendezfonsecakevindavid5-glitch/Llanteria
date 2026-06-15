using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class BodegaController : Controller
{
    private readonly BodegaService ser;

    public BodegaController(BodegaService bodegaService)
    {
        ser = bodegaService;
    }

    public IActionResult Index()
    {
        return View(ser.GetBodegas());
    }

    public IActionResult Create()
    {
        return View();
    }

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

    public IActionResult Edit(int id)
    {
        var b = ser.GetBodega(id);
        if (b == null) return NotFound();
        return View(b);
    }

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

    public IActionResult Delete(int id)
    {
        ser.DeleteBodega(id);
        return RedirectToAction(nameof(Index));
    }
}