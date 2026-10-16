using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Llanteria.Controllers;

/// <summary>
/// Reportes en PDF (Rotativa) y Excel (ClosedXML) de ingresos, egresos, ventas, gastos e inventario.
/// Los PDF comparten encabezado, estilos y firmas: Views/Shared/_Pdf*.cshtml.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ReporteController : Controller
{
    private readonly LlanteriaDbContext _context;

    public ReporteController(LlanteriaDbContext context) => _context = context;

    // ======================================================================================================
    //  PDF
    // ======================================================================================================

    // Reporte general: estado de resultados, egresos por categoría, evolución, rankings y detalle.
    public IActionResult GenerarGeneral(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var vm = ConstruirFinanciero(inicio, fin);
        return Pdf("ReporteGeneral", vm, $"Reporte_General_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");
    }

    // tipo: Factura (ventas) | Gasto | Inventario
    public IActionResult GenerarIndividual(string tipo, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        switch (tipo)
        {
            case "Factura":
                var ventas = ConstruirVentas(inicio, fin);
                return Pdf("ReporteVentas", ventas, $"Reporte_Ventas_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");

            case "Gasto":
                var gastos = ConstruirGastos(inicio, fin);
                return Pdf("ReporteGastos", gastos, $"Reporte_Gastos_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");

            case "Inventario":
                var inv = ConstruirInventario();
                return Pdf("ReporteInventario", inv, $"Reporte_Inventario_{DateTime.Now:yyyyMMdd}.pdf");

            default:
                return RedirectToAction("Index", "Dashboard");
        }
    }

    // ======================================================================================================
    //  EXCEL
    // ======================================================================================================

    // Libro con varias hojas: Resumen (estado de resultados), Evolución, Gastos por categoría, Ventas, Gastos, Rankings.
    public IActionResult ExcelGeneral(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var vm = ConstruirFinanciero(inicio, fin);
        var periodo = vm.Cabecera.Periodo;

        var resumen = new ResumenHoja
        {
            Nombre = "Resumen",
            Titulo = "Reporte general de ingresos y egresos",
            Subtitulo = $"Periodo: {periodo}",
            Lineas =
            {
                new("INGRESOS", null, TipoCol.Moneda, EstiloLinea.Seccion),
                new("Ventas facturadas (base, sin IVA)", vm.Ventas),
                new("(-) Costo de la mercancía vendida (estimado)", -vm.CostoVentas),
                new("UTILIDAD BRUTA", vm.UtilidadBruta, TipoCol.Moneda, EstiloLinea.Subtotal),
                new("Margen bruto", vm.MargenBruto, TipoCol.Porcentaje),
                new("EGRESOS", null, TipoCol.Moneda, EstiloLinea.Seccion),
            }
        };
        foreach (var c in vm.GastosPorCategoria) resumen.Lineas.Add(new($"(-) {c.Categoria}", -c.Monto));
        resumen.Lineas.Add(new("TOTAL EGRESOS", -vm.Egresos, TipoCol.Moneda, EstiloLinea.Subtotal));
        resumen.Lineas.Add(new("UTILIDAD DEL PERIODO", vm.Utilidad, TipoCol.Moneda, EstiloLinea.Total));
        resumen.Lineas.Add(new("Margen neto", vm.MargenNeto, TipoCol.Porcentaje));
        resumen.Lineas.Add(new("IMPUESTOS Y RECAUDO (base facturación)", null, TipoCol.Moneda, EstiloLinea.Seccion));
        resumen.Lineas.Add(new("IVA facturado", vm.IvaFacturado));
        resumen.Lineas.Add(new("(-) Retención en la fuente practicada", -vm.Retenciones));
        resumen.Lineas.Add(new("Total facturado a cobrar", vm.TotalFacturado, TipoCol.Moneda, EstiloLinea.Subtotal));
        resumen.Lineas.Add(new("(-) Egresos pagados", -vm.EgresosPagados));
        resumen.Lineas.Add(new("FLUJO NETO DEL PERIODO", vm.FlujoNeto, TipoCol.Moneda, EstiloLinea.Total));
        resumen.Lineas.Add(new("Egresos pendientes de pago", vm.EgresosPendientes));
        resumen.Lineas.Add(new("   de los cuales vencidos", vm.EgresosVencidos));
        resumen.Lineas.Add(new("Facturas emitidas", vm.Facturas.Count, TipoCol.Entero));
        resumen.Lineas.Add(new("Ticket promedio", vm.TicketPromedio));
        resumen.Lineas.Add(new("El costo de ventas usa el precio de compra actual de cada producto; los servicios no tienen costo registrado. " +
                               "Los egresos se toman por fecha de registro. El flujo asume cobro total de lo facturado.", null, TipoCol.Texto, EstiloLinea.Nota));

        var evolucion = new TablaHoja
        {
            Nombre = "Evolución",
            Titulo = "Ingresos vs. egresos",
            Subtitulo = $"Periodo: {periodo} · agrupado {vm.AgrupadoPor}",
            Columnas = { new("Periodo"), new("Facturas", TipoCol.Entero, true), new("Ventas (sin IVA)", TipoCol.Moneda, true), new("Egresos", TipoCol.Moneda, true), new("Resultado", TipoCol.Moneda, true) },
            Filas = vm.Evolucion.Select(e => new object?[] { e.Etiqueta, e.Facturas, e.Ventas, e.Gastos, e.Resultado }).ToList()
        };

        var categorias = new TablaHoja
        {
            Nombre = "Egresos por categoría",
            Titulo = "Egresos por categoría",
            Subtitulo = $"Periodo: {periodo}",
            Columnas = { new("Categoría"), new("Gastos", TipoCol.Entero, true), new("Monto", TipoCol.Moneda, true), new("% del total", TipoCol.Porcentaje) },
            Filas = vm.GastosPorCategoria.Select(c => new object?[] { c.Categoria, c.Cantidad, c.Monto, c.Porcentaje }).ToList()
        };

        return this.ExcelArchivo($"reporte_general_{inicio:yyyyMMdd}_{fin:yyyyMMdd}",
            resumen, evolucion, categorias,
            HojaVentas(vm.Facturas, periodo), HojaGastos(vm.Gastos, periodo),
            HojaTop("Top clientes", "Mejores clientes", vm.TopClientes, "Cliente", "Facturas", periodo),
            HojaTop("Top productos", "Productos y servicios más vendidos", vm.TopProductos, "Producto / servicio", "Unidades", periodo));
    }

    public IActionResult ExcelIndividual(string tipo, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var periodo = $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";
        switch (tipo)
        {
            case "Factura":
                return this.ExcelArchivo($"ventas_{inicio:yyyyMMdd}_{fin:yyyyMMdd}", HojaVentas(ConsultarFacturas(inicio, fin), periodo));
            case "Gasto":
                return this.ExcelArchivo($"gastos_{inicio:yyyyMMdd}_{fin:yyyyMMdd}", HojaGastos(ConsultarGastos(inicio, fin), periodo));
            case "Inventario":
                return this.ExcelArchivo("inventario", HojaInventario(ConstruirInventario().Items));
            default:
                return RedirectToAction("Index", "Dashboard");
        }
    }

    // ======================================================================================================
    //  Hojas de Excel reutilizables
    // ======================================================================================================

    private static TablaHoja HojaVentas(List<Factura> facturas, string? periodo) => new()
    {
        Nombre = "Ventas",
        Titulo = "Detalle de ventas",
        Subtitulo = $"Periodo: {periodo}",
        Columnas =
        {
            new("N.º", TipoCol.Entero), new("Fecha", TipoCol.Fecha), new("Cliente"), new("Documento"),
            new("Subtotal", TipoCol.Moneda, true), new("IVA", TipoCol.Moneda, true), new("Retención", TipoCol.Moneda, true), new("Total", TipoCol.Moneda, true)
        },
        Filas = facturas.OrderBy(f => f.Fecha).ThenBy(f => f.Id).Select(f => new object?[]
        {
            f.Id, f.Fecha, NombreCliente(f), f.IdClienteNavigation?.NumeroDocumento,
            f.TotalBruto, f.Iva, f.RteFte ?? 0, f.TotalPagar
        }).ToList()
    };

    private static TablaHoja HojaGastos(List<Gasto> gastos, string? periodo)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return new TablaHoja
        {
            Nombre = "Gastos",
            Titulo = "Detalle de egresos",
            Subtitulo = $"Periodo: {periodo}",
            Columnas =
            {
                new("Registro", TipoCol.Fecha), new("Categoría"), new("Descripción"), new("Vence", TipoCol.Fecha),
                new("Pagado el", TipoCol.Fecha), new("Estado"), new("Monto", TipoCol.Moneda, true)
            },
            Filas = gastos.OrderBy(g => g.FechaRegistro).Select(g => new object?[]
            {
                g.FechaRegistro, g.IdCategoriaNavigation?.Nombre, g.Descripcion, g.FechaVencimiento, g.FechaPago,
                EstadoGasto(g, hoy), g.Monto
            }).ToList()
        };
    }

    private static TablaHoja HojaTop(string hoja, string titulo, List<FilaTop> filas, string colNombre, string colCantidad, string? periodo) => new()
    {
        Nombre = hoja,
        Titulo = titulo,
        Subtitulo = $"Periodo: {periodo}",
        Columnas = { new(colNombre), new(colCantidad, TipoCol.Entero, true), new("Valor", TipoCol.Moneda, true) },
        Filas = filas.Select(f => new object?[] { f.Nombre, f.Cantidad, f.Valor }).ToList()
    };

    // Pública y estática para reutilizarla desde InventarioController (los métodos estáticos no son acciones).
    public static TablaHoja HojaInventario(List<Inventario> items, string? subtitulo = null) => new()
    {
        Nombre = "Inventario",
        Titulo = "Existencias en inventario",
        Subtitulo = subtitulo ?? $"Corte al {DateTime.Now:dd/MM/yyyy HH:mm}",
        Columnas =
        {
            new("Producto"), new("Código"), new("Bodega"), new("Estado"), new("Stock", TipoCol.Entero, true), new("Mínimo", TipoCol.Entero),
            new("Situación"), new("Costo unit.", TipoCol.Moneda), new("Valor a costo", TipoCol.Moneda, true),
            new("Precio venta", TipoCol.Moneda), new("Valor a precio venta", TipoCol.Moneda, true), new("Ingreso", TipoCol.Fecha)
        },
        Filas = items.Select(i => new object?[]
        {
            i.IdProductoNavigation?.Nombre, i.Codigo, i.IdBodegaNavigation?.NombreBodega, i.IdEstadoNavigation?.Nombre,
            i.StockActual ?? 0, i.StockMinimo ?? 0, Situacion(i),
            i.IdProductoNavigation?.PrecioCompra ?? 0, ReporteInventarioVM.ValorCosto(i),
            i.IdProductoNavigation?.PrecioVenta ?? 0, ReporteInventarioVM.ValorVenta(i), i.FechaIngreso
        }).ToList()
    };

    // ======================================================================================================
    //  Consultas y cálculos
    // ======================================================================================================

    private List<Factura> ConsultarFacturas(DateTime inicio, DateTime fin)
    {
        var desde = DateOnly.FromDateTime(inicio);
        var hasta = DateOnly.FromDateTime(fin);
        return _context.Facturas.AsNoTracking()
            .Include(f => f.IdClienteNavigation)
            .Include(f => f.DetalleFacturas).ThenInclude(d => d.IdProductoNavigation)
            .Where(f => f.Fecha.HasValue && f.Fecha.Value >= desde && f.Fecha.Value <= hasta)
            .OrderBy(f => f.Fecha).ThenBy(f => f.Id)
            .ToList();
    }

    private List<Gasto> ConsultarGastos(DateTime inicio, DateTime fin)
    {
        var finExclusivo = fin.AddDays(1);
        return _context.Gastos.AsNoTracking()
            .Include(g => g.IdCategoriaNavigation)
            .Where(g => g.FechaRegistro.HasValue && g.FechaRegistro.Value >= inicio && g.FechaRegistro.Value < finExclusivo)
            .OrderBy(g => g.FechaRegistro)
            .ToList();
    }

    private ReporteFinancieroVM ConstruirFinanciero(DateTime inicio, DateTime fin)
    {
        var facturas = ConsultarFacturas(inicio, fin);
        var gastos = ConsultarGastos(inicio, fin);
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var vm = new ReporteFinancieroVM
        {
            Cabecera = Cabecera("Reporte general de ingresos y egresos", "RG", $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}"),
            Desde = inicio,
            Hasta = fin,
            Facturas = facturas,
            Gastos = gastos,
            Ventas = facturas.Sum(f => f.TotalBruto),
            IvaFacturado = facturas.Sum(f => f.Iva),
            Retenciones = facturas.Sum(f => f.RteFte ?? 0),
            TotalFacturado = facturas.Sum(f => f.TotalPagar),
            CostoVentas = facturas.SelectMany(f => f.DetalleFacturas)
                .Where(d => d.IdProductoNavigation != null)
                .Sum(d => d.Cantidad * d.IdProductoNavigation!.PrecioCompra),
            Egresos = gastos.Sum(g => g.Monto),
            EgresosPagados = gastos.Where(g => g.Estado == "Pagado").Sum(g => g.Monto),
            EgresosVencidos = gastos.Where(g => g.Estado != "Pagado" && g.FechaVencimiento < hoy).Sum(g => g.Monto)
        };
        vm.EgresosPendientes = vm.Egresos - vm.EgresosPagados;
        vm.GastosPorCategoria = PorCategoria(gastos);

        // Evolución: por día (≤ 31 días), por semana (≤ 150 días) o por mes.
        var dias = (fin - inicio).Days + 1;
        var modo = dias <= 31 ? 0 : dias <= 150 ? 1 : 2;
        vm.AgrupadoPor = modo == 0 ? "por día" : modo == 1 ? "por semana" : "por mes";

        var grupos = new SortedDictionary<DateTime, FilaPeriodo>();
        FilaPeriodo Cubo(DateTime fecha)
        {
            DateTime clave;
            string etiqueta;
            if (modo == 0) { clave = fecha.Date; etiqueta = fecha.ToString("ddd dd/MM/yyyy"); }
            else if (modo == 1)
            {
                clave = fecha.Date.AddDays(-(((int)fecha.DayOfWeek + 6) % 7));
                etiqueta = $"Semana {clave:dd/MM} – {clave.AddDays(6):dd/MM/yyyy}";
            }
            else { clave = new DateTime(fecha.Year, fecha.Month, 1); etiqueta = fecha.ToString("MMMM yyyy"); }

            if (!grupos.TryGetValue(clave, out var fila)) grupos[clave] = fila = new FilaPeriodo { Etiqueta = etiqueta };
            return fila;
        }
        foreach (var f in facturas.Where(f => f.Fecha.HasValue))
        {
            var fila = Cubo(f.Fecha!.Value.ToDateTime(TimeOnly.MinValue));
            fila.Facturas++;
            fila.Ventas += f.TotalBruto;
        }
        foreach (var g in gastos.Where(g => g.FechaRegistro.HasValue))
            Cubo(g.FechaRegistro!.Value).Gastos += g.Monto;
        vm.Evolucion = grupos.Values.ToList();

        vm.TopClientes = facturas.GroupBy(f => NombreCliente(f))
            .Select(g => new FilaTop { Nombre = g.Key, Cantidad = g.Count(), Valor = g.Sum(f => f.TotalPagar) })
            .OrderByDescending(x => x.Valor).Take(10).ToList();

        vm.TopProductos = facturas.SelectMany(f => f.DetalleFacturas)
            .GroupBy(d => d.Descripción)
            .Select(g => new FilaTop { Nombre = g.Key, Cantidad = g.Sum(d => d.Cantidad), Valor = g.Sum(d => d.Subtotal) })
            .OrderByDescending(x => x.Valor).Take(10).ToList();

        return vm;
    }

    private ReporteVentasVM ConstruirVentas(DateTime inicio, DateTime fin)
    {
        var facturas = ConsultarFacturas(inicio, fin);
        return new ReporteVentasVM
        {
            Cabecera = Cabecera("Reporte de ventas", "RV", $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}"),
            Facturas = facturas,
            TopClientes = facturas.GroupBy(f => NombreCliente(f))
                .Select(g => new FilaTop { Nombre = g.Key, Cantidad = g.Count(), Valor = g.Sum(f => f.TotalPagar) })
                .OrderByDescending(x => x.Valor).Take(5).ToList()
        };
    }

    private ReporteGastosVM ConstruirGastos(DateTime inicio, DateTime fin)
    {
        var gastos = ConsultarGastos(inicio, fin);
        return new ReporteGastosVM
        {
            Cabecera = Cabecera("Reporte de gastos y egresos", "RE", $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}"),
            Gastos = gastos,
            PorCategoria = PorCategoria(gastos),
            Hoy = DateOnly.FromDateTime(DateTime.Today)
        };
    }

    private ReporteInventarioVM ConstruirInventario() => new()
    {
        Cabecera = Cabecera("Reporte de inventario", "RI", $"Corte al {DateTime.Now:dd/MM/yyyy HH:mm}"),
        Items = _context.Inventarios.AsNoTracking()
            .Include(i => i.IdProductoNavigation)
            .Include(i => i.IdBodegaNavigation)
            .Include(i => i.IdEstadoNavigation)
            .OrderBy(i => i.IdBodegaNavigation.NombreBodega).ThenBy(i => i.IdProductoNavigation.Nombre)
            .ToList()
    };

    private static List<FilaCategoria> PorCategoria(List<Gasto> gastos)
    {
        var total = gastos.Sum(g => g.Monto);
        return gastos.GroupBy(g => g.IdCategoriaNavigation?.Nombre ?? "Sin categoría")
            .Select(g => new FilaCategoria
            {
                Categoria = g.Key,
                Cantidad = g.Count(),
                Monto = g.Sum(x => x.Monto),
                Porcentaje = total > 0 ? g.Sum(x => x.Monto) / total : 0
            })
            .OrderByDescending(c => c.Monto).ToList();
    }

    // ======================================================================================================
    //  Utilidades
    // ======================================================================================================

    private static (DateTime inicio, DateTime fin) Rango(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var fin = (fechaFin ?? DateTime.Today).Date;
        if (fin < inicio) (inicio, fin) = (fin, inicio);
        return (inicio, fin);
    }

    private PdfCabecera Cabecera(string titulo, string prefijo, string periodo)
        => new(titulo, $"{prefijo}-{DateTime.Now:yyMMdd-HHmm}", periodo, User.Identity?.Name, DateTime.Now);

    private static string NombreCliente(Factura f)
        => $"{f.IdClienteNavigation?.Nombres} {f.IdClienteNavigation?.Apellidos}".Trim() is { Length: > 0 } n ? n : "Sin cliente";

    private static string EstadoGasto(Gasto g, DateOnly hoy)
        => g.Estado == "Pagado" ? "Pagado" : g.FechaVencimiento < hoy ? "Vencido" : "Pendiente";

    private static string Situacion(Inventario i)
        => ReporteInventarioVM.EsAgotado(i) ? "Agotado" : ReporteInventarioVM.EsBajo(i) ? "Stock bajo" : "Normal";

    // Márgenes y pie con numeración de páginas. El pie lleva el título del documento ([title]) y "n / total".
    // Se evitan las tildes en los argumentos de línea de comandos; el texto con tildes sale del <title> del HTML.
    private static ViewAsPdf Pdf(string vista, object modelo, string archivo) => new(vista, modelo)
    {
        FileName = archivo,
        PageSize = Size.A4,
        PageOrientation = Orientation.Portrait,
        PageMargins = new Margins(16, 14, 18, 14),
        CustomSwitches = "--print-media-type --footer-font-size 7 --footer-spacing 5 --footer-line " +
                         "--footer-left \"[title]\" --footer-right \"[page] / [topage]\""
    };
}
