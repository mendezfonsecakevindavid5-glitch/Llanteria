using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services
{
    public class GastoService
    {
        private readonly LlanteriaDbContext _context;

        public GastoService(LlanteriaDbContext context)
        {
            _context = context;
        }

        public List<Gasto> GetGastos()
        {
            return _context.Gastos
                .Include(g => g.IdCategoriaNavigation)
                .ToList();
        }

        public Gasto? GetGasto(int id)
        {
            return _context.Gastos
                .Include(g => g.IdCategoriaNavigation)
                .FirstOrDefault(g => g.Id == id);
        }

        public void AddGasto(Gasto obj)
        {
            _context.Gastos.Add(obj);
            _context.SaveChanges();
        }

        public void UpdateGasto(Gasto obj)
        {
            _context.Gastos.Update(obj);
            _context.SaveChanges();
        }

        public void DeleteGasto(int id)
        {
            var obj = _context.Gastos.Find(id);
            if (obj != null)
            {
                _context.Gastos.Remove(obj);
                _context.SaveChanges();
            }
        }
    }
}
