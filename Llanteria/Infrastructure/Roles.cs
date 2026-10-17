using System.Security.Claims;

namespace Llanteria.Infrastructure;

/// <summary>
/// Nombres de rol del sistema.
///
///  • Administrador / Empleado: personal del negocio (panel).
///  • Cliente: rol anterior de quienes ya estaban registrados ANTES de existir "Usuario registrado". Se respeta tal cual
///    (conservan MiCuenta, puntos y reseñas); no se les cambia el rol.
///  • Usuario registrado: rol por defecto de todo el que se registra desde ahora. Solo accede a su perfil y al sitio público.
///  • Usuario no registrado: rol de todo visitante SIN sesión. No existe como fila en la tabla Roles porque no hay un
///    usuario al que asignársela: la aplicación se la da a la solicitud anónima (ver UseRolVisitante).
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";
    public const string Cliente = "Cliente";
    public const string UsuarioRegistrado = "Usuario registrado";
    public const string UsuarioNoRegistrado = "Usuario no registrado";

    /// <summary>Roles que NO se pueden asignar desde los formularios del panel (son de clientes o del visitante).</summary>
    public static readonly string[] NoAsignables = { Cliente, UsuarioRegistrado, UsuarioNoRegistrado };

    public static bool EsPersonal(this ClaimsPrincipal u) => u.IsInRole(Administrador) || u.IsInRole(Empleado);

    /// <summary>
    /// Toda solicitud sin sesión recibe el rol "Usuario no registrado". La identidad se crea SIN tipo de autenticación,
    /// así que IsAuthenticated sigue siendo false y [Authorize] sigue mandando al login.
    /// </summary>
    public static IApplicationBuilder UseRolVisitante(this IApplicationBuilder app) => app.Use(async (ctx, next) =>
    {
        if (ctx.User.Identity?.IsAuthenticated != true)
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, UsuarioNoRegistrado) }));
        await next();
    });
}
