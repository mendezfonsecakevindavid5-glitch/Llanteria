namespace Llanteria.Models;

/// <summary>Datos de la página de un artículo: secciones numeradas, tabla de contenido y quién puede editar.</summary>
public class BlogDetalleVM
{
    public Articulo Articulo { get; set; } = null!;
    public string Intro { get; set; } = "";
    public List<BlogCatalogo.Seccion> Secciones { get; set; } = new();
    /// <summary>Secciones de la tabla de contenido, en el orden elegido por el autor.</summary>
    public List<BlogCatalogo.Seccion> Tabla { get; set; } = new();
    public bool PuedeEditar { get; set; }
    public bool EsAutor { get; set; }
    public string AutorUsuario { get; set; } = "";
    public List<BlogPost> Relacionados { get; set; } = new();
}

public class SeccionFormVM
{
    public string Slug { get; set; } = "";
    public string TituloArticulo { get; set; } = "";
    /// <summary>0 = introducción (sin título); -1 = sección nueva.</summary>
    public int Numero { get; set; }
    public string? Titulo { get; set; }
    public string Cuerpo { get; set; } = "";
}
