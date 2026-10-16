using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de lubricantes (categoría "Aceites" del panel).</summary>
public class LubricantesController : CatalogoControllerBase
{
    public LubricantesController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Aceites";

    [HttpGet]
    public IActionResult Index(string[]? marca, int? min, int? max, string? orden, int pagina = 1)
        => View(Armar(null, null, null, marca, min, max, orden, pagina));
}
