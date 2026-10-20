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
    /// <summary>Qué roles pueden entrar a cada controlador del panel. El que no aparece aquí es público
    /// (sitio) o tiene su propia regla (ver <see cref="ContenidoCompartido"/>).</summary>
    private static readonly Dictionary<string, string[]> RolesPorControlador = new()
    {
        ["Producto"] = new[] { RolesApp.Administrador },
        ["Marca"] = new[] { RolesApp.Administrador },
        ["Bodega"] = new[] { RolesApp.Administrador },
        ["Proveedore"] = new[] { RolesApp.Administrador },
        ["Gasto"] = new[] { RolesApp.Administrador },
        ["Empleado"] = new[] { RolesApp.Administrador },
        ["Log"] = new[] { RolesApp.Administrador },
        ["TipoServicio"] = new[] { RolesApp.Administrador },
        ["CatalogoIncentivo"] = new[] { RolesApp.Administrador },
        ["Editores"] = new[] { RolesApp.Administrador },
        ["Sugerencias"] = new[] { RolesApp.Administrador },

        ["Dashboard"] = new[] { RolesApp.Administrador, RolesApp.Secretario, RolesApp.Empleado },
        ["Factura"] = new[] { RolesApp.Administrador, RolesApp.Empleado },
        ["Cliente"] = new[] { RolesApp.Administrador, RolesApp.Empleado },
        ["Vehiculo"] = new[] { RolesApp.Administrador, RolesApp.Empleado },
        ["Inventario"] = new[] { RolesApp.Administrador, RolesApp.Empleado },
        ["Pqrs"] = new[] { RolesApp.Administrador, RolesApp.Empleado },
        ["OrdenTrabajo"] = new[] { RolesApp.Administrador, RolesApp.Empleado },

        // El secretario entra al panel y puede crear usuarios, pero solo con rol "Empleado"
        // (esa restricción de qué rol puede asignar va en UsuarioController, no aquí).
        ["Usuario"] = new[] { RolesApp.Administrador, RolesApp.Secretario }
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

        if (!RolesPorControlador.TryGetValue(controller.ControllerName, out var roles)) return;

        var policy = new AuthorizationPolicyBuilder().RequireRole(roles).Build();
        controller.Filters.Add(new AuthorizeFilter(policy));
    }
}
