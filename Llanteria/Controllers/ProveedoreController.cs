using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Llanteria.Controllers;

public class ProveedoreController : Controller
{
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    private readonly ProveedoreService ser;
    private readonly IWebHostEnvironment _env;

    public ProveedoreController(ProveedoreService proveedoreService, IWebHostEnvironment env)
    {
        ser = proveedoreService;
        _env = env;
    }

    public IActionResult Index() => View(ser.GetProveedores());

    public IActionResult Create() => View(new Proveedore());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Proveedore prov)
    {
        ValidarLogo(prov.ImagenArchivo);
        if (!ModelState.IsValid) return View(prov);

        ser.AddProveedor(prov); // genera el Id, que se usa como nombre del logo
        GuardarLogo(prov.Id, prov.ImagenArchivo);
        TempData["Exito"] = "Proveedor registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var prov = ser.GetProveedor(id);
        return prov == null ? NotFound() : View(prov);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Proveedore prov)
    {
        prov.Id = id;
        ValidarLogo(prov.ImagenArchivo);
        if (!ModelState.IsValid) return View(prov);

        ser.UpdateProveedor(prov);
        GuardarLogo(id, prov.ImagenArchivo);
        TempData["Exito"] = "Proveedor actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        ser.DeleteProveedor(id); // si tiene productos, el filtro global avisa del error
        EliminarLogo(id);
        TempData["Exito"] = "Proveedor eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private string Carpeta => Path.Combine(_env.WebRootPath, "images", "proveedores");

    private void ValidarLogo(IFormFile? archivo)
    {
        if (archivo == null) return;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            ModelState.AddModelError(nameof(Proveedore.ImagenArchivo), "El logotipo debe ser una imagen.");
        else if (archivo.Length > 3 * 1024 * 1024)
            ModelState.AddModelError(nameof(Proveedore.ImagenArchivo), "El logotipo no puede superar 3 MB.");
    }

    // Siempre se guarda como {id}.png, que es lo que muestran las vistas.
    private void GuardarLogo(int id, IFormFile? archivo)
    {
        if (archivo == null) return;
        Directory.CreateDirectory(Carpeta);
        using var stream = archivo.OpenReadStream();
        using var imagen = Image.Load(stream);
        imagen.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(400, 400), Mode = ResizeMode.Max }));
        EliminarLogo(id);
        imagen.Save(Path.Combine(Carpeta, id + ".png"), new PngEncoder());
    }

    private void EliminarLogo(int id)
    {
        foreach (var ext in new[] { ".png", ".jpg", ".jpeg" })
        {
            var ruta = Path.Combine(Carpeta, id + ext);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }
}
