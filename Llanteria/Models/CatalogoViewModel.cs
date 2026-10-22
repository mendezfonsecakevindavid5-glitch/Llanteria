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
    public string? BuscarMarca { get; set; }
    public int? PrecioMin { get; set; }
    public int? PrecioMax { get; set; }

    // Filtros de evento y de promoción (reemplazan al filtro de marca del panel; la barra de búsqueda sigue filtrando por marca).
    public int? Evento { get; set; }
    public int? Promocion { get; set; }
    /// <summary>"nombre" (orden alfabético, por defecto), "precio-asc" o "precio-desc". Los chips de precio arrancan sin marcar.</summary>
    public string Orden { get; set; } = "nombre";

    // Filtros extra de Lubricantes
    public string? Viscosidad { get; set; }
    public string? TipoAceite { get; set; }

    // Filtros extra de Baterías
    public string? Amperaje { get; set; }
    public string? Polaridad { get; set; }

    // Filtro extra compartido (Lubricantes y Baterías)
    public int? Garantia { get; set; }

    /// <summary>Qué bloque de filtros adicionales mostrar en el panel izquierdo: "Llantas", "Lubricantes", "Baterias" o "".</summary>
    public string ModoExtra { get; set; } = "";

    // Opciones disponibles
    public List<string> AnchosDisponibles { get; set; } = new();
    public List<string> PerfilesDisponibles { get; set; } = new();
    public List<string> DiametrosDisponibles { get; set; } = new();
    public List<(string Nombre, int Cantidad)> MarcasDisponibles { get; set; } = new();
    public List<(int Id, string Nombre, int Cantidad)> EventosDisponibles { get; set; } = new();
    public List<(int Id, string Nombre, int Cantidad)> PromocionesDisponibles { get; set; } = new();
    public List<string> ViscosidadesDisponibles { get; set; } = new();
    public List<string> TiposAceiteDisponibles { get; set; } = new();
    public List<string> AmperajesDisponibles { get; set; } = new();
    public List<string> PolaridadesDisponibles { get; set; } = new();
    public List<int> GarantiasDisponibles { get; set; } = new();
    public int PrecioMasBajo { get; set; }
    public int PrecioMasAlto { get; set; }

    public bool HayFiltros => !string.IsNullOrEmpty(Ancho) || !string.IsNullOrEmpty(Perfil) || !string.IsNullOrEmpty(Diametro) || !string.IsNullOrEmpty(Tipo)
                              || Marcas.Length > 0 || Evento.HasValue || Promocion.HasValue || !string.IsNullOrEmpty(BuscarMarca) || PrecioMin.HasValue || PrecioMax.HasValue
                              || !string.IsNullOrEmpty(Viscosidad) || !string.IsNullOrEmpty(TipoAceite)
                              || !string.IsNullOrEmpty(Amperaje) || !string.IsNullOrEmpty(Polaridad) || Garantia.HasValue;
}
