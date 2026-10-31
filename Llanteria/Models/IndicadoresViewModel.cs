namespace Llanteria.Models;

public class IndicadoresViewModel
{
    public int TotalClientes { get; set; }
    public int TotalProductos { get; set; }
    public int VehiculosAtendidosAnio { get; set; }
    public int ServiciosRealizadosAnio { get; set; }
    public int TotalResenias { get; set; }
    public double PromedioResenias { get; set; }

    public List<IndicadorMes> VehiculosPorMes { get; set; } = new();
}

public record IndicadorMes(string Etiqueta, int Cantidad);
