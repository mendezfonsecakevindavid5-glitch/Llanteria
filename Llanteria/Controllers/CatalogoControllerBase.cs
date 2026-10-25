using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Lógica común de los catálogos públicos con filtros (llantas, lubricantes y baterías).</summary>
public abstract class CatalogoControllerBase : Controller
{
    protected const int PorPagina = 20;
    protected readonly LlanteriaDbContext _db;

    protected CatalogoControllerBase(LlanteriaDbContext db) => _db = db;

    protected abstract string Categoria { get; }
    protected virtual bool PorMedida => false;

    /// <summary>Qué bloque de filtros adicionales va en el panel izquierdo: "Llantas", "Lubricantes", "Baterias" o "".</summary>
    protected virtual string ModoExtra => "";

    /// <summary>True si la petición pide solo el bloque de resultados (búsqueda en vivo), no la página completa.</summary>
    protected bool EsAjax() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    protected CatalogoViewModel Armar(string? ancho, string? perfil, string? diametro, string[]? marca,
        int? min, int? max, string? orden, int pagina, string? tipo = null, string? qmarca = null,
        string? viscosidad = null, string? tipoAceite = null, string? amperaje = null, string? polaridad = null, int? garantia = null,
        int? evento = null, int? promocion = null)
    {
        // Base: productos de la categoría. Las llantas también se reconocen por tener medida cargada.
        IQueryable<Producto> baseQ = _db.Productos.AsNoTracking()
            .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
            .Include(p => p.OfertaNavigation).ThenInclude(o => o!.EventoNavigation);
        baseQ = PorMedida
            ? baseQ.Where(p => p.Categoria == Categoria || (p.DetalleProducto != null && p.DetalleProducto.Ancho != null))
            : baseQ.Where(p => p.Categoria == Categoria);

        var vm = new CatalogoViewModel
        {
            Ancho = Limpio(ancho),
            Perfil = Limpio(perfil),
            Diametro = Limpio(diametro),
            Tipo = string.IsNullOrWhiteSpace(tipo) ? null : TiposVehiculo.Normalizar(tipo), // "Auto" o "Moto" (banner del inicio y chips)
            Marcas = (marca ?? Array.Empty<string>()).Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct().ToArray(),
            BuscarMarca = Limpio(qmarca),
            PrecioMin = min is > 0 ? min : null,
            PrecioMax = max is > 0 ? max : null,
            Orden = orden is "precio-asc" or "precio-desc" ? orden : "nombre", // alfabético por defecto
            Evento = evento is > 0 ? evento : null,
            Promocion = promocion is > 0 ? promocion : null,
            Viscosidad = Limpio(viscosidad),
            TipoAceite = Limpio(tipoAceite),
            Amperaje = Limpio(amperaje),
            Polaridad = Limpio(polaridad),
            Garantia = garantia is > 0 ? garantia : null,
            ModoExtra = ModoExtra
        };

        // Opciones del panel (calculadas sobre toda la categoría, no sobre el resultado filtrado).
        var todos = baseQ.ToList();
        if (PorMedida)
        {
            vm.AnchosDisponibles = todos.Select(p => p.DetalleProducto?.Ancho).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.PerfilesDisponibles = todos.Select(p => p.DetalleProducto?.Perfil).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.DiametrosDisponibles = todos.Select(p => p.DetalleProducto?.Diametro).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
        }
        if (ModeExtraEs("Lubricantes"))
        {
            vm.ViscosidadesDisponibles = todos.Select(p => p.DetalleProducto?.Viscosidad).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.TiposAceiteDisponibles = todos.Select(p => p.DetalleProducto?.TipoAceite).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.GarantiasDisponibles = todos.Select(p => p.DetalleProducto?.GarantiaMeses).Where(v => v.HasValue).Select(v => v!.Value).Distinct().OrderBy(v => v).ToList();
        }
        if (ModeExtraEs("Baterias"))
        {
            vm.AmperajesDisponibles = todos.Select(p => p.DetalleProducto?.Amperaje).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.PolaridadesDisponibles = todos.Select(p => p.DetalleProducto?.Polaridad).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();
            vm.GarantiasDisponibles = todos.Select(p => p.DetalleProducto?.GarantiaMeses).Where(v => v.HasValue).Select(v => v!.Value).Distinct().OrderBy(v => v).ToList();
        }
        vm.MarcasDisponibles = todos
            .Select(p => p.DetalleProducto?.IdMarcaNavigation?.Nombre).Where(n => !string.IsNullOrEmpty(n))
            .GroupBy(n => n!).OrderBy(g => g.Key).Select(g => (g.Key, g.Count())).ToList();
        // Promociones vigentes que cubren productos de esta categoría, y los eventos a los que se deben.
        // Una promoción ligada a un evento solo se muestra el día del evento (de 12:00 a. m. a 11:59 p. m.).
        var hoy = DateTime.Now;
        var conOferta = todos.Where(p => p.OfertaNavigation != null && p.OfertaNavigation.VigenteAhora(hoy)).ToList();
        vm.PromocionesDisponibles = conOferta
            .GroupBy(p => p.OfertaNavigation!.Id).OrderBy(g => g.First().OfertaNavigation!.Titulo)
            .Select(g => (g.Key, g.First().OfertaNavigation!.Titulo, g.Count())).ToList();
        vm.EventosDisponibles = conOferta.Where(p => p.OfertaNavigation!.EventoNavigation != null)
            .GroupBy(p => p.OfertaNavigation!.EventoNavigation!.Id).OrderBy(g => g.First().OfertaNavigation!.EventoNavigation!.Nombre)
            .Select(g => (g.Key, g.First().OfertaNavigation!.EventoNavigation!.Nombre, g.Count())).ToList();
        if (todos.Count > 0)
        {
            // Se miden con el precio que realmente paga el cliente (con el descuento vigente).
            vm.PrecioMasBajo = (int)Math.Floor(todos.Min(p => p.PrecioActual));
            vm.PrecioMasAlto = (int)Math.Ceiling(todos.Max(p => p.PrecioActual));
        }

        // Filtros elegidos
        var q = baseQ;
        if (vm.Ancho != null) q = q.Where(p => p.DetalleProducto!.Ancho == vm.Ancho);
        if (vm.Perfil != null) q = q.Where(p => p.DetalleProducto!.Perfil == vm.Perfil);
        if (vm.Diametro != null) q = q.Where(p => p.DetalleProducto!.Diametro == vm.Diametro);
        if (vm.Tipo != null) q = q.Where(p => p.DetalleProducto!.TipoVehiculo == vm.Tipo);
        if (vm.Marcas.Length > 0)
        {
            var marcas = vm.Marcas;
            q = q.Where(p => p.DetalleProducto != null && p.DetalleProducto.IdMarcaNavigation != null
                             && marcas.Contains(p.DetalleProducto.IdMarcaNavigation.Nombre));
        }
        if (vm.BuscarMarca != null)
        {
            var texto = vm.BuscarMarca;
            q = q.Where(p => p.DetalleProducto != null && p.DetalleProducto.IdMarcaNavigation != null
                             && p.DetalleProducto.IdMarcaNavigation.Nombre.Contains(texto));
        }
        if (vm.Viscosidad != null) q = q.Where(p => p.DetalleProducto!.Viscosidad == vm.Viscosidad);
        if (vm.TipoAceite != null) q = q.Where(p => p.DetalleProducto!.TipoAceite == vm.TipoAceite);
        if (vm.Amperaje != null) q = q.Where(p => p.DetalleProducto!.Amperaje == vm.Amperaje);
        if (vm.Polaridad != null) q = q.Where(p => p.DetalleProducto!.Polaridad == vm.Polaridad);
        if (vm.Garantia.HasValue) q = q.Where(p => p.DetalleProducto!.GarantiaMeses == vm.Garantia.Value);

        // Lo que sigue se resuelve en memoria porque depende del precio con descuento (PrecioActual), que cambia con la fecha
        // y la base de datos no puede calcular. Antes se ordenaba por PrecioVenta: un producto en promoción quedaba
        // "fuera de lugar" frente al precio que se ve en la tarjeta. Ahora el filtro y el orden usan el precio real.
        var filtrados = q.ToList();

        if (vm.Evento.HasValue)
        {
            var idEvento = vm.Evento.Value;
            filtrados = filtrados.Where(p => p.OfertaNavigation != null && p.OfertaNavigation.IdEvento == idEvento && p.OfertaNavigation.VigenteAhora(hoy)).ToList();
        }
        if (vm.Promocion.HasValue)
        {
            var idPromocion = vm.Promocion.Value;
            filtrados = filtrados.Where(p => p.IdOferta == idPromocion && p.OfertaNavigation != null && p.OfertaNavigation.VigenteAhora(hoy)).ToList();
        }
        if (vm.PrecioMin.HasValue) filtrados = filtrados.Where(p => p.PrecioActual >= vm.PrecioMin.Value).ToList();
        if (vm.PrecioMax.HasValue) filtrados = filtrados.Where(p => p.PrecioActual <= vm.PrecioMax.Value).ToList();

        // El orden se aplica a TODOS los productos filtrados y recién después se corta la página de 20.
        var ordenados = vm.Orden switch
        {
            "precio-desc" => filtrados.OrderByDescending(p => p.PrecioActual).ThenBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList(),
            "precio-asc" => filtrados.OrderBy(p => p.PrecioActual).ThenBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList(),
            _ => filtrados.OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList() // alfabético por defecto
        };
        vm.Resultados = Paginado<Producto>.De(ordenados, pagina, PorPagina);

        // Un producto con inventario cargado y sin unidades disponibles se muestra como agotado.
        var ids = vm.Resultados.Items.Select(p => p.Id).ToList();
        var danado = StockHelper.IdEstado(_db, UsuariosIniciales.Estados.Danado);
        var inv = _db.Inventarios.AsNoTracking().Where(i => ids.Contains(i.IdProducto)).ToList()
            .GroupBy(i => i.IdProducto)
            .ToDictionary(g => g.Key, g => g.Where(i => i.IdEstado != danado).Sum(i => i.StockActual ?? 0));
        vm.Agotados = inv.Where(kv => kv.Value <= 0).Select(kv => kv.Key).ToHashSet();
        return vm;
    }

    private bool ModeExtraEs(string valor) => string.Equals(ModoExtra, valor, StringComparison.Ordinal);

    private static string? Limpio(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
