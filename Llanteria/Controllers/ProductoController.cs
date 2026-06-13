using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
        public JsonResult GetProductosJson()
        {
            var productos = ser.GetProductos()
                .Select(p => new {
                    nombre = p.Nombre,
                    precio = p.PrecioVenta.ToString("C0"),
                    img = "/images/Productos/" + (string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen),
                    categoria = p.Categoria,
                    detalles = p.DetalleProducto != null ? new
                    {
                        ancho = p.DetalleProducto.Ancho,
                        perfil = p.DetalleProducto.Perfil,
                        diametro = p.DetalleProducto.Diametro,
                        viscosidad = p.DetalleProducto.Viscosidad,
                        tipoAceite = p.DetalleProducto.TipoAceite
                    } : null
                }).ToList();

            return Json(productos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Producto p)
        {
            if (ModelState.IsValid)
            {
                p.RutaImagen = ProcesarImagen(p.ImagenArchivo);
                ser.AddProducto(p);
                return RedirectToAction(nameof(Index));
            }
            CargarCombos();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Producto ob)
        {
            if (ModelState.IsValid)
            {
                var original = ser.GetProducto(id);

                // Si el usuario sube nueva foto, procesamos y borramos la anterior
                if (ob.ImagenArchivo != null)
                {
                    EliminarImagen(original?.RutaImagen);
                    ob.RutaImagen = ProcesarImagen(ob.ImagenArchivo);
                }
                else
                {
                    ob.RutaImagen = original?.RutaImagen;
                }

                ser.UpdateProducto(ob);
                return RedirectToAction(nameof(Index));
            }
            CargarCombos();
            return View(ob);
        }

        // --- MÉTODOS AUXILIARES PRIVADOS PARA MANTENER EL CÓDIGO LIMPIO ---

        private string ProcesarImagen(IFormFile? archivo)
        {
            if (archivo == null) return "default-producto.png";

            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            // Mantiene el nombre original descriptivo
            string nombre = Path.GetFileName(archivo.FileName).ToLower().Replace(" ", "-");
            string ruta = Path.Combine(carpeta, nombre);

            using (var fs = new FileStream(ruta, FileMode.Create))
            {
                archivo.CopyTo(fs);
            }
            return nombre;
        }

        private void EliminarImagen(string? nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo) || nombreArchivo == "default-producto.png") return;

            string ruta = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos", nombreArchivo);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }

        private void CargarCombos()
        {
            ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
            ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
        }

        public ActionResult Index() => View(ser.GetProductos());
    }
}