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
        private readonly BodegaService bodSer;          // ← NUEVO
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(
            ProductoService productoService,
            ProveedoreService proveedoreService,
            MarcaService marcaService,
            BodegaService bodegaService,               // ← NUEVO
            IWebHostEnvironment webHostEnvironment)
        {
            ser = productoService;
            provSer = proveedoreService;
            marcSer = marcaService;
            bodSer = bodegaService;                    // ← NUEVO
            _webHostEnvironment = webHostEnvironment;
        }

        // --- VISTAS Y LOGICA (MÉTODOS GET) ---

        [HttpGet]
        public ActionResult Index() => View(ser.GetProductos());

        [HttpGet]
        public ActionResult Create()
        {
            CargarCombos();
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

            CargarCombos();
            return View(producto);
        }

        [HttpGet]
        public JsonResult GetProductosJson()
        {
            var productos = ser.GetProductos()
                .Select(p => new {
                    nombre = p.Nombre,
                    precio = p.PrecioVenta.ToString("C0"),
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
            // === DIAGNÓSTICO ===
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(" | ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();

                ViewBag.Errores = string.Join("<br>", errores);
            }
            // ===================

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
                // Si subieron una imagen nueva
                if (ob.ImagenArchivo != null)
                {
                    var original = ser.GetProducto(id);
                    EliminarImagen(original?.RutaImagen);
                    ob.RutaImagen = ProcesarImagen(ob.ImagenArchivo);
                }

                ser.UpdateProducto(ob);
                return RedirectToAction(nameof(Index));
            }

            CargarCombos();
            return View(ob);
        }

        // --- MÉTODOS AUXILIARES ---

        private string ProcesarImagen(IFormFile? archivo)
        {
            if (archivo == null) return "default-producto.png";

            // ← Aquí cambiamos a "productoos"
            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productoos");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            string nombreBase = Path.GetFileNameWithoutExtension(archivo.FileName).ToLower().Replace(" ", "-");
            string nombreWebp = nombreBase + ".webp";
            string ruta = Path.Combine(carpeta, nombreWebp);

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

        private void EliminarImagen(string? nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo) || nombreArchivo == "default-producto.png") return;

            // ← También aquí
            string ruta = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productoos", nombreArchivo);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }

        private void CargarCombos()
        {
            ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
            ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
            ViewBag.IdBodega = new SelectList(bodSer.GetBodegas(), "Id", "NombreBodega"); // ← corregido
        }
    }
 }
