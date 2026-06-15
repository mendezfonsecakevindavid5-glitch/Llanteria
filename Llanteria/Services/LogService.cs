using Llanteria.Data;
using Llanteria.Models;
using System;
using System.Threading.Tasks;

namespace Llanteria.Services
{
    public class LogService : ILogService
    {
        private readonly LlanteriaDbContext _context;

        public LogService(LlanteriaDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarActividadAsync(int usuarioId, string accion, string tablaAfectada, string ip)
        {
            var log = new LogActividad
            {
                IdUsuario = usuarioId,
                Accion = accion,
                TablaAfectada = tablaAfectada,
                DireccionIp = ip,
                FechaRegistro = DateTime.Now
            };

            _context.LogActividads.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}