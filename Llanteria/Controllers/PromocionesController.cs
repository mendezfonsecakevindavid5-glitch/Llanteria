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
        var ahora = DateTime.Now;
        var hoy = ahora.Date;

        var ofertas = await _db.Ofertas.AsNoTracking()
            .Include(o => o.EventoNavigation)
            .Where(o => o.Estado != false && o.FechaInicio <= hoy.AddDays(1) && o.FechaFin >= hoy)
            .OrderBy(o => o.FechaFin).ToListAsync();

        // Los eventos son pocos: se revisan en memoria con el reloj, así aparecen a las 12:00 a. m. de su día y se ocultan a las 11:59 p. m.
        var eventosHoy = (await _db.Eventos.AsNoTracking().ToListAsync())
            .Where(e => e.EsHoy(ahora)).OrderBy(e => e.Nombre).ToList();

        // Una promoción ligada a un evento solo se ve el día de ese evento.
        ofertas = ofertas.Where(o => !o.IdEvento.HasValue || (o.EventoNavigation != null && o.EventoNavigation.EsHoy(ahora))).ToList();

        // Productos con precio promocional o con descuento por porcentaje vigente (el precio final lo calcula Producto.PrecioActual).
        var candidatos = await _db.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
            .Where(p => p.PrecioPromocion > 0 || p.PorcentajeDescuento > 0)
            .OrderBy(p => p.Nombre).ToListAsync();

        var vm = new PromocionesViewModel
        {
            Ofertas = ofertas,
            EventosHoy = eventosHoy,
            Productos = candidatos.Where(p => p.EnPromocion).ToList()
        };

        var disp = Carrito.Disponibilidad(_db, vm.Productos.Select(p => p.Id));
        vm.Agotados = disp.Where(kv => kv.Value is <= 0).Select(kv => kv.Key).ToHashSet();
        return View(vm);
    }
}
