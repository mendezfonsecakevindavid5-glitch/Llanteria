using Llanteria.Infrastructure;

namespace Llanteria.Models;

/// <summary>Datos de una página de catálogo público (llantas o baterías): resultados, filtros disponibles y filtros elegidos.</summary>
public class CatalogoViewModel
{
    public Paginado<Producto> Resultados { get; set; } = null!;
    public HashSet<int> Agotados { get; set; } = new();

    // Filtros elegidos
    public string? Ancho { get; set; }
    public string? Perfil { get; set; }
    public string? Diametro { get; set; }
    public string? Tipo { get; set; }
    public string[] Marcas { get; set; } = Array.Empty<string>();
    public int? PrecioMin { get; set; }
    public int? PrecioMax { get; set; }
    public string Orden { get; set; } = "precio-asc";

    // Opciones disponibles
    public List<string> AnchosDisponibles { get; set; } = new();
    public List<string> PerfilesDisponibles { get; set; } = new();
    public List<string> DiametrosDisponibles { get; set; } = new();
    public List<(string Nombre, int Cantidad)> MarcasDisponibles { get; set; } = new();
    public int PrecioMasBajo { get; set; }
    public int PrecioMasAlto { get; set; }

    public bool HayFiltros => !string.IsNullOrEmpty(Ancho) || !string.IsNullOrEmpty(Perfil) || !string.IsNullOrEmpty(Diametro) || !string.IsNullOrEmpty(Tipo)
                              || Marcas.Length > 0 || PrecioMin.HasValue || PrecioMax.HasValue;
}
