using System.Net.Mail;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Servicio para empresas y flotas: página pública con formulario y, para el personal, la lista de solicitudes.</summary>
public class EmpresasController : Controller
{
    private const string RolesPersonal = "Administrador,Empleado";

    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;

    public EmpresasController(LlanteriaDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    // ---------- Público ----------

    [HttpGet]
    public IActionResult Index() => View(new SolicitudEmpresa());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitar(SolicitudEmpresa s)
    {
        s.Empresa = (s.Empresa ?? "").Trim();
        s.Contacto = (s.Contacto ?? "").Trim();
        s.Telefono = (s.Telefono ?? "").Trim();
        s.Correo = (s.Correo ?? "").Trim();
        s.Nit = string.IsNullOrWhiteSpace(s.Nit) ? null : s.Nit.Trim();
        s.Ciudad = string.IsNullOrWhiteSpace(s.Ciudad) ? null : s.Ciudad.Trim();
        s.Mensaje = string.IsNullOrWhiteSpace(s.Mensaje) ? null : s.Mensaje.Trim();

        string? error = null;
        if (s.Empresa.Length is 0 or > 120) error = "Escribe el nombre de la empresa.";
        else if (s.Contacto.Length is 0 or > 100) error = "Escribe el nombre de la persona de contacto.";
        else if (s.Telefono.Length is < 7 or > 20) error = "Escribe un teléfono válido.";
        else if (s.Correo.Length > 100 || !MailAddress.TryCreate(s.Correo, out var m) || m.Address != s.Correo) error = "Escribe un correo válido.";
        else if (!SolicitudEmpresa.TiposFlota.Contains(s.TipoFlota)) error = "Elige el tipo de flota.";
        else if (s.NumeroVehiculos is < 1 or > 10000) error = "Indica cuántos vehículos tiene la flota.";
        else if (s.Nit?.Length > 20 || s.Ciudad?.Length > 60 || s.Mensaje?.Length > 500) error = "Alguno de los textos es demasiado largo.";

        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Index), null, null, "solicitud");
        }

        _db.SolicitudesEmpresa.Add(new SolicitudEmpresa
        {
            Empresa = s.Empresa, Nit = s.Nit, Contacto = s.Contacto, Telefono = s.Telefono, Correo = s.Correo,
            Ciudad = s.Ciudad, TipoFlota = s.TipoFlota, NumeroVehiculos = s.NumeroVehiculos, Mensaje = s.Mensaje,
            Estado = SolicitudEmpresa.Pendiente, FechaCreacion = DateTime.Now
        });
        _db.SaveChanges();

        // Aviso por correo al taller: es un extra, la solicitud ya quedó guardada.
        try
        {
            var cuerpo = EmailService.PlantillaAviso($"Nueva solicitud de empresa: {s.Empresa}", new (string, string?)[]
            {
                ("Empresa", s.Empresa), ("NIT", s.Nit), ("Contacto", s.Contacto), ("Teléfono", s.Telefono), ("Correo", s.Correo),
                ("Ciudad", s.Ciudad), ("Tipo de flota", s.TipoFlota), ("Vehículos", s.NumeroVehiculos.ToString())
            }, s.Mensaje);
            await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llantería Valledupar",
                $"[Empresas] {s.Empresa} – {s.NumeroVehiculos} vehículos", cuerpo, responderA: s.Correo);
        }
        catch { /* se ignora a propósito */ }

        TempData["Exito"] = "Recibimos tu solicitud. Un asesor te contactará en las próximas horas hábiles.";
        return RedirectToAction(nameof(Index), null, null, "solicitud");
    }

    // ---------- Gestión (personal) ----------

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Solicitudes(string? estado, string? buscar, int pagina = 1)
    {
        ViewBag.Estado = estado;
        ViewBag.Buscar = buscar;
        ViewBag.Pendientes = _db.SolicitudesEmpresa.Count(x => x.Estado == SolicitudEmpresa.Pendiente);
        ViewBag.Vehiculos = _db.SolicitudesEmpresa.Where(x => x.Estado != SolicitudEmpresa.Cerrada).Sum(x => (int?)x.NumeroVehiculos) ?? 0;
        var q = Filtrar(estado, buscar).OrderBy(x => x.Estado == SolicitudEmpresa.Pendiente ? 0 : x.Estado == SolicitudEmpresa.Contactada ? 1 : 2)
            .ThenByDescending(x => x.FechaCreacion);
        return View(Paginado<SolicitudEmpresa>.De(q, pagina));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesPersonal)]
    public IActionResult CambiarEstado(int id, string estado, string? retorno)
    {
        var s = _db.SolicitudesEmpresa.Find(id);
        if (s == null) return NotFound();
        if (!SolicitudEmpresa.Estados.Contains(estado)) TempData["Error"] = "Estado no válido.";
        else
        {
            s.Estado = estado;
            _db.SaveChanges();
            TempData["Exito"] = $"Solicitud de {s.Empresa} marcada como {estado.ToLower()}.";
        }
        return !string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno) ? LocalRedirect(retorno) : RedirectToAction(nameof(Solicitudes));
    }

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Exportar(string? estado, string? buscar)
    {
        var filas = Filtrar(estado, buscar).OrderByDescending(x => x.FechaCreacion).ToList().Select(x => new string?[]
        {
            x.FechaCreacion.ToString("dd/MM/yyyy"), x.Empresa, x.Nit, x.Contacto, x.Telefono, x.Correo, x.Ciudad,
            x.TipoFlota, x.NumeroVehiculos.ToString(), x.Estado, x.Mensaje
        });
        return Csv.Archivo("solicitudes_empresas", new[] { "Fecha", "Empresa", "NIT", "Contacto", "Teléfono", "Correo", "Ciudad", "Tipo de flota", "Vehículos", "Estado", "Mensaje" }, filas);
    }

    private IQueryable<SolicitudEmpresa> Filtrar(string? estado, string? buscar)
    {
        var q = _db.SolicitudesEmpresa.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && SolicitudEmpresa.Estados.Contains(estado)) q = q.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(x => x.Empresa.Contains(t) || x.Contacto.Contains(t) || x.Correo.Contains(t) || (x.Nit != null && x.Nit.Contains(t)));
        }
        return q;
    }
}
