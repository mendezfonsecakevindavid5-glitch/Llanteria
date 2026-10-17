using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Gestión interna de PQRS (solo personal; el acceso lo controla RoleConvention).</summary>
public class PqrsController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;

    public PqrsController(LlanteriaDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public IActionResult Index(string? estado, string? tipo, string? buscar, int pagina = 1)
    {
        ViewBag.Estado = estado;
        ViewBag.Tipo = tipo;
        ViewBag.Buscar = buscar;
        ViewBag.Pendientes = _db.RegistrosPqrs.Count(x => x.Estado != Pqrs.Resuelta);
        var hoy = DateTime.Today;
        var abiertas = _db.RegistrosPqrs.AsNoTracking().Where(x => x.Estado != Pqrs.Resuelta).Select(x => x.FechaCreacion).ToList();
        ViewBag.Vencidas = abiertas.Count(f => new Pqrs { FechaCreacion = f }.FechaLimite < hoy);

        var q = _db.RegistrosPqrs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && Pqrs.Estados.Contains(estado)) q = q.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(tipo) && Pqrs.Tipos.Contains(tipo)) q = q.Where(x => x.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(x => x.Radicado.Contains(t) || x.Nombre.Contains(t) || x.Asunto.Contains(t) || x.Correo.Contains(t));
        }
        return View(Paginado<Pqrs>.De(q.OrderBy(x => x.Estado == Pqrs.Resuelta ? 1 : 0).ThenByDescending(x => x.FechaCreacion), pagina));
    }

    [HttpGet]
    public IActionResult Detalle(int id)
    {
        var p = _db.RegistrosPqrs.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(int id, string estado, string? respuesta)
    {
        var p = _db.RegistrosPqrs.Find(id);
        if (p == null) return NotFound();
        if (!Pqrs.Estados.Contains(estado))
        {
            TempData["Error"] = "Estado no válido.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        respuesta = string.IsNullOrWhiteSpace(respuesta) ? null : respuesta.Trim();
        if (respuesta?.Length > 1500)
        {
            TempData["Error"] = "La respuesta no puede superar 1.500 caracteres.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        if (estado == Pqrs.Resuelta && respuesta == null)
        {
            TempData["Error"] = "Escribe la respuesta antes de marcarla como resuelta.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var yaResuelta = p.Estado == Pqrs.Resuelta;
        p.Estado = estado;
        p.Respuesta = respuesta;
        if (estado == Pqrs.Resuelta && !yaResuelta) p.FechaRespuesta = DateTime.Now;
        _db.SaveChanges();

        if (estado == Pqrs.Resuelta && !yaResuelta)
        {
            try
            {
                var cuerpo = EmailService.PlantillaAviso($"Respuesta a tu {p.Tipo.ToLower()}", new (string, string?)[]
                {
                    ("Radicado", p.Radicado), ("Asunto", p.Asunto)
                }, p.Respuesta);
                await _email.EnviarCorreoAsync(p.Correo, p.Nombre, $"Respuesta a tu solicitud {p.Radicado}", cuerpo);
            }
            catch { /* se ignora a propósito */ }
        }

        TempData["Exito"] = $"Solicitud {p.Radicado} actualizada.";
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
