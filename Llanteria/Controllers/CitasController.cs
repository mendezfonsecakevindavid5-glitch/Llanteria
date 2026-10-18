using System.Globalization;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Agendar es para cualquier usuario con sesión (clientes o personal); la gestión es solo del personal.</summary>
[Authorize]
public class CitasController : Controller
{
    private const string RolesPersonal = "Administrador,Empleado";
    private const int CupoPorHorario = 2; // citas simultáneas por servicio, fecha y hora
    private static readonly string[] FormasPago = { "Efectivo", "Tarjeta", "Transferencia" };

    private readonly EmailService _email;
    private readonly LlanteriaDbContext _db;
    private readonly NegocioInfo _negocio;

    public CitasController(EmailService email, LlanteriaDbContext db, NegocioInfo negocio)
    {
        _email = email;
        _db = db;
        _negocio = negocio;
    }

    // ---------- Solicitar (público con sesión) ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Agendar(string slug, DateTime? fecha, string hora, string vehiculo,
        string? direccion, string formaPago, string? notas)
    {
        var servicio = _db.ServicioPublicado(slug);
        if (servicio == null) return NotFound();

        string Volver(string clave, string mensaje)
        {
            TempData[clave] = mensaje;
            return Url.Action("Detalle", "Servicios", new { slug })!;
        }

        vehiculo = (vehiculo ?? "").Trim();
        if (!fecha.HasValue || fecha.Value.Date < DateTime.Today)
            return Redirect(Volver("Error", "Elige una fecha de hoy en adelante."));
        if (!servicio.Horarios.Contains(hora))
            return Redirect(Volver("Error", "Elige un horario de la lista."));
        if (vehiculo.Length is 0 or > 150)
            return Redirect(Volver("Error", "Indica el vehículo (placa y modelo)."));
        if (servicio.EsDomicilio && string.IsNullOrWhiteSpace(direccion))
            return Redirect(Volver("Error", "Indica la dirección donde atenderemos el vehículo."));
        if (!FormasPago.Contains(formaPago)) formaPago = "Efectivo";

        var dia = fecha.Value.Date;
        var ocupadas = _db.Citas.Count(c => c.Slug == slug && c.Fecha == dia && c.Hora == hora && c.Estado != Cita.Cancelada);
        if (ocupadas >= CupoPorHorario)
            return Redirect(Volver("Error", "Ese horario ya está lleno. Elige otra hora u otro día."));

        var cita = new Cita
        {
            Slug = slug,
            Servicio = servicio.Titulo,
            Fecha = dia,
            Hora = hora,
            Vehiculo = vehiculo,
            Direccion = servicio.EsDomicilio ? Recortar(direccion, 200) : null,
            FormaPago = formaPago,
            Notas = Recortar(notas, 500),
            Solicitante = User.Identity?.Name ?? "—",
            FechaCreacion = DateTime.Now
        };

        // Si quien agenda es un cliente del club, se guardan sus datos de contacto.
        if (int.TryParse(User.FindFirst("ClienteId")?.Value, out var idCliente))
        {
            var c = _db.Clientes.AsNoTracking().FirstOrDefault(x => x.Id == idCliente);
            if (c != null)
            {
                cita.IdCliente = c.Id;
                cita.Solicitante = $"{c.Nombres} {c.Apellidos}".Trim();
                cita.Telefono = c.Telefono;
                cita.Correo = c.Correo;
            }
        }

        _db.Citas.Add(cita);
        _db.SaveChanges();

        // El aviso por correo es un extra: si falla, la cita ya quedó registrada.
        try
        {
            var cuerpo = EmailService.PlantillaAviso($"Nueva cita: {servicio.Titulo}", new (string, string?)[]
            {
                ("Solicitante", cita.Solicitante), ("Teléfono", cita.Telefono), ("Servicio", servicio.Titulo),
                ("Fecha", dia.ToString("dd/MM/yyyy")), ("Hora", hora), ("Vehículo", vehiculo),
                ("Dirección", cita.Direccion), ("Forma de pago", formaPago)
            }, notas);
            await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llantería Valledupar",
                $"[Cita] {servicio.Titulo} – {dia:dd/MM/yyyy} {hora}", cuerpo);
        }
        catch { /* se ignora a propósito */ }

        // Acuse de recibo al cliente (si dejó correo en el club).
        await NotificarClienteAsync(cita, "Recibimos tu solicitud de cita",
            "Tu cita quedó solicitada. Te confirmaremos pronto; cuando la confirmemos recibirás otro correo.");

        return Redirect(Volver("Exito", $"Tu cita quedó solicitada para el {dia:dd/MM/yyyy} a las {hora}. Te confirmaremos por teléfono."));
    }

    // ---------- Gestión (personal) ----------

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Index(string? estado, string? buscar, int pagina = 1)
    {
        var consulta = Filtrar(estado, buscar)
            .OrderBy(c => c.Estado == Cita.Pendiente ? 0 : c.Estado == Cita.Confirmada ? 1 : 2)
            .ThenBy(c => c.Fecha).ThenBy(c => c.Hora);

        ViewBag.Estado = estado;
        ViewBag.Buscar = buscar;
        ViewBag.Pendientes = _db.Citas.Count(c => c.Estado == Cita.Pendiente);
        ViewBag.ProximasSemana = _db.Citas.Count(c => (c.Estado == Cita.Pendiente || c.Estado == Cita.Confirmada)
            && c.Fecha >= DateTime.Today && c.Fecha < DateTime.Today.AddDays(7));
        return View(Paginado<Cita>.De(consulta, pagina));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesPersonal)]
    public async Task<IActionResult> CambiarEstado(int id, string estado, string? retorno)
    {
        var cita = _db.Citas.Find(id);
        if (cita == null) return NotFound();
        if (!Cita.Estados.Contains(estado))
        {
            TempData["Error"] = "Estado no válido.";
        }
        else
        {
            cita.Estado = estado;
            _db.SaveChanges();
            TempData["Exito"] = $"Cita de {cita.Solicitante} marcada como {estado.ToLower()}.";
            if (estado == Cita.Confirmada)
                await NotificarClienteAsync(cita, "Tu cita fue confirmada", "Ya tienes cupo reservado. ¡Te esperamos!");
            else if (estado == Cita.Cancelada)
                await NotificarClienteAsync(cita, "Tu cita fue cancelada", $"Si quieres reprogramarla, escríbenos por WhatsApp o llama al {_negocio.Telefono}.");
        }
        return !string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno) ? LocalRedirect(retorno) : RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Exportar(string? estado, string? buscar)
    {
        var lista = Filtrar(estado, buscar).OrderBy(c => c.Fecha).ThenBy(c => c.Hora).ToList();
        var filtros = new List<string>();
        if (!string.IsNullOrWhiteSpace(estado)) filtros.Add($"estado {estado}");
        if (!string.IsNullOrWhiteSpace(buscar)) filtros.Add($"búsqueda \"{buscar.Trim()}\"");
        return this.ExcelArchivo("citas", new TablaHoja
        {
            Nombre = "Citas",
            Titulo = "Agenda de citas",
            Subtitulo = filtros.Count == 0 ? "Todas las citas" : "Filtro: " + string.Join(" · ", filtros),
            Columnas =
            {
                new("Fecha", TipoCol.Fecha), new("Hora"), new("Servicio"), new("Solicitante"), new("Teléfono"), new("Vehículo"),
                new("Dirección"), new("Forma de pago"), new("Estado"), new("Notas")
            },
            Filas = lista.Select(c => new object?[]
            {
                c.Fecha, c.Hora, c.Servicio, c.Solicitante, c.Telefono, c.Vehiculo, c.Direccion, c.FormaPago, c.Estado, c.Notas
            }).ToList()
        });
    }

    private IQueryable<Cita> Filtrar(string? estado, string? buscar)
    {
        var q = _db.Citas.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && Cita.Estados.Contains(estado)) q = q.Where(c => c.Estado == estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(c => c.Solicitante.Contains(t) || c.Servicio.Contains(t) || c.Vehiculo.Contains(t)
                             || (c.Telefono != null && c.Telefono.Contains(t)));
        }
        return q;
    }

    // Correo al cliente; es un extra, nunca debe impedir guardar la cita ni cambiar su estado.
    private async Task NotificarClienteAsync(Cita cita, string titulo, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(cita.Correo)) return;
        try
        {
            var cuerpo = EmailService.PlantillaAviso(titulo, new (string, string?)[]
            {
                ("Servicio", cita.Servicio), ("Fecha", cita.Fecha.ToString("dd/MM/yyyy")), ("Hora", cita.Hora),
                ("Vehículo", cita.Vehiculo), ("Lugar", cita.Direccion ?? _negocio.Direccion)
            }, mensaje);
            await _email.EnviarCorreoAsync(cita.Correo, cita.Solicitante, $"{titulo} – {_negocio.Nombre}", cuerpo);
        }
        catch { /* se ignora a propósito */ }
    }

    private static string? Recortar(string? v, int max)
    {
        v = v?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        return v.Length > max ? v[..max] : v;
    }
}
