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
        // Buscador por tipo de vehículo: las medidas sugeridas salen de las llantas cargadas para cada tipo.
        var detalles = _context.DetalleProductos.AsNoTracking().ToList();
        List<string> Valores(string tipo, Func<DetalleProducto, string?> campo) => detalles
            .Where(d => TiposVehiculo.Normalizar(d.TipoVehiculo) == tipo)
            .Select(campo).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()).Distinct()
            .OrderBy(v => int.TryParse(v, out var n) ? n : int.MaxValue).ThenBy(v => v).ToList();
        ViewBag.Medidas = TiposVehiculo.Todos.ToDictionary(t => t, t => new
        {
            anchos = Valores(t, d => d.Ancho), perfiles = Valores(t, d => d.Perfil), diametros = Valores(t, d => d.Diametro)
        });

        // Tarjetas del inicio: llantas más recientes con su disponibilidad.
        var destacadas = _context.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
            .Where(p => p.Categoria == "Llantas" || (p.DetalleProducto != null && p.DetalleProducto.Ancho != null))
            .OrderByDescending(p => p.Id).Take(8).ToList();
        ViewBag.Destacadas = destacadas;
        ViewBag.Agotados = Agotados(destacadas.Select(p => p.Id).ToList());

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

    private HashSet<int> Agotados(List<int> ids)
    {
        var danado = StockHelper.IdEstado(_context, DatosIniciales.Estados.Danado);
        var inv = _context.Inventarios.AsNoTracking().Where(i => ids.Contains(i.IdProducto)).ToList()
            .GroupBy(i => i.IdProducto)
            .ToDictionary(g => g.Key, g => g.Where(i => i.IdEstado != danado).Sum(i => i.StockActual ?? 0));
        return inv.Where(kv => kv.Value <= 0).Select(kv => kv.Key).ToHashSet();
    }

    private static string Digitos(string? v) => new string((v ?? "").Where(char.IsDigit).ToArray());

    /// <summary>Búsqueda del inicio: tipo de vehículo + medida (ancho / perfil / rin). El modelo o marca solo ordena: primero las que coinciden.</summary>
    [HttpGet]
    public IActionResult BuscarLlantas(string? tipo, string? modelo, string? ancho, string? perfil, string? diametro)
    {
        tipo = TiposVehiculo.Normalizar(tipo);
        ancho = Digitos(ancho); perfil = Digitos(perfil); diametro = Digitos(diametro);
        if (ancho == "" || perfil == "" || diametro == "") return BadRequest();
        modelo = (modelo ?? "").Trim();

        var candidatos = _context.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
            .Where(p => p.DetalleProducto != null && p.DetalleProducto.Ancho == ancho && p.DetalleProducto.Perfil == perfil
                        && p.DetalleProducto.Diametro == diametro && p.DetalleProducto.TipoVehiculo == tipo)
            .ToList();

        bool Coincide(Producto p) => modelo.Length > 0 &&
            (p.Nombre.Contains(modelo, StringComparison.OrdinalIgnoreCase)
             || (p.DetalleProducto?.IdMarcaNavigation?.Nombre ?? "").Contains(modelo, StringComparison.OrdinalIgnoreCase));

        var agotados = Agotados(candidatos.Select(p => p.Id).ToList());
        var lista = candidatos.OrderByDescending(Coincide).ThenBy(p => p.PrecioActual).Select(p => new
        {
            id = p.Id,
            nombre = p.Nombre,
            marca = p.DetalleProducto?.IdMarcaNavigation?.Nombre,
            medida = $"{ancho}/{perfil} R{diametro}",
            img = "/images/productoos/" + (string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen),
            precio = p.PrecioActual.ToString("C0"),
            antes = p.EnPromocion ? p.PrecioVenta.ToString("C0") : null,
            agotado = agotados.Contains(p.Id),
            coincide = Coincide(p)
        });
        return Json(lista);
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
