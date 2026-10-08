using Llanteria.Data;
using Llanteria.Models;
using System.Collections.Generic;
using System.Linq;

namespace Llanteria.Services
{
    public class PaisService
    {
        private readonly LlanteriaDbContext _context;

        public PaisService(LlanteriaDbContext context)
        {
            _context = context;
        }

        public List<Pais> GetPaises()
        {
            return _context.Paises.ToList();
        }
    }
}

