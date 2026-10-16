using Llanteria.Data;
using Llanteria.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class CarritoController : Controller
{
    private readonly LlanteriaDbContext _db;

    public CarritoController(LlanteriaDbContext db) => _db = db;

    public IActionResult Index()
    {
        var r = ResumenCompra.Armar(_db, Carrito.Leer(Request), Carrito.Cupon(Request));
        if (r.CuponInvalido)
        {
            Carrito.QuitarCupon(Response);
            TempData["Error"] = "El cupón ya no es válido.";
        }
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Agregar(int id, int cantidad = 1)
    {
        cantidad = Math.Clamp(cantidad, 1, Carrito.MaxPorLinea);
        var p = _db.Productos.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (p == null) return NotFound();

        var cart = Carrito.Leer(Request);
        var disp = Carrito.Disponibilidad(_db, new[] { id })[id];
        var nuevo = cart.GetValueOrDefault(id) + cantidad;

        if (disp.HasValue && nuevo > disp.Value)
        {
            if (disp.Value <= 0)
            {
                TempData["Error"] = $"«{p.Nombre}» está agotado por ahora.";
                return Volver();
            }
            nuevo = disp.Value;
            TempData["Error"] = $"Solo hay {disp.Value} unidades de «{p.Nombre}»; las agregamos todas a tu carrito.";
        }
        else
        {
            TempData["Exito"] = $"«{p.Nombre}» se agregó a tu carrito.";
        }

        cart[id] = Math.Min(nuevo, Carrito.MaxPorLinea);
        Carrito.Guardar(Request, Response, cart);
        return Volver();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Actualizar(int id, int cantidad)
    {
        var cart = Carrito.Leer(Request);
        if (!cart.ContainsKey(id)) return RedirectToAction(nameof(Index));

        if (cantidad <= 0) cart.Remove(id);
        else
        {
            var disp = Carrito.Disponibilidad(_db, new[] { id })[id];
            cart[id] = Math.Min(cantidad, disp.HasValue ? Math.Max(disp.Value, 1) : Carrito.MaxPorLinea);
        }
        Carrito.Guardar(Request, Response, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Quitar(int id)
    {
        var cart = Carrito.Leer(Request);
        cart.Remove(id);
        Carrito.Guardar(Request, Response, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Vaciar()
    {
        Carrito.Vaciar(Response);
        return RedirectToAction(nameof(Index));
    }

    // Código vacío = quitar el cupón.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cupon(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            Carrito.QuitarCupon(Response);
            return RedirectToAction(nameof(Index));
        }

        var r = ResumenCompra.Armar(_db, Carrito.Leer(Request), codigo);
        if (r.Cupon == null)
        {
            TempData["Error"] = "Ese código no existe, venció o no aplica a tu carrito.";
        }
        else
        {
            Carrito.GuardarCupon(Request, Response, r.Cupon);
            TempData["Exito"] = $"Cupón aplicado: {r.TituloCupon}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Vuelve a la página donde estaba el cliente (solo rutas del propio sitio).
    private IActionResult Volver()
    {
        var referer = Request.Headers.Referer.ToString();
        if (Uri.TryCreate(referer, UriKind.Absolute, out var uri) && uri.Host == Request.Host.Host)
            return LocalRedirect(uri.PathAndQuery);
        return RedirectToAction(nameof(Index));
    }
}
