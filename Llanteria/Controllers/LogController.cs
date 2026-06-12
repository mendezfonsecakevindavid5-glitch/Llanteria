using Microsoft.AspNetCore.Mvc;
using Llanteria.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Llanteria.Controllers
{
    public class LogController : Controller
    {
        private readonly LlanteriaDbContext _context;

        public LogController(LlanteriaDbContext context)
        {
            _context = context;
        }

        // Vista de Auditoría para el Administrador
        public IActionResult Index()
        {
            // Cargamos el log incluyendo la información del usuario que realizó la acción
            var logs = _context.LogActividads
                .Include(l => l.IdUsuarioNavigation)
                .OrderByDescending(l => l.FechaRegistro)
                .ToList();

            return View(logs);
        }
    }
}