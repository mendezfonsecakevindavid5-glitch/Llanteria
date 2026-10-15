using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

/// <summary>Blog público con artículos largos sobre llantas, aceites, baterías y seguridad vial.</summary>
public class BlogController : Controller
{
    [HttpGet("Blog")]
    public IActionResult Index(string? categoria, string? orden)
    {
        IEnumerable<BlogPost> lista = BlogCatalogo.Todos;
        if (!string.IsNullOrEmpty(categoria) && BlogCatalogo.Categorias.Contains(categoria))
            lista = lista.Where(p => p.Categoria == categoria);
        else
            categoria = null;

        var antiguos = orden == "antiguos";
        lista = antiguos ? lista.OrderBy(p => p.Fecha) : lista.OrderByDescending(p => p.Fecha);

        ViewBag.Categoria = categoria;
        ViewBag.Orden = antiguos ? "antiguos" : "recientes";
        return View(lista.ToList());
    }

    [HttpGet("Blog/{slug}")]
    public IActionResult Detalle(string slug)
    {
        var post = BlogCatalogo.Buscar(slug);
        return post == null ? NotFound() : View(post);
    }
}
