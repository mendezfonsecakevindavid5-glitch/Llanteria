using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

/// <summary>
/// Una vez al día (entre las 8:00 y las 18:00) avisa por correo a los clientes cuyo próximo mantenimiento
/// llega en los próximos 7 días o ya venció (hasta 30 días atrás). Marca cada orden para no repetir el aviso.
/// Si Brevo no está configurado, no hace nada. El aviso por kilometraje se muestra en "Mi cuenta"; no se envía por correo
/// porque el taller solo conoce el kilometraje cuando el cliente visita.
/// </summary>
public class RecordatorioMantenimientoService : BackgroundService
{
    private const int DiasAntes = 7;
    private const int DiasVencido = 30;

    private readonly IServiceScopeFactory _scopes;
    private readonly EmailService _email;
    private readonly NegocioInfo _negocio;
    private readonly ILogger<RecordatorioMantenimientoService> _log;

    public RecordatorioMantenimientoService(IServiceScopeFactory scopes, EmailService email, NegocioInfo negocio, ILogger<RecordatorioMantenimientoService> log)
    {
        _scopes = scopes; _email = email; _negocio = negocio; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(45), ct); // deja que la app termine de arrancar (y de crear el esquema)
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (DateTime.Now.Hour is >= 8 and < 18) await EnviarAsync(ct);
            }
            catch (Exception ex) { _log.LogWarning(ex, "No se pudieron enviar los recordatorios de mantenimiento."); }
            await Task.Delay(TimeSpan.FromHours(3), ct);
        }
    }

    private async Task EnviarAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlanteriaDbContext>();

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-DiasVencido);
        var hasta = hoy.AddDays(DiasAntes);

        var pendientes = await db.OrdenesTrabajo
            .Include(o => o.Vehiculo).ThenInclude(v => v.Cliente)
            .Where(o => o.Estado == OrdenTrabajo.Entregado && !o.RecordatorioEnviado
                        && o.ProximaFecha != null && o.ProximaFecha >= desde && o.ProximaFecha <= hasta)
            .ToListAsync(ct);

        foreach (var o in pendientes)
        {
            // Si el vehículo ya tuvo un servicio del mismo tipo después de esta orden, el recordatorio ya no aplica.
            var yaAtendido = await db.OrdenesTrabajo.AnyAsync(x => x.IdVehiculo == o.IdVehiculo && x.Tipo == o.Tipo
                && x.Id != o.Id && x.FechaIngreso > o.FechaEntrega && x.Estado != OrdenTrabajo.Cancelado, ct);
            if (yaAtendido)
            {
                o.RecordatorioEnviado = true;
                await db.SaveChangesAsync(ct);
                continue;
            }

            var cli = o.Vehiculo.Cliente;
            if (string.IsNullOrWhiteSpace(cli.Correo)) continue;

            var cuando = o.ProximaFecha!.Value.Date < hoy ? "ya venció" : $"toca el {o.ProximaFecha.Value:dd/MM/yyyy}";
            var cuerpo = EmailService.PlantillaAviso($"Tu mantenimiento: {o.Tipo.ToLower()}", new (string, string?)[]
            {
                ("Vehículo", $"{o.Vehiculo.Placa} · {o.Vehiculo.Marca} {o.Vehiculo.Modelo}"),
                ("Último servicio", o.FechaEntrega?.ToString("dd/MM/yyyy")),
                ("Próximo servicio", o.ProximaFecha.Value.ToString("dd/MM/yyyy"))
            }, $"El próximo servicio de «{o.Tipo.ToLower()}» de tu vehículo {cuando}. Agenda tu cita en nuestro sitio o escríbenos al {_negocio.Telefono}.");

            if (await _email.EnviarCorreoAsync(cli.Correo, $"{cli.Nombres} {cli.Apellidos}", $"Recordatorio de mantenimiento: {o.Tipo}", cuerpo))
            {
                o.RecordatorioEnviado = true;
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
