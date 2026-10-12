using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Infrastructure;

/// <summary>
/// Exportación a CSV pensada para Excel en español: separador ";" y UTF-8 con BOM (así respeta tildes y la ñ).
/// Sin dependencias externas.
/// </summary>
public static class Csv
{
    public static FileContentResult Archivo(string nombreBase, string[] encabezados, IEnumerable<string?[]> filas)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', encabezados.Select(Celda)));
        foreach (var fila in filas) sb.AppendLine(string.Join(';', fila.Select(Celda)));

        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new FileContentResult(bytes, "text/csv; charset=utf-8")
        {
            FileDownloadName = $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };
    }

    private static string Celda(string? v)
    {
        v ??= "";
        // Evita que Excel interprete texto del usuario como fórmula (=, +, @, o - seguido de letra).
        if (v.Length > 0 && (v[0] is '=' or '+' or '@' || (v[0] == '-' && v.Length > 1 && !char.IsDigit(v[1]))))
            v = "'" + v;
        if (v.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0)
            v = "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }
}
