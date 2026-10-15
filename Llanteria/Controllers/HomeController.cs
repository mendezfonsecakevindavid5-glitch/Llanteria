using System.Diagnostics;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class HomeController : Controller
{
    private readonly LlanteriaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public HomeController(LlanteriaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index()
    {
        var detalles = _context.DetalleProductos.AsNoTracking();
        ViewBag.Anchos = detalles.Where(d => d.Ancho != null && d.Ancho != "").Select(d => d.Ancho!).Distinct().OrderBy(a => a).ToList();
        ViewBag.Perfiles = detalles.Where(d => d.Perfil != null && d.Perfil != "").Select(d => d.Perfil!).Distinct().OrderBy(p => p).ToList();
        ViewBag.Diametros = detalles.Where(d => d.Diametro != null && d.Diametro != "").Select(d => d.Diametro!).Distinct().OrderBy(d => d).ToList();

        ViewBag.Marcas = Vitrina.Marcas(_context, _env.WebRootPath);

        var visibles = _context.Resenias.AsNoTracking().Where(r => r.Visible);
        ViewBag.TotalResenias = visibles.Count();
        ViewBag.PromedioResenias = ViewBag.TotalResenias > 0 ? visibles.Average(r => (double)r.Calificacion) : 0d;
        ViewBag.Resenias = visibles.OrderByDescending(r => r.Fecha).Take(6).ToList();

        // Se obtienen los productos para pasarlos como modelo a la vista
        var productos = _context.Productos
            .Include(p => p.DetalleProducto)
            .AsNoTracking()
            .ToList();

        return View(productos);
    }

    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}