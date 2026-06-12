using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class BodegaService
{
    private readonly LlanteriaDbContext _context;

    public BodegaService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<Bodega> GetBodegas()
    {
        return _context.Bodegas.ToList();
    }

    public Bodega? GetBodega(int id)
    {
        return _context.Bodegas.Find(id);
    }

    public void AddBodega(Bodega b)
    {
        _context.Bodegas.Add(b);
        _context.SaveChanges();
    }

    public void UpdateBodega(Bodega b)
    {
        _context.Bodegas.Update(b);
        _context.SaveChanges();
    }

    public void DeleteBodega(int id)
    {
        var b = _context.Bodegas.Find(id);
        if (b != null)
        {
            _context.Bodegas.Remove(b);
            _context.SaveChanges();
        }
    }
}