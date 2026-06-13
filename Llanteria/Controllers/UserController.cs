using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Llanteria.Models;
using Llanteria.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Llanteria.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IPerfilService _perfilService;

        // Solo dependemos de IPerfilService para mantener el código limpio
        public UserController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // GET: /User/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            int usuarioId = int.Parse(userIdClaim);
            var model = await _perfilService.ObtenerPerfilPorUsuarioIdAsync(usuarioId);

            if (model == null)
            {
                return NotFound("No se encontró el perfil para el usuario especificado.");
            }

            return View(model);
        }

        // POST: /User/ActualizarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(PerfilUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Perfil", model);
            }

            var guardadoExitoso = await _perfilService.ActualizarPerfilAsync(model);

            if (guardadoExitoso)
                TempData["SuccessMessage"] = "¡Tu perfil se ha actualizado con éxito!";
            else
                TempData["ErrorMessage"] = "No se pudieron guardar los cambios. Inténtalo de nuevo.";

            return RedirectToAction(nameof(Perfil));
        }

        // POST: /User/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string actual, string nueva)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            // Usamos el método asíncrono que definimos en el servicio
            bool exito = await _perfilService.ActualizarPasswordAsync(userId, actual, nueva);

            if (exito)
            {
                TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al cambiar la contraseña. Verifica tu clave actual.";
            }

            return RedirectToAction(nameof(Perfil));
        }
    }
}