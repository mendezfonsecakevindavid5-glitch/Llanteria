using Llanteria.Data;
using Llanteria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Llanteria.Infrastructure;

/// <summary>
/// Deja pasar al administrador y a los usuarios a quienes un administrador invitó como editores del recurso
/// (blog o servicios). Quien no ha iniciado sesión va al login; quien no tiene permiso, a «acceso denegado».
/// </summary>
public class EditorContenidoFilter : IAuthorizationFilter
{
    private readonly string _recurso;

    public EditorContenidoFilter(string recurso) => _recurso = recurso;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (user.IsInRole(RolesApp.Administrador)) return;

        var db = context.HttpContext.RequestServices.GetRequiredService<LlanteriaDbContext>();
        if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) || !db.EsEditor(id, _recurso))
            context.Result = new ForbidResult();
    }
}
