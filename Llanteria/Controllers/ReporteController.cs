using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Llanteria.Models;
using Llanteria.Data;

public class ReporteController : Controller
{
    private readonly LlanteriaDbContext _context;
    public ReporteController(LlanteriaDbContext context) { _context = context; }

    // Reporte Individual (ej: /Reporte/GenerarIndividual?tipo=Factura)
    public IActionResult GenerarIndividual(string tipo)
    {
        object datos = null;
        string vista = "";

        switch (tipo)
        {
            case "Factura":
                datos = _context.Facturas.Include(f => f.IdClienteNavigation).ToList();
                vista = "ReporteFacturas";
                break;
            case "Gasto":
                datos = _context.Gastos.Include(g => g.IdCategoriaNavigation).ToList();
                vista = "ReporteGastos";
                break;
            case "Inventario":
                datos = _context.Inventarios.Include(i => i.IdProductoNavigation).ToList();
                vista = "ReporteInventario";
                break;
        }

        return new ViewAsPdf(vista, datos) { FileName = $"Reporte_{tipo}_{DateTime.Now:yyyyMMdd}.pdf" };
    }

    public IActionResult GenerarGeneral(DateTime fechaInicio, DateTime fechaFin)
    {
        // 1. Traemos los datos a memoria
        var listaFacturas = _context.Facturas.Include(f => f.IdClienteNavigation).ToList();
        var listaGastos = _context.Gastos.ToList();

        // 2. Filtramos comparando año, mes y día
        var model = new ReporteGeneralViewModel
        {
            Facturas = listaFacturas.Where(f => f.Fecha.HasValue &&
                                                // Creamos un objeto DateTime nuevo solo para comparar
                                                new DateTime(f.Fecha.Value.Year, f.Fecha.Value.Month, f.Fecha.Value.Day) >= fechaInicio.Date &&
                                                new DateTime(f.Fecha.Value.Year, f.Fecha.Value.Month, f.Fecha.Value.Day) <= fechaFin.Date)
                                    .ToList(),

            Gastos = listaGastos.Where(g => g.FechaRegistro.HasValue &&
                                            new DateTime(g.FechaRegistro.Value.Year, g.FechaRegistro.Value.Month, g.FechaRegistro.Value.Day) >= fechaInicio.Date &&
                                            new DateTime(g.FechaRegistro.Value.Year, g.FechaRegistro.Value.Month, g.FechaRegistro.Value.Day) <= fechaFin.Date)
                                .ToList()
        };

        return new ViewAsPdf("ReporteGeneral", model);
    }
}