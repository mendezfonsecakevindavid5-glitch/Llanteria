using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class ReseniasController : Controller
{
    private readonly LlanteriaDbContext _db;
    public ReseniasController(LlanteriaDbContext db) => _db = db;

    // Solo clientes registrados califican (evita spam). Una por cliente: si vuelve a opinar, se actualiza.
    [HttpPost, Authorize(Roles = "Cliente"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int calificacion, string? comentario)
    {
        comentario = comentario?.Trim();
        if (calificacion < 1 || calificacion > 5 || string.IsNullOrWhiteSpace(comentario) || comentario.Length < 10)
        {
            TempData["Error"] = "Elige de 1 a 5 estrellas y cuéntanos tu experiencia (mínimo 10 caracteres).";
            return Redirect("/#opiniones");
        }
        if (comentario.Length > 400) comentario = comentario[..400];

        if (!int.TryParse(User.FindFirstValue("ClienteId"), out var idCliente)) return Forbid();
        var nombre = (User.Identity?.Name ?? "Cliente").Split(' ')[0];

        var r = await _db.Resenias.FirstOrDefaultAsync(x => x.IdCliente == idCliente);
        if (r == null) _db.Resenias.Add(new Resenia { IdCliente = idCliente, Nombre = nombre, Calificacion = calificacion, Comentario = comentario });
        else { r.Calificacion = calificacion; r.Comentario = comentario; r.Nombre = nombre; r.Fecha = DateTime.Now; r.Visible = true; }
        await _db.SaveChangesAsync();

        TempData["Exito"] = "¡Gracias por tu calificación!";
        return Redirect("/#opiniones");
    }

    // ---- Moderación (panel) ----
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Index()
        => View(await _db.Resenias.AsNoTracking().OrderByDescending(r => r.Fecha).ToListAsync());

    [HttpPost, Authorize(Roles = "Administrador,Empleado"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(int id)
    {
        var r = await _db.Resenias.FindAsync(id);
        if (r != null) { r.Visible = !r.Visible; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Administrador,Empleado"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await _db.Resenias.FindAsync(id);
        if (r != null) { _db.Resenias.Remove(r); await _db.SaveChangesAsync(); TempData["Exito"] = "Opinión eliminada."; }
        return RedirectToAction(nameof(Index));
    }
}
