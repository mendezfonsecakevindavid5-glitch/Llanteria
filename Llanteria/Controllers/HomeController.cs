using System.Diagnostics;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class HomeController : Controller
{
    private readonly LlanteriaDbContext _context;

    public HomeController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index()
    {
        var detalles = _context.DetalleProductos.AsNoTracking();
        ViewBag.Anchos = detalles.Where(d => d.Ancho != null && d.Ancho != "").Select(d => d.Ancho!).Distinct().OrderBy(a => a).ToList();
        ViewBag.Perfiles = detalles.Where(d => d.Perfil != null && d.Perfil != "").Select(d => d.Perfil!).Distinct().OrderBy(p => p).ToList();
        ViewBag.Diametros = detalles.Where(d => d.Diametro != null && d.Diametro != "").Select(d => d.Diametro!).Distinct().OrderBy(d => d).ToList();

        var productos = _context.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto)
            .OrderBy(p => p.Nombre)
            .ToList();
        return View(productos);
    }

    // Búsqueda por medida desde el buscador de la página de inicio (público).
    [HttpGet]
    public IActionResult BuscarLlantas(string? ancho, string? perfil, string? diametro)
    {
        var consulta = _context.Productos.AsNoTracking().Where(p => p.DetalleProducto != null);
        if (!string.IsNullOrEmpty(ancho)) consulta = consulta.Where(p => p.DetalleProducto!.Ancho == ancho);
        if (!string.IsNullOrEmpty(perfil)) consulta = consulta.Where(p => p.DetalleProducto!.Perfil == perfil);
        if (!string.IsNullOrEmpty(diametro)) consulta = consulta.Where(p => p.DetalleProducto!.Diametro == diametro);

        var resultados = consulta.OrderBy(p => p.PrecioVenta).Take(24).AsEnumerable()
            .Select(p => new
            {
                nombre = p.Nombre,
                precio = p.PrecioVenta.ToString("C0"),
                img = Url.Content("~/images/productoos/" + (string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen))
            }).ToList();
        return Json(resultados);
    }

    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
