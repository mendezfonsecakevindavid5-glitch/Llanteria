using Llanteria.Data;
using Llanteria.Models;

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

    public void AddBodega(Bodega obj)
    {
        _context.Bodegas.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateBodega(Bodega obj)
    {
        _context.Bodegas.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteBodega(int id)
    {
        var obj = _context.Bodegas.Find(id);
        if (obj != null)
        {
            _context.Bodegas.Remove(obj);
            _context.SaveChanges();
        }
    }
}