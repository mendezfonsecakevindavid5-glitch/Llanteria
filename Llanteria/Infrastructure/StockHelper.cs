using Llanteria.Data;
using Llanteria.Models;

namespace Llanteria.Infrastructure;

/// <summary>
/// Movimientos de stock ligados a las facturas. No llaman a SaveChanges: quien los usa guarda
/// junto con la factura, para que venta y stock queden siempre consistentes.
/// </summary>
public static class StockHelper
{
    /// <summary>Descuenta unidades. Devuelve un mensaje de error, o null si salió bien. Los productos sin registros de inventario no se controlan.</summary>
    public static string? Descontar(LlanteriaDbContext db, int idProducto, int cantidad, string nombre)
    {
        var filas = db.Inventarios.Where(i => i.IdProducto == idProducto).ToList();
        if (filas.Count == 0) return null;

        var danado = IdEstado(db, UsuariosIniciales.Estados.Danado);
        var disponible = filas.Where(f => f.IdEstado != danado).Sum(f => f.StockActual ?? 0);
        if (disponible < cantidad)
            return $"Stock insuficiente de «{nombre}»: hay {disponible} y pediste {cantidad}.";

        var agotado = IdEstado(db, UsuariosIniciales.Estados.Agotado);
        var restante = cantidad;
        foreach (var f in filas.Where(x => x.IdEstado != danado).OrderByDescending(x => x.StockActual))
        {
            if (restante == 0) break;
            var toma = Math.Min(f.StockActual ?? 0, restante);
            f.StockActual = (f.StockActual ?? 0) - toma;
            restante -= toma;
            if (f.StockActual == 0 && agotado > 0) f.IdEstado = agotado;
        }
        return null;
    }

    /// <summary>Devuelve unidades al inventario (factura editada o eliminada).</summary>
    public static void Reponer(LlanteriaDbContext db, int idProducto, int cantidad)
    {
        var fila = db.Inventarios.Where(i => i.IdProducto == idProducto).OrderByDescending(i => i.StockActual).FirstOrDefault();
        if (fila == null) return;
        fila.StockActual = (fila.StockActual ?? 0) + cantidad;
        var disponible = IdEstado(db, UsuariosIniciales.Estados.Disponible);
        if (fila.IdEstado == IdEstado(db, UsuariosIniciales.Estados.Agotado) && disponible > 0) fila.IdEstado = disponible;
    }

    public static int IdEstado(LlanteriaDbContext db, string nombre)
        => db.EstadoInventarios.Where(e => e.Nombre == nombre).Select(e => e.Id).FirstOrDefault();
}
