using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Llanteria.Controllers;

/// <summary>
/// Páginas legales y de atención al cliente (públicas): términos, privacidad, garantía, devoluciones,
/// preguntas frecuentes y radicación de PQRS.
/// </summary>
public class LegalController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;

    public LegalController(LlanteriaDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    // Los cuatro documentos legales se muestran como PDF (el pie del sitio los abre en una pestaña nueva).
    // Preguntas frecuentes y PQRS siguen siendo páginas normales, sin menú lateral.
    public IActionResult Terminos() => DocumentoPdf("Terminos");
    public IActionResult Privacidad() => DocumentoPdf("Privacidad");
    public IActionResult Garantia() => DocumentoPdf("Garantia");
    public IActionResult Devoluciones() => DocumentoPdf("Devoluciones");
    public IActionResult Faq() => View();

    // Sin FileName el navegador lo abre en su visor de PDF en vez de descargarlo.
    // "LegalPdf" le indica a _ViewStart que use el diseño de documento (_LayoutPdfLegal) y no el del sitio.
    private IActionResult DocumentoPdf(string vista)
    {
        HttpContext.Items["LegalPdf"] = true;

        // Si por algún motivo falta wkhtmltopdf.exe, el documento se muestra como página HTML en vez de romper con un error 500.
        var web = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        var hayWkhtml = new[] { "Rotativa", Path.Combine("Rotativa", "Rotativa") }
            .Any(r => System.IO.File.Exists(Path.Combine(web, r, "wkhtmltopdf.exe")));
        if (!hayWkhtml) return View(vista);

        return new ViewAsPdf(vista)
        {
            PageSize = Size.A4,
            PageOrientation = Orientation.Portrait,
            PageMargins = new Margins(16, 16, 18, 16),
            CustomSwitches = "--print-media-type --footer-font-size 7 --footer-spacing 5 --footer-line " +
                             "--footer-left \"[title]\" --footer-right \"[page] / [topage]\""
        };
    }

    // ---------- PQRS ----------

    // La acción se llama FormularioPqrs en el código (para no chocar con la clase Pqrs); la URL sigue siendo /Legal/Pqrs.
    [HttpGet, ActionName("Pqrs")]
    public IActionResult FormularioPqrs(string? tipo)
    {
        var vm = new Pqrs { Tipo = NormalizarTipo(tipo) };
        if (User.Identity?.IsAuthenticated == true) Prellenar(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarPqrs(Pqrs p, bool aceptaDatos)
    {
        p.Tipo = NormalizarTipo(p.Tipo);
        p.Nombre = (p.Nombre ?? "").Trim();
        p.Correo = (p.Correo ?? "").Trim();
        p.Asunto = (p.Asunto ?? "").Trim();
        p.Mensaje = (p.Mensaje ?? "").Trim();
        p.Telefono = string.IsNullOrWhiteSpace(p.Telefono) ? null : p.Telefono.Trim();
        p.NumeroDocumento = string.IsNullOrWhiteSpace(p.NumeroDocumento) ? null : p.NumeroDocumento.Trim();

        string? error = null;
        if (p.Nombre.Length is < 3 or > 100) error = "Escribe tu nombre completo.";
        else if (!Validaciones.CorreoValido(p.Correo)) error = "Escribe un correo válido para enviarte la respuesta.";
        else if (p.Telefono?.Length > 25 || p.NumeroDocumento?.Length > 20) error = "El teléfono o el documento es demasiado largo.";
        else if (p.Asunto.Length is < 5 or > 120) error = "Escribe un asunto de 5 a 120 caracteres.";
        else if (p.Mensaje.Length is < 20 or > 1500) error = "Cuéntanos tu caso con al menos 20 caracteres (máximo 1.500).";
        else if (!aceptaDatos) error = "Debes autorizar el tratamiento de tus datos personales para radicar la solicitud.";

        if (error != null)
        {
            TempData["Error"] = error;
            return View("Pqrs", p);
        }

        int? idCliente = int.TryParse(User.FindFirst("ClienteId")?.Value, out var c) ? c : null;

        var registro = new Pqrs
        {
            Radicado = "PROVISIONAL-" + Guid.NewGuid().ToString("N")[..8],
            Tipo = p.Tipo,
            Nombre = p.Nombre,
            NumeroDocumento = p.NumeroDocumento,
            Correo = p.Correo,
            Telefono = p.Telefono,
            Asunto = p.Asunto,
            Mensaje = p.Mensaje,
            AutorizaDatos = true,
            IdCliente = idCliente,
            Estado = Pqrs.Recibida,
            FechaCreacion = DateTime.Now
        };
        _db.RegistrosPqrs.Add(registro);
        _db.SaveChanges();

        // El radicado lleva el Id: se completa después de guardar.
        registro.Radicado = $"PQ-{registro.FechaCreacion:yy}-{registro.Id:000000}";
        _db.SaveChanges();

        // Correos: son un extra, la solicitud ya quedó guardada.
        try
        {
            var limite = registro.FechaLimite.ToString("dd/MM/yyyy");
            var cuerpoCliente = EmailService.PlantillaAviso($"Recibimos tu {registro.Tipo.ToLower()}", new (string, string?)[]
            {
                ("Radicado", registro.Radicado), ("Asunto", registro.Asunto), ("Fecha límite de respuesta", limite)
            }, "Guarda este número de radicado: con él puedes consultar el estado de tu solicitud escribiéndonos o llamándonos.");
            await _email.EnviarCorreoAsync(registro.Correo, registro.Nombre, $"Radicado {registro.Radicado}", cuerpoCliente);

            var cuerpoTaller = EmailService.PlantillaAviso($"Nueva {registro.Tipo.ToLower()}: {registro.Asunto}", new (string, string?)[]
            {
                ("Radicado", registro.Radicado), ("Nombre", registro.Nombre), ("Correo", registro.Correo),
                ("Teléfono", registro.Telefono), ("Límite de respuesta", limite)
            }, registro.Mensaje);
            await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llandupar",
                $"[PQRS] {registro.Radicado} · {registro.Tipo}", cuerpoTaller, responderA: registro.Correo);
        }
        catch { /* se ignora a propósito */ }

        return RedirectToAction(nameof(PqrsRecibida), new { radicado = registro.Radicado });
    }

    [HttpGet]
    public IActionResult PqrsRecibida(string radicado)
    {
        var p = _db.RegistrosPqrs.AsNoTracking().FirstOrDefault(x => x.Radicado == radicado);
        if (p == null) return RedirectToAction("Pqrs");
        return View(p);
    }

    private static string NormalizarTipo(string? tipo)
        => Pqrs.Tipos.FirstOrDefault(t => string.Equals(t, tipo, StringComparison.OrdinalIgnoreCase)) ?? "Petición";

    private void Prellenar(Pqrs vm)
    {
        if (!int.TryParse(User.FindFirst("ClienteId")?.Value, out var id)) return;
        var cli = _db.Clientes.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Nombres, x.Apellidos, x.Correo, x.Telefono, x.NumeroDocumento }).FirstOrDefault();
        if (cli == null) return;
        vm.Nombre = $"{cli.Nombres} {cli.Apellidos}";
        vm.Correo = cli.Correo;
        vm.Telefono = cli.Telefono;
        vm.NumeroDocumento = cli.NumeroDocumento;
    }
}
