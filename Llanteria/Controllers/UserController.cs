using System.Security.Claims;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IPerfilService _perfilService;

    public UserController(IPerfilService perfilService)
    {
        _perfilService = perfilService;
    }

    private bool TryGetUserId(out int id) => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out id);

    /// <summary>
    /// Vista principal del perfil del usuario actual.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        if (!TryGetUserId(out var id)) return RedirectToAction("Login", "Account");
        return View(await _perfilService.ObtenerPerfilPorUsuarioIdAsync(id));
    }

    /// <summary>
    /// Foto del usuario con sesión (el avatar del header la pide aquí; 404 si no tiene y se muestran las iniciales).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Foto()
    {
        if (!TryGetUserId(out var id)) return NotFound();

        var foto = await _perfilService.ObtenerFotoAsync(id);
        if (foto is not { } f) return NotFound();

        var etag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(
            "\"" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(f.Datos))[..32] + "\"");

        // El navegador revalida con ETag: si cambia la foto, se ve al instante
        Response.Headers.CacheControl = "private, no-cache";
        return File(f.Datos, f.Tipo, lastModified: null, entityTag: etag);
    }

    /// <summary>
    /// Actualiza los datos del perfil del usuario.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarPerfil(PerfilUsuarioViewModel model)
    {
        if (!TryGetUserId(out var id)) return RedirectToAction("Login", "Account");
        model.Id = id;

        // El correo, facturas, rol y vehículos no se editan desde este formulario.
        ModelState.Remove(nameof(PerfilUsuarioViewModel.Correo));
        ModelState.Remove(nameof(PerfilUsuarioViewModel.FacturasRecientes));
        ModelState.Remove(nameof(PerfilUsuarioViewModel.Rol));
        ModelState.Remove(nameof(PerfilUsuarioViewModel.Vehiculos));

        if (!string.IsNullOrEmpty(model.FotoRecortada) && !PerfilService.EsFotoRecortadaValida(model.FotoRecortada))
            ModelState.AddModelError(nameof(PerfilUsuarioViewModel.FotoRecortada), "La foto no es válida. Elígela de nuevo.");

        if (!ModelState.IsValid)
        {
            var actual = await _perfilService.ObtenerPerfilPorUsuarioIdAsync(id);
            model.FotoBase64 = actual.FotoBase64;
            model.Correo = actual.Correo;
            model.Rol = actual.Rol;
            model.Vehiculos = actual.Vehiculos;
            return View("Perfil", model);
        }

        model.Correo = null!; // El correo no se modifica desde el perfil

        if (await _perfilService.ActualizarPerfilAsync(model))
        {
            TempData["Exito"] = "Perfil actualizado.";
        }
        else
        {
            TempData["Error"] = "No se pudieron guardar los cambios. Inténtalo de nuevo.";
        }

        return RedirectToAction(nameof(Perfil));
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(string actual, string nueva, string confirmacion)
    {
        if (!TryGetUserId(out var id)) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(nueva) || nueva.Length < 6)
        {
            TempData["Error"] = "La nueva contraseña debe tener al menos 6 caracteres.";
        }
        else if (nueva != confirmacion)
        {
            TempData["Error"] = "La confirmación no coincide con la nueva contraseña.";
        }
        else if (await _perfilService.ActualizarPasswordAsync(id, actual, nueva))
        {
            TempData["Exito"] = "Contraseña actualizada.";
        }
        else
        {
            TempData["Error"] = "No se pudo cambiar la contraseña. Verifica tu clave actual.";
        }

        return RedirectToAction(nameof(Perfil));
    }
}