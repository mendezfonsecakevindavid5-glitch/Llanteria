using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Administración de los servicios que se muestran en el sitio (solo administrador, ver RoleConvention).</summary>
public class ServicioWebController : Controller
{
    private const string CarpetaImagenes = "servicios";

    private readonly LlanteriaDbContext _db;
    private readonly AlmacenImagenes _imagenes;

    public ServicioWebController(LlanteriaDbContext db, AlmacenImagenes imagenes)
    {
        _db = db;
        _imagenes = imagenes;
    }

    public IActionResult Index()
        => View(_db.ServiciosWeb.AsNoTracking().OrderBy(s => s.Orden).ThenBy(s => s.Titulo).ToList());

    public IActionResult Create()
        => View(new ServicioWeb { Orden = _db.ServiciosWeb.Count() + 1 });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(ServicioWeb s)
    {
        Validar(s, null);
        if (!ModelState.IsValid) return View(s);

        s.Imagen = s.ImagenArchivo != null ? _imagenes.Guardar(s.ImagenArchivo, CarpetaImagenes, 1200) : null;
        _db.ServiciosWeb.Add(s);
        _db.SaveChanges();
        TempData["Exito"] = "Servicio creado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var s = _db.ServiciosWeb.Find(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ServicioWeb cambios)
    {
        var s = _db.ServiciosWeb.Find(id);
        if (s == null) return NotFound();

        Validar(cambios, id);
        if (!ModelState.IsValid)
        {
            cambios.Id = id;
            cambios.Imagen = s.Imagen;
            return View(cambios);
        }

        CopiarCampos(cambios, s);
        if (cambios.ImagenArchivo != null)
        {
            var anterior = s.Imagen;
            s.Imagen = _imagenes.Guardar(cambios.ImagenArchivo, CarpetaImagenes, 1200);
            _imagenes.Eliminar(anterior, CarpetaImagenes);
        }

        _db.SaveChanges();
        TempData["Exito"] = "Servicio actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CambiarPublicacion(int id)
    {
        var s = _db.ServiciosWeb.Find(id);
        if (s == null) return NotFound();

        s.Publicado = !s.Publicado;
        _db.SaveChanges();
        TempData["Exito"] = s.Publicado ? $"«{s.Titulo}» ya se muestra en el sitio." : $"«{s.Titulo}» quedó oculto.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var s = _db.ServiciosWeb.Find(id);
        if (s == null) return RedirectToAction(nameof(Index));

        _imagenes.Eliminar(s.Imagen, CarpetaImagenes);
        _db.ServiciosWeb.Remove(s);
        _db.SaveChanges();
        TempData["Exito"] = "Servicio eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static void CopiarCampos(ServicioWeb origen, ServicioWeb destino)
    {
        destino.Slug = origen.Slug;
        destino.Titulo = origen.Titulo;
        destino.Subtitulo = origen.Subtitulo;
        destino.Icono = origen.Icono;
        destino.Descripcion = origen.Descripcion;
        destino.Ventajas = origen.Ventajas;
        destino.InfoCita = origen.InfoCita;
        destino.TituloVisitante = origen.TituloVisitante;
        destino.TextoVisitante = origen.TextoVisitante;
        destino.EtiquetaPrecio = origen.EtiquetaPrecio;
        destino.Precio = origen.Precio;
        destino.PlaceholderVehiculo = origen.PlaceholderVehiculo;
        destino.PlaceholderNotas = origen.PlaceholderNotas;
        destino.PlaceholderMensaje = origen.PlaceholderMensaje;
        destino.EsDomicilio = origen.EsDomicilio;
        destino.Publicado = origen.Publicado;
        destino.Orden = origen.Orden;
    }

    private void Validar(ServicioWeb s, int? idActual)
    {
        s.Titulo = (s.Titulo ?? "").Trim();
        s.Slug = string.IsNullOrWhiteSpace(s.Slug) ? Vitrina.Slug(s.Titulo) : Vitrina.Slug(s.Slug);

        if (s.Slug.Length == 0)
            ModelState.AddModelError(nameof(ServicioWeb.Slug), "No se pudo generar la dirección: escribe un nombre con letras o números.");
        else if (_db.ServiciosWeb.Any(x => x.Slug == s.Slug && x.Id != idActual))
            ModelState.AddModelError(nameof(ServicioWeb.Slug), "Ya hay un servicio con esa dirección. Cámbiala o cambia el nombre.");

        if (!(s.Icono ?? "").StartsWith("fa-"))
            ModelState.AddModelError(nameof(ServicioWeb.Icono), "El ícono debe ser un nombre de Font Awesome que empiece por «fa-» (por ejemplo fa-oil-can).");

        var errorVentajas = ServicioWeb.ValidarLineas(s.Ventajas, 3, "Ventajas");
        if (errorVentajas != null) ModelState.AddModelError(nameof(ServicioWeb.Ventajas), errorVentajas);

        var errorInfo = ServicioWeb.ValidarLineas(s.InfoCita, 2, "Información para la cita");
        if (errorInfo != null) ModelState.AddModelError(nameof(ServicioWeb.InfoCita), errorInfo);

        var errorImagen = AlmacenImagenes.Validar(s.ImagenArchivo);
        if (errorImagen != null) ModelState.AddModelError(nameof(ServicioWeb.ImagenArchivo), errorImagen);
    }
}
