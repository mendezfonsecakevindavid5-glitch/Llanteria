using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Llanteria.Models;
using Llanteria.Data;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador")]
public class ReporteController : Controller
{
    private readonly LlanteriaDbContext _context;
    public ReporteController(LlanteriaDbContext context) { _context = context; }

    public IActionResult GenerarIndividual(string tipo)
    {
        // Preparar un modelo que la vista existente (ReporteFacturas) espera
        // Para evitar excepciones de tipo, normalizamos el modelo que pasamos a la vista.
        string vista;

        switch (tipo)
        {
            case "Factura":
                var facturas = _context.Facturas.Include(f => f.IdClienteNavigation).OrderByDescending(f => f.Fecha).ToList();
                // La vista ReporteFacturas espera un ReporteGeneralViewModel
                var modelFact = new Llanteria.Models.ReporteGeneralViewModel
                {
                    Facturas = facturas,
                    // Incluir los gastos también para que los totales y el resumen funcionen
                    Gastos = _context.Gastos.OrderByDescending(g => g.FechaRegistro).ToList()
                };
                vista = "ReporteFacturas";
                return new ViewAsPdf(vista, modelFact)
                {
                    FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4
                };

            case "Gasto":
                var gastos = _context.Gastos.OrderByDescending(g => g.FechaRegistro).ToList();
                var modelGasto = new Llanteria.Models.ReporteGeneralViewModel
                {
                    Gastos = gastos,
                    Facturas = _context.Facturas.OrderByDescending(f => f.Fecha).ToList()
                };
                // Reutilizamos la vista ReporteFacturas (ya que muestra ambos conjuntos)
                vista = "ReporteFacturas";
                return new ViewAsPdf(vista, modelGasto)
                {
                    FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4
                };

            case "Inventario":
                var inventario = _context.Inventarios.Include(i => i.IdProductoNavigation).ToList();
                // Para inventario devolvemos una vista específica (la creamos si no existe)
                vista = "ReporteInventario";
                return new ViewAsPdf(vista, inventario)
                {
                    FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4
                };

            default:
                return RedirectToAction("Index", "Dashboard");
        }
    }

    public IActionResult GenerarGeneral(DateTime fechaInicio, DateTime fechaFin)
    {
        // 1. Valores por defecto
        if (fechaInicio == DateTime.MinValue) fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (fechaFin == DateTime.MinValue) fechaFin = DateTime.Now;

        // Ajustar fechaFin al final del día (23:59:59)
        DateTime fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

        // Convertir a DateOnly
        DateOnly inicioDO = DateOnly.FromDateTime(fechaInicio);
        DateOnly finDO = DateOnly.FromDateTime(fechaFinAjustada);

        // 2. Consulta filtrada
        var model = new ReporteGeneralViewModel
        {
            Facturas = _context.Facturas
                .Include(f => f.IdClienteNavigation)
                .Where(f => f.Fecha.HasValue && f.Fecha.Value >= inicioDO && f.Fecha.Value <= finDO)
                .OrderByDescending(f => f.Fecha)
                .ToList(),

            Gastos = _context.Gastos
                .Where(g => g.FechaRegistro.HasValue && g.FechaRegistro.Value >= fechaInicio.Date && g.FechaRegistro.Value <= fechaFinAjustada)
                .OrderByDescending(g => g.FechaRegistro)
                .ToList()
        };

        // 3. Retorno del PDF
        return new ViewAsPdf("ReporteGeneral", model)
        {
            FileName = $"Reporte_General_{fechaInicio:yyyyMMdd}_al_{fechaFin:yyyyMMdd}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            CustomSwitches = "--viewport-size 1280x1024 --print-media-type --footer-right [page]/[toPage]"
        };
    }
}