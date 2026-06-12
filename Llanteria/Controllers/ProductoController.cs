using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting; // 👈 Obligatorio para IWebHostEnvironment
using System.IO;
using System;
using System.Linq;

namespace Llanteria.Controllers;

public class ProductoController : Controller
{
    private readonly ProductoService ser;
    private readonly ProveedoreService provSer; // Para el combo de proveedores
    private readonly MarcaService marcSer; // Para el combo de marcas
    private readonly IWebHostEnvironment _webHostEnvironment; // 👈 CLAVE para encontrar la carpeta wwwroot

    // Inyectamos el entorno web dentro del constructor actual
    public ProductoController(ProductoService productoService, ProveedoreService proveedoreService, MarcaService marcaService, IWebHostEnvironment webHostEnvironment)
    {
        ser = productoService;
        provSer = proveedoreService;
        marcSer = marcaService;
        _webHostEnvironment = webHostEnvironment;
    }

    // Listado de productos
    public ActionResult Index()
    {
        var lista = ser.GetProductos();
        return View(lista);
    }

    // ACCIÓN NUEVA: Filtrar por medidas desde la Home
    public IActionResult FiltrarMedidas(string ancho, string perfil, string diametro)
    {
        var resultados = ser.GetProductos().Where(p =>
            (string.IsNullOrEmpty(ancho) || p.DetalleProducto?.Ancho == ancho) &&
            (string.IsNullOrEmpty(perfil) || p.DetalleProducto?.Perfil == perfil) &&
            (string.IsNullOrEmpty(diametro) || p.DetalleProducto?.Diametro == diametro)
        ).ToList();

        ViewBag.FiltroActivo = true;
        return View("Index", resultados);
    }

    // Vista para crear nuevo producto
    public ActionResult Create()
    {
        CargarCombos();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Producto p)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // 📂 PROCESAR IMAGEN COMPU -> WWWROOT
                if (p.ImagenArchivo != null)
                {
                    string carpetaProductos = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");

                    // Si la carpeta de productos no existe, el sistema la crea automáticamente
                    if (!Directory.Exists(carpetaProductos))
                    {
                        Directory.CreateDirectory(carpetaProductos);
                    }

                    // Generar un nombre de archivo único e irrepetible
                    string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(p.ImagenArchivo.FileName);
                    string rutaDestino = Path.Combine(carpetaProductos, nombreUnico);

                    // Guardar físicamente el archivo cargado en el disco
                    using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                    {
                        p.ImagenArchivo.CopyTo(fileStream);
                    }

                    // Asignamos el nombre único en la columna Categoria de tu base de datos
                    p.Categoria = nombreUnico;
                }
                else
                {
                    // Si el usuario no sube una foto, asignamos la imagen genérica del sistema
                    p.Categoria = "default-producto.png";
                }

                ser.AddProducto(p);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar: " + ex.Message);
        }
        CargarCombos();
        return View(p);
    }

    // Vista para editar producto
    public ActionResult Edit(int id)
    {
        var p = ser.GetProducto(id);
        if (p == null) return NotFound();

        CargarCombos();
        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Producto ob)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Recuperamos el estado actual del producto para conservar o limpiar su foto anterior
                var productoExistente = ser.GetProducto(id);

                if (ob.ImagenArchivo != null)
                {
                    string carpetaProductos = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");

                    // Generar un nuevo nombre único de archivo
                    string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ob.ImagenArchivo.FileName);
                    string rutaDestino = Path.Combine(carpetaProductos, nombreUnico);

                    using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                    {
                        ob.ImagenArchivo.CopyTo(fileStream);
                    }

                    // Borramos físicamente del disco la foto anterior para no acumular basura (excepto si es la por defecto)
                    if (productoExistente != null && !string.IsNullOrEmpty(productoExistente.Categoria) && productoExistente.Categoria != "default-producto.png")
                    {
                        string rutaFotoAnterior = Path.Combine(carpetaProductos, productoExistente.Categoria);
                        if (System.IO.File.Exists(rutaFotoAnterior))
                        {
                            System.IO.File.Delete(rutaFotoAnterior);
                        }
                    }

                    // Guardamos el nuevo nombre en el objeto modificado
                    ob.Categoria = nombreUnico;
                }
                else
                {
                    // Si el usuario edita datos del producto pero no sube una nueva foto, retenemos la que ya tenía
                    if (productoExistente != null)
                    {
                        ob.Categoria = productoExistente.Categoria;
                    }
                }

                ser.UpdateProducto(ob);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "Error al actualizar.");
        }
        CargarCombos();
        return View(ob);
    }

    // Acción para eliminar
    public ActionResult Delete(int id)
    {
        // 🗑️ Limpieza automática del disco al eliminar el producto
        var p = ser.GetProducto(id);
        if (p != null && !string.IsNullOrEmpty(p.Categoria) && p.Categoria != "default-producto.png")
        {
            string rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos", p.Categoria);
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }
        }

        ser.DeleteProducto(id);
        return RedirectToAction(nameof(Index));
    }

    // Llenar los selectores para las vistas
    private void CargarCombos()
    {
        ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
        ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
    }
}