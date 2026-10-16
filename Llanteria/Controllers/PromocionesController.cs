using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class PromocionesController : Controller
{
    private readonly LlanteriaDbContext _db;
    public PromocionesController(LlanteriaDbContext db) => _db = db;

    // Productos con precio promocional + ofertas/cupones vigentes de la tabla Ofertas.
    [HttpGet("Promociones")]
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var vm = new PromocionesViewModel
        {
            Ofertas = await _db.Ofertas.AsNoTracking()
                .Where(o => o.Estado != false && o.FechaInicio <= hoy.AddDays(1) && o.FechaFin >= hoy)
                .OrderBy(o => o.FechaFin).ToListAsync(),
            Productos = await _db.Productos.AsNoTracking()
                .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
                .Where(p => p.PrecioPromocion > 0 && p.PrecioPromocion < p.PrecioVenta)
                .OrderBy(p => p.Nombre).ToListAsync()
        };

        var disp = Carrito.Disponibilidad(_db, vm.Productos.Select(p => p.Id));
        vm.Agotados = disp.Where(kv => kv.Value is <= 0).Select(kv => kv.Key).ToHashSet();
        return View(vm);
    }
}
