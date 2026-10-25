namespace Llanteria.Infrastructure;

/// <summary>
/// Busca en wwwroot/images una imagen fija del sitio por el comienzo de su nombre
/// (p. ej. "banner-auto" encuentra banner-auto.jpg, Banner-Auto.webp...). Prueba los prefijos en orden y devuelve la primera
/// que exista, o null si no hay ninguna (el diseño trae su propio color de respaldo).
/// Un prefijo puede incluir subcarpetas de wwwroot/images: "banners/banner-moto" busca wwwroot/images/banners/banner-moto*.*
/// El ?v= hace que el navegador recargue la imagen cuando la reemplazas.
/// Solo se aceptan fotos (jpg, jpeg, png, webp, avif, gif): un .svg se ignora a propósito, porque los banners deben ser imágenes reales.
/// </summary>
public static class ImagenSitio
{
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif" };

    public static string? Url(IWebHostEnvironment env, params string[] prefijos)
    {
        var raiz = Path.Combine(env.WebRootPath, "images");
        if (!Directory.Exists(raiz)) return null;

        foreach (var prefijo in prefijos)
        {
            var partes = prefijo.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0 || partes.Contains("..")) continue; // nunca salir de wwwroot/images

            var nombre = partes[^1];
            var subcarpetas = partes[..^1];
            var carpeta = Path.Combine(new[] { raiz }.Concat(subcarpetas).ToArray());
            if (!Directory.Exists(carpeta)) continue;

            var archivo = Directory.EnumerateFiles(carpeta, nombre + "*.*")
                .Where(f => Extensiones.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (archivo == null) continue;

            var ruta = string.Join('/', subcarpetas.Append(Path.GetFileName(archivo)).Select(Uri.EscapeDataString));
            return "/images/" + ruta + "?v=" + File.GetLastWriteTimeUtc(archivo).Ticks;
        }
        return null;
    }
}
