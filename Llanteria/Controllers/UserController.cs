using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Llanteria.Controllers
{
    public class UserController : Controller
    {
        private readonly IPerfilService _perfilService;

        // Inyección de dependencias del servicio de perfil
        public UserController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // GET: /User/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            // ID temporal de prueba (Mock) para desarrollo. 
            // Cuando integres el login real, aquí extraerás el ID del usuario autenticado.
            int usuarioIdMock = 1;

            var model = await _perfilService.ObtenerPerfilPorUsuarioIdAsync(usuarioIdMock);

            if (model == null)
            {
                // Si por alguna razón el usuario no tiene fila en PerfilUsuario, puedes lanzar un error controlado
                return NotFound("No se encontró el perfil para el usuario especificado.");
            }

            return View(model);
        }

        // POST: /User/ActualizarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra ataques CSRF
        public async Task<IActionResult> ActualizarPerfil(PerfilUsuarioViewModel model)
        {
            // Validamos que los campos obligatorios del ViewModel (como el Nombre o Teléfono) se cumplan
            if (!ModelState.IsValid)
            {
                // Si la validación falla, recargamos la vista mostrando los errores
                return View("Perfil", model);
            }

            // Llamamos al servicio para guardar la Bio, Preferencias y procesar la Foto circular
            var guardadoExitoso = await _perfilService.ActualizarPerfilAsync(model);

            if (guardadoExitoso)
            {
                // Mensaje que leerá el script de SweetAlert2 en la vista
                TempData["SuccessMessage"] = "¡Tu perfil se ha actualizado con éxito!";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudieron guardar los cambios. Inténtalo de nuevo.";
            }

            // Redireccionamos al GET para limpiar el envío del formulario y refrescar la pantalla
            return RedirectToAction(nameof(Perfil));
        }
    }
}