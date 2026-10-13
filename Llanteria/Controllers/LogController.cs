using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class LogController : Controller
{
    private readonly LlanteriaDbContext _context;

    public LogController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index(string? buscar, int pagina = 1)
    {
        var consulta = _context.LogActividads.AsNoTracking().Include(l => l.IdUsuarioNavigation).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var q = buscar.Trim();
            consulta = consulta.Where(l => l.Accion.Contains(q) || (l.TablaAfectada != null && l.TablaAfectada.Contains(q))
                                           || l.IdUsuarioNavigation.Username.Contains(q));
        }

        ViewBag.Buscar = buscar;
        return View(Paginado<LogActividad>.De(consulta.OrderByDescending(l => l.FechaRegistro).ThenByDescending(l => l.Id), pagina, 30));
    }
}
