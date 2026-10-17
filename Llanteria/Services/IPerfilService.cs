using Llanteria.Models;
using System.Threading.Tasks;

namespace Llanteria.Services
{
    public interface IPerfilService
    {
        Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId);

        Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model);

        /// <summary>Foto del usuario (perfil, cliente o empleado, en ese orden) lista para enviar al navegador; null si no tiene.</summary>
        Task<(byte[] Datos, string Tipo)?> ObtenerFotoAsync(int usuarioId);

        // Nueva firma añadida para soportar el cambio de contraseña
        Task<bool> ActualizarPasswordAsync(int usuarioId, string passwordActual, string nuevaPassword);
    }
}