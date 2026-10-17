using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public static class HistorialVehiculos
{
    /// <summary>Vehículos de un cliente con sus trabajos (sin los cancelados), del más reciente al más antiguo.</summary>
    public static List<VehiculoHistorial> De(LlanteriaDbContext db, int idCliente)
    {
        var vehiculos = db.Vehiculos.AsNoTracking().Where(v => v.IdCliente == idCliente).OrderBy(v => v.Placa).ToList();
        if (vehiculos.Count == 0) return new();

        var ids = vehiculos.Select(v => v.Id).ToList();
        var ordenes = db.OrdenesTrabajo.AsNoTracking()
            .Where(o => ids.Contains(o.IdVehiculo) && o.Estado != OrdenTrabajo.Cancelado)
            .OrderByDescending(o => o.FechaIngreso)
            .ToList();

        return vehiculos.Select(v => new VehiculoHistorial
        {
            Vehiculo = v,
            Ordenes = ordenes.Where(o => o.IdVehiculo == v.Id).ToList()
        }).ToList();
    }
}
