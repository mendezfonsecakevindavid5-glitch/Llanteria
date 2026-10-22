namespace Llanteria.Infrastructure;

/// <summary>Fechas en lenguaje natural para las tarjetas del sitio.</summary>
public static class Fechas
{
    /// <summary>"hoy", "hace 3 días", "hace 2 meses", "hace 1 año"...</summary>
    public static string Hace(DateTime fecha)
    {
        var d = DateTime.Now - fecha;
        if (d.TotalMinutes < 1) return "justo ahora";
        if (d.TotalHours < 1) return $"hace {(int)d.TotalMinutes} min";
        if (d.TotalDays < 1) return $"hace {(int)d.TotalHours} h";
        var dias = (int)d.TotalDays;
        if (dias < 30) return dias == 1 ? "hace 1 día" : $"hace {dias} días";
        var meses = dias / 30;
        if (meses < 12) return meses == 1 ? "hace 1 mes" : $"hace {meses} meses";
        var anios = dias / 365;
        return anios <= 1 ? "hace 1 año" : $"hace {anios} años";
    }
}
