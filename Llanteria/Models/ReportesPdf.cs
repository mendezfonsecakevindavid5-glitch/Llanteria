namespace Llanteria.Models;

/// <summary>Datos del encabezado común de todos los PDF.</summary>
public record PdfCabecera(string Titulo, string Codigo, string? Periodo, string? GeneradoPor, DateTime Generado);

public class FilaPeriodo
{
    public string Etiqueta { get; set; } = "";
    public int Facturas { get; set; }
    public decimal Ventas { get; set; }
    public decimal Gastos { get; set; }
    public decimal Resultado => Ventas - Gastos;
}

public class FilaCategoria
{
    public string Categoria { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
}

public class FilaTop
{
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>Reporte general de ingresos y egresos (estado de resultados + flujo).</summary>
public class ReporteFinancieroVM
{
    public PdfCabecera Cabecera { get; set; } = null!;
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public string AgrupadoPor { get; set; } = "";

    public List<Factura> Facturas { get; set; } = new();
    public List<Gasto> Gastos { get; set; } = new();

    // Estado de resultados
    public decimal Ventas { get; set; }            // subtotal facturado (sin IVA)
    public decimal CostoVentas { get; set; }       // cantidad vendida × precio de compra actual
    public decimal UtilidadBruta => Ventas - CostoVentas;
    public decimal Egresos { get; set; }
    public decimal Utilidad => UtilidadBruta - Egresos;
    public decimal MargenBruto => Ventas > 0 ? UtilidadBruta / Ventas : 0;
    public decimal MargenNeto => Ventas > 0 ? Utilidad / Ventas : 0;

    // Impuestos y recaudo
    public decimal IvaFacturado { get; set; }
    public decimal Retenciones { get; set; }
    public decimal TotalFacturado { get; set; }    // total a pagar de las facturas

    // Egresos por estado
    public decimal EgresosPagados { get; set; }
    public decimal EgresosPendientes { get; set; }
    public decimal EgresosVencidos { get; set; }

    public decimal FlujoNeto => TotalFacturado - EgresosPagados;
    public decimal TicketPromedio => Facturas.Count > 0 ? TotalFacturado / Facturas.Count : 0;

    public List<FilaCategoria> GastosPorCategoria { get; set; } = new();
    public List<FilaPeriodo> Evolucion { get; set; } = new();
    public List<FilaTop> TopClientes { get; set; } = new();
    public List<FilaTop> TopProductos { get; set; } = new();
}

public class ReporteVentasVM
{
    public PdfCabecera Cabecera { get; set; } = null!;
    public List<Factura> Facturas { get; set; } = new();
    public List<FilaTop> TopClientes { get; set; } = new();

    public decimal Subtotal => Facturas.Sum(f => f.TotalBruto);
    public decimal Iva => Facturas.Sum(f => f.Iva);
    public decimal Retenciones => Facturas.Sum(f => f.RteFte ?? 0);
    public decimal Total => Facturas.Sum(f => f.TotalPagar);
    public decimal TicketPromedio => Facturas.Count > 0 ? Total / Facturas.Count : 0;
}

public class ReporteGastosVM
{
    public PdfCabecera Cabecera { get; set; } = null!;
    public List<Gasto> Gastos { get; set; } = new();
    public List<FilaCategoria> PorCategoria { get; set; } = new();
    public DateOnly Hoy { get; set; }

    public decimal Total => Gastos.Sum(g => g.Monto);
    public decimal Pagado => Gastos.Where(g => g.Estado == "Pagado").Sum(g => g.Monto);
    public decimal Pendiente => Total - Pagado;
    public decimal Vencido => Gastos.Where(g => g.Estado != "Pagado" && g.FechaVencimiento < Hoy).Sum(g => g.Monto);
}

public class ReporteInventarioVM
{
    public PdfCabecera Cabecera { get; set; } = null!;
    public List<Inventario> Items { get; set; } = new();

    public static bool EsBajo(Inventario i) => i.StockMinimo.HasValue && (i.StockActual ?? 0) <= i.StockMinimo.Value && (i.StockActual ?? 0) > 0;
    public static bool EsAgotado(Inventario i) => (i.StockActual ?? 0) <= 0;
    public static decimal ValorCosto(Inventario i) => (i.StockActual ?? 0) * (i.IdProductoNavigation?.PrecioCompra ?? 0);
    public static decimal ValorVenta(Inventario i) => (i.StockActual ?? 0) * (i.IdProductoNavigation?.PrecioVenta ?? 0);

    public int Unidades => Items.Sum(i => i.StockActual ?? 0);
    public int Bajos => Items.Count(EsBajo);
    public int Agotados => Items.Count(EsAgotado);
    public decimal TotalCosto => Items.Sum(ValorCosto);
    public decimal TotalVenta => Items.Sum(ValorVenta);
}
