using System.Security.Claims;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Llanteria.Filters;

/// <summary>Registra en la bitácora las acciones exitosas del panel. Nunca interrumpe la acción si el registro falla.</summary>
public class LogActionFilter : ActionFilterAttribute
{
    private readonly string _accion;
    private readonly string _tabla;

    public LogActionFilter(string accion, string tabla)
    {
        _accion = accion;
        _tabla = tabla;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultado = await next();
        if (resultado.Exception != null && !resultado.ExceptionHandled) return;

        var http = context.HttpContext;
        // Si el usuario no está identificado no hay a quién atribuir el registro (la tabla exige un usuario válido).
        if (!int.TryParse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) || userId <= 0) return;

        try
        {
            var log = http.RequestServices.GetService<ILogService>();
            if (log == null) return;
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "Desconocida";
            await log.RegistrarActividadAsync(userId, _accion, _tabla, ip);
        }
        catch (Exception ex)
        {
            http.RequestServices.GetService<ILogger<LogActionFilter>>()?.LogWarning(ex, "No se pudo registrar la actividad.");
        }
    }
}
