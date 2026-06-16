using Microsoft.AspNetCore.Mvc.Filters;
using Llanteria.Services;
using System.Security.Claims;

namespace Llanteria.Filters
{
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
            // Esperamos a que la acción se ejecute primero
            var resultContext = await next();

            // Solo registramos si la acción fue exitosa (puedes quitar este if si quieres registrar todo)
            if (resultContext.Exception == null)
            {
                var logService = context.HttpContext.RequestServices.GetService<ILogService>();

                // Obtenemos el Id del usuario desde los Claims (ajusta según tu lógica de login)
                var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.TryParse(userIdClaim, out int id) ? id : 0;

                string ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconocida";

                await logService.RegistrarActividadAsync(userId, _accion, _tabla, ip);
            }
        }
    }
}