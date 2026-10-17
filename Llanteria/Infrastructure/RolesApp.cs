namespace Llanteria.Infrastructure;

/// <summary>
/// Nombres de rol. "Usuario registrado" es el rol por defecto de quien se registra (solo ve su perfil).
/// "Usuario no registrado" es el rol implícito de quien no tiene sesión: no existe como fila en la base
/// porque nadie puede tenerlo asignado; <see cref="De"/> lo devuelve cuando no hay sesión iniciada.
/// </summary>
public static class RolesApp
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";
    public const string Cliente = "Cliente";
    public const string UsuarioRegistrado = "Usuario registrado";
    public const string UsuarioNoRegistrado = "Usuario no registrado";

    public static string De(System.Security.Claims.ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return UsuarioNoRegistrado;
        return user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? UsuarioRegistrado;
    }

    public static bool EsPersonal(System.Security.Claims.ClaimsPrincipal user)
        => user.IsInRole(Administrador) || user.IsInRole(Empleado);
}
