using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;

namespace Llanteria.Controllers;

public class ProveedoreController : Controller
{
    private readonly ProveedoreService ser;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProveedoreController(ProveedoreService proveedoreService, IWebHostEnvironment webHostEnvironment)
    {
        ser = proveedoreService;
        _webHostEnvironment = webHostEnvironment;
    }

    public ActionResult Index()
    {
        var lista = ser.GetProveedores();
        return View(lista);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Proveedore prov)
    {
        if (ModelState.IsValid)
        {
            // 1. Primero guardamos el proveedor en la base de datos para que SQL le asigne un ID
            ser.AddProveedor(prov);

            // 2. Una vez guardado, verificamos si subió un archivo desde la PC
            if (prov.ImagenArchivo != null)
            {
                string carpetaProveedores = Path.Combine(_webHostEnvironment.WebRootPath, "images", "proveedores");
                if (!Directory.Exists(carpetaProveedores)) Directory.CreateDirectory(carpetaProveedores);

                // El nombre del archivo será exactamente su ID (ej: "5.jpg")
                string extension = Path.GetExtension(prov.ImagenArchivo.FileName);
                string nombreArchivo = prov.Id + extension;
                string rutaDestino = Path.Combine(carpetaProveedores, nombreArchivo);

                using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                {
                    prov.ImagenArchivo.CopyTo(fileStream);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        return View(prov);
    }

    public ActionResult Edit(int id)
    {
        var prov = ser.GetProveedor(id);
        if (prov == null) return NotFound();
        return View(prov);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Proveedore prov)
    {
        if (ModelState.IsValid)
        {
            ser.UpdateProveedor(prov);

            // Si en la edición decide cambiar el logotipo por uno nuevo
            if (prov.ImagenArchivo != null)
            {
                string carpetaProveedores = Path.Combine(_webHostEnvironment.WebRootPath, "images", "proveedores");

                // Borramos cualquier logo previo con extensiones comunes para evitar duplicados (.jpg, .png)
                string[] posiblesArchivos = { prov.Id + ".jpg", prov.Id + ".png", prov.Id + ".jpeg" };
                foreach (var archivo in posiblesArchivos)
                {
                    string rutaVieja = Path.Combine(carpetaProveedores, archivo);
                    if (System.IO.File.Exists(rutaVieja)) System.IO.File.Delete(rutaVieja);
                }

                // Guardamos el nuevo
                string extension = Path.GetExtension(prov.ImagenArchivo.FileName);
                string rutaDestino = Path.Combine(carpetaProveedores, prov.Id + extension);

                using (var fileStream = new FileStream(rutaDestino, FileMode.Create))
                {
                    prov.ImagenArchivo.CopyTo(fileStream);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        return View(prov);
    }

    public ActionResult Delete(int id)
    {
        // Al eliminar, borramos también su foto del servidor
        string carpetaProveedores = Path.Combine(_webHostEnvironment.WebRootPath, "images", "proveedores");
        string[] posiblesArchivos = { id + ".jpg", id + ".png", id + ".jpeg" };
        foreach (var archivo in posiblesArchivos)
        {
            string rutaImagen = Path.Combine(carpetaProveedores, archivo);
            if (System.IO.File.Exists(rutaImagen)) System.IO.File.Delete(rutaImagen);
        }

        ser.DeleteProveedor(id);
        return RedirectToAction(nameof(Index));
    }
}