using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Zona del cliente del club de puntos: sus puntos, facturas, vehículos, citas y premios.</summary>
[Authorize(Roles = "Cliente")]
public class MiCuentaController : Controller
{
    private readonly LlanteriaDbContext _db;

    public MiCuentaController(LlanteriaDbContext db) => _db = db;

    private int? IdCliente => int.TryParse(User.FindFirst("ClienteId")?.Value, out var id) ? id : null;

    public IActionResult Index()
    {
        if (IdCliente is not int id) return RedirectToAction("Login", "Account");

        var cliente = _db.Clientes.AsNoTracking().FirstOrDefault(c => c.Id == id);
        if (cliente == null) return RedirectToAction("Logout", "Account");

        var vm = new MiCuentaViewModel
        {
            Cliente = cliente,
            Facturas = _db.Facturas.AsNoTracking().Where(f => f.IdCliente == id)
                .OrderByDescending(f => f.Fecha).ThenByDescending(f => f.Id).Take(10).ToList(),
            Vehiculos = _db.Vehiculos.AsNoTracking().Where(v => v.IdCliente == id).OrderBy(v => v.Placa).ToList(),
            Citas = _db.Citas.AsNoTracking().Where(c => c.IdCliente == id)
                .OrderByDescending(c => c.Fecha).ThenByDescending(c => c.Id).Take(10).ToList(),
            Canjes = _db.CanjeIncentivos.AsNoTracking().Include(c => c.IdIncentivoNavigation)
                .Where(c => c.IdCliente == id).OrderByDescending(c => c.FechaCanje).Take(10).ToList(),
            Historial = HistorialVehiculos.De(_db, id),
            Premios = _db.CatalogoIncentivos.AsNoTracking()
                .Where(p => p.StockDisponible == null || p.StockDisponible > 0)
                .OrderBy(p => p.PuntosRequeridos).ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CambiarPassword(string actual, string nueva, string confirmacion)
    {
        if (IdCliente is not int id) return RedirectToAction("Login", "Account");
        var cliente = _db.Clientes.Find(id);
        if (cliente == null) return RedirectToAction("Logout", "Account");

        if (string.IsNullOrWhiteSpace(nueva) || nueva.Length < 6)
            TempData["Error"] = "La nueva contraseña debe tener al menos 6 caracteres.";
        else if (nueva != confirmacion)
            TempData["Error"] = "La confirmación no coincide con la nueva contraseña.";
        else if (!Claves.Verificar(cliente.PasswordHash, actual, out _))
            TempData["Error"] = "La contraseña actual no es correcta.";
        else
        {
            cliente.PasswordHash = Claves.Hash(nueva);
            _db.SaveChanges();
            TempData["Exito"] = "Contraseña actualizada.";
        }
        return RedirectToAction(nameof(Index));
    }
}

public class MiCuentaViewModel
{
    public Cliente Cliente { get; set; } = null!;
    public List<Factura> Facturas { get; set; } = new();
    public List<Vehiculo> Vehiculos { get; set; } = new();
    public List<Cita> Citas { get; set; } = new();
    public List<CanjeIncentivo> Canjes { get; set; } = new();
    public List<CatalogoIncentivo> Premios { get; set; } = new();
    public List<VehiculoHistorial> Historial { get; set; } = new();
}
