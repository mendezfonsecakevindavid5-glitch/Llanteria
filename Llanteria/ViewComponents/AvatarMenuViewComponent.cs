using System.Security.Cryptography;
using System.Text;
using Llanteria.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.ViewComponents;

/// <summary>
/// Avatar circular del usuario con sesión y su menú (Cuenta / Cerrar sesión).
/// Muestra la foto guardada en la base (se pide a User/Foto); si no hay foto, las iniciales
/// (primer nombre + primer apellido) sobre un color de fondo propio de cada usuario, con letras
/// negras o blancas según el brillo del fondo para que siempre se lean.
/// </summary>
public class AvatarMenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int tamano = 44, bool modal = false)
    {
        var u = HttpContext.User;
        var username = u.Identity?.Name ?? "Usuario";
        var nombres = u.FindFirst("Nombres")?.Value;
        var apellidos = u.FindFirst("Apellidos")?.Value;

        var (fondo, texto) = Colores(username);
        var rol = RolesApp.De(u); // Administrador, Secretario, Empleado, Cliente o Usuario registrado
        // El administrador, el secretario y el empleado trabajan en el panel: desde "Cuenta" (que usa el sitio
        // público) necesitan un camino de vuelta al Dashboard, porque ese menú no está dentro del panel.
        var esPersonal = RolesApp.EsPersonal(u);
        return View(new AvatarMenuModel(Iniciales(nombres, apellidos, username), fondo, texto, tamano, username, modal, rol, esPersonal));
    }

    /// <summary>Primera letra del primer nombre + primera letra del primer apellido. Sin datos: las 2 primeras letras del usuario.</summary>
    internal static string Iniciales(string? nombres, string? apellidos, string username)
    {
        static string Primera(string? s)
        {
            var t = (s ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrEmpty(t) ? "" : char.ToUpperInvariant(t[0]).ToString();
        }

        var ini = Primera(nombres) + Primera(apellidos);
        if (ini.Length > 0) return ini;
        var limpio = new string(username.Where(char.IsLetterOrDigit).ToArray());
        return (limpio.Length >= 2 ? limpio[..2] : limpio.PadRight(1, 'U')).ToUpperInvariant();
    }

    /// <summary>
    /// Color de fondo "aleatorio" pero estable para cada usuario (si fuera aleatorio en cada carga, el avatar cambiaría
    /// de color en cada página). Sale de un hash del usuario. El color de las letras se elige por luminancia (WCAG).
    /// </summary>
    internal static (string Fondo, string Texto) Colores(string semilla)
    {
        var h = SHA256.HashData(Encoding.UTF8.GetBytes(semilla));
        double tono = (h[0] << 8 | h[1]) % 360;
        double sat = 0.45 + (h[2] % 40) / 100.0;   // 45–84 %
        double luz = 0.22 + (h[3] % 60) / 100.0;   // 22–81 %: incluye fondos oscuros (letras blancas) y claros (letras negras)

        var (r, g, b) = HslARgb(tono, sat, luz);
        double L = 0.2126 * Lineal(r) + 0.7152 * Lineal(g) + 0.0722 * Lineal(b);
        // Contraste con blanco = 1.05 / (L + 0.05); con negro = (L + 0.05) / 0.05. Gana el mayor.
        var texto = 1.05 / (L + 0.05) >= (L + 0.05) / 0.05 ? "#FFFFFF" : "#000000";
        return ($"#{r:X2}{g:X2}{b:X2}", texto);
    }

    private static double Lineal(int c)
    {
        var v = c / 255.0;
        return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    private static (int R, int G, int B) HslARgb(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        double m = l - c / 2;
        (double r, double g, double b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };
        return ((int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255));
    }
}

public record AvatarMenuModel(string Iniciales, string Fondo, string Texto, int Tamano, string Usuario, bool Modal = false, string Rol = "", bool EsPersonal = false);
