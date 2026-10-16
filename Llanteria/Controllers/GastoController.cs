using Llanteria.Data;
using Llanteria.Filters;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class GastoController : Controller
{
    private static readonly string[] Extensiones = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

    private readonly GastoService ser;
    private readonly LlanteriaDbContext _db;
    private readonly IWebHostEnvironment _env;

    public GastoController(GastoService gastoService, LlanteriaDbContext db, IWebHostEnvironment env)
    {
        ser = gastoService;
        _db = db;
        _env = env;
    }

    public IActionResult Index() => View(ser.GetGastos().OrderByDescending(g => g.FechaVencimiento).ToList());

    public IActionResult Exportar()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var lista = ser.GetGastos().OrderByDescending(g => g.FechaVencimiento).ToList();
        return this.ExcelArchivo("gastos", new TablaHoja
        {
            Nombre = "Gastos",
            Titulo = "Egresos y cuentas por pagar",
            Subtitulo = "Todos los gastos registrados",
            Columnas =
            {
                new("Registro", TipoCol.Fecha), new("Categoría"), new("Descripción"), new("Vence", TipoCol.Fecha),
                new("Pagado el", TipoCol.Fecha), new("Estado"), new("Monto", TipoCol.Moneda, true)
            },
            Filas = lista.Select(g => new object?[]
            {
                g.FechaRegistro, g.IdCategoriaNavigation?.Nombre, g.Descripcion, g.FechaVencimiento, g.FechaPago,
                g.Estado == "Pagado" ? "Pagado" : g.FechaVencimiento < hoy ? "Vencido" : "Pendiente", g.Monto
            }).ToList()
        });
    }

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Gasto { FechaVencimiento = DateOnly.FromDateTime(DateTime.Today), Estado = "Pendiente" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Registró un nuevo gasto", "Gasto" })]
    public IActionResult Create(Gasto g, IFormFile? soporte)
    {
        ValidarSoporte(soporte);
        if (!ModelState.IsValid)
        {
            CargarCombos(g.IdCategoria);
            return View(g);
        }

        g.FechaRegistro = DateTime.Now;
        if (soporte != null) g.ArchivoSupport = GuardarSoporte(soporte);
        ser.AddGasto(g);
        TempData["Exito"] = "Gasto registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var g = ser.GetGasto(id);
        if (g == null) return NotFound();
        CargarCombos(g.IdCategoria);
        return View(g);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó un gasto", "Gasto" })]
    public IActionResult Edit(int id, Gasto g, IFormFile? soporte)
    {
        ValidarSoporte(soporte);
        var original = ser.GetGasto(id);
        if (original == null) return NotFound();
        if (!ModelState.IsValid)
        {
            g.Id = id;
            g.ArchivoSupport = original.ArchivoSupport;
            CargarCombos(g.IdCategoria);
            return View(g);
        }

        // Se copian solo los campos editables sobre la entidad ya cargada.
        original.IdCategoria = g.IdCategoria;
        original.Descripcion = g.Descripcion;
        original.Monto = g.Monto;
        original.FechaVencimiento = g.FechaVencimiento;
        original.FechaPago = g.FechaPago;
        original.Estado = g.Estado;
        if (soporte != null)
        {
            EliminarSoporte(original.ArchivoSupport);
            original.ArchivoSupport = GuardarSoporte(soporte);
        }
        _db.SaveChanges();
        TempData["Exito"] = "Gasto actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó un gasto", "Gasto" })]
    public IActionResult Delete(int id)
    {
        var g = ser.GetGasto(id);
        if (g != null)
        {
            var archivo = g.ArchivoSupport;
            ser.DeleteGasto(id);
            EliminarSoporte(archivo);
            TempData["Exito"] = "Gasto eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }

    private void CargarCombos(int? seleccionada = null)
        => ViewBag.IdCategoria = new SelectList(_db.CategoriaGastos.OrderBy(c => c.Nombre).ToList(), "Id", "Nombre", seleccionada);

    private void ValidarSoporte(IFormFile? archivo)
    {
        if (archivo == null) return;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            ModelState.AddModelError("soporte", "El soporte debe ser PDF o imagen.");
        else if (archivo.Length > 5 * 1024 * 1024)
            ModelState.AddModelError("soporte", "El soporte no puede superar 5 MB.");
    }

    private string GuardarSoporte(IFormFile archivo)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "gastos");
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + Path.GetExtension(archivo.FileName).ToLowerInvariant();
        using var fs = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
        archivo.CopyTo(fs);
        return nombre;
    }

    private void EliminarSoporte(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return;
        var ruta = Path.Combine(_env.WebRootPath, "uploads", "gastos", Path.GetFileName(nombre));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }
}
