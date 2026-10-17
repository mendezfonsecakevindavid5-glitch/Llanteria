using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.ViewComponents;

/// <summary>
/// Círculo con la foto del usuario (o sus iniciales) y el mensaje "Cuenta / Cerrar sesión".
/// No carga la foto: solo averigua si existe; la imagen se pide aparte (User/Foto) para no arrastrar
/// megabytes en cada página.
/// </summary>
public class UserMenuViewComponent : ViewComponent
{
    private readonly LlanteriaDbContext _db;
    public UserMenuViewComponent(LlanteriaDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = HttpContext.User;
        var username = user.Identity?.Name ?? "Usuario";
        _ = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id);

        var d = await _db.Usuarios.AsNoTracking().Where(u => u.Id == id).Select(u => new
        {
            CNombres = u.IdClienteNavigation != null ? u.IdClienteNavigation.Nombres : null,
            CApellidos = u.IdClienteNavigation != null ? u.IdClienteNavigation.Apellidos : null,
            ENombres = u.IdEmpleadoNavigation != null ? u.IdEmpleadoNavigation.Nombres : null,
            EApellidos = u.IdEmpleadoNavigation != null ? u.IdEmpleadoNavigation.Apellidos : null,
            FotoPerfil = u.PerfilUsuario != null && u.PerfilUsuario.FotoCircular != null,
            FotoCliente = u.IdClienteNavigation != null && u.IdClienteNavigation.Foto != null,
            FotoEmpleado = u.IdEmpleadoNavigation != null && u.IdEmpleadoNavigation.Foto != null
        }).FirstOrDefaultAsync();

        var nombres = d?.CNombres ?? d?.ENombres;
        var apellidos = d?.CApellidos ?? d?.EApellidos;

        // Primera inicial del primer nombre + primera inicial del primer apellido.
        var iniciales = $"{Inicial(nombres)}{Inicial(apellidos)}";
        if (iniciales.Length < 2) // cuentas sin ficha de persona: se usan las letras del usuario
            iniciales = new string(username.Where(char.IsLetterOrDigit).Take(2).ToArray()).ToUpperInvariant();
        if (iniciales.Length == 0) iniciales = "U";

        var fondo = user.FindFirstValue("AvatarColor");
        if (!ColorAvatar.EsValido(fondo)) fondo = ColorAvatar.DeNombre(username);

        return View(new UserMenuModel
        {
            Iniciales = iniciales,
            ColorFondo = fondo!,
            ColorTexto = ColorAvatar.TextoPara(fondo!),
            TieneFoto = d is { FotoPerfil: true } || d is { FotoCliente: true } || d is { FotoEmpleado: true },
            Nombre = string.Join(' ', new[] { nombres?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(), apellidos?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() }
                .Where(x => !string.IsNullOrEmpty(x)))
        });
    }

    private static string Inicial(string? texto)
    {
        var t = texto?.Trim();
        return string.IsNullOrEmpty(t) ? "" : char.ToUpperInvariant(t[0]).ToString();
    }
}

public class UserMenuModel
{
    public string Iniciales { get; set; } = "";
    public string ColorFondo { get; set; } = "#000000";
    public string ColorTexto { get; set; } = "#FFFFFF";
    public bool TieneFoto { get; set; }
    public string Nombre { get; set; } = "";
}
