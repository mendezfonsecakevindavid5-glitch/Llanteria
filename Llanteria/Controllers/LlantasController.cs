using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de llantas con búsqueda por medida, marca y precio, en su propia página.</summary>
public class LlantasController : CatalogoControllerBase
{
    public LlantasController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Llantas";
    protected override bool PorMedida => true;
    protected override string ModoExtra => "Llantas";

    [HttpGet]
    public IActionResult Index(string? ancho, string? perfil, string? diametro, string[]? marca,
        int? min, int? max, string? orden, int pagina = 1, string? tipo = null, string? qmarca = null)
    {
        var vm = Armar(ancho, perfil, diametro, marca, min, max, orden, pagina, tipo, qmarca);
        return EsAjax() ? PartialView("_ResultadosCatalogo", vm) : View(vm);
    }
}
