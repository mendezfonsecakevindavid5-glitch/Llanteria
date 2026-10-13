using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Llanteria.Controllers;

public class ProductoController : Controller
{
    private const string ImagenPorDefecto = "default-producto.png";
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    private readonly ProductoService ser;
    private readonly ProveedoreService provSer;
    private readonly MarcaService marcSer;
    private readonly BodegaService bodSer;
    private readonly IWebHostEnvironment _env;

    public ProductoController(ProductoService productoService, ProveedoreService proveedoreService,
        MarcaService marcaService, BodegaService bodegaService, IWebHostEnvironment env)
    {
        ser = productoService;
        provSer = proveedoreService;
        marcSer = marcaService;
        bodSer = bodegaService;
        _env = env;
    }

    public IActionResult Index() => View(ser.GetProductos());

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Producto { DetalleProducto = new DetalleProducto() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Producto p)
    {
        ValidarImagen(p.ImagenArchivo);
        ValidarDetalle(p);
        if (!ModelState.IsValid)
        {
            CargarCombos();
            return View(p);
        }

        LimpiarDetalle(p);
        p.RutaImagen = p.ImagenArchivo != null ? GuardarImagen(p.ImagenArchivo) : ImagenPorDefecto;
        ser.AddProducto(p);
        TempData["Exito"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var producto = ser.GetProducto(id);
        if (producto == null) return NotFound();
        producto.DetalleProducto ??= new DetalleProducto();
        CargarCombos();
        return View(producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Producto p)
    {
        p.Id = id;
        ValidarImagen(p.ImagenArchivo);
        ValidarDetalle(p);
        var original = ser.GetProducto(id);
        if (original == null) return NotFound();

        if (!ModelState.IsValid)
        {
            p.RutaImagen = original.RutaImagen;
            CargarCombos();
            return View(p);
        }

        LimpiarDetalle(p);
        var imagenAnterior = original.RutaImagen;
        if (p.ImagenArchivo != null)
        {
            p.RutaImagen = GuardarImagen(p.ImagenArchivo);
            EliminarImagen(imagenAnterior);
        }
        else
        {
            p.RutaImagen = imagenAnterior;
        }

        ser.UpdateProducto(p);
        TempData["Exito"] = "Producto actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var p = ser.GetProducto(id);
        if (p == null) return RedirectToAction(nameof(Index));
        var imagen = p.RutaImagen;
        ser.DeleteProducto(id);
        EliminarImagen(imagen);
        TempData["Exito"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarDetalle(Producto p)
    {
        var d = p.DetalleProducto;
        if (d == null || d.IdMarca > 0) return;
        var conDatos = !string.IsNullOrWhiteSpace(d.Ancho) || !string.IsNullOrWhiteSpace(d.Perfil) || !string.IsNullOrWhiteSpace(d.Diametro)
                       || !string.IsNullOrWhiteSpace(d.Viscosidad) || !string.IsNullOrWhiteSpace(d.TipoAceite) || d.GarantiaMeses.HasValue;
        if (conDatos) ModelState.AddModelError("DetalleProducto.IdMarca", "Selecciona la marca para guardar la ficha técnica.");
    }

    // Si no se eligió marca, no se guarda el detalle (evita violar la llave foránea).
    private static void LimpiarDetalle(Producto p)
    {
        if (p.DetalleProducto == null || p.DetalleProducto.IdMarca <= 0) p.DetalleProducto = null;
    }

    private void ValidarImagen(IFormFile? archivo)
    {
        if (archivo == null) return;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            ModelState.AddModelError(nameof(Producto.ImagenArchivo), "El archivo debe ser una imagen (jpg, png, webp, gif o bmp).");
        else if (archivo.Length > 5 * 1024 * 1024)
            ModelState.AddModelError(nameof(Producto.ImagenArchivo), "La imagen no puede superar 5 MB.");
    }

    private string GuardarImagen(IFormFile archivo)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "images", "productoos");
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + ".webp";

        using var stream = archivo.OpenReadStream();
        using var imagen = Image.Load(stream);
        imagen.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(800, 800), Mode = ResizeMode.Max }));
        imagen.Save(Path.Combine(carpeta, nombre), new WebpEncoder { Quality = 75 });
        return nombre;
    }

    private void EliminarImagen(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre == ImagenPorDefecto) return;
        var ruta = Path.Combine(_env.WebRootPath, "images", "productoos", Path.GetFileName(nombre));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }

    private void CargarCombos()
    {
        ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
        ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
        ViewBag.IdBodega = new SelectList(bodSer.GetBodegas(), "Id", "NombreBodega");
    }
}
