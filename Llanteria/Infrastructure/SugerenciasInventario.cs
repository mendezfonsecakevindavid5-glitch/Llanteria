using Llanteria.Data;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>Una sugerencia de reposición para un producto: cuánto se vende por día, en cuántos días se agota y cuánto pedir.</summary>
public record SugerenciaReabastecimiento(
    int IdProducto,
    string Producto,
    int StockActual,
    int StockMinimo,
    double VentaDiariaPromedio,
    int? DiasParaAgotarse,
    int CantidadSugerida);

/// <summary>
/// Calcula qué productos conviene reabastecer pronto, a partir del ritmo de venta real de cada uno
/// (no es un modelo de IA: es un promedio simple de unidades vendidas por día sobre los últimos 30 días).
/// Sirve para no esperar a que el stock ya esté agotado, y para sugerir una cantidad de pedido con base
/// en el consumo real en vez de un número fijo.
/// </summary>
public static class SugerenciasInventario
{
    /// <summary>Historial de ventas que se usa para calcular el ritmo de venta diario de cada producto.</summary>
    public const int DiasHistorial = 30;

    /// <summary>Si al ritmo actual el stock se agota en menos de estos días, el producto se marca en riesgo.</summary>
    public const int DiasAlertaDefault = 7;

    /// <summary>Días de venta que debe cubrir la cantidad sugerida (un estimado simple de tiempo de reposición; ajústalo según tu proveedor real).</summary>
    public const int DiasReposicion = 14;

    public static List<SugerenciaReabastecimiento> Calcular(LlanteriaDbContext db, DateTime hoy, int diasAlerta = DiasAlertaDefault, int maximo = 10)
    {
        var desde = DateOnly.FromDateTime(hoy.AddDays(-DiasHistorial));

        // Unidades vendidas por producto en la ventana de historial (solo renglones de factura ligados a un producto).
        var ventasPorProducto = db.DetalleFacturas.AsNoTracking()
            .Where(d => d.IdProducto != null && d.IdFacturaNavigation.Fecha != null && d.IdFacturaNavigation.Fecha >= desde)
            .GroupBy(d => d.IdProducto!.Value)
            .Select(g => new { IdProducto = g.Key, Unidades = g.Sum(d => d.Cantidad) })
            .ToDictionary(x => x.IdProducto, x => x.Unidades);

        // Stock actual y mínimo agregados por producto (un producto puede tener varias filas de inventario, p. ej. por bodega).
        var inventarioPorProducto = db.Inventarios.AsNoTracking()
            .Include(i => i.IdProductoNavigation)
            .Where(i => i.StockActual != null)
            .GroupBy(i => i.IdProducto)
            .Select(g => new
            {
                IdProducto = g.Key,
                Nombre = g.First().IdProductoNavigation!.Nombre,
                StockActual = g.Sum(i => i.StockActual ?? 0),
                StockMinimo = g.Max(i => i.StockMinimo ?? 0),
            })
            .ToList();

        var resultado = new List<SugerenciaReabastecimiento>();
        foreach (var p in inventarioPorProducto)
        {
            ventasPorProducto.TryGetValue(p.IdProducto, out var unidadesVendidas);
            var ventaDiaria = unidadesVendidas / (double)DiasHistorial;

            int? diasParaAgotarse = ventaDiaria > 0 ? (int)Math.Floor(p.StockActual / ventaDiaria) : null;
            var enRiesgo = (diasParaAgotarse.HasValue && diasParaAgotarse.Value <= diasAlerta) || p.StockActual <= p.StockMinimo;
            if (!enRiesgo) continue;

            // Con historial de ventas: cubrir el consumo esperado durante el tiempo de reposición.
            // Sin ventas registradas todavía: al menos volver a alcanzar el mínimo configurado.
            var cantidadSugerida = ventaDiaria > 0
                ? Math.Max(0, (int)Math.Ceiling(ventaDiaria * DiasReposicion) - p.StockActual)
                : Math.Max(0, p.StockMinimo - p.StockActual);

            if (cantidadSugerida <= 0) continue; // en riesgo por mínimo pero ya cubierto para el período de reposición

            resultado.Add(new SugerenciaReabastecimiento(
                p.IdProducto, p.Nombre ?? "(sin nombre)", p.StockActual, p.StockMinimo,
                Math.Round(ventaDiaria, 2), diasParaAgotarse, cantidadSugerida));
        }

        return resultado
            .OrderBy(s => s.DiasParaAgotarse ?? int.MaxValue)
            .ThenBy(s => s.StockActual)
            .Take(maximo)
            .ToList();
    }
}
