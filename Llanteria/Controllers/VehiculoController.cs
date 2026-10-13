using Llanteria.Data;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class VehiculoController : Controller
{
    private readonly VehiculoService ser;
    private readonly ClienteService cliSer;
    private readonly LlanteriaDbContext _db;

    public VehiculoController(VehiculoService vehiculoService, ClienteService clienteService, LlanteriaDbContext db)
    {
        ser = vehiculoService;
        cliSer = clienteService;
        _db = db;
    }

    public IActionResult Index() => View(ser.GetVehiculos());

    public IActionResult Create(int? idCliente)
    {
        CargarCombos(idCliente);
        return View(new Vehiculo { IdCliente = idCliente ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Vehiculo v)
    {
        Normalizar(v);
        if (_db.Vehiculos.AsNoTracking().Any(x => x.Placa == v.Placa))
            ModelState.AddModelError(nameof(Vehiculo.Placa), "Ya existe un vehículo con esa placa.");
        if (!ModelState.IsValid)
        {
            CargarCombos(v.IdCliente);
            return View(v);
        }

        ser.AddVehiculo(v);
        TempData["Exito"] = "Vehículo registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var v = ser.GetVehiculo(id);
        if (v == null) return NotFound();
        CargarCombos(v.IdCliente);
        return View(v);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Vehiculo v)
    {
        v.Id = id;
        Normalizar(v);
        if (_db.Vehiculos.AsNoTracking().Any(x => x.Placa == v.Placa && x.Id != id))
            ModelState.AddModelError(nameof(Vehiculo.Placa), "Ya existe otro vehículo con esa placa.");
        if (!ModelState.IsValid)
        {
            CargarCombos(v.IdCliente);
            return View(v);
        }

        ser.UpdateVehiculo(v);
        TempData["Exito"] = "Vehículo actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        ser.DeleteVehiculo(id);
        TempData["Exito"] = "Vehículo eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalizar(Vehiculo v)
    {
        v.Placa = (v.Placa ?? "").Trim().ToUpperInvariant();
        v.Marca = (v.Marca ?? "").Trim();
        v.Modelo = (v.Modelo ?? "").Trim();
    }

    private void CargarCombos(int? seleccionado = null)
    {
        var clientes = cliSer.GetClientes()
            .OrderBy(c => c.Nombres)
            .Select(c => new { c.Id, Nombre = $"{c.Nombres} {c.Apellidos} — {c.NumeroDocumento}" });
        ViewBag.IdCliente = new SelectList(clientes, "Id", "Nombre", seleccionado);
    }
}
