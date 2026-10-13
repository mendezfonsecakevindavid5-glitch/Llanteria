namespace Llanteria.Models;

/// <summary>Encabezado estándar de las páginas del panel.</summary>
public record PageHeader(
    string Titulo,
    string Subtitulo,
    string VolverController = "Dashboard",
    string VolverAction = "Index",
    string? BotonTexto = null,
    string? BotonAction = null,
    string? BotonController = null);

public record Ventaja(string Icono, string Titulo, string Texto);

public record DatoInfo(string Etiqueta, string Texto);

/// <summary>Datos de una página de servicio (reemplaza las 6 views casi idénticas).</summary>
public class ServicioViewModel
{
    public string Slug { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Subtitulo { get; set; } = "";
    public string Icono { get; set; } = "fa-gears";
    public string Imagen { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<Ventaja> Ventajas { get; set; } = new();
    public List<DatoInfo> InfoCita { get; set; } = new();
    public string TituloVisitante { get; set; } = "";
    public string TextoVisitante { get; set; } = "";
    public string EtiquetaPrecio { get; set; } = "Total estimado:";
    public string Precio { get; set; } = "";
    public string PlaceholderVehiculo { get; set; } = "Ej. ABC-123 / Renault Logan";
    public string PlaceholderMensaje { get; set; } = "";
    public string PlaceholderNotas { get; set; } = "";
    public bool EsDomicilio { get; set; }
    public string[] Horarios { get; set; } = { "08:00", "09:30", "11:00", "14:00", "15:30", "17:00" };
}
