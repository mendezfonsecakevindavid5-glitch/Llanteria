using System.Security.Claims;
using Llanteria.Services;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class ReseniasController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly IPerfilService _perfil;
    public ReseniasController(LlanteriaDbContext db, IPerfilService perfil) { _db = db; _perfil = perfil; }

    /// <summary>Foto de perfil de quien escribió una reseña visible (la tarjeta del inicio la pide aquí; 404 si no tiene y se muestran las iniciales).</summary>
    [HttpGet]
    public async Task<IActionResult> Foto(int id)
    {
        var r = await _db.Resenias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Visible);
        if (r == null) return NotFound();

        var idUsuario = r.IdUsuario
            ?? await _db.Usuarios.AsNoTracking().Where(u => u.IdCliente == r.IdCliente).Select(u => (int?)u.Id).FirstOrDefaultAsync();
        if (idUsuario == null) return NotFound();

        var foto = await _perfil.ObtenerFotoAsync(idUsuario.Value);
        if (foto is not { } f) return NotFound();

        Response.Headers.CacheControl = "public, max-age=300";
        return File(f.Datos, f.Tipo);
    }

    // Solo clientes registrados califican (evita spam). Una por cliente: si vuelve a opinar, se actualiza.
    [HttpPost, Authorize(Roles = "Cliente"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int calificacion, string? comentario)
    {
        comentario = comentario?.Trim();
        if (calificacion < 1 || calificacion > 5 || string.IsNullOrWhiteSpace(comentario) || comentario.Length < 10)
        {
            TempData["Error"] = "Elige de 1 a 5 llantas y cuéntanos tu experiencia (mínimo 10 caracteres).";
            return Redirect("/#opiniones");
        }
        if (comentario.Length > 400) comentario = comentario[..400];

        if (!int.TryParse(User.FindFirstValue("ClienteId"), out var idCliente)) return Forbid();
        var usuario = User.Identity?.Name ?? "Cliente";
        var nombre = usuario.Split(' ')[0];
        int? idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var iu) ? iu : null;

        var r = await _db.Resenias.FirstOrDefaultAsync(x => x.IdCliente == idCliente);
        if (r == null) _db.Resenias.Add(new Resenia { IdCliente = idCliente, Nombre = nombre, Usuario = usuario, IdUsuario = idUsuario, Calificacion = calificacion, Comentario = comentario });
        else { r.Calificacion = calificacion; r.Comentario = comentario; r.Nombre = nombre; r.Usuario = usuario; r.IdUsuario = idUsuario; r.Fecha = DateTime.Now; r.Visible = true; }
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
