using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Carrito guardado en una cookie ("id:cantidad|id:cantidad", 30 días). La sesión de la app caduca a los 10 minutos
/// de inactividad, así que no sirve para un carrito. En la cookie solo van ids y cantidades: los precios
/// siempre se leen de la base de datos.
/// </summary>
public static class Carrito
{
    private const string CookieCarrito = "carrito";
    private const string CookieCupon = "cupon";
    public const int MaxPorLinea = 50;

    public static Dictionary<int, int> Leer(HttpRequest req)
    {
        var d = new Dictionary<int, int>();
        if (!req.Cookies.TryGetValue(CookieCarrito, out var raw) || string.IsNullOrWhiteSpace(raw)) return d;
        foreach (var par in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var p = par.Split(':');
            if (p.Length == 2 && int.TryParse(p[0], out var id) && int.TryParse(p[1], out var c) && id > 0 && c > 0)
                d[id] = Math.Min(c, MaxPorLinea);
        }
        return d;
    }

    public static void Guardar(HttpRequest req, HttpResponse res, Dictionary<int, int> carrito)
    {
        if (carrito.Count == 0) { res.Cookies.Delete(CookieCarrito); return; }
        res.Cookies.Append(CookieCarrito, string.Join('|', carrito.Select(kv => $"{kv.Key}:{kv.Value}")), Opciones(req));
    }

    public static int Unidades(HttpRequest req) => Leer(req).Values.Sum();

    public static string? Cupon(HttpRequest req)
        => req.Cookies.TryGetValue(CookieCupon, out var c) && !string.IsNullOrWhiteSpace(c) ? c : null;

    public static void GuardarCupon(HttpRequest req, HttpResponse res, string codigo)
        => res.Cookies.Append(CookieCupon, codigo, Opciones(req));

    public static void QuitarCupon(HttpResponse res) => res.Cookies.Delete(CookieCupon);

    public static void Vaciar(HttpResponse res)
    {
        res.Cookies.Delete(CookieCarrito);
        res.Cookies.Delete(CookieCupon);
    }

    private static CookieOptions Opciones(HttpRequest req) => new()
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = req.IsHttps,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    };

    /// <summary>Unidades disponibles por producto. null = el producto no tiene inventario cargado (no se controla).</summary>
    public static Dictionary<int, int?> Disponibilidad(LlanteriaDbContext db, IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        var danado = StockHelper.IdEstado(db, DatosIniciales.Estados.Danado);
        var inv = db.Inventarios.AsNoTracking().Where(i => lista.Contains(i.IdProducto)).ToList()
            .GroupBy(i => i.IdProducto)
            .ToDictionary(g => g.Key, g => (int?)g.Where(i => i.IdEstado != danado).Sum(i => i.StockActual ?? 0));
        return lista.ToDictionary(id => id, id => inv.TryGetValue(id, out var v) ? v : null);
    }
}

public class LineaCarrito
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = "";
    public string? Marca { get; set; }
    public string Imagen { get; set; } = "default-producto.png";
    public decimal PrecioLista { get; set; }
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
    public int? Disponible { get; set; }

    public decimal Subtotal => Precio * Cantidad;
    public bool EnPromocion => Precio < PrecioLista;
    public bool SinStock => Disponible.HasValue && Cantidad > Disponible.Value;
}

/// <summary>Totales de una compra. Los precios incluyen IVA (como en el comercio minorista colombiano): aquí se desglosa.</summary>
public class ResumenCompra
{
    public const decimal TasaIva = 0.19m;

    public List<LineaCarrito> Lineas { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal BaseGravable { get; set; }
    public decimal Iva { get; set; }
    public decimal Envio { get; set; }
    public decimal Total { get; set; }

    public string? Cupon { get; set; }
    public string? TituloCupon { get; set; }
    public bool CuponInvalido { get; set; }

    public int Unidades => Lineas.Sum(l => l.Cantidad);
    public bool HayProblemas => Lineas.Any(l => l.SinStock);

    public static ResumenCompra Armar(LlanteriaDbContext db, Dictionary<int, int> carrito, string? cupon)
    {
        var r = new ResumenCompra();
        var ids = carrito.Keys.ToList();
        var productos = db.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
            .Where(p => ids.Contains(p.Id)).ToList();
        var disp = Carrito.Disponibilidad(db, productos.Select(p => p.Id));

        foreach (var p in productos.OrderBy(p => p.Nombre))
        {
            r.Lineas.Add(new LineaCarrito
            {
                IdProducto = p.Id,
                Nombre = p.Nombre,
                Marca = p.DetalleProducto?.IdMarcaNavigation?.Nombre,
                Imagen = string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen,
                PrecioLista = p.PrecioVenta,
                Precio = p.PrecioActual,
                Cantidad = carrito[p.Id],
                Disponible = disp[p.Id]
            });
        }

        r.Subtotal = r.Lineas.Sum(l => l.Subtotal);

        if (!string.IsNullOrWhiteSpace(cupon) && r.Subtotal > 0)
        {
            var codigo = cupon.Trim().ToUpper();
            var ahora = DateTime.Now;
            var o = db.Ofertas.AsNoTracking().FirstOrDefault(x =>
                x.CodigoPromocional == codigo && x.Estado != false && x.FechaInicio <= ahora && x.FechaFin >= ahora.Date);
            decimal desc = 0;
            if (o != null)
            {
                if (o.PorcentajeDescuento > 0) desc = Math.Round(r.Subtotal * o.PorcentajeDescuento!.Value / 100m, 0);
                else if (o.MontoDescuentoFijo > 0) desc = Math.Min(o.MontoDescuentoFijo!.Value, r.Subtotal);
            }
            if (desc > 0) { r.Cupon = codigo; r.TituloCupon = o!.Titulo; r.Descuento = desc; }
            else r.CuponInvalido = true;
        }

        var neto = r.Subtotal - r.Descuento;
        r.BaseGravable = Math.Round(neto / (1 + TasaIva), 0);
        r.Iva = neto - r.BaseGravable;
        r.Envio = 0; // el costo de envío se confirma con el cliente según el destino
        r.Total = neto + r.Envio;
        return r;
    }
}
