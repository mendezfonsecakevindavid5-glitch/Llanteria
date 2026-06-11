using Llanteria.Models;
using Llanteria.Data; // Asegúrate de incluir el namespace de tu DbContext
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace Llanteria.Controllers
{
    public class HomeController : Controller
    {
        private readonly LlanteriaDbContext _context;

        // Inyectamos el contexto para acceder a DetalleProducto
        public HomeController(LlanteriaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Extraemos los valores únicos de la tabla DetalleProducto para llenar los selectores
            ViewBag.Anchos = _context.DetalleProductos
                .Where(d => d.Ancho != null)
                .Select(d => d.Ancho)
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            ViewBag.Perfiles = _context.DetalleProductos
                .Where(d => d.Perfil != null)
                .Select(d => d.Perfil)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            ViewBag.Diametros = _context.DetalleProductos
                .Where(d => d.Diametro != null)
                .Select(d => d.Diametro)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}