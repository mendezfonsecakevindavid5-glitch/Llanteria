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
// Importaciones necesarias para ImageSharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

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

        // --- VISTAS Y LOGICA (MÉTODOS GET) ---

        [HttpGet]
        public ActionResult Index() => View(ser.GetProductos());

        [HttpGet]
        public ActionResult Create()
        {
            CargarCombos(); // Carga las listas de Proveedores y Marcas en el ViewBag
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var producto = ser.GetProducto(id);
            if (producto == null)
            {
                return NotFound();
            }

            CargarCombos(); // Carga las listas de Proveedores y Marcas para la edición
            return View(producto);
        }

        [HttpGet]
        public JsonResult GetProductosJson()
        {
            var productos = ser.GetProductos()
                .Select(p => new {
                    nombre = p.Nombre,
                    precio = p.PrecioVenta.ToString("C0"),
                    // Se unifica la ruta a "productos" en minúscula para evitar fallos de rutas
                    img = "/images/productos/" + (string.IsNullOrEmpty(p.RutaImagen) ? "default-producto.png" : p.RutaImagen),
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

        // --- PROCESAMIENTO DE FORMULARIOS (MÉTODOS POST) ---

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

        // --- MÉTODOS AUXILIARES OPTIMIZADOS ---

        private string ProcesarImagen(IFormFile? archivo)
        {
            if (archivo == null) return "default-producto.png";

            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            // Nombre limpio terminado en .webp
            string nombreBase = Path.GetFileNameWithoutExtension(archivo.FileName).ToLower().Replace(" ", "-");
            string nombreWebp = nombreBase + ".webp";
            string ruta = Path.Combine(carpeta, nombreWebp);

            // Redimensionamiento y optimización a WebP
            using (Image image = Image.Load(archivo.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 800),
                    Mode = ResizeMode.Max
                }));

                image.Save(ruta, new WebpEncoder { Quality = 75 });
            }

            return nombreWebp;
        }

        private void EmptyImagen(string? nombreArchivo) // Alias por claridad interna si fuera necesario
        {
            EliminarImagen(nombreArchivo);
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
    }
}