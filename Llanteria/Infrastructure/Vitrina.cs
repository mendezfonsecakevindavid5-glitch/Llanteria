using System.Globalization;
using System.Text;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>Arma la franja de marcas del inicio a partir de los productos cargados.</summary>
public static class Vitrina
{
    private static readonly string[] Extensiones = { ".svg", ".png", ".webp", ".jpg", ".jpeg" };

    public static List<MarcaVitrina> Marcas(LlanteriaDbContext db, string webRoot)
    {
        var detalles = db.DetalleProductos.AsNoTracking()
            .Include(d => d.IdMarcaNavigation)
            .Include(d => d.IdProductoNavigation)
            .ToList();

        var resultado = new List<MarcaVitrina>();
        foreach (var g in detalles.Where(d => d.IdMarcaNavigation != null).GroupBy(d => d.IdMarcaNavigation!.Nombre).OrderBy(g => g.Key))
        {
            var llantas = g.Any(d => d.IdProductoNavigation?.Categoria == "Llantas" || !string.IsNullOrEmpty(d.Ancho));
            var baterias = g.Any(d => d.IdProductoNavigation?.Categoria == "Baterías");
            if (!llantas && !baterias) continue; // marcas de aceites/accesorios no tienen catálogo propio
            resultado.Add(new MarcaVitrina
            {
                Nombre = g.Key,
                Logo = BuscarLogo(webRoot, g.Key),
                Catalogo = llantas ? "Llantas" : "Baterias"
            });
        }
        return resultado;
    }

    /// <summary>Busca wwwroot/images/marcas/{nombre-en-minusculas}.(svg|png|webp|jpg).</summary>
    public static string? BuscarLogo(string webRoot, string nombre)
    {
        var slug = Slug(nombre);
        foreach (var ext in Extensiones)
            if (File.Exists(System.IO.Path.Combine(webRoot, "images", "marcas", slug + ext)))
                return $"/images/marcas/{slug}{ext}";
        return null;
    }

    public static string Slug(string s)
    {
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        return string.Join('-', sb.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
