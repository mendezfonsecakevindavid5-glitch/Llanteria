using Llanteria.Models;
using System.Threading.Tasks;

namespace Llanteria.Services
{
    public interface IPerfilService
    {
        Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId);

        Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model);

        // Nueva firma añadida para soportar el cambio de contraseña
        Task<bool> ActualizarPasswordAsync(int usuarioId, string passwordActual, string nuevaPassword);
    }
}