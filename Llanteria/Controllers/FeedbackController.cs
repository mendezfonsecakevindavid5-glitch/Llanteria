using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Llanteria.Controllers;

/// <summary>Sección pública donde cualquier visitante puede dejar sugerencias para mejorar el sitio. La bandeja es SugerenciasController.</summary>
public class FeedbackController : Controller
{
    private readonly LlanteriaDbContext _db;

    public FeedbackController(LlanteriaDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Index() => View(new Sugerencia());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(Sugerencia s)
    {
        if (!Sugerencia.Areas.Contains(s.Area ?? ""))
            ModelState.AddModelError(nameof(Sugerencia.Area), "Elige una de las áreas de la lista.");
        if (!string.IsNullOrWhiteSpace(s.Correo) && !Validaciones.CorreoValido(s.Correo))
            ModelState.AddModelError(nameof(Sugerencia.Correo), "Correo no válido.");
        if (!ModelState.IsValid) return View(s);

        _db.Sugerencias.Add(new Sugerencia
        {
            Area = s.Area!,
            Calificacion = s.Calificacion,
            Mensaje = s.Mensaje.Trim(),
            Nombre = string.IsNullOrWhiteSpace(s.Nombre) ? null : s.Nombre.Trim(),
            Correo = string.IsNullOrWhiteSpace(s.Correo) ? null : s.Correo.Trim(),
            IdUsuario = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null
        });
        _db.SaveChanges();

        TempData["Exito"] = "Gracias por tu sugerencia. La revisaremos para mejorar el sitio.";
        return RedirectToAction(nameof(Index));
    }
}
