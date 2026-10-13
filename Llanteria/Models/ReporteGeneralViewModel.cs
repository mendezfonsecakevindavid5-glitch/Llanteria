namespace Llanteria.Models;

public class ReporteGeneralViewModel
{
    public string Titulo { get; set; } = "Reporte";
    public string? Periodo { get; set; }
    public IEnumerable<Factura> Facturas { get; set; } = new List<Factura>();
    public IEnumerable<Gasto> Gastos { get; set; } = new List<Gasto>();
}
