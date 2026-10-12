using Microsoft.EntityFrameworkCore;
using Llanteria.Data;
using Llanteria.Models;

public class TipoServicioService
{
    private readonly LlanteriaDbContext _context;

    public TipoServicioService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<TipoServicio> GetTiposServicio()
    {
        return _context.TipoServicios.ToList();
    }

    public TipoServicio? GetTipoServicio(int id)
    {
        return _context.TipoServicios.AsNoTracking().FirstOrDefault(t => t.Id == id);
    }

    public void AddTipoServicio(TipoServicio obj)
    {
        _context.TipoServicios.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateTipoServicio(TipoServicio obj)
    {
        _context.TipoServicios.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteTipoServicio(int id)
    {
        var obj = _context.TipoServicios.Find(id);
        if (obj != null)
        {
            _context.TipoServicios.Remove(obj);
            _context.SaveChanges();
        }
    }
}