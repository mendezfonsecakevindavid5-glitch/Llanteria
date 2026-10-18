using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

/// <summary>
/// Servicio que se muestra en el sitio público (tarjeta del inicio y página de detalle). Lo administra el administrador desde
/// el panel. No es el mismo concepto que <see cref="TipoServicio"/>, que es la tarifa que se factura.
/// Las ventajas y los datos de la cita se guardan como texto, una por línea, con las partes separadas por «|».
/// </summary>
public class ServicioWeb
{
    public const string ImagenPorDefecto = "default-servicio.png";
    private const char Separador = '|';

    public int Id { get; set; }

    [StringLength(120)] public string Slug { get; set; } = "";

    [Required(ErrorMessage = "Escribe el nombre del servicio."), StringLength(100)]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Escribe una frase corta que describa el servicio."), StringLength(160)]
    public string Subtitulo { get; set; } = "";

    [Required(ErrorMessage = "Elige un ícono."), StringLength(40)]
    public string Icono { get; set; } = "fa-gears";

    [StringLength(120)] public string? Imagen { get; set; }

    [Required(ErrorMessage = "Explica en qué consiste el servicio."), StringLength(1200)]
    public string Descripcion { get; set; } = "";

    public string? Ventajas { get; set; }
    public string? InfoCita { get; set; }

    [StringLength(100)] public string? TituloVisitante { get; set; }
    [StringLength(500)] public string? TextoVisitante { get; set; }
    [Required(ErrorMessage = "Escribe la etiqueta del precio (por ejemplo «Total estimado:»)."), StringLength(40)]
    public string EtiquetaPrecio { get; set; } = "Total estimado:";

    [Required(ErrorMessage = "Escribe el precio como se mostrará (por ejemplo $40.000 COP)."), StringLength(40)]
    public string Precio { get; set; } = "";

    [StringLength(100)] public string? PlaceholderVehiculo { get; set; }
    [StringLength(150)] public string? PlaceholderNotas { get; set; }
    [StringLength(150)] public string? PlaceholderMensaje { get; set; }

    public bool EsDomicilio { get; set; }
    public bool Publicado { get; set; } = true;
    public int Orden { get; set; }

    [NotMapped] public IFormFile? ImagenArchivo { get; set; }

    public ServicioViewModel ComoVista() => new()
    {
        Slug = Slug,
        Titulo = Titulo,
        Subtitulo = Subtitulo,
        Icono = Icono,
        Imagen = Imagen ?? "",
        Descripcion = Descripcion,
        Ventajas = Partir(Ventajas ?? "", 3).Select(p => new Ventaja(p[0], p[1], p[2])).ToList(),
        InfoCita = Partir(InfoCita ?? "", 2).Select(p => new DatoInfo(p[0], p[1])).ToList(),
        TituloVisitante = TituloVisitante ?? "",
        TextoVisitante = TextoVisitante ?? "",
        EtiquetaPrecio = EtiquetaPrecio,
        Precio = Precio,
        PlaceholderVehiculo = PlaceholderVehiculo ?? "",
        PlaceholderNotas = PlaceholderNotas ?? "",
        PlaceholderMensaje = PlaceholderMensaje ?? "",
        EsDomicilio = EsDomicilio
    };

    public static ServicioWeb DesdeVista(ServicioViewModel v, int orden) => new()
    {
        Slug = v.Slug,
        Titulo = v.Titulo,
        Subtitulo = v.Subtitulo,
        Icono = v.Icono,
        Descripcion = v.Descripcion,
        Ventajas = string.Join('\n', v.Ventajas.Select(x => Unir(x.Icono, x.Titulo, x.Texto))),
        InfoCita = string.Join('\n', v.InfoCita.Select(x => Unir(x.Etiqueta, x.Texto))),
        TituloVisitante = v.TituloVisitante,
        TextoVisitante = v.TextoVisitante,
        EtiquetaPrecio = v.EtiquetaPrecio,
        Precio = v.Precio,
        PlaceholderVehiculo = v.PlaceholderVehiculo,
        PlaceholderNotas = v.PlaceholderNotas,
        PlaceholderMensaje = v.PlaceholderMensaje,
        EsDomicilio = v.EsDomicilio,
        Orden = orden
    };

    /// <summary>Devuelve null si todas las líneas son válidas; si no, el texto del primer problema.</summary>
    public static string? ValidarLineas(string? texto, int partes, string nombre)
    {
        var lineas = (texto ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < lineas.Length; i++)
            if (lineas[i].Split(Separador).Count(p => p.Trim().Length > 0) != partes || lineas[i].Count(c => c == Separador) != partes - 1)
                return $"{nombre}: la línea {i + 1} debe tener {partes} partes separadas por «|».";
        return null;
    }

    private static string Unir(params string[] partes) => string.Join($" {Separador} ", partes);

    private static IEnumerable<string[]> Partir(string texto, int partes)
        => texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => l.Split(Separador, partes, StringSplitOptions.TrimEntries))
            .Where(p => p.Length == partes);
}
