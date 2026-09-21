using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                .FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            // Obtener los datos del empleado/usuario relacionado
            var usuario = await _context.Usuarios
                .Include(u => u.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            var empleado = usuario?.IdEmpleadoNavigation;

            return new PerfilUsuarioViewModel
            {
                Id = usuarioId, // Mantenemos el IdUsuario como referencia principal
                Nombre = empleado != null ? $"{empleado.Nombres} {empleado.Apellidos}".Trim() : "Usuario",
                Correo = empleado?.Correo ?? "",
                Telefono = empleado?.Telefono ?? "",
                Bio = perfil?.Bio ?? "",
                TemaPreferencia = perfil?.TemaPreferencia ?? "Light",
                NotificacionesActivas = perfil?.NotificacionesActivas ?? false,
                FotoBase64 = perfil?.FotoCircular != null
                    ? $"data:image/png;base64,{Convert.ToBase64String(perfil.FotoCircular)}"
                    : null,
                FacturasRecientes = await _context.Facturas
                    .Where(f => f.IdCliente == usuarioId)
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
            // 1. Buscar o crear el perfil en la tabla PerfilUsuarios
            var perfil = await _context.PerfilUsuarios
                .FirstOrDefaultAsync(p => p.IdUsuario == model.Id);

            if (perfil == null)
            {
                perfil = new PerfilUsuario
                {
                    IdUsuario = model.Id,
                    Bio = model.Bio,
                    NotificacionesActivas = model.NotificacionesActivas,
                    TemaPreferencia = model.TemaPreferencia ?? "Light"
                };
                _context.PerfilUsuarios.Add(perfil);
            }
            else
            {
                perfil.Bio = model.Bio;
                perfil.NotificacionesActivas = model.NotificacionesActivas;
                perfil.TemaPreferencia = model.TemaPreferencia;
            }

            // 2. Actualizar Nombre, Teléfono y Correo en la entidad Empleados/Usuarios
            var usuario = await _context.Usuarios
                .Include(u => u.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (usuario?.IdEmpleadoNavigation != null)
            {
                // Separar el nombre completo en Nombres y Apellidos
                if (!string.IsNullOrWhiteSpace(model.Nombre))
                {
                    var partes = model.Nombre.Trim().Split(' ');
                    if (partes.Length > 1)
                    {
                        usuario.IdEmpleadoNavigation.Nombres = partes[0];
                        usuario.IdEmpleadoNavigation.Apellidos = string.Join(" ", partes.Skip(1));
                    }
                    else
                    {
                        usuario.IdEmpleadoNavigation.Nombres = model.Nombre;
                    }
                }

                usuario.IdEmpleadoNavigation.Telefono = model.Telefono;

                if (!string.IsNullOrWhiteSpace(model.Correo))
                {
                    usuario.IdEmpleadoNavigation.Correo = model.Correo;
                }
            }

            // 3. Procesar foto si subió una nueva
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
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarPasswordAsync(int usuarioId, string passwordActual, string nuevaPassword)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null) return false;

            if (usuario.PasswordHash != passwordActual)
            {
                return false;
            }

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