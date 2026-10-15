using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

/// <summary>
/// Cada 30 minutos, entre las 8:00 y las 18:00, envía por correo el recordatorio de las citas de mañana
/// (confirmadas y con correo). Marca cada cita para no repetirlo. Si Brevo no está configurado, no hace nada.
/// </summary>
public class RecordatorioCitasService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly EmailService _email;
    private readonly NegocioInfo _negocio;
    private readonly ILogger<RecordatorioCitasService> _log;

    public RecordatorioCitasService(IServiceScopeFactory scopes, EmailService email, NegocioInfo negocio, ILogger<RecordatorioCitasService> log)
    {
        _scopes = scopes; _email = email; _negocio = negocio; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), ct); // deja que la app termine de arrancar (y de crear el esquema)
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var hora = DateTime.Now.Hour;
                if (hora is >= 8 and < 18) await EnviarAsync(ct);
            }
            catch (Exception ex) { _log.LogWarning(ex, "No se pudieron enviar los recordatorios de citas."); }
            await Task.Delay(TimeSpan.FromMinutes(30), ct);
        }
    }

    private async Task EnviarAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlanteriaDbContext>();
        var manana = DateTime.Today.AddDays(1);
        var citas = await db.Citas
            .Where(c => c.Fecha == manana && c.Estado == Cita.Confirmada && !c.RecordatorioEnviado && c.Correo != null && c.Correo != "")
            .ToListAsync(ct);

        foreach (var c in citas)
        {
            var cuerpo = EmailService.PlantillaAviso("Recordatorio de tu cita", new (string, string?)[]
            {
                ("Servicio", c.Servicio), ("Fecha", c.Fecha.ToString("dd/MM/yyyy")), ("Hora", c.Hora),
                ("Vehículo", c.Vehiculo), ("Lugar", c.Direccion ?? _negocio.Direccion)
            }, $"Te esperamos mañana. Si no puedes asistir, avísanos por WhatsApp o al {_negocio.Telefono}.");
            if (await _email.EnviarCorreoAsync(c.Correo!, c.Solicitante, $"Recordatorio: {c.Servicio} mañana a las {c.Hora}", cuerpo))
            {
                c.RecordatorioEnviado = true;
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
