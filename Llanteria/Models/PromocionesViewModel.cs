namespace Llanteria.Models;

public class PromocionesViewModel
{
    public List<Oferta> Ofertas { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
    public HashSet<int> Agotados { get; set; } = new();
}
