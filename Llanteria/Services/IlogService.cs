using Llanteria.Models;
using System.Threading.Tasks;

namespace Llanteria.Services
{
    public interface ILogService
    {
        Task RegistrarActividadAsync(int usuarioId, string accion, string tablaAfectada, string ip);
    }
}