using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Llanteria.Services;

/// <summary>
/// Guarda y borra las imágenes que se suben desde el panel (productos, artículos del blog...).
/// Las reduce al tamaño necesario y las convierte a WebP, que pesa mucho menos.
/// </summary>
public class AlmacenImagenes
{
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;

    public AlmacenImagenes(IWebHostEnvironment env) => _env = env;

    /// <summary>Devuelve el mensaje de error si el archivo no sirve; null si es válido (o no se envió).</summary>
    public static string? Validar(IFormFile? archivo)
    {
        if (archivo == null) return null;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            return "El archivo debe ser una imagen (jpg, png, webp, gif o bmp).";
        if (archivo.Length > MaxBytes)
            return "La imagen no puede superar 5 MB.";
        return null;
    }

    /// <summary>Guarda la imagen en wwwroot/images/{carpeta} y devuelve el nombre del archivo.</summary>
    public string Guardar(IFormFile archivo, string carpeta, int ladoMaximo = 800)
    {
        var destino = Path.Combine(_env.WebRootPath, "images", carpeta);
        Directory.CreateDirectory(destino);
        var nombre = Guid.NewGuid().ToString("N") + ".webp";

        using var stream = archivo.OpenReadStream();
        using var imagen = Image.Load(stream);
        imagen.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(ladoMaximo, ladoMaximo), Mode = ResizeMode.Max }));
        imagen.Save(Path.Combine(destino, nombre), new WebpEncoder { Quality = 75 });
        return nombre;
    }

    /// <summary>Borra la imagen, salvo que sea la imagen por defecto de la carpeta o una de las que vienen con el proyecto.</summary>
    public void Eliminar(string? nombre, string carpeta, string? imagenPorDefecto = null)
    {
        if (string.IsNullOrEmpty(nombre) || nombre == imagenPorDefecto) return;
        var ruta = Path.Combine(_env.WebRootPath, "images", carpeta, Path.GetFileName(nombre));
        if (File.Exists(ruta)) File.Delete(ruta);
    }
}
