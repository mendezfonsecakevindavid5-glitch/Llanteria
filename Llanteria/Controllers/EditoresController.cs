using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Llanteria.Controllers;

/// <summary>
/// El administrador invita a otros usuarios (por su nombre de usuario) a crear y editar el blog o los servicios del sitio.
/// Solo administrador (ver RoleConvention). La invitación da el permiso al instante y avisa por correo.
/// </summary>
public class EditoresController : Controller
{
    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;

    public EditoresController(LlanteriaDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public IActionResult Index()
    {
        ViewBag.Recursos = EditorContenido.Recursos;
        var editores = _db.EditoresContenido.AsNoTracking().Include(e => e.IdUsuarioNavigation)
            .OrderBy(e => e.Recurso).ThenBy(e => e.IdUsuarioNavigation.Username).ToList();
        return View(editores);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Invitar(string username, string recurso)
    {
        username = (username ?? "").Trim();
        var error = Validar(username, recurso, out var usuario);
        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Index));
        }

        _db.EditoresContenido.Add(new EditorContenido
        {
            IdUsuario = usuario!.Id,
            Recurso = recurso,
            IdInvitadoPor = IdActual()
        });
        _db.SaveChanges();

        var enviado = await EnviarInvitacionAsync(usuario, recurso);
        TempData["Exito"] = enviado
            ? $"{usuario.Username} ya puede editar «{recurso}». Le enviamos la invitación por correo."
            : $"{usuario.Username} ya puede editar «{recurso}», pero no pudimos enviarle el correo. Avísale por otro medio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Revocar(int id)
    {
        var e = _db.EditoresContenido.Find(id);
        if (e != null)
        {
            _db.EditoresContenido.Remove(e);
            _db.SaveChanges();
            TempData["Exito"] = "Se quitó el permiso de edición.";
        }
        return RedirectToAction(nameof(Index));
    }

    private string? Validar(string username, string recurso, out Usuario? usuario)
    {
        usuario = null;
        if (!EditorContenido.Recursos.Contains(recurso)) return "Elige qué quieres compartir: blog o servicios.";
        if (username.Length == 0) return "Escribe el nombre de usuario de la persona que quieres invitar.";

        usuario = _db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Username == username);
        if (usuario == null) return $"No existe ningún usuario llamado «{username}». Revisa que esté bien escrito.";
        if (usuario.Estado != "Activo") return $"El usuario {usuario.Username} no está activo.";
        if (usuario.IdRol == AdminRolId()) return $"{usuario.Username} ya es administrador y tiene acceso completo.";
        var idUsuarioEncontrado = usuario.Id;
        if (_db.EditoresContenido.Any(e => e.IdUsuario == idUsuarioEncontrado && e.Recurso == recurso))
            return $"{usuario.Username} ya es editor de «{recurso}».";
        return null;
    }

    private int AdminRolId() => _db.Roles.AsNoTracking().Where(r => r.NombreRol == RolesApp.Administrador).Select(r => r.Id).FirstOrDefault();

    private int IdActual() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private Task<bool> EnviarInvitacionAsync(Usuario destino, string recurso)
    {
        var accion = recurso == EditorContenido.Blog ? "Articulo" : "ServicioWeb";
        var enlace = Url.Action("Index", accion, null, Request.Scheme) ?? "";
        var cuerpo = EmailService.PlantillaAviso("Te invitaron al equipo de Llantería Valledupar", new (string, string?)[]
        {
            ("Invitado por", User.Identity?.Name),
            ("Contenido", recurso == EditorContenido.Blog ? "Blog" : "Servicios del sitio"),
            ("Permisos", "Crear, editar y publicar")
        }, $"Ya puedes entrar con tu cuenta y administrar este contenido desde el panel:\n{enlace}");

        return _email.EnviarCorreoAsync(destino.Correo, destino.Username,
            "Invitación para colaborar en Llantería Valledupar", cuerpo, responderA: null);
    }
}
