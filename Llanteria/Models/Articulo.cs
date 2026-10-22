using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

/// <summary>Artículo del blog. Se escribe y se edita desde el panel (Contenido → Blog).</summary>
public partial class Articulo
{
    public const string ImagenPorDefecto = "sin-portada.svg";

    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200)]
    public string Titulo { get; set; } = null!;

    /// <summary>Dirección del artículo (/Blog/mi-articulo). Si se deja vacía se genera a partir del título.</summary>
    [StringLength(120)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Usa solo minúsculas, números y guiones.")]
    public string Slug { get; set; } = "";

    [Required(ErrorMessage = "El resumen es obligatorio.")]
    [StringLength(400)]
    public string Resumen { get; set; } = null!;

    /// <summary>Opcional: sirve para filtrar el blog, pero un artículo puede no tener categoría.</summary>
    [StringLength(40)]
    public string? Categoria { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(100)]
    public string Autor { get; set; } = BlogCatalogo.AutorPorDefecto;

    /// <summary>Nombre del archivo de la portada dentro de wwwroot/images/blog.</summary>
    [StringLength(120)]
    public string? Imagen { get; set; }

    [Required(ErrorMessage = "Escribe el contenido del artículo.")]
    public string Contenido { get; set; } = null!;

    /// <summary>Fuentes citadas, una por línea: "Texto a mostrar | https://enlace". Opcional.</summary>
    public string? Referencias { get; set; }

    /// <summary>Los artículos sin publicar solo los ve el administrador.</summary>
    public bool Publicado { get; set; } = true;

    /// <summary>Usuario que creó el artículo (el autor). Es el único que puede añadir secciones, y solo con el artículo sin publicar.</summary>
    public int? IdAutor { get; set; }

    /// <summary>Imagen de la tarjeta del blog. Si no se sube, se usa la del banner (Imagen).</summary>
    [StringLength(120)]
    public string? ImagenPresentacion { get; set; }

    /// <summary>Números de sección que aparecen en la tabla de contenido, en orden ("1, 2, 3"). Vacío = todas las secciones.</summary>
    [StringLength(200)]
    public string? TablaContenido { get; set; }

    [NotMapped]
    public IFormFile? ImagenArchivo { get; set; }

    [NotMapped]
    public IFormFile? ImagenPresentacionArchivo { get; set; }

    /// <summary>Imagen que se ve en la tarjeta: la de presentación o, si no hay, la del banner.</summary>
    [NotMapped]
    public string ImagenTarjeta => !string.IsNullOrEmpty(ImagenPresentacion) ? ImagenPresentacion : string.IsNullOrEmpty(Imagen) ? ImagenPorDefecto : Imagen;

    public BlogPost ComoPost()
        => new(Slug, Titulo, Resumen, Categoria ?? "", DateOnly.FromDateTime(Fecha), Autor,
               string.IsNullOrEmpty(Imagen) ? ImagenPorDefecto : Imagen, Contenido, Referencias);
}
