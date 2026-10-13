using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Llanteria.Controllers;

[Authorize(Roles = "Administrador")]
public class ReporteController : Controller
{
    private readonly LlanteriaDbContext _context;

    public ReporteController(LlanteriaDbContext context) => _context = context;

    // tipo: Factura | Gasto | Inventario
    public IActionResult GenerarIndividual(string tipo)
    {
        switch (tipo)
        {
            case "Factura":
                return Pdf("ReporteFacturas", new ReporteGeneralViewModel
                {
                    Titulo = "Reporte de ventas",
                    Facturas = _context.Facturas.Include(f => f.IdClienteNavigation).OrderByDescending(f => f.Fecha).ToList(),
                    Gastos = new List<Gasto>()
                }, tipo);

            case "Gasto":
                return Pdf("ReporteFacturas", new ReporteGeneralViewModel
                {
                    Titulo = "Reporte de gastos",
                    Facturas = new List<Factura>(),
                    Gastos = _context.Gastos.Include(g => g.IdCategoriaNavigation).OrderByDescending(g => g.FechaRegistro).ToList()
                }, tipo);

            case "Inventario":
                var inventario = _context.Inventarios
                    .Include(i => i.IdProductoNavigation)
                    .Include(i => i.IdBodegaNavigation)
                    .Include(i => i.IdEstadoNavigation)
                    .OrderBy(i => i.IdProductoNavigation.Nombre)
                    .ToList();
                return Pdf("ReporteInventario", inventario, tipo);

            default:
                return RedirectToAction("Index", "Dashboard");
        }
    }

    public IActionResult GenerarGeneral(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var fin = (fechaFin ?? DateTime.Today).Date;
        if (fin < inicio) (inicio, fin) = (fin, inicio);

        var inicioDO = DateOnly.FromDateTime(inicio);
        var finDO = DateOnly.FromDateTime(fin);
        var finExclusivo = fin.AddDays(1);

        var model = new ReporteGeneralViewModel
        {
            Titulo = "Reporte general",
            Periodo = $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}",
            Facturas = _context.Facturas.Include(f => f.IdClienteNavigation)
                .Where(f => f.Fecha.HasValue && f.Fecha.Value >= inicioDO && f.Fecha.Value <= finDO)
                .OrderByDescending(f => f.Fecha).ToList(),
            Gastos = _context.Gastos.Include(g => g.IdCategoriaNavigation)
                .Where(g => g.FechaRegistro.HasValue && g.FechaRegistro.Value >= inicio && g.FechaRegistro.Value < finExclusivo)
                .OrderByDescending(g => g.FechaRegistro).ToList()
        };

        return new ViewAsPdf("ReporteFacturas", model)
        {
            FileName = $"Reporte_General_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf",
            PageSize = Size.A4,
            PageOrientation = Orientation.Portrait,
            CustomSwitches = "--print-media-type --footer-right [page]/[toPage]"
        };
    }

    private ViewAsPdf Pdf(string vista, object modelo, string tipo) => new(vista, modelo)
    {
        FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf",
        PageSize = Size.A4
    };
}
