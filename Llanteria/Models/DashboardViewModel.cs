namespace Llanteria.Models
{
    public class DashboardViewModel
    {
        // KPIs Financieros
        public decimal TotalVentasMes { get; set; }
        public decimal TotalGastosMes { get; set; }
        public int TotalClientes { get; set; }
        public int AlertasInventario { get; set; }

        // Datos para las tablas del dashboard
        public IEnumerable<Llanteria.Models.Factura> UltimasFacturas { get; set; }
        public IEnumerable<Llanteria.Models.LogActividad> UltimosLogs { get; set; }
    }
}