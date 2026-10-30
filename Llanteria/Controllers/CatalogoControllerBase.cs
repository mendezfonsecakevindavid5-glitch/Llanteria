using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Llanteria.Controllers;

/// <summary>Lógica común de los catálogos públicos con filtros (llantas, lubricantes, baterías y accesorios).</summary>
public abstract class CatalogoControllerBase : Controller
{
    protected const int PorPagina = 20;
    protected readonly LlanteriaDbContext _db;

    protected CatalogoControllerBase(LlanteriaDbContext db) => _db = db;

    protected abstract string Categoria { get; }
    protected virtual bool PorMedida => false;

    /// <summary>Qué bloque de filtros adicionales va en el panel izquierdo: "Llantas", "Lubricantes", "Baterias", "Accesorios" o "".</summary>
    protected virtual string ModoExtra => "";

    /// <summary>True si la petición pide solo el bloque de resultados (búsqueda en vivo), no la página completa.</summary>
    protected bool EsAjax() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    /// <summary>
    /// Productos de la categoría con marca y promoción ya cargadas. Se guardan 45 segundos en memoria: antes cada petición
    /// (y cada letra escrita en la búsqueda) hacía DOS consultas completas a la base; con conexiones lentas eso se notaba.
    /// El precio con descuento y el "agotado" se siguen calculando al momento, así que un cambio de precio o de stock aparece en menos de un minuto.
    /// </summary>
    private List<Producto> CargarCategoria()
    {
        var cache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        return cache.GetOrCreate($"catalogo:{Categoria}:{PorMedida}", entrada =>
        {
            entrada.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(45);
            IQueryable<Producto> baseQ = _db.Productos.AsNoTracking()
                .Include(p => p.DetalleProducto).ThenInclude(d => d!.IdMarcaNavigation)
                .Include(p => p.OfertaNavigation).ThenInclude(o => o!.EventoNavigation);
            // Las llantas también se reconocen por tener medida cargada.
            baseQ = PorMedida
                ? baseQ.Where(p => p.Categoria == Categoria || (p.DetalleProducto != null && p.DetalleProducto.Ancho != null))
                : baseQ.Where(p => p.Categoria == Categoria);
            return baseQ.ToList();
        })!;
    }

    private static bool Ig(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static List<string> Valores(IEnumerable<Producto> productos, Func<DetalleProducto, string?> campo)
        => productos.Select(p => p.DetalleProducto == null ? null : campo(p.DetalleProducto))
            .Where(v => !string.IsNullOrEmpty(v)).Select(v => v!).Distinct().OrderBy(v => v).ToList();

    protected CatalogoViewModel Armar(string? ancho, string? perfil, string? diametro, string[]? marca,
        int? min, int? max, string? orden, int pagina, string? tipo = null, string? qmarca = null,
        string? viscosidad = null, string? tipoAceite = null, string? amperaje = null, string? polaridad = null, int? garantia = null,
        int? evento = null, int? promocion = null,
        string? voltaje = null, string? tecnologia = null, string? aplicacion = null, string? presentacion = null,
        string? tipoAccesorio = null, string? ubicacion = null, bool promociones = false)
    {
        var vm = new CatalogoViewModel
        {
            Ancho = Limpio(ancho),
            Perfil = Limpio(perfil),
            Diametro = Limpio(diametro),
            Tipo = string.IsNullOrWhiteSpace(tipo) ? null : TiposVehiculo.Normalizar(tipo), // Auto, Moto, Pesada o Agricola (banner del inicio y chips)
            Marcas = (marca ?? Array.Empty<string>()).Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct().ToArray(),
            BuscarMarca = Limpio(qmarca),
            PrecioMin = min is > 0 ? min : null,
            PrecioMax = max is > 0 ? max : null,
            Orden = orden is "precio-asc" or "precio-desc" ? orden : "nombre", // alfabético por defecto
            Evento = evento is > 0 ? evento : null,
            Promocion = promocion is > 0 ? promocion : null,
            SoloPromociones = promociones,
            Viscosidad = Limpio(viscosidad),
            TipoAceite = Limpio(tipoAceite),
            Amperaje = Limpio(amperaje),
            Polaridad = Limpio(polaridad),
            Voltaje = Limpio(voltaje),
            TecnologiaBateria = Limpio(tecnologia),
            Aplicacion = Limpio(aplicacion),
            Presentacion = Limpio(presentacion),
            TipoAccesorio = Limpio(tipoAccesorio),
            Ubicacion = Limpio(ubicacion),
            Garantia = garantia is > 0 ? garantia : null,
            ModoExtra = ModoExtra
        };

        // Opciones del panel (calculadas sobre toda la categoría, no sobre el resultado filtrado).
        var todos = CargarCategoria();
        if (PorMedida)
        {
            vm.AnchosDisponibles = Valores(todos, d => d.Ancho);
            vm.PerfilesDisponibles = Valores(todos, d => d.Perfil);
            vm.DiametrosDisponibles = Valores(todos, d => d.Diametro);
        }
        if (ModeExtraEs("Lubricantes"))
        {
            vm.ViscosidadesDisponibles = Valores(todos, d => d.Viscosidad);
            vm.TiposAceiteDisponibles = Valores(todos, d => d.TipoAceite);
            vm.AplicacionesDisponibles = Valores(todos, d => d.AplicacionLubricante);
            vm.PresentacionesDisponibles = Valores(todos, d => d.PresentacionLubricante);
        }
        if (ModeExtraEs("Baterias"))
        {
            vm.AmperajesDisponibles = Valores(todos, d => d.Amperaje);
            vm.PolaridadesDisponibles = Valores(todos, d => d.Polaridad);
            vm.VoltajesDisponibles = Valores(todos, d => d.Voltaje);
            vm.TecnologiasDisponibles = Valores(todos, d => d.TecnologiaBateria);
        }
        if (ModeExtraEs("Lubricantes") || ModeExtraEs("Baterias"))
            vm.GarantiasDisponibles = todos.Select(p => p.DetalleProducto?.GarantiaMeses).Where(v => v.HasValue).Select(v => v!.Value).Distinct().OrderBy(v => v).ToList();
        if (ModeExtraEs("Accesorios"))
        {
            vm.TiposAccesorioDisponibles = Valores(todos, d => d.TipoAccesorio);
            vm.UbicacionesDisponibles = Valores(todos, d => d.UbicacionAccesorio);
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

        // Filtros elegidos. Todo se resuelve en memoria sobre la lista ya cargada (una sola consulta, y ni siquiera eso si está en caché).
        IEnumerable<Producto> q = todos;
        if (vm.Ancho != null) q = q.Where(p => Ig(p.DetalleProducto?.Ancho, vm.Ancho));
        if (vm.Perfil != null) q = q.Where(p => Ig(p.DetalleProducto?.Perfil, vm.Perfil));
        if (vm.Diametro != null) q = q.Where(p => Ig(p.DetalleProducto?.Diametro, vm.Diametro));
        if (vm.Tipo != null) q = q.Where(p => Ig(p.DetalleProducto?.TipoVehiculo, vm.Tipo));
        if (vm.Marcas.Length > 0)
        {
            var marcas = new HashSet<string>(vm.Marcas, StringComparer.OrdinalIgnoreCase);
            q = q.Where(p => p.DetalleProducto?.IdMarcaNavigation?.Nombre is { } n && marcas.Contains(n));
        }
        if (vm.BuscarMarca != null)
        {
            var texto = vm.BuscarMarca;
            q = q.Where(p => p.DetalleProducto?.IdMarcaNavigation?.Nombre is { } n && n.Contains(texto, StringComparison.CurrentCultureIgnoreCase));
        }
        if (vm.Viscosidad != null) q = q.Where(p => Ig(p.DetalleProducto?.Viscosidad, vm.Viscosidad));
        if (vm.TipoAceite != null) q = q.Where(p => Ig(p.DetalleProducto?.TipoAceite, vm.TipoAceite));
        if (vm.Aplicacion != null) q = q.Where(p => Ig(p.DetalleProducto?.AplicacionLubricante, vm.Aplicacion));
        if (vm.Presentacion != null) q = q.Where(p => Ig(p.DetalleProducto?.PresentacionLubricante, vm.Presentacion));
        if (vm.Amperaje != null) q = q.Where(p => Ig(p.DetalleProducto?.Amperaje, vm.Amperaje));
        if (vm.Polaridad != null) q = q.Where(p => Ig(p.DetalleProducto?.Polaridad, vm.Polaridad));
        if (vm.Voltaje != null) q = q.Where(p => Ig(p.DetalleProducto?.Voltaje, vm.Voltaje));
        if (vm.TecnologiaBateria != null) q = q.Where(p => Ig(p.DetalleProducto?.TecnologiaBateria, vm.TecnologiaBateria));
        if (vm.TipoAccesorio != null) q = q.Where(p => Ig(p.DetalleProducto?.TipoAccesorio, vm.TipoAccesorio));
        if (vm.Ubicacion != null) q = q.Where(p => Ig(p.DetalleProducto?.UbicacionAccesorio, vm.Ubicacion));
        if (vm.Garantia.HasValue) q = q.Where(p => p.DetalleProducto?.GarantiaMeses == vm.Garantia.Value);

        // El filtro y el orden usan el precio real (PrecioActual, con el descuento vigente), que cambia con la fecha.
        var filtrados = q.ToList();

        if (vm.Evento.HasValue)
        {
            var idEvento = vm.Evento.Value;
            filtrados = filtrados.Where(p => p.OfertaNavigation != null && p.OfertaNavigation.IdEvento == idEvento && p.OfertaNavigation.VigenteAhora(hoy)).ToList();
        }
        if (vm.Promocion.HasValue) // se conserva para los enlaces que ya apuntan a una promoción concreta
        {
            var idPromocion = vm.Promocion.Value;
            filtrados = filtrados.Where(p => p.IdOferta == idPromocion && p.OfertaNavigation != null && p.OfertaNavigation.VigenteAhora(hoy)).ToList();
        }
        if (vm.SoloPromociones) // chip "Promociones": cualquier promoción vigente o descuento propio del producto
            filtrados = filtrados.Where(p => p.EnPromocion || (p.OfertaNavigation != null && p.OfertaNavigation.VigenteAhora(hoy))).ToList();
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

        // Un producto con inventario cargado y sin unidades disponibles se muestra como agotado (consulta en vivo, solo de la página mostrada).
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
