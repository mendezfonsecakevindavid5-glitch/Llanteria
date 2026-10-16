using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Infrastructure;

// =====================================================================================================================
//  Exportación a Excel (.xlsx) con formato profesional. Requiere el paquete NuGet "ClosedXML".
//
//  Uso desde un controlador:
//      return this.ExcelArchivo("ventas", new TablaHoja
//      {
//          Nombre = "Ventas", Titulo = "Listado de ventas", Subtitulo = "Todas las facturas",
//          Columnas = { new Col("N.º", TipoCol.Entero), new Col("Fecha", TipoCol.Fecha), new Col("Total", TipoCol.Moneda, total: true) },
//          Filas = filas   // List<object?[]> con un valor por columna (string, int, decimal, DateOnly, DateTime...)
//      });
//
//  Cada hoja lleva: encabezado del negocio, título, fecha de generación, tabla con filas alternadas, filtros,
//  fila de totales (SUBTOTAL: respeta los filtros), paneles inmovilizados y configuración de impresión (carta, ajustada al ancho).
// =====================================================================================================================

public enum TipoCol { Texto, Entero, Moneda, Fecha, FechaHora, Porcentaje }

/// <param name="Total">Si es true, la fila de totales suma esta columna.</param>
public record Col(string Titulo, TipoCol Tipo = TipoCol.Texto, bool Total = false);

public abstract class Hoja
{
    public string Nombre { get; init; } = "Hoja";
    public string Titulo { get; init; } = "";
    public string? Subtitulo { get; init; }
}

/// <summary>Tabla de datos con encabezados (listados, detalles).</summary>
public class TablaHoja : Hoja
{
    public List<Col> Columnas { get; init; } = new();
    public List<object?[]> Filas { get; init; } = new();
    public bool Totales { get; init; } = true;
}

public enum EstiloLinea { Normal, Seccion, Subtotal, Total, Nota }

public record LineaResumen(string Texto, object? Valor = null, TipoCol Tipo = TipoCol.Moneda, EstiloLinea Estilo = EstiloLinea.Normal);

/// <summary>Hoja de dos columnas (concepto / valor) para estados de resultados y resúmenes.</summary>
public class ResumenHoja : Hoja
{
    public List<LineaResumen> Lineas { get; init; } = new();
}

public static class ExcelExport
{
    private const string Tinta = "#1F2328";
    private const string Marca = "#C92B0A";
    private const string Gris = "#6B7280";
    private const string LineaSuave = "#E3E6EA";
    private const string FondoSuave = "#F6F7F9";
    private const string FondoTotal = "#EBEDF0";

    private const string FormatoMoneda = "\"$\" #,##0;[Red]-\"$\" #,##0";
    private const string FormatoEntero = "#,##0";
    private const string FormatoFecha = "dd/mm/yyyy";
    private const string FormatoFechaHora = "dd/mm/yyyy hh:mm";
    private const string FormatoPorcentaje = "0.0%";

    public static FileContentResult ExcelArchivo(this ControllerBase controlador, string nombreBase, params Hoja[] hojas)
    {
        var negocio = (NegocioInfo?)controlador.HttpContext.RequestServices.GetService(typeof(NegocioInfo)) ?? new NegocioInfo();
        var usuario = controlador.User.Identity?.Name;

        using var libro = new XLWorkbook();
        libro.Properties.Author = negocio.Nombre;
        libro.Properties.Title = hojas.Length > 0 ? hojas[0].Titulo : nombreBase;

        foreach (var hoja in hojas)
        {
            switch (hoja)
            {
                case TablaHoja t: DibujarTabla(libro, negocio, usuario, t); break;
                case ResumenHoja r: DibujarResumen(libro, negocio, usuario, r); break;
            }
        }

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
    }

    // ---------------------------------------------------------------------------------------------------------------
    //  Tabla
    // ---------------------------------------------------------------------------------------------------------------
    private static void DibujarTabla(XLWorkbook libro, NegocioInfo negocio, string? usuario, TablaHoja t)
    {
        var ws = libro.Worksheets.Add(NombreHoja(t.Nombre));
        var n = Math.Max(t.Columnas.Count, 1);
        var filaEnc = Encabezado(ws, negocio, usuario, t, n);

        // Encabezados de columna
        for (var i = 0; i < t.Columnas.Count; i++)
        {
            var c = ws.Cell(filaEnc, i + 1);
            c.Value = t.Columnas[i].Titulo;
            c.Style.Alignment.Horizontal = Alineacion(t.Columnas[i].Tipo, encabezado: true);
        }
        var enc = ws.Range(filaEnc, 1, filaEnc, n);
        enc.Style.Font.Bold = true;
        enc.Style.Font.FontSize = 10;
        enc.Style.Font.FontColor = XLColor.White;
        enc.Style.Fill.BackgroundColor = XLColor.FromHtml(Tinta);
        enc.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        enc.Style.Alignment.WrapText = true;
        ws.Row(filaEnc).Height = 24;

        // Datos
        var primera = filaEnc + 1;
        var fila = primera;
        var anchos = t.Columnas.Select(c => (double)c.Titulo.Length).ToArray();

        foreach (var datos in t.Filas)
        {
            for (var i = 0; i < t.Columnas.Count; i++)
            {
                var valor = i < datos.Length ? datos[i] : null;
                var celda = ws.Cell(fila, i + 1);
                Poner(celda, valor, t.Columnas[i].Tipo);
                anchos[i] = Math.Max(anchos[i], LargoVisible(valor, t.Columnas[i].Tipo));
            }
            fila++;
        }
        var ultima = fila - 1;
        var hayDatos = ultima >= primera;

        if (hayDatos)
        {
            var cuerpo = ws.Range(primera, 1, ultima, n);
            cuerpo.Style.Font.FontSize = 10;
            cuerpo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cuerpo.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cuerpo.Style.Border.BottomBorderColor = XLColor.FromHtml(LineaSuave);

            for (var r = primera; r <= ultima; r++)
            {
                if ((r - primera) % 2 == 1) ws.Range(r, 1, r, n).Style.Fill.BackgroundColor = XLColor.FromHtml(FondoSuave);
            }
            for (var i = 0; i < t.Columnas.Count; i++)
            {
                var col = ws.Range(primera, i + 1, ultima, i + 1);
                col.Style.NumberFormat.Format = Formato(t.Columnas[i].Tipo);
                col.Style.Alignment.Horizontal = Alineacion(t.Columnas[i].Tipo, encabezado: false);
                if (t.Columnas[i].Tipo == TipoCol.Texto && anchos[i] > 46) col.Style.Alignment.WrapText = true;
            }
            ws.Range(filaEnc, 1, ultima, n).SetAutoFilter();
        }

        // Totales (SUBTOTAL 109 = suma que ignora las filas ocultas por el filtro)
        var filaTotal = ultima + 1;
        if (hayDatos && t.Totales && t.Columnas.Any(c => c.Total))
        {
            ws.Cell(filaTotal, 1).Value = $"TOTAL  ({t.Filas.Count:N0} registros)";
            for (var i = 0; i < t.Columnas.Count; i++)
            {
                if (!t.Columnas[i].Total) continue;
                var letra = XLHelper.GetColumnLetterFromNumber(i + 1);
                var c = ws.Cell(filaTotal, i + 1);
                c.FormulaA1 = $"SUBTOTAL(109,{letra}{primera}:{letra}{ultima})";
                c.Style.NumberFormat.Format = Formato(t.Columnas[i].Tipo);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                anchos[i] = Math.Max(anchos[i], 15);
            }
            var tot = ws.Range(filaTotal, 1, filaTotal, n);
            tot.Style.Font.Bold = true;
            tot.Style.Font.FontSize = 10;
            tot.Style.Fill.BackgroundColor = XLColor.FromHtml(FondoTotal);
            tot.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            tot.Style.Border.TopBorderColor = XLColor.FromHtml(Tinta);
            ws.Row(filaTotal).Height = 20;
            tot.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        if (!hayDatos)
        {
            ws.Cell(filaEnc + 1, 1).Value = "No hay registros para mostrar.";
            ws.Cell(filaEnc + 1, 1).Style.Font.Italic = true;
            ws.Cell(filaEnc + 1, 1).Style.Font.FontColor = XLColor.FromHtml(Gris);
        }

        // Anchos
        for (var i = 0; i < t.Columnas.Count; i++)
        {
            var minimo = t.Columnas[i].Tipo switch { TipoCol.Moneda => 15, TipoCol.Fecha => 12, TipoCol.FechaHora => 17, TipoCol.Entero => 9, _ => 10 };
            ws.Column(i + 1).Width = Math.Clamp(anchos[i] + 3, minimo, 52);
        }

        ws.SheetView.FreezeRows(filaEnc);
        Impresion(ws, negocio, n, filaEnc, horizontal: n > 7);
    }

    // ---------------------------------------------------------------------------------------------------------------
    //  Resumen (concepto / valor)
    // ---------------------------------------------------------------------------------------------------------------
    private static void DibujarResumen(XLWorkbook libro, NegocioInfo negocio, string? usuario, ResumenHoja r)
    {
        var ws = libro.Worksheets.Add(NombreHoja(r.Nombre));
        const int n = 2;
        var fila = Encabezado(ws, negocio, usuario, r, n);

        foreach (var l in r.Lineas)
        {
            var rango = ws.Range(fila, 1, fila, n);
            var a = ws.Cell(fila, 1);
            var b = ws.Cell(fila, 2);
            a.Value = l.Texto;
            rango.Style.Font.FontSize = 10;
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            if (l.Valor != null && l.Estilo != EstiloLinea.Seccion && l.Estilo != EstiloLinea.Nota)
            {
                Poner(b, l.Valor, l.Tipo);
                b.Style.NumberFormat.Format = Formato(l.Tipo);
                b.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            switch (l.Estilo)
            {
                case EstiloLinea.Seccion:
                    rango.Style.Font.Bold = true;
                    rango.Style.Font.FontColor = XLColor.White;
                    rango.Style.Fill.BackgroundColor = XLColor.FromHtml(Tinta);
                    ws.Row(fila).Height = 20;
                    break;
                case EstiloLinea.Subtotal:
                    rango.Style.Font.Bold = true;
                    rango.Style.Fill.BackgroundColor = XLColor.FromHtml(FondoTotal);
                    rango.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rango.Style.Border.TopBorderColor = XLColor.FromHtml(Tinta);
                    break;
                case EstiloLinea.Total:
                    rango.Style.Font.Bold = true;
                    rango.Style.Font.FontSize = 11;
                    rango.Style.Fill.BackgroundColor = XLColor.FromHtml(FondoTotal);
                    rango.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                    rango.Style.Border.TopBorderColor = XLColor.FromHtml(Marca);
                    rango.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                    rango.Style.Border.BottomBorderColor = XLColor.FromHtml(Marca);
                    ws.Row(fila).Height = 22;
                    break;
                case EstiloLinea.Nota:
                    a.Style.Font.Italic = true;
                    a.Style.Font.FontSize = 9;
                    a.Style.Font.FontColor = XLColor.FromHtml(Gris);
                    break;
                default:
                    a.Style.Alignment.Indent = 1;
                    rango.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rango.Style.Border.BottomBorderColor = XLColor.FromHtml(LineaSuave);
                    break;
            }
            fila++;
        }

        ws.Column(1).Width = 58;
        ws.Column(2).Width = 22;
        Impresion(ws, negocio, n, 0, horizontal: false);
    }

    // ---------------------------------------------------------------------------------------------------------------
    //  Piezas comunes
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>Escribe las 4 filas de cabecera de la hoja y devuelve la fila donde empieza la tabla.</summary>
    private static int Encabezado(IXLWorksheet ws, NegocioInfo negocio, string? usuario, Hoja h, int columnas)
    {
        ws.ShowGridLines = false;
        ws.TabColor = XLColor.FromHtml(Marca);
        ws.Style.Font.FontName = "Calibri";
        ws.Style.Font.FontSize = 10;

        var a = ws.Cell(1, 1);
        a.Value = negocio.Nombre.ToUpperInvariant();
        a.Style.Font.Bold = true;
        a.Style.Font.FontSize = 15;
        a.Style.Font.FontColor = XLColor.FromHtml(Tinta);
        ws.Row(1).Height = 24;

        var b = ws.Cell(2, 1);
        b.Value = h.Titulo;
        b.Style.Font.Bold = true;
        b.Style.Font.FontSize = 12;
        b.Style.Font.FontColor = XLColor.FromHtml(Marca);
        var linea = ws.Range(2, 1, 2, Math.Max(columnas, 2));
        linea.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        linea.Style.Border.BottomBorderColor = XLColor.FromHtml(Marca);
        ws.Row(2).Height = 20;

        if (!string.IsNullOrWhiteSpace(h.Subtitulo))
        {
            ws.Cell(3, 1).Value = h.Subtitulo;
            ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml(Gris);
        }

        var nit = string.IsNullOrWhiteSpace(negocio.Nit) ? "" : $"NIT {negocio.Nit}  ·  ";
        var por = string.IsNullOrWhiteSpace(usuario) ? "" : $" por {usuario}";
        ws.Cell(4, 1).Value = $"{nit}Generado el {DateTime.Now:dd/MM/yyyy HH:mm}{por}";
        ws.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml(Gris);
        ws.Cell(4, 1).Style.Font.FontSize = 9;

        ws.Row(5).Height = 8;
        return 6;
    }

    private static void Impresion(IXLWorksheet ws, NegocioInfo negocio, int columnas, int filaEncabezado, bool horizontal)
    {
        var p = ws.PageSetup;
        p.PaperSize = XLPaperSize.LetterPaper;
        p.PageOrientation = horizontal ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
        p.FitToPages(1, 0);
        p.CenterHorizontally = true;
        p.Margins.Left = 0.5;
        p.Margins.Right = 0.5;
        p.Margins.Top = 0.6;
        p.Margins.Bottom = 0.7;
        if (filaEncabezado > 0) p.SetRowsToRepeatAtTop(filaEncabezado, filaEncabezado);

        p.Footer.Left.AddText(negocio.Nombre, XLHFOccurrence.AllPages);
        p.Footer.Right.AddText("Página ", XLHFOccurrence.AllPages);
        p.Footer.Right.AddText(XLHFPredefinedText.PageNumber, XLHFOccurrence.AllPages);
        p.Footer.Right.AddText(" de ", XLHFOccurrence.AllPages);
        p.Footer.Right.AddText(XLHFPredefinedText.NumberOfPages, XLHFOccurrence.AllPages);
    }

    private static void Poner(IXLCell celda, object? valor, TipoCol tipo)
    {
        switch (valor)
        {
            case null: return;
            case string s: celda.Value = s; break;
            case DateOnly d: celda.Value = d.ToDateTime(TimeOnly.MinValue); break;
            case DateTime dt: celda.Value = dt; break;
            case decimal m: celda.Value = (double)m; break;
            case double db: celda.Value = db; break;
            case int i: celda.Value = i; break;
            case long l: celda.Value = l; break;
            case bool bo: celda.Value = bo ? "Sí" : "No"; break;
            default: celda.Value = valor.ToString() ?? ""; break;
        }
    }

    private static double LargoVisible(object? valor, TipoCol tipo) => valor switch
    {
        null => 0,
        string s => s.Split('\n').Max(x => x.Length),
        decimal or double => 15,
        DateOnly or DateTime => tipo == TipoCol.FechaHora ? 16 : 11,
        _ => valor.ToString()?.Length ?? 0
    };

    private static string Formato(TipoCol tipo) => tipo switch
    {
        TipoCol.Moneda => FormatoMoneda,
        TipoCol.Entero => FormatoEntero,
        TipoCol.Fecha => FormatoFecha,
        TipoCol.FechaHora => FormatoFechaHora,
        TipoCol.Porcentaje => FormatoPorcentaje,
        _ => "@"
    };

    private static XLAlignmentHorizontalValues Alineacion(TipoCol tipo, bool encabezado) => tipo switch
    {
        TipoCol.Moneda or TipoCol.Entero or TipoCol.Porcentaje => XLAlignmentHorizontalValues.Right,
        TipoCol.Fecha or TipoCol.FechaHora => XLAlignmentHorizontalValues.Center,
        _ => XLAlignmentHorizontalValues.Left
    };

    // Excel: máximo 31 caracteres y sin : \ / ? * [ ]
    private static string NombreHoja(string nombre)
    {
        var limpio = new string(nombre.Where(c => !":\\/?*[]".Contains(c)).ToArray()).Trim();
        if (limpio.Length == 0) limpio = "Hoja";
        return limpio.Length > 31 ? limpio[..31] : limpio;
    }
}
