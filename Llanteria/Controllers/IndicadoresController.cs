using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Llanteria.Controllers;

/// <summary>
/// Panel público de indicadores del negocio: la versión "ASOPARTES" (que muestra estadísticas del sector) aplicada
/// a un solo negocio. A propósito NO muestra cifras de dinero (ventas, ingresos): solo conteos (vehículos atendidos,
/// servicios realizados, clientes, reseñas), que son los que tiene sentido mostrar en una página pública.
/// </summary>
public class IndicadoresController : Controller
{
    private readonly LlanteriaDbContext _context;

    public IndicadoresController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index()
    {
        var hoy = DateTime.Today;
        var inicioAnio = new DateTime(hoy.Year, 1, 1);
        var inicioSerie = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-5);
        var cultura = new CultureInfo("es-CO");

        var vm = new IndicadoresViewModel
        {
            TotalClientes = _context.Clientes.Count(),
            TotalProductos = _context.Productos.Count(),
            VehiculosAtendidosAnio = _context.OrdenesTrabajo.AsNoTracking()
                .Count(o => o.Estado == OrdenTrabajo.Entregado && o.FechaEntrega != null && o.FechaEntrega >= inicioAnio),
            ServiciosRealizadosAnio = _context.Citas.AsNoTracking()
                .Count(c => c.Estado == Cita.Completada && c.Fecha >= inicioAnio),
        };

        var visibles = _context.Resenias.AsNoTracking().Where(r => r.Visible).ToList();
        vm.TotalResenias = visibles.Count;
        vm.PromedioResenias = visibles.Count > 0 ? visibles.Average(r => (double)r.Calificacion) : 0d;

        // Órdenes de trabajo entregadas por mes (conteo, no dinero) en los últimos 6 meses.
        var entregas = _context.OrdenesTrabajo.AsNoTracking()
            .Where(o => o.Estado == OrdenTrabajo.Entregado && o.FechaEntrega != null && o.FechaEntrega >= inicioSerie)
            .Select(o => o.FechaEntrega!.Value).ToList();

        for (var i = 0; i < 6; i++)
        {
            var mes = inicioSerie.AddMonths(i);
            var cantidad = entregas.Count(f => f.Year == mes.Year && f.Month == mes.Month);
            vm.VehiculosPorMes.Add(new IndicadorMes(cultura.DateTimeFormat.GetAbbreviatedMonthName(mes.Month).TrimEnd('.'), cantidad));
        }

        return View(vm);
    }
}
