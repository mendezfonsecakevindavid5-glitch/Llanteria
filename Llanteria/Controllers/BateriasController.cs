using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de baterías con filtro por marca y por precio (menor a mayor y al revés).</summary>
public class BateriasController : CatalogoControllerBase
{
    public BateriasController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Baterías";
    protected override string ModoExtra => "Baterias";

    [HttpGet]
    public IActionResult Index(string[]? marca, int? min, int? max, string? orden, int pagina = 1, string? tipo = null,
        string? qmarca = null, string? amperaje = null, string? polaridad = null, int? garantia = null)
    {
        var vm = Armar(null, null, null, marca, min, max, orden, pagina, tipo, qmarca, amperaje: amperaje, polaridad: polaridad, garantia: garantia);
        return EsAjax() ? PartialView("_ResultadosCatalogo", vm) : View(vm);
    }
}
