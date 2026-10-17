using System.Globalization;

namespace Llanteria.Infrastructure;

/// <summary>Color aleatorio del círculo con iniciales y color de letra que siempre contrasta con él.</summary>
public static class ColorAvatar
{
    /// <summary>Un color aleatorio por inicio de sesión (claro u oscuro, para que la letra cambie de color).</summary>
    public static string Aleatorio()
    {
        var r = Random.Shared;
        return Hsl(r.Next(0, 360), r.Next(45, 90) / 100.0, r.Next(22, 82) / 100.0);
    }

    /// <summary>Si el color es de respaldo (sesiones anteriores sin color), se deriva del nombre: siempre el mismo.</summary>
    public static string DeNombre(string nombre)
    {
        uint h = 2166136261;
        foreach (var c in nombre) h = (h ^ c) * 16777619;
        return Hsl((int)(h % 360), .65, .45);
    }

    public static bool EsValido(string? hex)
        => hex is { Length: 7 } && hex[0] == '#' && int.TryParse(hex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

    /// <summary>Blanco sobre fondos oscuros, casi negro sobre fondos claros (contraste WCAG).</summary>
    public static string TextoPara(string hex)
    {
        var (r, g, b) = Rgb(hex);
        static double Lin(int v) { var c = v / 255.0; return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
        var luminancia = 0.2126 * Lin(r) + 0.7152 * Lin(g) + 0.0722 * Lin(b);
        return luminancia > 0.35 ? "#111111" : "#FFFFFF";
    }

    private static (int r, int g, int b) Rgb(string hex)
        => (int.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber), int.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber), int.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber));

    private static string Hsl(int h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s, x = c * (1 - Math.Abs(h / 60.0 % 2 - 1)), m = l - c / 2;
        (double r, double g, double b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };
        int B(double v) => (int)Math.Round((v + m) * 255);
        return $"#{B(r):X2}{B(g):X2}{B(b):X2}";
    }
}
