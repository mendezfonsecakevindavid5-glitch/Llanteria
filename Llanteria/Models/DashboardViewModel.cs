namespace Llanteria.Models;

public class DashboardViewModel
{
    public decimal VentasMes { get; set; }
    public decimal GastosMes { get; set; }
    public int FacturasMes { get; set; }
    public int TotalClientes { get; set; }
    public int TotalProductos { get; set; }
    public int GastosPorVencer { get; set; }
    public decimal GastosPendientes { get; set; }

    public List<MesSerie> Meses { get; set; } = new();
    public List<Factura> UltimasFacturas { get; set; } = new();
    public List<Inventario> StockBajo { get; set; } = new();
}

public record MesSerie(string Etiqueta, decimal Ventas, decimal Gastos);
