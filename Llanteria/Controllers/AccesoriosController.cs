using Llanteria.Data;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Catálogo público de accesorios.</summary>
public class AccesoriosController : CatalogoControllerBase
{
    public AccesoriosController(LlanteriaDbContext db) : base(db) { }

    protected override string Categoria => "Accesorios";
    protected override string ModoExtra => "Accesorios";

    [HttpGet]
    public IActionResult Index(string[]? marca, int? min, int? max, string? orden, int pagina = 1, string? tipo = null, string? qmarca = null,
        string? tipoAccesorio = null, string? ubicacion = null, int? evento = null, int? promocion = null, bool promociones = false)
    {
        var vm = Armar(null, null, null, marca, min, max, orden, pagina, tipo, qmarca, evento: evento, promocion: promocion,
            tipoAccesorio: tipoAccesorio, ubicacion: ubicacion, promociones: promociones);
        return EsAjax() ? PartialView("_ResultadosCatalogo", vm) : View(vm);
    }
}
