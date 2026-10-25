namespace Llanteria.Models;

public class PromocionesViewModel
{
    public List<Oferta> Ofertas { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
    public HashSet<int> Agotados { get; set; } = new();

    /// <summary>Eventos cuyo día es hoy (visibles de 12:00 a. m. a 11:59 p. m.).</summary>
    public List<Evento> EventosHoy { get; set; } = new();
}
