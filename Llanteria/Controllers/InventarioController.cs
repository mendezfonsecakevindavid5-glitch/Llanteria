using Llanteria.Data;
using Llanteria.Filters;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

// Todo el personal consulta las existencias; solo el administrador crea, edita o elimina registros.
public class InventarioController : Controller
{
    private readonly LlanteriaDbContext _db;

    public InventarioController(LlanteriaDbContext db) => _db = db;

    public IActionResult Index(string? buscar, string? filtro)
    {
        var consulta = _db.Inventarios.AsNoTracking()
            .Include(i => i.IdProductoNavigation)
            .Include(i => i.IdBodegaNavigation)
            .Include(i => i.IdEstadoNavigation)
            .AsQueryable();

        var todos = consulta.ToList();
        ViewBag.Referencias = todos.Count;
        ViewBag.Bajos = todos.Count(i => EsBajo(i));
        ViewBag.Agotados = todos.Count(i => (i.StockActual ?? 0) <= 0);
        ViewBag.Unidades = todos.Sum(i => i.StockActual ?? 0);

        ViewBag.Buscar = buscar;
        ViewBag.Filtro = filtro;
        return View(Aplicar(todos, buscar, filtro));
    }

    public IActionResult Exportar(string? buscar, string? filtro)
    {
        var todos = _db.Inventarios.AsNoTracking()
            .Include(i => i.IdProductoNavigation).Include(i => i.IdBodegaNavigation).Include(i => i.IdEstadoNavigation).ToList();
        var partes = new List<string> { $"Corte al {DateTime.Now:dd/MM/yyyy HH:mm}" };
        if (filtro == "bajo") partes.Add("solo stock bajo");
        if (!string.IsNullOrWhiteSpace(buscar)) partes.Add($"búsqueda \"{buscar.Trim()}\"");
        return this.ExcelArchivo("inventario", ReporteController.HojaInventario(Aplicar(todos, buscar, filtro), string.Join(" · ", partes)));
    }

    private static List<Inventario> Aplicar(IEnumerable<Inventario> lista, string? buscar, string? filtro)
    {
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var q = buscar.Trim();
            lista = lista.Where(i => i.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (i.IdProductoNavigation?.Nombre ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (i.IdBodegaNavigation?.NombreBodega ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        if (filtro == "bajo") lista = lista.Where(EsBajo);
        return lista.OrderBy(i => i.IdProductoNavigation?.Nombre).ThenBy(i => i.Codigo).ToList();
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create(int? idProducto)
    {
        CargarCombos();
        return View(new Inventario { IdProducto = idProducto ?? 0, StockMinimo = 4, StockActual = 0, FechaIngreso = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Registró inventario", "Inventario" })]
    public IActionResult Create(Inventario i)
    {
        Validar(i);
        if (!ModelState.IsValid)
        {
            CargarCombos(i);
            return View(i);
        }

        i.Codigo = i.Codigo.Trim().ToUpperInvariant();
        i.CantidadIngresada = i.StockActual ?? 0;
        i.FechaIngreso ??= DateOnly.FromDateTime(DateTime.Today);
        i.IdEstado = EstadoSegunStock(i.StockActual ?? 0);
        _db.Inventarios.Add(i);
        _db.SaveChanges();
        TempData["Exito"] = "Existencias registradas.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Edit(int id)
    {
        var i = _db.Inventarios.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (i == null) return NotFound();
        CargarCombos(i);
        return View(i);
    }

    // El stock no se edita aquí: se cambia con entradas o con las ventas, para que quede trazabilidad.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó inventario", "Inventario" })]
    public IActionResult Edit(int id, Inventario i)
    {
        var original = _db.Inventarios.Find(id);
        if (original == null) return NotFound();

        i.Id = id;
        i.StockActual = original.StockActual;
        Validar(i);
        if (!ModelState.IsValid)
        {
            CargarCombos(i);
            return View(i);
        }

        original.Codigo = i.Codigo.Trim().ToUpperInvariant();
        original.IdProducto = i.IdProducto;
        original.IdBodega = i.IdBodega;
        original.StockMinimo = i.StockMinimo;
        original.IdEstado = i.IdEstado;
        _db.SaveChanges();
        TempData["Exito"] = "Registro actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // Entrada de mercancía: suma unidades y registra la fecha.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Registró entrada de mercancía", "Inventario" })]
    public IActionResult Entrada(int id, int cantidad)
    {
        var i = _db.Inventarios.Find(id);
        if (i == null) return NotFound();
        if (cantidad <= 0 || cantidad > 100000)
        {
            TempData["Error"] = "Indica una cantidad válida.";
            return RedirectToAction(nameof(Index));
        }

        i.StockActual = (i.StockActual ?? 0) + cantidad;
        i.CantidadIngresada += cantidad;
        i.FechaIngreso = DateOnly.FromDateTime(DateTime.Today);
        if (i.IdEstado == StockHelper.IdEstado(_db, UsuariosIniciales.Estados.Agotado))
            i.IdEstado = EstadoSegunStock(i.StockActual.Value);
        _db.SaveChanges();
        TempData["Exito"] = $"Se sumaron {cantidad} unidades. Stock actual: {i.StockActual}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó inventario", "Inventario" })]
    public IActionResult Delete(int id)
    {
        var i = _db.Inventarios.Find(id);
        if (i != null)
        {
            _db.Inventarios.Remove(i);
            _db.SaveChanges();
            TempData["Exito"] = "Registro eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static bool EsBajo(Inventario i) => i.StockActual.HasValue && i.StockMinimo.HasValue && i.StockActual <= i.StockMinimo;

    private int EstadoSegunStock(int stock)
        => StockHelper.IdEstado(_db, stock > 0 ? UsuariosIniciales.Estados.Disponible : UsuariosIniciales.Estados.Agotado);

    private void Validar(Inventario i)
    {
        i.Codigo = (i.Codigo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(i.Codigo)) ModelState.AddModelError(nameof(Inventario.Codigo), "El código es obligatorio.");
        else if (_db.Inventarios.AsNoTracking().Any(x => x.Codigo == i.Codigo && x.Id != i.Id))
            ModelState.AddModelError(nameof(Inventario.Codigo), "Ya existe un registro con ese código.");
        if (i.IdProducto <= 0) ModelState.AddModelError(nameof(Inventario.IdProducto), "Selecciona un producto.");
        if (i.IdBodega <= 0) ModelState.AddModelError(nameof(Inventario.IdBodega), "Selecciona una bodega.");
        if ((i.StockActual ?? 0) < 0) ModelState.AddModelError(nameof(Inventario.StockActual), "El stock no puede ser negativo.");
        if ((i.StockMinimo ?? 0) < 0) ModelState.AddModelError(nameof(Inventario.StockMinimo), "El mínimo no puede ser negativo.");
    }

    private void CargarCombos(Inventario? i = null)
    {
        ViewBag.IdProducto = new SelectList(_db.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToList(), "Id", "Nombre", i?.IdProducto);
        ViewBag.IdBodega = new SelectList(_db.Bodegas.AsNoTracking().OrderBy(b => b.NombreBodega).ToList(), "Id", "NombreBodega", i?.IdBodega);
        ViewBag.IdEstado = new SelectList(_db.EstadoInventarios.AsNoTracking().ToList(), "Id", "Nombre", i?.IdEstado);
    }
}
