using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Bandeja de sugerencias enviadas desde la sección pública «Ayúdanos a mejorar» (solo administrador, ver RoleConvention).</summary>
public class SugerenciasController : Controller
{
    private readonly LlanteriaDbContext _db;

    public SugerenciasController(LlanteriaDbContext db) => _db = db;

    public IActionResult Index(string? estado, int pagina = 1)
    {
        var consulta = _db.Sugerencias.AsNoTracking().AsQueryable();
        if (Sugerencia.Estados.Contains(estado ?? "")) consulta = consulta.Where(s => s.Estado == estado);

        ViewBag.Estado = estado;
        ViewBag.Nuevas = _db.Sugerencias.Count(s => s.Estado == Sugerencia.Nueva);
        ViewBag.Promedio = _db.Sugerencias.Any() ? _db.Sugerencias.Average(s => (double)s.Calificacion) : 0d;
        return View(Paginado<Sugerencia>.De(consulta.OrderByDescending(s => s.Fecha), pagina));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, string estado)
    {
        var s = _db.Sugerencias.Find(id);
        if (s != null && Sugerencia.Estados.Contains(estado))
        {
            s.Estado = estado;
            _db.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
}
