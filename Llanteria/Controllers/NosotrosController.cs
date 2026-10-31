using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>
/// Ficha pública del negocio: una sola página con los datos de contacto, horario, ubicación, redes y servicios,
/// en el formato en que un gremio (p. ej. ASOPARTES / Directorio Motriz) pide la información de un negocio afiliado.
/// </summary>
public class NosotrosController : Controller
{
    private readonly LlanteriaDbContext _context;

    public NosotrosController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index()
    {
        ViewBag.Servicios = _context.ServiciosPublicados();
        return View();
    }
}
