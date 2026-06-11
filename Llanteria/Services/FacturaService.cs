using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class FacturaService
{
    private readonly LlanteriaDbContext _context;

    public FacturaService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<Factura> GetFacturas()
    {
        // ✅ CORREGIDO: Ahora incluye la navegación hacia el Cliente
        return _context.Facturas
            .Include(f => f.IdClienteNavigation)
            .ToList();
    }

    public Factura? GetFactura(int id)
    {
        // ✅ CORREGIDO: Ahora incluye el Cliente e incluye el detalle de productos cobrados
        return _context.Facturas
            .Include(f => f.IdClienteNavigation)
            .Include(f => f.DetalleFacturas)
            .FirstOrDefault(f => f.Id == id);
    }

    public void AddFactura(Factura obj)
    {
        _context.Facturas.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateFactura(Factura obj)
    {
        _context.Facturas.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteFactura(int id)
    {
        var obj = _context.Facturas.Find(id);
        if (obj != null)
        {
            _context.Facturas.Remove(obj);
            _context.SaveChanges();
        }
    }
}