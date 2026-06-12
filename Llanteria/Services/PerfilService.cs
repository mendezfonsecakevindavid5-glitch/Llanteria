using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Llanteria.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly LlanteriaDbContext _context;

        public PerfilService(LlanteriaDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId)
        {
            var perfil = await _context.PerfilUsuarios
                .Include(p => p.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            if (perfil == null) return null;

            var empleado = perfil.IdUsuarioNavigation.IdEmpleadoNavigation;

            var viewModel = new PerfilUsuarioViewModel
            {
                Id = perfil.Id,
                Nombre = $"{empleado.Nombres} {empleado.Apellidos}",
                Correo = empleado.Correo ?? "Sin correo",
                Bio = perfil.Bio ?? "",
                TemaPreferencia = perfil.TemaPreferencia ?? "Light",

                // Usamos Convert.ToBoolean para manejar posibles nulos de la BD
                NotificacionesActivas = perfil.NotificacionesActivas ?? false,

                FotoBase64 = perfil.FotoCircular != null ? Convert.ToBase64String(perfil.FotoCircular) : null,

                FacturasRecientes = await _context.Facturas
                    .Where(f => f.IdCliente == perfil.IdUsuario)
                    .OrderByDescending(f => f.Fecha)
                    .Take(5)
                    .Select(f => new FacturaViewModel
                    {
                        Id = f.Id,
                        NumeroFactura = f.Id.ToString(),

                        // CORRECCIÓN: Conversión segura de DateOnly? a DateTime
                        Fecha = f.Fecha.HasValue
                                ? f.Fecha.Value.ToDateTime(TimeOnly.MinValue)
                                : DateTime.Now,

                        TotalPagar = f.TotalPagar,
                        EstadoPago = "Pagado"
                    }).ToListAsync()
            };

            return viewModel;
        }

        public async Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model)
        {
            var perfil = await _context.PerfilUsuarios.FindAsync(model.Id);
            if (perfil == null) return false;

            perfil.Bio = model.Bio;
            perfil.NotificacionesActivas = model.NotificacionesActivas;
            perfil.TemaPreferencia = model.TemaPreferencia;

            if (model.NuevaFoto != null && model.NuevaFoto.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await model.NuevaFoto.CopyToAsync(ms);
                    perfil.FotoCircular = ms.ToArray();
                }
            }

            _context.PerfilUsuarios.Update(perfil);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}