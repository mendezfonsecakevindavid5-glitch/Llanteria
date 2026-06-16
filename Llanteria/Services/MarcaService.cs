using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class MarcaService
{
    private readonly LlanteriaDbContext _context;

    public MarcaService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<Marca> GetMarcas()
    {
        return _context.Marcas.ToList();
    }

    public Marca? GetMarca(int id)
    {
        return _context.Marcas.Find(id);
    }

    public void AddMarca(Marca obj)
    {
        _context.Marcas.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateMarca(Marca obj)
    {
        _context.Marcas.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteMarca(int id)
    {
        var obj = _context.Marcas.Find(id);
        if (obj != null)
        {
            _context.Marcas.Remove(obj);
            _context.SaveChanges();
        }
    }
}