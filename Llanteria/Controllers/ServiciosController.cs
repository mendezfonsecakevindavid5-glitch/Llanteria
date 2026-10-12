using System.Net.Mail;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

public class ServiciosController : Controller
{
    private readonly EmailService _email;

    public ServiciosController(EmailService email) => _email = email;

    // /Servicios -> sección de servicios de la página de inicio
    [HttpGet("Servicios")]
    public IActionResult Index() => Redirect("/#servicios");

    // /Servicios/montaje-balanceo, /Servicios/alineacion-3d, ...
    [HttpGet("Servicios/{slug}")]
    public IActionResult Detalle(string slug)
    {
        var servicio = ServiciosCatalogo.Buscar(slug);
        return servicio == null ? NotFound() : View(servicio);
    }

    [HttpPost("Servicios/SolicitarContacto"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarContacto(string slug, string nombre, string correo, string telefono, string? mensaje)
    {
        var servicio = ServiciosCatalogo.Buscar(slug);
        if (servicio == null) return NotFound();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono) || !CorreoValido(correo))
        {
            TempData["Error"] = "Completa tu nombre, un correo válido y tu teléfono.";
            return RedirectToAction(nameof(Detalle), new { slug });
        }

        var cuerpo = EmailService.PlantillaAviso($"Solicitud de información: {servicio.Titulo}", new (string, string?)[]
        {
            ("Servicio", servicio.Titulo), ("Nombre", nombre), ("Correo", correo), ("Teléfono", telefono)
        }, mensaje);

        var enviado = await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llantería Valledupar",
            $"[Servicio] {servicio.Titulo} – {nombre}", cuerpo, responderA: correo);

        if (enviado) TempData["Exito"] = "Recibimos tu solicitud. Te contactaremos pronto.";
        else TempData["Error"] = "No pudimos enviar tu solicitud en este momento. Llámanos al +57 311 772 3978.";
        return RedirectToAction(nameof(Detalle), new { slug });
    }

    private static bool CorreoValido(string? correo)
        => !string.IsNullOrWhiteSpace(correo) && MailAddress.TryCreate(correo.Trim(), out var m) && m.Address == correo.Trim();
}
