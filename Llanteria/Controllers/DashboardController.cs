using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Llanteria.Controllers;

public class DashboardController : Controller
{
    private readonly LlanteriaDbContext _context;

    public DashboardController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioSerie = inicioMes.AddMonths(-5);
        var inicioSerieDO = DateOnly.FromDateTime(inicioSerie);
        var esAdmin = User.IsInRole("Administrador");
        var cultura = new CultureInfo("es-CO");

        // Se traen solo los registros de los últimos 6 meses y se agrupan en memoria.
        var facturas = _context.Facturas.AsNoTracking()
            .Where(f => f.Fecha.HasValue && f.Fecha.Value >= inicioSerieDO)
            .Select(f => new { Fecha = f.Fecha!.Value, f.TotalPagar }).ToList();
        var gastos = esAdmin
            ? _context.Gastos.AsNoTracking()
                .Where(g => g.FechaRegistro.HasValue && g.FechaRegistro.Value >= inicioSerie)
                .Select(g => new { Fecha = g.FechaRegistro!.Value, g.Monto }).ToList()
            : null;

        var vm = new DashboardViewModel
        {
            TotalClientes = _context.Clientes.Count(),
            TotalProductos = _context.Productos.Count(),
            UltimasFacturas = _context.Facturas.AsNoTracking().Include(f => f.IdClienteNavigation)
                .OrderByDescending(f => f.Fecha).ThenByDescending(f => f.Id).Take(6).ToList(),
            StockBajo = _context.Inventarios.AsNoTracking()
                .Include(i => i.IdProductoNavigation).Include(i => i.IdBodegaNavigation)
                .Where(i => i.StockActual != null && i.StockMinimo != null && i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual).Take(8).ToList()
        };

        for (var i = 0; i < 6; i++)
        {
            var mes = inicioSerie.AddMonths(i);
            var ventasMes = facturas.Where(f => f.Fecha.Year == mes.Year && f.Fecha.Month == mes.Month).Sum(f => f.TotalPagar);
            var gastosMes = gastos?.Where(g => g.Fecha.Year == mes.Year && g.Fecha.Month == mes.Month).Sum(g => g.Monto) ?? 0;
            vm.Meses.Add(new MesSerie(cultura.DateTimeFormat.GetAbbreviatedMonthName(mes.Month).TrimEnd('.'), ventasMes, gastosMes));
        }

        var actual = vm.Meses.Last();
        vm.VentasMes = actual.Ventas;
        vm.GastosMes = actual.Gastos;
        vm.FacturasMes = facturas.Count(f => f.Fecha.Year == hoy.Year && f.Fecha.Month == hoy.Month);

        if (esAdmin)
        {
            var limite = DateOnly.FromDateTime(hoy.AddDays(7));
            var pendientes = _context.Gastos.AsNoTracking().Where(g => g.Estado != "Pagado");
            vm.GastosPendientes = pendientes.Sum(g => (decimal?)g.Monto) ?? 0;
            vm.GastosPorVencer = pendientes.Count(g => g.FechaVencimiento <= limite);
        }

        return View(vm);
    }
}
