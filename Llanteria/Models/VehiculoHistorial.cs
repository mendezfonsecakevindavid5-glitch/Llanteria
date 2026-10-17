namespace Llanteria.Models;

/// <summary>Un vehículo con sus últimos trabajos y su próximo mantenimiento (para "Mi cuenta" y el perfil).</summary>
public class VehiculoHistorial
{
    public Vehiculo Vehiculo { get; set; } = null!;
    public List<OrdenTrabajo> Ordenes { get; set; } = new();

    /// <summary>Próximos servicios programados: uno por tipo de trabajo (el de la entrega más reciente).</summary>
    public List<OrdenTrabajo> Proximos => Ordenes
        .Where(o => o.Estado == OrdenTrabajo.Entregado && (o.ProximaFecha.HasValue || o.ProximoKm.HasValue))
        .GroupBy(o => o.Tipo)
        .Select(g => g.OrderByDescending(o => o.FechaEntrega ?? o.FechaIngreso).First())
        .OrderBy(o => o.ProximaFecha ?? DateTime.MaxValue)
        .ToList();
}
