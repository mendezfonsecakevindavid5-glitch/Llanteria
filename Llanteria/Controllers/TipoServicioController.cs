using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

public class TipoServicioController : Controller
{
    private const string ImagenPorDefecto = "default-servicio.png";
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly TipoServicioService ser;
    private readonly IWebHostEnvironment _env;

    public TipoServicioController(TipoServicioService tipoServicioService, IWebHostEnvironment env)
    {
        ser = tipoServicioService;
        _env = env;
    }

    private string Carpeta => Path.Combine(_env.WebRootPath, "images", "servicios");

    public IActionResult Index() => View(ser.GetTiposServicio());

    public IActionResult Create() => View(new TipoServicio());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(TipoServicio ts)
    {
        ValidarImagen(ts.ImagenArchivo);
        if (!ModelState.IsValid) return View(ts);

        ts.IconoPath = ts.ImagenArchivo != null ? GuardarImagen(ts.ImagenArchivo) : ImagenPorDefecto;
        ser.AddTipoServicio(ts);
        TempData["Exito"] = "Servicio registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var ts = ser.GetTipoServicio(id);
        return ts == null ? NotFound() : View(ts);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TipoServicio ts)
    {
        ts.Id = id;
        ValidarImagen(ts.ImagenArchivo);
        var existente = ser.GetTipoServicio(id);
        if (existente == null) return NotFound();
        if (!ModelState.IsValid)
        {
            ts.IconoPath = existente.IconoPath;
            return View(ts);
        }

        if (ts.ImagenArchivo != null)
        {
            ts.IconoPath = GuardarImagen(ts.ImagenArchivo);
            EliminarImagen(existente.IconoPath);
        }
        else
        {
            ts.IconoPath = existente.IconoPath;
        }

        ser.UpdateTipoServicio(ts);
        TempData["Exito"] = "Servicio actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var ts = ser.GetTipoServicio(id);
        if (ts == null) return RedirectToAction(nameof(Index));
        var imagen = ts.IconoPath;
        ser.DeleteTipoServicio(id); // si tiene facturas, el filtro global avisa del error
        EliminarImagen(imagen);
        TempData["Exito"] = "Servicio eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarImagen(IFormFile? archivo)
    {
        if (archivo == null) return;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            ModelState.AddModelError(nameof(TipoServicio.ImagenArchivo), "El archivo debe ser una imagen.");
        else if (archivo.Length > 5 * 1024 * 1024)
            ModelState.AddModelError(nameof(TipoServicio.ImagenArchivo), "La imagen no puede superar 5 MB.");
    }

    private string GuardarImagen(IFormFile archivo)
    {
        Directory.CreateDirectory(Carpeta);
        var nombre = Guid.NewGuid().ToString("N") + Path.GetExtension(archivo.FileName).ToLowerInvariant();
        using var fs = new FileStream(Path.Combine(Carpeta, nombre), FileMode.Create);
        archivo.CopyTo(fs);
        return nombre;
    }

    private void EliminarImagen(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre == ImagenPorDefecto || nombre.StartsWith("fa-")) return;
        var ruta = Path.Combine(Carpeta, Path.GetFileName(nombre));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }
}
