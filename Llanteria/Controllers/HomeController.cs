using System.Diagnostics;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class HomeController : Controller
{
    private static readonly string[] ExtensionesLogo = { ".svg", ".png", ".webp", ".jpg", ".jpeg", ".avif" };

    private readonly LlanteriaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public HomeController(LlanteriaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index()
    {
        // Buscador por medida: el formulario envía a /Llantas con estos mismos valores.
        var detalles = _context.DetalleProductos.AsNoTracking();
        ViewBag.Anchos = detalles.Where(d => d.Ancho != null && d.Ancho != "").Select(d => d.Ancho!).Distinct().OrderBy(a => a).ToList();
        ViewBag.Perfiles = detalles.Where(d => d.Perfil != null && d.Perfil != "").Select(d => d.Perfil!).Distinct().OrderBy(p => p).ToList();
        ViewBag.Diametros = detalles.Where(d => d.Diametro != null && d.Diametro != "").Select(d => d.Diametro!).Distinct().OrderBy(d => d).ToList();

        ViewBag.Logos = Logos();

        // Opiniones
        var visibles = _context.Resenias.AsNoTracking().Where(r => r.Visible);
        var total = visibles.Count();
        ViewBag.TotalResenias = total;
        ViewBag.PromedioResenias = total > 0 ? visibles.Average(r => (double)r.Calificacion) : 0d;
        ViewBag.Resenias = visibles.OrderByDescending(r => r.Fecha).Take(6).ToList();

        var distribucion = new int[5]; // posición 0 = 1 estrella ... posición 4 = 5 estrellas
        foreach (var g in visibles.GroupBy(r => r.Calificacion).Select(g => new { Estrellas = g.Key, N = g.Count() }).ToList())
            if (g.Estrellas is >= 1 and <= 5) distribucion[g.Estrellas - 1] = g.N;
        ViewBag.Distribucion = distribucion;

        Resenia? mia = null;
        if (User.IsInRole("Cliente") && int.TryParse(User.FindFirst("ClienteId")?.Value, out var idCliente))
            mia = _context.Resenias.AsNoTracking().FirstOrDefault(r => r.IdCliente == idCliente);
        ViewBag.MiResenia = mia;

        return View();
    }

    /// <summary>
    /// Logos de la franja de marcas: cualquier imagen que pongas en wwwroot/images/marcas aparece sola.
    /// Si el nombre del archivo coincide con una marca del catálogo (p. ej. "cooper-tires.png"), el logo enlaza a sus productos.
    /// Si la carpeta está vacía, se muestran los nombres de las marcas cargadas.
    /// </summary>
    private List<MarcaVitrina> Logos()
    {
        var conProductos = Vitrina.Marcas(_context, _env.WebRootPath)
            .GroupBy(m => Vitrina.Slug(m.Nombre)).ToDictionary(g => g.Key, g => g.First());

        var carpeta = Path.Combine(_env.WebRootPath, "images", "marcas");
        var resultado = new List<MarcaVitrina>();
        if (Directory.Exists(carpeta))
        {
            foreach (var archivo in Directory.GetFiles(carpeta)
                         .Where(f => ExtensionesLogo.Contains(Path.GetExtension(f).ToLowerInvariant()))
                         .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var nombre = Path.GetFileNameWithoutExtension(archivo);
                conProductos.TryGetValue(Vitrina.Slug(nombre), out var marca);
                resultado.Add(new MarcaVitrina
                {
                    Nombre = marca?.Nombre ?? nombre.Replace('-', ' ').Replace('_', ' '),
                    Logo = "/images/marcas/" + Path.GetFileName(archivo),
                    Catalogo = marca?.Catalogo ?? ""
                });
            }
        }
        return resultado.Count > 0 ? resultado : conProductos.Values.ToList();
    }

    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
