using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Llanteria.Infrastructure;

/// <summary>
/// El administrador tiene acceso a todo el panel (Dashboard y todos sus formularios): cualquier requisito de
/// autorización pendiente (roles por controlador, [Authorize(Roles = ...)] o políticas) se da por cumplido
/// cuando el usuario tiene el rol "Administrador". Los demás roles siguen limitados por RoleConvention.
/// </summary>
public class AdministradorAccesoTotal : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var u = context.User;
        if (u.Identity?.IsAuthenticated == true &&
            u.FindAll(ClaimTypes.Role).Any(c => string.Equals(c.Value?.Trim(), RolesApp.Administrador, StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var requisito in context.PendingRequirements.ToList())
                context.Succeed(requisito);
        }
        return Task.CompletedTask;
    }
}
