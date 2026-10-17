using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services
{
    /// <summary>
    /// Perfil de cualquier usuario con sesión: administrador, empleado, cliente antiguo o usuario registrado.
    /// Los datos personales salen del cliente o del empleado ligado a la cuenta.
    /// </summary>
    public class PerfilService : IPerfilService
    {
        private readonly LlanteriaDbContext _context;

        public PerfilService(LlanteriaDbContext context) => _context = context;

        public async Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId)
        {
            var perfil = await _context.PerfilUsuarios.AsNoTracking().FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            var usuario = await _context.Usuarios.AsNoTracking()
                .Include(u => u.IdEmpleadoNavigation)
                .Include(u => u.IdClienteNavigation)
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            var cliente = usuario?.IdClienteNavigation;
            var empleado = usuario?.IdEmpleadoNavigation;

            string nombres = cliente?.Nombres ?? empleado?.Nombres ?? usuario?.Username ?? "Usuario";
            string apellidos = cliente?.Apellidos ?? empleado?.Apellidos ?? "";

            var foto = await ObtenerFotoAsync(usuarioId);

            return new PerfilUsuarioViewModel
            {
                Id = usuarioId,
                Nombres = nombres,
                Apellidos = apellidos,
                Rol = usuario?.IdRolNavigation?.NombreRol,
                Correo = cliente?.Correo ?? empleado?.Correo ?? usuario?.Correo ?? "",
                Telefono = cliente?.Telefono ?? empleado?.Telefono ?? "",
                Puntos = cliente?.PuntosAcumulados ?? 0,
                Bio = perfil?.Bio ?? "",
                TemaPreferencia = perfil?.TemaPreferencia ?? "Light",
                NotificacionesActivas = perfil?.NotificacionesActivas ?? false,
                FotoBase64 = foto is { } f ? $"data:{f.Tipo};base64,{Convert.ToBase64String(f.Datos)}" : null
            };
        }

        public async Task<(byte[] Datos, string Tipo)?> ObtenerFotoAsync(int usuarioId)
        {
            var perfil = await _context.PerfilUsuarios.AsNoTracking().FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);
            if (perfil?.FotoCircular is { Length: > 0 } propia)
                return (propia, TipoImagen(propia));

            var u = await _context.Usuarios.AsNoTracking()
                .Include(x => x.IdClienteNavigation).Include(x => x.IdEmpleadoNavigation)
                .FirstOrDefaultAsync(x => x.Id == usuarioId);

            // El cliente guarda la foto como data URL (data:image/png;base64,....).
            var url = u?.IdClienteNavigation?.Foto;
            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var coma = url.IndexOf(',');
                if (coma > 0)
                {
                    try
                    {
                        var semi = url.IndexOf(';');
                        var tipo = semi > 5 && semi < coma ? url[5..semi] : "image/png";
                        return (Convert.FromBase64String(url[(coma + 1)..]), tipo);
                    }
                    catch (FormatException) { /* foto corrupta: se ignora y se muestran las iniciales */ }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            if (u?.IdEmpleadoNavigation?.Foto is { Length: > 0 } emp)
                return (emp, TipoImagen(emp));

            return null;
        }

        // Detecta el tipo real por los primeros bytes: PNG, GIF, WebP o JPEG (por defecto).
        private static string TipoImagen(byte[] d)
        {
            if (d.Length > 8 && d[0] == 0x89 && d[1] == 0x50) return "image/png";
            if (d.Length > 3 && d[0] == 0x47 && d[1] == 0x49) return "image/gif";
            if (d.Length > 12 && d[8] == 0x57 && d[9] == 0x45) return "image/webp";
            return "image/jpeg";
        }

        public async Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model)
        {
            var perfil = await _context.PerfilUsuarios.FirstOrDefaultAsync(p => p.IdUsuario == model.Id);
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

            var usuario = await _context.Usuarios
                .Include(u => u.IdEmpleadoNavigation)
                .Include(u => u.IdClienteNavigation)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            var nombres = (model.Nombres ?? "").Trim();
            var apellidos = (model.Apellidos ?? "").Trim();

            if (usuario?.IdClienteNavigation is { } c)
            {
                c.Nombres = nombres;
                c.Apellidos = apellidos;
                c.Telefono = model.Telefono;
            }
            else if (usuario?.IdEmpleadoNavigation is { } e)
            {
                e.Nombres = nombres;
                e.Apellidos = apellidos;
                e.Telefono = model.Telefono;
            }

            if (model.NuevaFoto != null && model.NuevaFoto.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.NuevaFoto.CopyToAsync(ms);
                perfil.FotoCircular = ms.ToArray();
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
            var usuario = await _context.Usuarios.Include(u => u.IdClienteNavigation).FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario == null) return false;

            if (!Llanteria.Infrastructure.Claves.Verificar(usuario.PasswordHash, passwordActual, out _))
                return false;

            var hash = Llanteria.Infrastructure.Claves.Hash(nuevaPassword);
            usuario.PasswordHash = hash;
            if (usuario.IdClienteNavigation != null) usuario.IdClienteNavigation.PasswordHash = hash;

            try { return await _context.SaveChangesAsync() > 0; }
            catch (Exception) { return false; }
        }
    }
}
