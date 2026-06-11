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
        // Indicadores clave para el negocio
        ViewBag.TotalVentas = _context.Facturas.Sum(f => f.TotalPagar);
        ViewBag.CantidadClientes = _context.Clientes.Count();
        ViewBag.CantidadProductos = _context.Productos.Count();
        ViewBag.TotalGastos = _context.Gastos.Sum(g => g.Monto); // Asumiendo propiedad Monto en Gasto

        // Listado de últimas 5 ventas para mostrar en una tabla rápida
        var ultimasVentas = _context.Facturas
            .OrderByDescending(f => f.Id)
            .Take(5)
            .ToList();

        return View(ultimasVentas);
    }
}