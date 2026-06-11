using Llanteria.Data;
using Llanteria.Models;

public class CatalogoIncentivoService
{
    private readonly LlanteriaDbContext _context;

    public CatalogoIncentivoService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<CatalogoIncentivo> GetIncentivos()
    {
        return _context.CatalogoIncentivos.ToList();
    }

    public CatalogoIncentivo? GetIncentivo(int id)
    {
        return _context.CatalogoIncentivos.Find(id);
    }

    public void AddIncentivo(CatalogoIncentivo obj)
    {
        _context.CatalogoIncentivos.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateIncentivo(CatalogoIncentivo obj)
    {
        _context.CatalogoIncentivos.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteIncentivo(int id)
    {
        var obj = _context.CatalogoIncentivos.Find(id);
        if (obj != null)
        {
            _context.CatalogoIncentivos.Remove(obj);
            _context.SaveChanges();
        }
    }
}