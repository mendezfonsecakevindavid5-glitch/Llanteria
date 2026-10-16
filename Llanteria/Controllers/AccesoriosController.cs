using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de accesorios.</summary>
public class AccesoriosController : CatalogoControllerBase
{
    public AccesoriosController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Accesorios";

    [HttpGet]
    public IActionResult Index(string[]? marca, int? min, int? max, string? orden, int pagina = 1)
        => View(Armar(null, null, null, marca, min, max, orden, pagina));
}
