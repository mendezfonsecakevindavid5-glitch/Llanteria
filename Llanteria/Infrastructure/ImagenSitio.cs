namespace Llanteria.Infrastructure;

/// <summary>
/// Busca en wwwroot/images una imagen fija del sitio por el comienzo de su nombre
/// (p. ej. "banner-auto" encuentra banner-auto.jpg, Banner-Auto.webp...). Prueba los prefijos en orden y devuelve la primera
/// que exista, o null si no hay ninguna (el diseño trae su propio color de respaldo).
/// El ?v= hace que el navegador recargue la imagen cuando la reemplazas.
/// </summary>
public static class ImagenSitio
{
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif" };

    public static string? Url(IWebHostEnvironment env, params string[] prefijos)
    {
        var carpeta = Path.Combine(env.WebRootPath, "images");
        if (!Directory.Exists(carpeta)) return null;

        foreach (var prefijo in prefijos)
        {
            var archivo = Directory.EnumerateFiles(carpeta, prefijo + "*.*")
                .Where(f => Extensiones.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (archivo != null)
                return "/images/" + Uri.EscapeDataString(Path.GetFileName(archivo)) + "?v=" + File.GetLastWriteTimeUtc(archivo).Ticks;
        }
        return null;
    }
}
