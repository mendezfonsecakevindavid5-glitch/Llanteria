using System.Text;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>robots.txt y sitemap.xml para que Google encuentre las páginas públicas.</summary>
public class SeoController : Controller
{
    private readonly LlanteriaDbContext _db;

    public SeoController(LlanteriaDbContext db) => _db = db;

    [HttpGet("robots.txt")]
    public ContentResult Robots()
    {
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        foreach (var ruta in new[] { "Dashboard", "Factura", "Cliente", "Vehiculo", "Inventario", "Producto", "Marca", "Bodega", "Proveedore", "Gasto", "Empleado", "Usuario", "Log", "Reporte", "Account", "MiCuenta", "Citas", "User", "Pqrs", "OrdenTrabajo", "Pedidos", "Articulo", "ServicioWeb", "Editores", "Sugerencias" })
            sb.AppendLine($"Disallow: /{ruta}");
        sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("sitemap.xml")]
    public ContentResult Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var rutas = new List<string> { "/", "/Llantas", "/Baterias", "/Promociones", "/Empresas", "/Blog", "/Contacto",
            "/Legal/Terminos", "/Legal/Privacidad", "/Legal/Garantia", "/Legal/Devoluciones", "/Legal/Faq", "/Legal/Pqrs" };
        rutas.AddRange(_db.ServiciosWeb.AsNoTracking().Where(s => s.Publicado).Select(s => $"/Servicios/{s.Slug}").ToList());
        rutas.AddRange(_db.Articulos.AsNoTracking().Where(a => a.Publicado).Select(a => $"/Blog/{a.Slug}").ToList());

        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
        foreach (var r in rutas) sb.AppendLine($"  <url><loc>{baseUrl}{r}</loc></url>");
        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
