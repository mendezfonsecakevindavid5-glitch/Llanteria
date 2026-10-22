namespace Llanteria.Models;

/// <summary>Calificación (1 a 5) que un cliente registrado le da al sitio. Un cliente tiene una sola; si vuelve a opinar, se actualiza.</summary>
public partial class Resenia
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = null!;
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = null!;
    public bool Visible { get; set; } = true;

    /// <summary>Usuario (cuenta) que escribió la reseña y su nombre de usuario: es lo que se muestra en la tarjeta, junto con su foto de perfil.</summary>
    public int? IdUsuario { get; set; }
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}
