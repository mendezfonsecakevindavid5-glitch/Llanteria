using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;

namespace Llanteria.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ProductoService ser;
        private readonly ProveedoreService provSer;
        private readonly MarcaService marcSer;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(ProductoService productoService, ProveedoreService proveedoreService, MarcaService marcaService, IWebHostEnvironment webHostEnvironment)
        {
            ser = productoService;
            provSer = proveedoreService;
            marcSer = marcaService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public JsonResult GetLlantasJson()
        {
            var productos = ser.GetProductos()
                .Where(p => p.DetalleProducto != null)
                .Select(p => new {
                    ancho = p.DetalleProducto.Ancho,
                    perfil = p.DetalleProducto.Perfil,
                    diametro = p.DetalleProducto.Diametro,
                    nombre = p.Nombre,
                    precio = p.PrecioVenta.ToString("C0"),
                    img = "/images/productos/" + (string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen)
                }).ToList();

            return Json(productos);
        }

        public ActionResult Index() => View(ser.GetProductos());

        public ActionResult Create()
        {
            CargarCombos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Creó nuevo producto", "Producto" })]
        public ActionResult Create(Producto p)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (p.ImagenArchivo != null)
                    {
                        string carpetaProductos = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
                        if (!Directory.Exists(carpetaProductos)) Directory.CreateDirectory(carpetaProductos);

                        string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(p.ImagenArchivo.FileName);
                        string rutaDestino = Path.Combine(carpetaProductos, nombreUnico);

                        using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                        {
                            p.ImagenArchivo.CopyTo(fileStream);
                        }
                        p.RutaImagen = nombreUnico;
                    }
                    else
                    {
                        p.RutaImagen = "default-producto.png";
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

        public ActionResult Edit(int id)
        {
            var p = ser.GetProducto(id);
            if (p == null) return NotFound();
            CargarCombos();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó un producto", "Producto" })]
        public ActionResult Edit(int id, Producto ob)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var productoExistente = ser.GetProducto(id);
                    string carpetaProductos = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");

                    if (ob.ImagenArchivo != null)
                    {
                        // Guardar nueva imagen
                        string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ob.ImagenArchivo.FileName);
                        string rutaDestino = Path.Combine(carpetaProductos, nombreUnico);

                        using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                        {
                            ob.ImagenArchivo.CopyTo(fileStream);
                        }

                        // Eliminar imagen anterior si existe y no es la default
                        if (productoExistente != null && !string.IsNullOrEmpty(productoExistente.RutaImagen) && productoExistente.RutaImagen != "default-producto.png")
                        {
                            string rutaFotoAnterior = Path.Combine(carpetaProductos, productoExistente.RutaImagen);
                            if (System.IO.File.Exists(rutaFotoAnterior)) System.IO.File.Delete(rutaFotoAnterior);
                        }
                        ob.RutaImagen = nombreUnico;
                    }
                    else if (productoExistente != null)
                    {
                        ob.RutaImagen = productoExistente.RutaImagen;
                    }

                    ser.UpdateProducto(ob);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch { ModelState.AddModelError("", "Error al actualizar."); }
            CargarCombos();
            return View(ob);
        }

        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó un producto", "Producto" })]
        public ActionResult Delete(int id)
        {
            var p = ser.GetProducto(id);
            if (p != null && !string.IsNullOrEmpty(p.RutaImagen) && p.RutaImagen != "default-producto.png")
            {
                string rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos", p.RutaImagen);
                if (System.IO.File.Exists(rutaImagen)) System.IO.File.Delete(rutaImagen);
            }
            ser.DeleteProducto(id);
            return RedirectToAction(nameof(Index));
        }

        private void CargarCombos()
        {
            ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
            ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
        }
    }
}