using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Hosting; // 👈 Obligatorio para IWebHostEnvironment
using System.IO;
using System;

namespace Llanteria.Controllers;

public class TipoServicioController : Controller
{
    private readonly TipoServicioService ser;
    private readonly IWebHostEnvironment _webHostEnvironment; // 👈 CLAVE para encontrar la carpeta wwwroot

    // Inyectamos el servicio y el entorno web en el constructor
    public TipoServicioController(TipoServicioService tipoServicioService, IWebHostEnvironment webHostEnvironment)
    {
        ser = tipoServicioService;
        _webHostEnvironment = webHostEnvironment;
    }

    // Listado de servicios ofrecidos en la llantería
    public IActionResult Index()
    {
        var lista = ser.GetTiposServicio();
        return View(lista);
    }

    // Vista para agregar un nuevo tipo de servicio
    public IActionResult Create()
    {
        return View();
    }

    // Guarda el servicio, procesa la imagen y registra su costo base
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TipoServicio ts)
    {
        if (ModelState.IsValid)
        {
            // 📂 PROCESAR IMAGEN COMPU -> WWWROOT
            if (ts.ImagenArchivo != null)
            {
                string carpetaServicios = Path.Combine(_webHostEnvironment.WebRootPath, "images", "servicios");

                // Crea la carpeta si no existe por seguridad
                if (!Directory.Exists(carpetaServicios))
                {
                    Directory.CreateDirectory(carpetaServicios);
                }

                // Generar nombre único para evitar archivos duplicados
                string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ts.ImagenArchivo.FileName);
                string rutaDestino = Path.Combine(carpetaServicios, nombreUnico);

                // Guardar físicamente el archivo cargado
                using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                {
                    ts.ImagenArchivo.CopyTo(fileStream); // Sincrónico ya que usas IActionResult
                }

                // Asignamos el nombre del archivo al campo de texto que va a la base de datos
                ts.IconoPath = nombreUnico;
            }
            else
            {
                // Si no subió foto, asignamos la imagen por defecto
                ts.IconoPath = "default-servicio.png";
            }

            ser.AddTipoServicio(ts);
            return RedirectToAction(nameof(Index));
        }
        return View(ts);
    }

    // Vista para editar el precio o nombre de un servicio
    public IActionResult Edit(int id)
    {
        var ts = ser.GetTipoServicio(id);
        if (ts == null) return NotFound();
        return View(ts);
    }

    // Procesa la actualización del servicio e imagen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TipoServicio ts)
    {
        if (ModelState.IsValid)
        {
            // Recuperamos el servicio actual sin cambios para saber qué imagen tenía antes
            var servicioExistente = ser.GetTipoServicio(id);

            if (ts.ImagenArchivo != null)
            {
                string carpetaServicios = Path.Combine(_webHostEnvironment.WebRootPath, "images", "servicios");

                // Generamos un nuevo nombre único
                string nombreUnico = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ts.ImagenArchivo.FileName);
                string rutaDestino = Path.Combine(carpetaServicios, nombreUnico);

                using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                {
                    ts.ImagenArchivo.CopyTo(fileStream);
                }

                // Borramos la imagen anterior si existía y no era la por defecto (Mantiene limpio el servidor)
                if (servicioExistente != null && !string.IsNullOrEmpty(servicioExistente.IconoPath) && servicioExistente.IconoPath != "default-servicio.png")
                {
                    string rutaImagenAnterior = Path.Combine(carpetaServicios, servicioExistente.IconoPath);
                    if (System.IO.File.Exists(rutaImagenAnterior))
                    {
                        System.IO.File.Delete(rutaImagenAnterior);
                    }
                }

                // Asignamos la nueva ruta de texto
                ts.IconoPath = nombreUnico;
            }
            else
            {
                // Si no se subió una nueva imagen en la edición, conservamos la que ya tenía antes
                if (servicioExistente != null)
                {
                    ts.IconoPath = servicioExistente.IconoPath;
                }
            }

            ser.UpdateTipoServicio(ts);
            return RedirectToAction(nameof(Index));
        }
        return View(ts);
    }

    // Elimina un tipo de servicio (si no tiene facturas asociadas)
    public IActionResult Delete(int id)
    {
        // Opcional: Podrías buscar el servicio y borrar su foto de la carpeta antes de eliminarlo
        var ts = ser.GetTipoServicio(id);
        if (ts != null && !string.IsNullOrEmpty(ts.IconoPath) && ts.IconoPath != "default-servicio.png")
        {
            string rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, "images", "servicios", ts.IconoPath);
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }
        }

        ser.DeleteTipoServicio(id);
        return RedirectToAction(nameof(Index));
    }
}