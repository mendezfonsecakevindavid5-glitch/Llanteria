using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de lubricantes (categoría "Aceites" del panel).</summary>
public class LubricantesController : CatalogoControllerBase
{
    public LubricantesController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Aceites";
    protected override string ModoExtra => "Lubricantes";

    [HttpGet]
    public IActionResult Index(string[]? marca, int? min, int? max, string? orden, int pagina = 1, string? tipo = null,
        string? qmarca = null, string? viscosidad = null, string? tipoAceite = null, int? garantia = null)
    {
        var vm = Armar(null, null, null, marca, min, max, orden, pagina, tipo, qmarca, viscosidad, tipoAceite, garantia: garantia);
        return EsAjax() ? PartialView("_ResultadosCatalogo", vm) : View(vm);
    }
}
