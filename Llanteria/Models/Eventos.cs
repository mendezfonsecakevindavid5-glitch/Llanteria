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

    /// <summary>
    /// Día en que ocurre el evento. El evento está disponible desde las 12:00 a. m. de ese día hasta las 11:59 p. m.;
    /// pasada esa hora el sistema lo oculta solo (no hay que deshabilitarlo a mano). Ver <see cref="EsHoy"/>.
    /// </summary>
    [Display(Name = "Fecha del evento")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    // FechaInicio y FechaFin se conservan por compatibilidad con bases anteriores; la visibilidad depende solo de Fecha.
    public DateTime FechaInicio { get; set; } = DateTime.Today;
    public DateTime FechaFin { get; set; } = DateTime.Today;

    /// <summary>True si se repite cada año (Navidad, Día de la Madre...): se muestra el mismo día y mes de cada año.</summary>
    public bool Recurrente { get; set; }

    /// <summary>
    /// ¿El evento está disponible en este momento? Sí durante todo el día de <see cref="Fecha"/> (00:00 a 23:59);
    /// los recurrentes además cada año en el mismo día y mes. Se calcula con el reloj en cada consulta, así que
    /// se activa a medianoche y se oculta a la medianoche siguiente sin ningún proceso aparte.
    /// </summary>
    public bool EsHoy(DateTime? ahora = null)
    {
        var hoy = (ahora ?? DateTime.Now).Date;
        if (Fecha.Date == hoy) return true;
        return Recurrente && Fecha.Month == hoy.Month && Fecha.Day == hoy.Day;
    }

    public int IdEstado { get; set; } = EstadoIds.Habilitado;
    public int? IdLlanteria { get; set; }

    public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
}

// Parte nueva de Oferta (promoción): puede deberse a un evento.
public partial class Oferta
{
    public int? IdEvento { get; set; }
    public virtual Evento? EventoNavigation { get; set; }

    /// <summary>
    /// ¿La promoción se puede mostrar ahora? Debe estar habilitada, dentro de sus fechas y, si se debe a un evento,
    /// ese evento tiene que estar disponible hoy (ver <see cref="Evento.EsHoy"/>). Requiere cargar EventoNavigation.
    /// </summary>
    public bool VigenteAhora(DateTime ahora)
    {
        if (Estado == false || FechaInicio > ahora || FechaFin < ahora) return false;
        if (!IdEvento.HasValue) return true;
        return EventoNavigation != null && EventoNavigation.EsHoy(ahora);
    }
}

// Parte nueva de Producto: la promoción que lo cubre (sirve para los filtros de promoción y de evento del catálogo).
public partial class Producto
{
    public int? IdOferta { get; set; }
    public virtual Oferta? OfertaNavigation { get; set; }
}
