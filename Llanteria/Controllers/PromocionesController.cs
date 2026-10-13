using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class PromocionesController : Controller
{
    private readonly LlanteriaDbContext _db;
    public PromocionesController(LlanteriaDbContext db) => _db = db;

    // Ofertas vigentes de la tabla Ofertas (si no hay, la vista muestra los beneficios fijos de la tienda).
    [HttpGet("Promociones")]
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var ofertas = await _db.Ofertas.AsNoTracking()
            .Where(o => o.Estado != false && o.FechaInicio <= hoy.AddDays(1) && o.FechaFin >= hoy)
            .OrderBy(o => o.FechaFin).ToListAsync();
        return View(ofertas);
    }
}
