using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

public class ServiciosController : Controller
{
    private readonly EmailService _email;
    private readonly LlanteriaDbContext _db;
    private readonly NegocioInfo _negocio;

    public ServiciosController(EmailService email, LlanteriaDbContext db, NegocioInfo negocio)
    {
        _email = email;
        _db = db;
        _negocio = negocio;
    }

    // /Servicios -> sección de servicios de la página de inicio
    [HttpGet("Servicios")]
    public IActionResult Index() => Redirect("/#servicios");

    // /Servicios/montaje-balanceo, /Servicios/alineacion-3d, ...
    [HttpGet("Servicios/{slug}")]
    public IActionResult Detalle(string slug)
    {
        var servicio = _db.ServicioPublicado(slug);
        return servicio == null ? NotFound() : View(servicio);
    }

    [HttpPost("Servicios/SolicitarContacto"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarContacto(string slug, string nombre, string correo, string telefono, string? mensaje)
    {
        var servicio = _db.ServicioPublicado(slug);
        if (servicio == null) return NotFound();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono) || !Validaciones.CorreoValido(correo))
        {
            TempData["Error"] = "Completa tu nombre, un correo válido y tu teléfono.";
            return RedirectToAction(nameof(Detalle), new { slug });
        }

        var cuerpo = EmailService.PlantillaAviso($"Solicitud de información: {servicio.Titulo}", new (string, string?)[]
        {
            ("Servicio", servicio.Titulo), ("Nombre", nombre), ("Correo", correo), ("Teléfono", telefono)
        }, mensaje);

        var enviado = await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llandupar",
            $"[Servicio] {servicio.Titulo} – {nombre}", cuerpo, responderA: correo);

        if (enviado) TempData["Exito"] = "Recibimos tu solicitud. Te contactaremos pronto.";
        else TempData["Error"] = $"No pudimos enviar tu solicitud en este momento. Llámanos al {_negocio.Telefono}.";
        return RedirectToAction(nameof(Detalle), new { slug });
    }
}
