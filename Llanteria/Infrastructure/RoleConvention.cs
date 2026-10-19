using Llanteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Llanteria.Infrastructure;

/// <summary>
/// Protege por rol los controladores del panel. Antes cualquier visitante podía abrir
/// /Producto/Delete/1 o /Usuario sin iniciar sesión.
/// </summary>
public class RoleConvention : IControllerModelConvention
{
    private static readonly HashSet<string> SoloAdmin = new()
    {
        "Producto", "Marca", "Bodega", "Proveedore", "Gasto", "Empleado",
        "Usuario", "Log", "TipoServicio", "CatalogoIncentivo", "Editores", "Sugerencias"
    };

    private static readonly HashSet<string> Personal = new()
    {
        "Dashboard", "Factura", "Cliente", "Vehiculo", "Inventario", "Pqrs", "OrdenTrabajo"
    };

    // Contenido que el administrador puede compartir con editores invitados (ver EditorContenido).
    private static readonly Dictionary<string, string> ContenidoCompartido = new()
    {
        ["Articulo"] = EditorContenido.Blog,
        ["ServicioWeb"] = EditorContenido.Servicios
    };

    public void Apply(ControllerModel controller)
    {
        if (ContenidoCompartido.TryGetValue(controller.ControllerName, out var recurso))
        {
            controller.Filters.Add(new EditorContenidoFilter(recurso));
            return;
        }

        string[]? roles = null;

        if (SoloAdmin.Contains(controller.ControllerName))
            roles = new[] { "Administrador" };
        else if (Personal.Contains(controller.ControllerName))
            roles = new[] { "Administrador", "Empleado" };

        if (roles == null) return;

        var policy = new AuthorizationPolicyBuilder().RequireRole(roles).Build();
        controller.Filters.Add(new AuthorizeFilter(policy));
    }
}
