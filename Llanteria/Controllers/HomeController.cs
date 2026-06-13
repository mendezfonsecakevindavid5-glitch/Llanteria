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

        public IActionResult Index(string ancho, string perfil, string diametro)
        {
            // Llenamos los ViewBag para los SelectList (esto es lo que ya tenías)
            ViewBag.Anchos = _context.DetalleProductos.Where(d => d.Ancho != null).Select(d => d.Ancho).Distinct().OrderBy(a => a).ToList();
            ViewBag.Perfiles = _context.DetalleProductos.Where(d => d.Perfil != null).Select(d => d.Perfil).Distinct().OrderBy(p => p).ToList();
            ViewBag.Diametros = _context.DetalleProductos.Where(d => d.Diametro != null).Select(d => d.Diametro).Distinct().OrderBy(d => d).ToList();

            // Lógica de búsqueda
            var productos = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(ancho) || !string.IsNullOrEmpty(perfil) || !string.IsNullOrEmpty(diametro))
            {
                productos = productos.Where(p => p.DetalleProducto != null);

                if (!string.IsNullOrEmpty(ancho)) productos = productos.Where(p => p.DetalleProducto.Ancho == ancho);
                if (!string.IsNullOrEmpty(perfil)) productos = productos.Where(p => p.DetalleProducto.Perfil == perfil);
                if (!string.IsNullOrEmpty(diametro)) productos = productos.Where(p => p.DetalleProducto.Diametro == diametro);

                // Retornamos la lista de resultados
                return View(productos.ToList());
            }

            // Si no hay búsqueda, enviamos una lista vacía
            return View(new List<Producto>());
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