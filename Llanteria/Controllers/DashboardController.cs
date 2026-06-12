using Microsoft.AspNetCore.Mvc;
using Llanteria.Data;
using System.Linq;

namespace Llanteria.Controllers;

public class DashboardController : Controller
{
    private readonly LlanteriaDbContext _context;

    public DashboardController(LlanteriaDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Indicadores existentes
        ViewBag.TotalVentas = _context.Facturas.Sum(f => f.TotalPagar);
        ViewBag.CantidadClientes = _context.Clientes.Count();
        ViewBag.CantidadProductos = _context.Productos.Count();
        ViewBag.TotalGastos = _context.Gastos.Sum(g => g.Monto);
        ViewBag.CantidadBodegas = _context.Bodegas.Count();

        // ✅ CORRECCIÓN: Contar productos cuyo stock actual es menor al mínimo definido
        ViewBag.ProductosBajos = _context.Inventarios
            .Count(i => i.StockActual != null && i.StockActual <= i.StockMinimo);

        // Listado de últimas 5 ventas
        var ultimasVentas = _context.Facturas
            .OrderByDescending(f => f.Id)
            .Take(5)
            .ToList();

        return View(ultimasVentas);
    }
}