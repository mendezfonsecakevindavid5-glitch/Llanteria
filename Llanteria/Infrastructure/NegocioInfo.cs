using System.Globalization;

namespace Llanteria.Infrastructure;

/// <summary>
/// Datos del negocio en un solo lugar (pie de página, contacto, mapa, WhatsApp, datos para Google, correos).
/// Los valores de abajo son los de partida: cámbialos en appsettings.json, sección "Negocio" (mira appsettings.EJEMPLO.json).
/// IMPORTANTE: ajusta Direccion, Latitud y Longitud a la ubicación real del local.
/// </summary>
public class NegocioInfo
{
    public string Nombre { get; set; } = "Llantería Valledupar";
    /// <summary>NIT del negocio. Si se llena (appsettings.json, sección "Negocio"), aparece en facturas, reportes y Excel.</summary>
    public string Nit { get; set; } = "";
    /// <summary>Texto legal al pie de las facturas en PDF (por ejemplo, resolución de numeración o régimen).</summary>
    public string LeyendaFactura { get; set; } = "Documento generado por el sistema de gestión de Llantería Valledupar.";
    public string Telefono { get; set; } = "+57 311 772 3978";
    /// <summary>Solo dígitos con indicativo de país, para wa.me.</summary>
    public string WhatsApp { get; set; } = "573117723978";
    public string Correo { get; set; } = "superllantasmedellinsas@hotmail.com";
    public string Direccion { get; set; } = "Valledupar, Cesar";
    public string Ciudad { get; set; } = "Valledupar";
    public double Latitud { get; set; } = 10.4631;
    public double Longitud { get; set; } = -73.2532;
    public string SemanaAbre { get; set; } = "07:30";
    public string SemanaCierra { get; set; } = "17:30";
    public string SabadoAbre { get; set; } = "08:00";
    public string SabadoCierra { get; set; } = "16:00";
    /// <summary>Opcional: enlace de "Cómo llegar" de Google Maps copiado de la ficha del negocio.</summary>
    public string? EnlaceMapa { get; set; }

    private static string Hora(string h)
    {
        if (!TimeSpan.TryParse(h, CultureInfo.InvariantCulture, out var t)) return h;
        var h12 = t.Hours % 12 == 0 ? 12 : t.Hours % 12;
        return $"{h12}:{t.Minutes:00} {(t.Hours < 12 ? "a.m." : "p.m.")}";
    }

    public string HorarioSemana => $"{Hora(SemanaAbre)} a {Hora(SemanaCierra)}";
    public string HorarioSabado => $"{Hora(SabadoAbre)} a {Hora(SabadoCierra)}";

    private string Coord => $"{Latitud.ToString(CultureInfo.InvariantCulture)},{Longitud.ToString(CultureInfo.InvariantCulture)}";
    public string MapaIncrustado => $"https://maps.google.com/maps?q={Coord}&z=16&output=embed";
    public string ComoLlegar => string.IsNullOrWhiteSpace(EnlaceMapa) ? $"https://www.google.com/maps/dir/?api=1&destination={Coord}" : EnlaceMapa!;

    public string TelefonoLimpio => new string(Telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());

    public string LinkWhatsApp(string? mensaje = null)
        => $"https://wa.me/{WhatsApp}" + (Uri.EscapeDataString(string.IsNullOrWhiteSpace(mensaje) ? "Hola, quiero información sobre llantas y servicios." : mensaje).Insert(0, "?text="));

    /// <summary>Convierte un teléfono colombiano (10 dígitos) al formato de wa.me; null si no sirve.</summary>
    public static string? WhatsAppDe(string? telefono)
    {
        var d = new string((telefono ?? "").Where(char.IsDigit).ToArray());
        if (d.Length == 10) d = "57" + d;
        return d.Length >= 11 ? d : null;
    }

    /// <summary>Datos estructurados (schema.org) para que Google muestre el negocio en búsquedas y mapas.</summary>
    public string JsonLd(string url)
    {
        var datos = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "TireShop",
            ["name"] = Nombre,
            ["url"] = url,
            ["telephone"] = Telefono,
            ["email"] = Correo,
            ["address"] = new Dictionary<string, object>
            {
                ["@type"] = "PostalAddress", ["streetAddress"] = Direccion, ["addressLocality"] = Ciudad,
                ["addressRegion"] = "Cesar", ["addressCountry"] = "CO"
            },
            ["geo"] = new Dictionary<string, object> { ["@type"] = "GeoCoordinates", ["latitude"] = Latitud, ["longitude"] = Longitud },
            ["openingHoursSpecification"] = new object[]
            {
                new Dictionary<string, object> { ["@type"] = "OpeningHoursSpecification", ["dayOfWeek"] = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }, ["opens"] = SemanaAbre, ["closes"] = SemanaCierra },
                new Dictionary<string, object> { ["@type"] = "OpeningHoursSpecification", ["dayOfWeek"] = "Saturday", ["opens"] = SabadoAbre, ["closes"] = SabadoCierra }
            }
        };
        return System.Text.Json.JsonSerializer.Serialize(datos);
    }
}
