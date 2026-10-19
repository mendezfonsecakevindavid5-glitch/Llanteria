using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

/// <summary>
/// Permiso que un administrador le da a otro usuario para crear y editar un tipo de contenido del sitio
/// (el blog o los servicios). El administrador siempre tiene acceso y no necesita una fila aquí.
/// </summary>
public class EditorContenido
{
    public const string Blog = "Blog";
    public const string Servicios = "Servicios";
    public static readonly string[] Recursos = { Blog, Servicios };

    public int Id { get; set; }

    public int IdUsuario { get; set; }

    [StringLength(20)]
    public string Recurso { get; set; } = Blog;

    /// <summary>Administrador que envió la invitación.</summary>
    public int IdInvitadoPor { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
