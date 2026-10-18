using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class ProductoController : Controller
{
    private const string ImagenPorDefecto = "default-producto.png";
    private const string CarpetaImagenes = "productoos";

    private readonly ProductoService ser;
    private readonly ProveedoreService provSer;
    private readonly MarcaService marcSer;
    private readonly BodegaService bodSer;
    private readonly AlmacenImagenes _imagenes;

    public ProductoController(ProductoService productoService, ProveedoreService proveedoreService,
        MarcaService marcaService, BodegaService bodegaService, AlmacenImagenes imagenes)
    {
        ser = productoService;
        provSer = proveedoreService;
        marcSer = marcaService;
        bodSer = bodegaService;
        _imagenes = imagenes;
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
        var error = AlmacenImagenes.Validar(archivo);
        if (error != null) ModelState.AddModelError(nameof(Producto.ImagenArchivo), error);
    }

    private string GuardarImagen(IFormFile archivo) => _imagenes.Guardar(archivo, CarpetaImagenes);

    private void EliminarImagen(string? nombre) => _imagenes.Eliminar(nombre, CarpetaImagenes, ImagenPorDefecto);

    private void CargarCombos()
    {
        ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
        ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
        ViewBag.IdBodega = new SelectList(bodSer.GetBodegas(), "Id", "NombreBodega");
    }
}
