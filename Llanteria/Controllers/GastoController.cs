using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;

namespace Llanteria.Controllers;

public class GastoController : Controller
{
    private readonly GastoService ser;

    // Inyección del servicio de gastos para gestionar egresos
    public GastoController(GastoService gastoService)
    {
        ser = gastoService;
    }

    // Listado de todos los gastos (incluye la categoría del gasto)
    public IActionResult Index()
    {
        var lista = ser.GetGastos();
        return View(lista);
    }

    // Vista para registrar un nuevo gasto o factura por pagar
    public IActionResult Create()
    {
        return View();
    }

    // Acción para guardar el registro del gasto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Gasto g)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddGasto(g);
                return RedirectToAction(nameof(Index));
            }
            return View(g);
        }
        catch
        {
            return View(g);
        }
    }

    // Vista para editar un gasto (ej. cambiar estado de Pendiente a Pago)
    public IActionResult Edit(int id)
    {
        var g = ser.GetGasto(id);
        if (g == null)
        {
            return NotFound();
        }
        return View(g);
    }

    // Acción para actualizar la información del gasto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Gasto g)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateGasto(g);
                return RedirectToAction(nameof(Index));
            }
            return View(g);
        }
        catch
        {
            return View(g);
        }
    }

    // Acción para eliminar un registro de gasto (compatible con SweetAlert2)
    public IActionResult Delete(int id)
    {
        ser.DeleteGasto(id);
        return RedirectToAction(nameof(Index));
    }
}