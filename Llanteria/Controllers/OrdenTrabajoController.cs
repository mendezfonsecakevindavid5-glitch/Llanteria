using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>
/// Órdenes de trabajo del taller (personal): seguimiento de cada vehículo desde que entra hasta que se entrega.
/// El acceso lo controla RoleConvention (Administrador y Empleado).
/// </summary>
public class OrdenTrabajoController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;
    private readonly NegocioInfo _negocio;

    public OrdenTrabajoController(LlanteriaDbContext db, EmailService email, NegocioInfo negocio)
    {
        _db = db;
        _email = email;
        _negocio = negocio;
    }

    // ---------- Listado ----------

    public IActionResult Index(string? estado, string? buscar, int pagina = 1)
    {
        ViewBag.Estado = estado;
        ViewBag.Buscar = buscar;
        ViewBag.EnTaller = _db.OrdenesTrabajo.Count(o => o.Estado == OrdenTrabajo.Recibido || o.Estado == OrdenTrabajo.EnProceso);
        ViewBag.Listas = _db.OrdenesTrabajo.Count(o => o.Estado == OrdenTrabajo.Listo);

        var q = _db.OrdenesTrabajo.AsNoTracking()
            .Include(o => o.Vehiculo).ThenInclude(v => v.Cliente)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && OrdenTrabajo.Estados.Contains(estado)) q = q.Where(o => o.Estado == estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(o => o.Numero.Contains(t) || o.Descripcion.Contains(t) || o.Vehiculo.Placa.Contains(t)
                             || o.Vehiculo.Cliente.Nombres.Contains(t) || o.Vehiculo.Cliente.Apellidos.Contains(t));
        }

        // Primero lo que sigue abierto (la más antigua arriba), después lo cerrado (la más reciente arriba).
        var ordenada = q.OrderBy(o => o.Estado == OrdenTrabajo.Entregado || o.Estado == OrdenTrabajo.Cancelado ? 1 : 0)
                        .ThenBy(o => o.Estado == OrdenTrabajo.Entregado || o.Estado == OrdenTrabajo.Cancelado ? 0 : 1)
                        .ThenByDescending(o => o.FechaIngreso);
        var lista = Paginado<OrdenTrabajo>.De(ordenada, pagina);

        var idsEmpleado = lista.Items.Where(o => o.IdEmpleado.HasValue).Select(o => o.IdEmpleado!.Value).Distinct().ToList();
        ViewBag.Tecnicos = _db.Empleados.AsNoTracking().Where(e => idsEmpleado.Contains(e.Id))
            .ToDictionary(e => e.Id, e => e.Nombres + " " + e.Apellidos);
        return View(lista);
    }

    // ---------- Crear ----------

    [HttpGet]
    public IActionResult Create(int? idVehiculo)
    {
        var o = new OrdenTrabajo { Tipo = Mantenimientos.Tipos.First() };
        if (idVehiculo.HasValue)
        {
            var v = _db.Vehiculos.AsNoTracking().FirstOrDefault(x => x.Id == idVehiculo.Value);
            if (v != null) { o.IdVehiculo = v.Id; o.Kilometraje = v.Kilometraje; }
        }
        CargarCombos(o);
        return View(o);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrdenTrabajo o)
    {
        Normalizar(o);
        Validar(o);
        if (!ModelState.IsValid)
        {
            CargarCombos(o);
            return View(o);
        }

        var nueva = new OrdenTrabajo
        {
            Numero = "PROVISIONAL-" + Guid.NewGuid().ToString("N")[..8],
            IdVehiculo = o.IdVehiculo,
            IdEmpleado = o.IdEmpleado,
            Tipo = o.Tipo,
            Descripcion = o.Descripcion,
            Kilometraje = o.Kilometraje,
            Observaciones = o.Observaciones,
            Total = o.Total,
            ProximaFecha = o.ProximaFecha,
            ProximoKm = o.ProximoKm,
            Estado = OrdenTrabajo.Recibido,
            FechaIngreso = DateTime.Now
        };
        _db.OrdenesTrabajo.Add(nueva);
        _db.SaveChanges();
        nueva.Numero = $"OT-{nueva.FechaIngreso:yy}-{nueva.Id:000000}";
        ActualizarKilometrajeVehiculo(nueva);
        _db.SaveChanges();

        await AvisarAsync(nueva, "Recibimos tu vehículo", "Ya registramos tu vehículo en el taller. Te avisaremos por este medio cuando esté listo.");

        TempData["Exito"] = $"Orden {nueva.Numero} creada.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Editar ----------

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var o = _db.OrdenesTrabajo.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (o == null) return NotFound();
        CargarCombos(o);
        return View(o);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, OrdenTrabajo o)
    {
        var actual = _db.OrdenesTrabajo.Find(id);
        if (actual == null) return NotFound();

        o.Id = id;
        Normalizar(o);
        Validar(o);
        if (!ModelState.IsValid)
        {
            o.Numero = actual.Numero;
            o.Estado = actual.Estado;
            CargarCombos(o);
            return View(o);
        }

        if (actual.ProximaFecha != o.ProximaFecha) actual.RecordatorioEnviado = false;
        actual.IdVehiculo = o.IdVehiculo;
        actual.IdEmpleado = o.IdEmpleado;
        actual.Tipo = o.Tipo;
        actual.Descripcion = o.Descripcion;
        actual.Kilometraje = o.Kilometraje;
        actual.Observaciones = o.Observaciones;
        actual.Total = o.Total;
        actual.ProximaFecha = o.ProximaFecha;
        actual.ProximoKm = o.ProximoKm;
        ActualizarKilometrajeVehiculo(actual);
        _db.SaveChanges();

        TempData["Exito"] = $"Orden {actual.Numero} actualizada.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Cambiar estado ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string estado, string? retorno)
    {
        var o = _db.OrdenesTrabajo.Include(x => x.Vehiculo).ThenInclude(v => v.Cliente).FirstOrDefault(x => x.Id == id);
        if (o == null) return NotFound();

        if (!OrdenTrabajo.Estados.Contains(estado))
            TempData["Error"] = "Estado no válido.";
        else if (o.Estado == estado)
            TempData["Error"] = $"La orden ya está en «{estado}».";
        else if (o.Estado is OrdenTrabajo.Entregado or OrdenTrabajo.Cancelado)
            TempData["Error"] = "Una orden entregada o cancelada ya no se puede cambiar.";
        else
        {
            o.Estado = estado;
            if (estado == OrdenTrabajo.Entregado)
            {
                o.FechaEntrega = DateTime.Now;
                ProgramarProximoServicio(o);
            }
            _db.SaveChanges();

            if (estado == OrdenTrabajo.Listo)
                await AvisarAsync(o, "Tu vehículo está listo", $"Ya puedes pasar por tu vehículo. Te esperamos en {_negocio.Direccion}.");
            else if (estado == OrdenTrabajo.Entregado)
                await AvisarAsync(o, "Gracias por confiar en nosotros", ProximoServicioTexto(o) ?? "Guardamos el historial de este trabajo en tu cuenta.");

            TempData["Exito"] = $"Orden {o.Numero}: {estado.ToLower()}.";
        }
        return !string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno) ? LocalRedirect(retorno) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var o = _db.OrdenesTrabajo.Find(id);
        if (o != null)
        {
            _db.OrdenesTrabajo.Remove(o);
            _db.SaveChanges();
            TempData["Exito"] = "Orden eliminada.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---------- Apoyo ----------

    private static void Normalizar(OrdenTrabajo o)
    {
        o.Descripcion = (o.Descripcion ?? "").Trim();
        o.Observaciones = string.IsNullOrWhiteSpace(o.Observaciones) ? null : o.Observaciones.Trim();
        o.Tipo = Mantenimientos.Intervalos.ContainsKey(o.Tipo ?? "") ? o.Tipo! : Mantenimientos.Otro;
    }

    private void Validar(OrdenTrabajo o)
    {
        // Los datos de la orden que no vienen del formulario no se validan.
        ModelState.Remove(nameof(OrdenTrabajo.Vehiculo));
        ModelState.Remove(nameof(OrdenTrabajo.Numero));
        ModelState.Remove(nameof(OrdenTrabajo.Estado));

        if (o.IdVehiculo > 0 && !_db.Vehiculos.AsNoTracking().Any(v => v.Id == o.IdVehiculo))
            ModelState.AddModelError(nameof(OrdenTrabajo.IdVehiculo), "El vehículo no existe.");
        if (o.IdEmpleado.HasValue && !_db.Empleados.AsNoTracking().Any(e => e.Id == o.IdEmpleado.Value))
            ModelState.AddModelError(nameof(OrdenTrabajo.IdEmpleado), "El técnico no existe.");
        if (o.ProximaFecha.HasValue && o.ProximaFecha.Value.Date <= DateTime.Today)
            ModelState.AddModelError(nameof(OrdenTrabajo.ProximaFecha), "La fecha del próximo servicio debe ser futura.");
    }

    private void CargarCombos(OrdenTrabajo o)
    {
        var vehiculos = _db.Vehiculos.AsNoTracking().Include(v => v.Cliente).OrderBy(v => v.Placa).ToList()
            .Select(v => new { v.Id, Nombre = $"{v.Placa} — {v.Marca} {v.Modelo} — {v.Cliente.Nombres} {v.Cliente.Apellidos}" });
        ViewBag.IdVehiculo = new SelectList(vehiculos, "Id", "Nombre", o.IdVehiculo);

        var tecnicos = _db.Empleados.AsNoTracking().OrderBy(e => e.Nombres).ToList()
            .Select(e => new { e.Id, Nombre = $"{e.Nombres} {e.Apellidos}" });
        ViewBag.IdEmpleado = new SelectList(tecnicos, "Id", "Nombre", o.IdEmpleado);

        ViewBag.Tipos = new SelectList(Mantenimientos.Tipos, o.Tipo);
    }

    /// <summary>El kilometraje del vehículo solo sube: se conserva el más alto que se haya registrado.</summary>
    private void ActualizarKilometrajeVehiculo(OrdenTrabajo o)
    {
        if (!o.Kilometraje.HasValue) return;
        var v = _db.Vehiculos.Find(o.IdVehiculo);
        if (v != null && (v.Kilometraje ?? 0) < o.Kilometraje.Value) v.Kilometraje = o.Kilometraje;
    }

    /// <summary>Al entregar: si no se indicó el próximo servicio, se calcula con los intervalos de referencia del tipo de trabajo.</summary>
    private static void ProgramarProximoServicio(OrdenTrabajo o)
    {
        if (!Mantenimientos.Intervalos.TryGetValue(o.Tipo, out var intervalo)) return;
        if (!o.ProximaFecha.HasValue && intervalo.Dias.HasValue)
        {
            o.ProximaFecha = DateTime.Today.AddDays(intervalo.Dias.Value);
            o.RecordatorioEnviado = false;
        }
        if (!o.ProximoKm.HasValue && intervalo.Km.HasValue && o.Kilometraje.HasValue)
            o.ProximoKm = o.Kilometraje.Value + intervalo.Km.Value;
    }

    private static string? ProximoServicioTexto(OrdenTrabajo o)
    {
        var partes = new List<string>();
        if (o.ProximaFecha.HasValue) partes.Add($"el {o.ProximaFecha.Value:dd/MM/yyyy}");
        if (o.ProximoKm.HasValue) partes.Add($"a los {o.ProximoKm.Value:N0} km");
        return partes.Count == 0 ? null : $"Tu próximo servicio de «{o.Tipo.ToLower()}» sería {string.Join(" o ", partes)}. Te lo recordaremos por correo.";
    }

    /// <summary>Aviso por correo al dueño del vehículo. Es un extra: si falla, la orden ya quedó guardada.</summary>
    private async Task AvisarAsync(OrdenTrabajo o, string titulo, string mensaje)
    {
        try
        {
            var v = o.Vehiculo ?? _db.Vehiculos.AsNoTracking().Include(x => x.Cliente).First(x => x.Id == o.IdVehiculo);
            var cli = v.Cliente ?? _db.Clientes.AsNoTracking().First(c => c.Id == v.IdCliente);
            if (string.IsNullOrWhiteSpace(cli.Correo)) return;

            var cuerpo = EmailService.PlantillaAviso(titulo, new (string, string?)[]
            {
                ("Orden", o.Numero), ("Vehículo", $"{v.Placa} · {v.Marca} {v.Modelo}"), ("Trabajo", o.Descripcion)
            }, mensaje);
            await _email.EnviarCorreoAsync(cli.Correo, $"{cli.Nombres} {cli.Apellidos}", $"{titulo} · {o.Numero}", cuerpo);
        }
        catch { /* se ignora a propósito */ }
    }
}
