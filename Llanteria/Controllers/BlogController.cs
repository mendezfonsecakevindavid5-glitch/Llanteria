using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Blog público: guías sobre llantas, aceites, baterías y seguridad vial. Los artículos se administran desde el panel.</summary>
public class BlogController : Controller
{
    private readonly LlanteriaDbContext _db;

    public BlogController(LlanteriaDbContext db) => _db = db;

    [HttpGet("Blog")]
    public IActionResult Index(string? categoria, string? orden)
    {
        var consulta = _db.Articulos.AsNoTracking().Where(a => a.Publicado);

        if (!string.IsNullOrEmpty(categoria) && BlogCatalogo.Categorias.Contains(categoria))
            consulta = consulta.Where(a => a.Categoria == categoria);
        else
            categoria = null;

        var antiguos = orden == "antiguos";
        consulta = antiguos ? consulta.OrderBy(a => a.Fecha) : consulta.OrderByDescending(a => a.Fecha);

        ViewBag.Categoria = categoria;
        ViewBag.Orden = antiguos ? "antiguos" : "recientes";
        return View(consulta.ToList().Select(a => a.ComoPost()).ToList());
    }

    [HttpGet("Blog/{slug}")]
    public IActionResult Detalle(string slug)
    {
        var articulo = _db.Articulos.AsNoTracking().FirstOrDefault(a => a.Slug == slug);
        // Un borrador solo lo puede ver el administrador (vista previa).
        if (articulo == null || (!articulo.Publicado && !User.IsInRole("Administrador"))) return NotFound();

        ViewBag.Relacionados = Relacionados(articulo);
        return View(articulo.ComoPost());
    }

    private List<BlogPost> Relacionados(Articulo actual, int cantidad = 3)
        => _db.Articulos.AsNoTracking()
              .Where(a => a.Publicado && a.Slug != actual.Slug)
              .OrderByDescending(a => a.Categoria == actual.Categoria).ThenByDescending(a => a.Fecha)
              .Take(cantidad).ToList().Select(a => a.ComoPost()).ToList();
}
