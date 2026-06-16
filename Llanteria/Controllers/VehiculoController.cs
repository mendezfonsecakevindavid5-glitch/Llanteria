using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class VehiculoController : Controller
{
    private readonly VehiculoService ser;
    private readonly ClienteService cliSer; // CAMBIO: Ahora usamos el servicio de Clientes directamente

    // Constructor actualizado con el servicio de Clientes
    public VehiculoController(VehiculoService vehiculoService, ClienteService clienteService)
    {
        ser = vehiculoService;
        cliSer = clienteService;
    }

    public IActionResult Index()
    {
        return View(ser.GetVehiculos());
    }

    public IActionResult Create()
    {
        CargarCombos();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Vehiculo v)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddVehiculo(v);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "No se pudo guardar el vehículo.");
        }
        CargarCombos();
        return View(v);
    }

    public IActionResult Edit(int id)
    {
        var v = ser.GetVehiculo(id);
        if (v == null) return NotFound();

        CargarCombos();
        return View(v);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Vehiculo v)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateVehiculo(v);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "No se pudo actualizar el vehículo.");
        }
        CargarCombos();
        return View(v);
    }

    public IActionResult Delete(int id)
    {
        ser.DeleteVehiculo(id);
        return RedirectToAction(nameof(Index));
    }

    // Método actualizado para llenar el Select con la lista de CLIENTES
    private void CargarCombos()
    {
        // Traemos la lista completa de clientes (ajusta .GetClientes() si tu método en el servicio se llama diferente, ej: .GetAll())
        var clientes = cliSer.GetClientes().Select(c => new
        {
            Id = c.Id,
            NombreFull = $"{c.Nombres} {c.Apellidos} - Documento: {c.NumeroDocumento}"
        });

        // Guardamos la lista en ViewBag.IdCliente (Recuerda cambiar en la vista Create/Edit el asp-for="IdConductor" por asp-for="IdCliente")
        ViewBag.IdCliente = new SelectList(clientes, "Id", "NombreFull");
    }
}