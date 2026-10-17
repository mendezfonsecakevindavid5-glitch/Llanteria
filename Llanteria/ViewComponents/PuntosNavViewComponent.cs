using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.ViewComponents;

/// <summary>Enlace "N puntos" del encabezado público: solo para clientes con sesión iniciada.</summary>
public class PuntosNavViewComponent : ViewComponent
{
    private readonly LlanteriaDbContext _db;
    public PuntosNavViewComponent(LlanteriaDbContext db) => _db = db;

    public IViewComponentResult Invoke()
    {
        if (!int.TryParse(HttpContext.User.FindFirst("ClienteId")?.Value, out var id))
            return Content(string.Empty);

        var puntos = _db.Clientes.AsNoTracking().Where(c => c.Id == id).Select(c => c.PuntosAcumulados).FirstOrDefault();
        return View(puntos);
    }
}
