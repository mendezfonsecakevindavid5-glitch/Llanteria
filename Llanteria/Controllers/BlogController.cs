using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>
/// Blog público: guías sobre llantas, aceites, baterías y seguridad vial.
/// El autor y los colaboradores (editores invitados) y el administrador pueden editar cada sección desde la página del artículo;
/// solo el autor añade secciones nuevas, y únicamente mientras el artículo no esté público.
/// </summary>
public class BlogController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly IPerfilService _perfil;

    public BlogController(LlanteriaDbContext db, IPerfilService perfil)
    {
        _db = db;
        _perfil = perfil;
    }

    [HttpGet("Blog")]
    public IActionResult Index(string? categoria, string? orden)
    {
        var consulta = _db.Articulos.AsNoTracking().Where(a => a.Publicado);

        if (!string.IsNullOrEmpty(categoria) && BlogCatalogo.Categorias.Contains(categoria))
            consulta = consulta.Where(a => a.Categoria == categoria);
        else
            categoria = null;

        var antiguos = orden == "antiguos";
        consulta = antiguos ? consulta.OrderBy(a => a.Fecha) : consulta.OrderByDescending(a => a.Fecha);

        ViewBag.Categoria = categoria;
        ViewBag.Orden = antiguos ? "antiguos" : "recientes";
        return View(consulta.ToList());
    }

    [HttpGet("Blog/{slug}")]
    public IActionResult Detalle(string slug)
    {
        var articulo = _db.Articulos.AsNoTracking().FirstOrDefault(a => a.Slug == slug);
        if (articulo == null) return NotFound();
        // Un borrador solo lo ven el administrador, el autor y los colaboradores.
        if (!articulo.Publicado && !PuedeEditar(articulo)) return NotFound();

        var (intro, secciones) = BlogCatalogo.Dividir(articulo.Contenido);
        var tabla = BlogCatalogo.NumerosDeTabla(articulo.TablaContenido, secciones.Count)
            .Select(n => secciones[n - 1]).ToList();

        var autor = articulo.IdAutor.HasValue
            ? _db.Usuarios.AsNoTracking().Where(u => u.Id == articulo.IdAutor).Select(u => u.Username).FirstOrDefault()
            : null;

        return View(new BlogDetalleVM
        {
            Articulo = articulo,
            Intro = intro,
            Secciones = secciones,
            Tabla = tabla,
            PuedeEditar = PuedeEditar(articulo),
            EsAutor = EsAutor(articulo),
            AutorUsuario = autor ?? articulo.Autor,
            Relacionados = Relacionados(articulo)
        });
    }

    /// <summary>Foto de perfil del autor de un artículo visible (404 si no tiene: la vista muestra sus iniciales).</summary>
    [HttpGet("Blog/{slug}/autor-foto")]
    public async Task<IActionResult> AutorFoto(string slug)
    {
        var a = await _db.Articulos.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug);
        if (a?.IdAutor == null || (!a.Publicado && !PuedeEditar(a))) return NotFound();

        var foto = await _perfil.ObtenerFotoAsync(a.IdAutor.Value);
        if (foto is not { } f) return NotFound();
        Response.Headers.CacheControl = "private, max-age=300";
        return File(f.Datos, f.Tipo);
    }

    // ---------- Editar una sección (el lápiz junto a cada título) ----------

    [Authorize, HttpGet("Blog/{slug}/seccion/{numero:int}")]
    public IActionResult EditarSeccion(string slug, int numero)
    {
        var a = _db.Articulos.AsNoTracking().FirstOrDefault(x => x.Slug == slug);
        if (a == null) return NotFound();
        if (!PuedeEditar(a)) return Forbid();

        var (intro, secciones) = BlogCatalogo.Dividir(a.Contenido);
        if (numero == 0) return View("SeccionForm", new SeccionFormVM { Slug = slug, TituloArticulo = a.Titulo, Numero = 0, Cuerpo = intro });
        if (numero < 1 || numero > secciones.Count) return NotFound();

        var s = secciones[numero - 1];
        return View("SeccionForm", new SeccionFormVM { Slug = slug, TituloArticulo = a.Titulo, Numero = numero, Titulo = s.Titulo, Cuerpo = s.Cuerpo });
    }

    [Authorize, HttpPost("Blog/{slug}/seccion/{numero:int}"), ValidateAntiForgeryToken]
    public IActionResult EditarSeccion(string slug, int numero, SeccionFormVM form)
    {
        var a = _db.Articulos.FirstOrDefault(x => x.Slug == slug);
        if (a == null) return NotFound();
        if (!PuedeEditar(a)) return Forbid();

        var (intro, secciones) = BlogCatalogo.Dividir(a.Contenido);
        form.Slug = slug; form.TituloArticulo = a.Titulo; form.Numero = numero;

        if (numero != 0)
        {
            if (numero < 1 || numero > secciones.Count) return NotFound();
            var titulo = BlogCatalogo.LimpiarTitulo(form.Titulo ?? "");
            if (titulo.Length == 0) ModelState.AddModelError(nameof(form.Titulo), "La sección necesita un título.");
            form.Titulo = titulo;
        }
        if (string.IsNullOrWhiteSpace(form.Cuerpo)) ModelState.AddModelError(nameof(form.Cuerpo), "Escribe el texto de la sección.");
        if (!ModelState.IsValid) return View("SeccionForm", form);

        if (numero == 0) intro = form.Cuerpo.Trim();
        else secciones[numero - 1] = new BlogCatalogo.Seccion(numero, form.Titulo!, form.Cuerpo.Trim());

        a.Contenido = BlogCatalogo.Unir(intro, secciones);
        _db.SaveChanges();
        TempData["Exito"] = "Sección actualizada.";
        return Redirect($"/Blog/{slug}#seccion-{numero}");
    }

    // ---------- Añadir una sección: solo el autor y con el artículo sin publicar ----------

    [Authorize, HttpGet("Blog/{slug}/nueva-seccion")]
    public IActionResult NuevaSeccion(string slug)
    {
        var a = _db.Articulos.AsNoTracking().FirstOrDefault(x => x.Slug == slug);
        if (a == null) return NotFound();
        if (!EsAutor(a)) return Forbid();
        if (a.Publicado)
        {
            TempData["Error"] = "Para añadir secciones primero deshabilita el artículo como público.";
            return Redirect($"/Blog/{slug}");
        }
        return View("SeccionForm", new SeccionFormVM { Slug = slug, TituloArticulo = a.Titulo, Numero = -1 });
    }

    [Authorize, HttpPost("Blog/{slug}/nueva-seccion"), ValidateAntiForgeryToken]
    public IActionResult NuevaSeccion(string slug, SeccionFormVM form)
    {
        var a = _db.Articulos.FirstOrDefault(x => x.Slug == slug);
        if (a == null) return NotFound();
        if (!EsAutor(a)) return Forbid();
        if (a.Publicado)
        {
            TempData["Error"] = "Para añadir secciones primero deshabilita el artículo como público.";
            return Redirect($"/Blog/{slug}");
        }

        form.Slug = slug; form.TituloArticulo = a.Titulo; form.Numero = -1;
        form.Titulo = BlogCatalogo.LimpiarTitulo(form.Titulo ?? "");
        if (form.Titulo.Length == 0) ModelState.AddModelError(nameof(form.Titulo), "La sección necesita un título.");
        if (string.IsNullOrWhiteSpace(form.Cuerpo)) ModelState.AddModelError(nameof(form.Cuerpo), "Escribe el texto de la sección.");
        if (!ModelState.IsValid) return View("SeccionForm", form);

        var (intro, secciones) = BlogCatalogo.Dividir(a.Contenido);
        var nuevas = secciones.Append(new BlogCatalogo.Seccion(secciones.Count + 1, form.Titulo, form.Cuerpo.Trim())).ToList();
        // Si la tabla de contenido ya era personalizada, la nueva sección se añade al final de ella.
        if (!string.IsNullOrWhiteSpace(a.TablaContenido)) a.TablaContenido = a.TablaContenido.Trim() + ", " + nuevas.Count;
        a.Contenido = BlogCatalogo.Unir(intro, nuevas);
        _db.SaveChanges();
        TempData["Exito"] = "Sección añadida.";
        return Redirect($"/Blog/{slug}#seccion-{nuevas.Count}");
    }

    // ---------- Tabla de contenido: el lápiz de la esquina; solo números ----------

    [Authorize, HttpPost("Blog/{slug}/tabla"), ValidateAntiForgeryToken]
    public IActionResult EditarTabla(string slug, string? numeros)
    {
        var a = _db.Articulos.FirstOrDefault(x => x.Slug == slug);
        if (a == null) return NotFound();
        if (!PuedeEditar(a)) return Forbid();

        var total = BlogCatalogo.Dividir(a.Contenido).Secciones.Count;
        var error = BlogCatalogo.ValidarTabla(numeros, total);
        if (error != null) TempData["Error"] = error;
        else
        {
            a.TablaContenido = string.IsNullOrWhiteSpace(numeros) ? null : string.Join(", ", System.Text.RegularExpressions.Regex.Matches(numeros, @"\d+").Select(m => m.Value));
            _db.SaveChanges();
            TempData["Exito"] = "Tabla de contenido actualizada.";
        }
        return Redirect($"/Blog/{slug}");
    }

    // ---------- Permisos ----------

    private int? IdUsuarioActual() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>El autor del artículo (o el administrador, si el artículo no tiene un autor registrado).</summary>
    private bool EsAutor(Articulo a)
    {
        if (User.Identity?.IsAuthenticated != true) return false;
        var id = IdUsuarioActual();
        return (a.IdAutor.HasValue && id == a.IdAutor) || (!a.IdAutor.HasValue && User.IsInRole(RolesApp.Administrador));
    }

    /// <summary>Autor, administrador o colaborador (editor del blog invitado por el administrador).</summary>
    private bool PuedeEditar(Articulo a)
    {
        if (User.Identity?.IsAuthenticated != true) return false;
        if (User.IsInRole(RolesApp.Administrador) || EsAutor(a)) return true;
        var id = IdUsuarioActual();
        return id.HasValue && _db.EsEditor(id.Value, EditorContenido.Blog);
    }

    private List<BlogPost> Relacionados(Articulo actual, int cantidad = 3)
        => _db.Articulos.AsNoTracking()
              .Where(a => a.Publicado && a.Slug != actual.Slug)
              .OrderByDescending(a => a.Categoria == actual.Categoria).ThenByDescending(a => a.Fecha)
              .Take(cantidad).ToList().Select(a => a.ComoPost()).ToList();
}
