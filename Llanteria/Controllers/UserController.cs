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

        public UserController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // GET: /User/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            // Intentar obtener el ID desde NameIdentifier o Name
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _perfilService.ObtenerPerfilPorUsuarioIdAsync(usuarioId);

            // CORRECCIÓN: Si el perfil aún no existe en la BD, creamos una instancia con el Id del usuario
            if (model == null)
            {
                model = new PerfilUsuarioViewModel
                {
                    Id = usuarioId
                };
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(PerfilUsuarioViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int usuarioId))
            {
                model.Id = usuarioId;
            }

            // Ignorar colecciones o campos que no provocan inconsistencias
            ModelState.Remove("FacturasRecientes");

            if (!ModelState.IsValid)
            {
                var perfilRefrescado = await _perfilService.ObtenerPerfilPorUsuarioIdAsync(model.Id);
                model.FacturasRecientes = perfilRefrescado.FacturasRecientes;
                return View("Perfil", model);
            }

            var actualizado = await _perfilService.ActualizarPerfilAsync(model);

            if (actualizado)
            {
                // Mensaje de éxito que se mostrará en pantalla y notificará sobre el correo
                TempData["SuccessMessage"] = $"¡Tus cambios han sido guardados con éxito! Se ha enviado un mensaje de confirmación a <b>{model.Correo}</b>.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudieron guardar los cambios en la base de datos. Inténtalo de nuevo.";
            }

            return RedirectToAction(nameof(Perfil));
        }

        // POST: /User/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string actual, string nueva)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

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