using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

/// <summary>Evento del calendario (día festivo o conmemorativo, temporada...). Una promoción (Oferta) puede deberse a un evento.</summary>
public partial class Evento : IConEstado, IDeLlanteria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del evento es obligatorio."), StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.Today;
    public DateTime FechaFin { get; set; } = DateTime.Today;

    /// <summary>True si se repite cada año (Navidad, Día de la Madre...).</summary>
    public bool Recurrente { get; set; }

    public int IdEstado { get; set; } = EstadoIds.Habilitado;
    public int? IdLlanteria { get; set; }

    public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
}

// Parte nueva de Oferta (promoción): puede deberse a un evento.
public partial class Oferta
{
    public int? IdEvento { get; set; }
    public virtual Evento? EventoNavigation { get; set; }
}

// Parte nueva de Producto: la promoción que lo cubre (sirve para los filtros de promoción y de evento del catálogo).
public partial class Producto
{
    public int? IdOferta { get; set; }
    public virtual Oferta? OfertaNavigation { get; set; }
}
