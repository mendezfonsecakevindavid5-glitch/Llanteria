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
        object datos = null;
        string vista = "";

        switch (tipo)
        {
            case "Factura":
                datos = _context.Facturas.Include(f => f.IdClienteNavigation).OrderByDescending(f => f.Fecha).ToList();
                vista = "ReporteFacturas";
                break;
            case "Gasto":
                datos = _context.Gastos.OrderByDescending(g => g.FechaRegistro).ToList();
                vista = "ReporteGastos";
                break;
            case "Inventario":
                datos = _context.Inventarios.Include(i => i.IdProductoNavigation).ToList();
                vista = "ReporteInventario";
                break;
            default:
                return RedirectToAction("Index", "Dashboard");
        }

        return new ViewAsPdf(vista, datos)
        {
            FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4
        };
    }

    public IActionResult GenerarGeneral(DateTime fechaInicio, DateTime fechaFin)
    {
        // 1. Valores por defecto
        if (fechaInicio == DateTime.MinValue) fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (fechaFin == DateTime.MinValue) fechaFin = DateTime.Now;

        // 2. Convertimos las entradas a tipos compatibles
        DateOnly inicioDO = DateOnly.FromDateTime(fechaInicio);
        DateOnly finDO = DateOnly.FromDateTime(fechaFin);

        // 3. Consulta filtrada directamente en la base de datos
        var model = new ReporteGeneralViewModel
        {
            // Factura usa DateOnly, comparamos con DateOnly
            Facturas = _context.Facturas
                .Include(f => f.IdClienteNavigation)
                .Where(f => f.Fecha.HasValue && f.Fecha.Value >= inicioDO && f.Fecha.Value <= finDO)
                .OrderByDescending(f => f.Fecha)
                .ToList(),

            // Gasto usa DateTime, comparamos con DateTime (usando .Date para ignorar la hora)
            Gastos = _context.Gastos
                .Where(g => g.FechaRegistro.HasValue && g.FechaRegistro.Value.Date >= fechaInicio.Date && g.FechaRegistro.Value.Date <= fechaFin.Date)
                .OrderByDescending(g => g.FechaRegistro)
                .ToList()
        };

        // 4. Retornamos usando tu vista de diseño profesional
        return new ViewAsPdf("ReporteGeneral", model)
        {
            FileName = $"Reporte_General_{fechaInicio:yyyyMMdd}_al_{fechaFin:yyyyMMdd}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            CustomSwitches = "--viewport-size 1280x1024 --print-media-type --footer-right [page]/[toPage]"
        };
    }
}