using Llanteria.Data;
using Llanteria.Filters;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Llanteria.Controllers;

public class FacturaController : Controller
{
    private const int ValorPorPunto = 1000; // 1 punto por cada $1.000 pagados

    private readonly LlanteriaDbContext _context;

    public FacturaController(LlanteriaDbContext context) => _context = context;

    public IActionResult Index(string? buscar, int pagina = 1)
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var delMes = _context.Facturas.AsNoTracking().Where(f => f.Fecha >= inicioMes);
        ViewBag.VendidoMes = delMes.Sum(f => (decimal?)f.TotalPagar) ?? 0;
        ViewBag.FacturasMes = delMes.Count();
        ViewBag.Buscar = buscar;
        return View(Paginado<Factura>.De(Filtrar(buscar), pagina));
    }

    public IActionResult Exportar(string? buscar)
    {
        var lista = Filtrar(buscar).ToList();
        var filtro = string.IsNullOrWhiteSpace(buscar) ? "Todas las facturas" : $"Filtro: \"{buscar.Trim()}\"";
        return this.ExcelArchivo("ventas", new TablaHoja
        {
            Nombre = "Ventas",
            Titulo = "Listado de ventas",
            Subtitulo = filtro,
            Columnas =
            {
                new("N.º", TipoCol.Entero), new("Fecha", TipoCol.Fecha), new("Cliente"), new("Documento"),
                new("Subtotal", TipoCol.Moneda, true), new("IVA", TipoCol.Moneda, true),
                new("Retención", TipoCol.Moneda, true), new("Total", TipoCol.Moneda, true), new("Observaciones")
            },
            Filas = lista.Select(f => new object?[]
            {
                f.Id, f.Fecha, $"{f.IdClienteNavigation?.Nombres} {f.IdClienteNavigation?.Apellidos}".Trim(),
                f.IdClienteNavigation?.NumeroDocumento, f.TotalBruto, f.Iva, f.RteFte ?? 0, f.TotalPagar, f.Observaciones
            }).ToList()
        });
    }

    // Factura individual en PDF (botón "Descargar PDF" del detalle de la factura).
    public IActionResult Pdf(int id)
    {
        var f = _context.Facturas.AsNoTracking()
            .Include(x => x.IdClienteNavigation).ThenInclude(c => c.IdDocumentoNavigation)
            .Include(x => x.DetalleFacturas)
            .FirstOrDefault(x => x.Id == id);
        if (f == null) return NotFound();

        return new ViewAsPdf("FacturaPdf", f)
        {
            FileName = $"Factura_{f.Id:D6}.pdf",
            PageSize = Size.A4,
            PageOrientation = Orientation.Portrait,
            PageMargins = new Margins(16, 14, 18, 14),
            CustomSwitches = "--print-media-type --footer-font-size 7 --footer-spacing 5 --footer-line " +
                             "--footer-left \"[title]\" --footer-right \"[page] / [topage]\""
        };
    }

    private IQueryable<Factura> Filtrar(string? buscar)
    {
        var q = _context.Facturas.AsNoTracking().Include(f => f.IdClienteNavigation).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim().TrimStart('#');
            var esNumero = int.TryParse(t, out var n);
            q = q.Where(f => (esNumero && f.Id == n)
                             || f.IdClienteNavigation.Nombres.Contains(t) || f.IdClienteNavigation.Apellidos.Contains(t));
        }
        return q.OrderByDescending(f => f.Fecha).ThenByDescending(f => f.Id);
    }

    public IActionResult Details(int id)
    {
        var f = _context.Facturas.AsNoTracking()
            .Include(x => x.IdClienteNavigation)
            .Include(x => x.DetalleFacturas)
            .FirstOrDefault(x => x.Id == id);
        return f == null ? NotFound() : View(f);
    }

    public IActionResult Create(int? idCliente)
    {
        CargarDatos(idCliente);
        return View(new FacturaFormViewModel { Fecha = DateOnly.FromDateTime(DateTime.Today), IdCliente = idCliente ?? 0, Lineas = { new LineaFacturaVM() } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Creó nueva factura", "Factura" })]
    public IActionResult Create(FacturaFormViewModel vm)
    {
        var detalles = ConstruirDetalles(vm);
        if (!ModelState.IsValid)
        {
            CargarDatos(vm.IdCliente);
            return View(vm);
        }

        var f = new Factura { DetalleFacturas = detalles };
        Aplicar(f, vm);
        _context.Facturas.Add(f);

        var cliente = _context.Clientes.Find(f.IdCliente);
        if (cliente != null) cliente.PuntosAcumulados += Puntos(f.TotalPagar);

        _context.SaveChanges();
        TempData["Exito"] = $"Factura #{f.Id} registrada.";
        return RedirectToAction(nameof(Details), new { id = f.Id });
    }

    public IActionResult Edit(int id)
    {
        var f = _context.Facturas.AsNoTracking().Include(x => x.DetalleFacturas).FirstOrDefault(x => x.Id == id);
        if (f == null) return NotFound();

        var vm = new FacturaFormViewModel
        {
            Id = f.Id, Fecha = f.Fecha, IdCliente = f.IdCliente, Iva = f.Iva, RteFte = f.RteFte, Observaciones = f.Observaciones,
            Lineas = f.DetalleFacturas.Select(d => new LineaFacturaVM
            {
                Tipo = d.IdProducto.HasValue ? "P" : "S",
                IdItem = d.IdProducto ?? d.IdServicio ?? 0,
                Cantidad = d.Cantidad,
                ValorUnitario = d.ValorUnitario
            }).ToList()
        };
        if (vm.Lineas.Count == 0) vm.Lineas.Add(new LineaFacturaVM());
        ViewBag.SinDetalle = !f.DetalleFacturas.Any();
        ViewBag.TotalAnterior = f.TotalPagar;
        CargarDatos(f.IdCliente);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó una factura", "Factura" })]
    public IActionResult Edit(int id, FacturaFormViewModel vm)
    {
        var f = _context.Facturas.Include(x => x.DetalleFacturas).FirstOrDefault(x => x.Id == id);
        if (f == null) return NotFound();
        vm.Id = id;

        // Primero se devuelve al inventario lo que la factura había descontado; luego se aplica lo nuevo.
        foreach (var d in f.DetalleFacturas.Where(d => d.IdProducto.HasValue))
            StockHelper.Reponer(_context, d.IdProducto!.Value, d.Cantidad);

        var nuevos = ConstruirDetalles(vm);
        if (!ModelState.IsValid)
        {
            // Nada de lo anterior se guarda: el contexto se descarta al terminar la petición.
            CargarDatos(vm.IdCliente);
            return View(vm);
        }

        var anterior = _context.Clientes.Find(f.IdCliente);
        if (anterior != null) anterior.PuntosAcumulados = Math.Max(0, anterior.PuntosAcumulados - Puntos(f.TotalPagar));

        _context.DetalleFacturas.RemoveRange(f.DetalleFacturas);
        f.DetalleFacturas = nuevos;
        Aplicar(f, vm);

        var nuevoCliente = _context.Clientes.Find(f.IdCliente);
        if (nuevoCliente != null) nuevoCliente.PuntosAcumulados += Puntos(f.TotalPagar);

        _context.SaveChanges();
        TempData["Exito"] = $"Factura #{id} actualizada.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó una factura", "Factura" })]
    public IActionResult Delete(int id)
    {
        var f = _context.Facturas.Include(x => x.DetalleFacturas).FirstOrDefault(x => x.Id == id);
        if (f == null) return RedirectToAction(nameof(Index));

        foreach (var d in f.DetalleFacturas.Where(d => d.IdProducto.HasValue))
            StockHelper.Reponer(_context, d.IdProducto!.Value, d.Cantidad);

        var cliente = _context.Clientes.Find(f.IdCliente);
        if (cliente != null) cliente.PuntosAcumulados = Math.Max(0, cliente.PuntosAcumulados - Puntos(f.TotalPagar));

        _context.DetalleFacturas.RemoveRange(f.DetalleFacturas);
        _context.Facturas.Remove(f);
        _context.SaveChanges();
        TempData["Exito"] = $"Factura #{id} eliminada; se devolvió el stock y se ajustaron los puntos.";
        return RedirectToAction(nameof(Index));
    }

    private static int Puntos(decimal total) => (int)(total / ValorPorPunto);

    /// <summary>Valida el formulario, arma las líneas y descuenta el stock en el contexto (sin guardar).</summary>
    private List<DetalleFactura> ConstruirDetalles(FacturaFormViewModel vm)
    {
        var detalles = new List<DetalleFactura>();

        if (vm.IdCliente <= 0 || !_context.Clientes.Any(c => c.Id == vm.IdCliente))
            ModelState.AddModelError(nameof(vm.IdCliente), "Selecciona un cliente válido.");

        var lineas = vm.Lineas.Where(l => l.IdItem > 0).ToList();
        if (lineas.Count == 0)
        {
            ModelState.AddModelError("", "Agrega al menos un producto o servicio.");
            return detalles;
        }

        foreach (var l in lineas)
        {
            if (l.Cantidad <= 0 || l.Cantidad > 100000) { ModelState.AddModelError("", "Las cantidades deben ser mayores a cero."); continue; }
            if (l.ValorUnitario < 0) { ModelState.AddModelError("", "El valor unitario no puede ser negativo."); continue; }

            if (l.Tipo == "S")
            {
                var s = _context.TipoServicios.AsNoTracking().FirstOrDefault(x => x.Id == l.IdItem);
                if (s == null) { ModelState.AddModelError("", "Hay un servicio que ya no existe."); continue; }
                detalles.Add(new DetalleFactura
                {
                    IdServicio = s.Id, CodigoItem = $"S-{s.Id}", Descripción = s.NombreServicio,
                    Cantidad = l.Cantidad, ValorUnitario = l.ValorUnitario, Subtotal = l.Cantidad * l.ValorUnitario
                });
            }
            else
            {
                var p = _context.Productos.AsNoTracking().FirstOrDefault(x => x.Id == l.IdItem);
                if (p == null) { ModelState.AddModelError("", "Hay un producto que ya no existe."); continue; }
                var error = StockHelper.Descontar(_context, p.Id, l.Cantidad, p.Nombre);
                if (error != null) { ModelState.AddModelError("", error); continue; }
                detalles.Add(new DetalleFactura
                {
                    IdProducto = p.Id, CodigoItem = $"P-{p.Id}", Descripción = p.Nombre,
                    Cantidad = l.Cantidad, ValorUnitario = l.ValorUnitario, Subtotal = l.Cantidad * l.ValorUnitario
                });
            }
        }

        var bruto = detalles.Sum(d => d.Subtotal);
        if (ModelState.IsValid && bruto + vm.Iva - (vm.RteFte ?? 0) <= 0)
            ModelState.AddModelError("", "El total a pagar debe ser mayor a cero.");

        return detalles;
    }

    // El total nunca se toma del navegador: se calcula aquí a partir de las líneas.
    private static void Aplicar(Factura f, FacturaFormViewModel vm)
    {
        f.Fecha = vm.Fecha;
        f.IdCliente = vm.IdCliente;
        f.TotalBruto = f.DetalleFacturas.Sum(d => d.Subtotal);
        f.Iva = vm.Iva;
        f.RteFte = vm.RteFte ?? 0;
        f.TotalPagar = f.TotalBruto + f.Iva - f.RteFte.Value;
        f.Observaciones = vm.Observaciones;
    }

    private void CargarDatos(int? idCliente)
    {
        var clientes = _context.Clientes.AsNoTracking().OrderBy(c => c.Nombres)
            .Select(c => new { c.Id, Nombre = c.Nombres + " " + c.Apellidos + " — " + c.NumeroDocumento }).ToList();
        ViewBag.IdCliente = new SelectList(clientes, "Id", "Nombre", idCliente);

        var stock = _context.Inventarios.AsNoTracking().GroupBy(i => i.IdProducto)
            .Select(g => new { IdProducto = g.Key, Total = g.Sum(i => i.StockActual ?? 0) }).ToDictionary(x => x.IdProducto, x => x.Total);
        ViewBag.Productos = _context.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToList()
            .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.PrecioVenta, stock = stock.TryGetValue(p.Id, out var s) ? (int?)s : null }).ToList();
        ViewBag.Servicios = _context.TipoServicios.AsNoTracking().OrderBy(s => s.NombreServicio).ToList()
            .Select(s => new { id = s.Id, nombre = s.NombreServicio, precio = s.ValorServicio }).ToList();
    }
}
