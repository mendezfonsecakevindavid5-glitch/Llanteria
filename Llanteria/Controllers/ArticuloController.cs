using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Administración del blog (solo administrador, ver RoleConvention).</summary>
public class ArticuloController : Controller
{
    private const string CarpetaImagenes = "blog";

    private readonly LlanteriaDbContext _db;
    private readonly AlmacenImagenes _imagenes;

    public ArticuloController(LlanteriaDbContext db, AlmacenImagenes imagenes)
    {
        _db = db;
        _imagenes = imagenes;
    }

    public IActionResult Index(string? buscar, string? estado, int pagina = 1)
    {
        var consulta = _db.Articulos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            consulta = consulta.Where(a => a.Titulo.Contains(t) || a.Categoria.Contains(t) || a.Autor.Contains(t));
        }
        if (estado == "publicados") consulta = consulta.Where(a => a.Publicado);
        else if (estado == "borradores") consulta = consulta.Where(a => !a.Publicado);

        ViewBag.Buscar = buscar;
        ViewBag.Estado = estado;
        ViewBag.Publicados = _db.Articulos.Count(a => a.Publicado);
        ViewBag.Borradores = _db.Articulos.Count(a => !a.Publicado);
        return View(Paginado<Articulo>.De(consulta.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id), pagina));
    }

    public IActionResult Create()
    {
        CargarCategorias();
        return View(new Articulo());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Articulo a)
    {
        Validar(a, null);
        if (!ModelState.IsValid)
        {
            CargarCategorias();
            return View(a);
        }

        a.Imagen = a.ImagenArchivo != null ? _imagenes.Guardar(a.ImagenArchivo, CarpetaImagenes, 1600) : null;
        _db.Articulos.Add(a);
        _db.SaveChanges();
        TempData["Exito"] = a.Publicado ? "Artículo publicado en el blog." : "Artículo guardado como borrador.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var a = _db.Articulos.Find(id);
        if (a == null) return NotFound();
        CargarCategorias();
        return View(a);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Articulo cambios)
    {
        var a = _db.Articulos.Find(id);
        if (a == null) return NotFound();

        Validar(cambios, id);
        if (!ModelState.IsValid)
        {
            cambios.Id = id;
            cambios.Imagen = a.Imagen;
            CargarCategorias();
            return View(cambios);
        }

        a.Titulo = cambios.Titulo;
        a.Slug = cambios.Slug;
        a.Resumen = cambios.Resumen;
        a.Categoria = cambios.Categoria;
        a.Fecha = cambios.Fecha;
        a.Autor = cambios.Autor;
        a.Contenido = cambios.Contenido;
        a.Publicado = cambios.Publicado;

        if (cambios.ImagenArchivo != null)
        {
            var anterior = a.Imagen;
            a.Imagen = _imagenes.Guardar(cambios.ImagenArchivo, CarpetaImagenes, 1600);
            EliminarPortada(anterior);
        }

        _db.SaveChanges();
        TempData["Exito"] = "Artículo actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CambiarPublicacion(int id)
    {
        var a = _db.Articulos.Find(id);
        if (a == null) return NotFound();

        a.Publicado = !a.Publicado;
        _db.SaveChanges();
        TempData["Exito"] = a.Publicado ? $"«{a.Titulo}» ya está publicado." : $"«{a.Titulo}» pasó a borrador.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var a = _db.Articulos.Find(id);
        if (a == null) return RedirectToAction(nameof(Index));

        EliminarPortada(a.Imagen);
        _db.Articulos.Remove(a);
        _db.SaveChanges();
        TempData["Exito"] = "Artículo eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // Reglas que dependen de la base de datos o de otros campos.
    private void Validar(Articulo a, int? idActual)
    {
        a.Titulo = (a.Titulo ?? "").Trim();
        a.Slug = string.IsNullOrWhiteSpace(a.Slug) ? Vitrina.Slug(a.Titulo) : a.Slug.Trim();

        if (a.Slug.Length == 0)
            ModelState.AddModelError(nameof(Articulo.Slug), "No se pudo generar la dirección del artículo: escribe un título con letras o números.");
        else if (_db.Articulos.Any(x => x.Slug == a.Slug && x.Id != idActual))
            ModelState.AddModelError(nameof(Articulo.Slug), "Ya hay un artículo con esa dirección. Cámbiala o cambia el título.");

        if (!string.IsNullOrEmpty(a.Categoria) && !BlogCatalogo.Categorias.Contains(a.Categoria))
            ModelState.AddModelError(nameof(Articulo.Categoria), "Elige una categoría de la lista.");

        var errorImagen = AlmacenImagenes.Validar(a.ImagenArchivo);
        if (errorImagen != null) ModelState.AddModelError(nameof(Articulo.ImagenArchivo), errorImagen);
    }

    // Las portadas que vienen con el proyecto (SVG) se conservan: solo se borran las que se subieron desde el panel.
    private void EliminarPortada(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) return;
        _imagenes.Eliminar(nombre, CarpetaImagenes);
    }

    private void CargarCategorias() => ViewBag.Categorias = new SelectList(BlogCatalogo.Categorias);
}
