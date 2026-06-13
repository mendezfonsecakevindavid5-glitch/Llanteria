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

            return new PerfilUsuarioViewModel
            {
                Id = perfil.Id,
                Nombre = empleado != null ? $"{empleado.Nombres} {empleado.Apellidos}" : "Usuario",
                Correo = empleado?.Correo ?? "Sin correo",
                Telefono = empleado?.Telefono ?? "",
                Bio = perfil.Bio ?? "",
                TemaPreferencia = perfil.TemaPreferencia ?? "Light",
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
                        Fecha = f.Fecha.HasValue ? f.Fecha.Value.ToDateTime(TimeOnly.MinValue) : DateTime.Now,
                        TotalPagar = f.TotalPagar,
                        EstadoPago = "Pagado"
                    }).ToListAsync()
            };
        }

        public async Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model)
        {
            var perfil = await _context.PerfilUsuarios
                .Include(p => p.IdUsuarioNavigation)
                .ThenInclude(u => u.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (perfil == null) return false;

            perfil.Bio = model.Bio;
            perfil.NotificacionesActivas = model.NotificacionesActivas;
            perfil.TemaPreferencia = model.TemaPreferencia;

            var empleado = perfil.IdUsuarioNavigation?.IdEmpleadoNavigation;
            if (empleado != null)
            {
                empleado.Telefono = model.Telefono;
            }

            if (model.NuevaFoto != null && model.NuevaFoto.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await model.NuevaFoto.CopyToAsync(ms);
                    perfil.FotoCircular = ms.ToArray();
                }
            }

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- NUEVO MÉTODO PARA EL CAMBIO DE CONTRASEÑA ---
        public async Task<bool> ActualizarPasswordAsync(int usuarioId, string passwordActual, string nuevaPassword)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null) return false;

            // Validación: ¿La contraseña actual es correcta?
            if (usuario.PasswordHash != passwordActual)
            {
                return false;
            }

            // Actualizamos la contraseña
            usuario.PasswordHash = nuevaPassword;

            try
            {
                _context.Usuarios.Update(usuario);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}