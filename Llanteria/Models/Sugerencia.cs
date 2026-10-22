using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

/// <summary>Sugerencia para mejorar el sitio web, enviada desde la sección de opiniones del sitio.</summary>
public partial class Sugerencia
{
    public const string Nueva = "Nueva";
    public const string Revisada = "Revisada";
    public const string Aplicada = "Aplicada";
    public static readonly string[] Estados = { Nueva, Revisada, Aplicada };

    public static readonly string[] Areas = { "Diseño y apariencia", "Facilidad de uso", "Catálogo y precios", "Servicios y citas", "Compra y pagos", "Velocidad", "Otro" };

    public int Id { get; set; }

    [Required(ErrorMessage = "Elige el área de la sugerencia.")]
    [StringLength(40)]
    public string Area { get; set; } = null!;

    [Range(1, 5, ErrorMessage = "Califica el sitio de 1 a 5.")]
    public int Calificacion { get; set; } = 5;

    [Required(ErrorMessage = "Cuéntanos tu sugerencia.")]
    [StringLength(1000, MinimumLength = 15, ErrorMessage = "Escribe entre 15 y 1000 caracteres.")]
    public string Mensaje { get; set; } = null!;

    [StringLength(100)]
    public string? Nombre { get; set; }

    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [StringLength(100)]
    public string? Correo { get; set; }

    public int? IdUsuario { get; set; }

    public string Estado { get; set; } = Nueva;

    public DateTime Fecha { get; set; } = DateTime.Now;
}
