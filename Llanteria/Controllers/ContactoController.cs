using System.Net.Mail;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

public class ContactoController : Controller
{
    private readonly EmailService _email;

    public ContactoController(EmailService email) => _email = email;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarMensaje(string nombre, string correo, string asunto, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(asunto) || string.IsNullOrWhiteSpace(mensaje)
            || !MailAddress.TryCreate((correo ?? "").Trim(), out var m) || m.Address != correo!.Trim())
        {
            TempData["Error"] = "Completa todos los campos con un correo válido.";
            return RedirectToAction(nameof(Index));
        }

        var cuerpo = EmailService.PlantillaAviso("Nuevo mensaje de contacto", new (string, string?)[]
        {
            ("Nombre", nombre), ("Correo", correo), ("Asunto", asunto)
        }, mensaje);

        var enviado = await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llantería Valledupar",
            $"[Contacto web] {asunto} – {nombre}", cuerpo, responderA: correo);

        if (enviado) TempData["Exito"] = "Mensaje enviado. Te responderemos al correo que indicaste.";
        else TempData["Error"] = "No pudimos enviar tu mensaje en este momento. Llámanos al +57 311 772 3978.";
        return RedirectToAction(nameof(Index));
    }
}
