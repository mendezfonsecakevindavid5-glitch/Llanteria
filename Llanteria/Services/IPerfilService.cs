using Llanteria.Models;

namespace Llanteria.Services
{
    public interface IPerfilService
    {
        Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId);
        Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model);
    }
}