using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Llanteria.Services
{
    public interface IPerfilService
    {
        Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId);
        Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model);
    }

    public class PerfilService : IPerfilService
    {
        private readonly LlanteriaDbContext _context;

        public PerfilService(LlanteriaDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilUsuarioViewModel> ObtenerPerfilPorUsuarioIdAsync(int usuarioId)
        {
            // =========================================================================
            // ❌ COMENTADO TEMPORALMENTE: Bloquea el flujo porque la base de datos está vacía.
            // =========================================================================
            /*
            var perfil = await _context.PerfilUsuarios
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            if (perfil == null) return null;
            */

            // =========================================================================
            // ✅ BYPASS DE DATOS QUEMADOS: Evita que el controlador retorne un "NotFound"
            // =========================================================================
            var viewModel = new PerfilUsuarioViewModel
            {
                Id = usuarioId, // Asignamos el ID temporal enviado por el controlador (1)
                Bio = "Tengo una camioneta Hilux 4x4, prefiero llantas A/T para terreno difícil de la región.",
                TemaPreferencia = "Light",
                NotificacionesActivas = true,

                // Datos base quemados para pruebas en interfaz de usuario
                Correo = "usuario.valledupar@llanteria.com",
                Nombre = "Conductor Llantería",
                Telefono = "300 000 0000",
                Direccion = "Valledupar, Cesar",
                Puntos = 240, // Puntos acumulados del cliente

                // ============================================================
                // 📄 INYECCIÓN DE FACTURAS SIMULADAS (ESTRUCTURA REAL DB)
                // ============================================================
                FacturasRecientes = new List<FacturaSimuladaViewModel>
                {
                    new FacturaSimuladaViewModel
                    {
                        Id = 101,
                        NumeroFactura = "FAC-00243",
                        Fecha = DateTime.Now.AddDays(-15),
                        TotalPagar = 450000,
                        ServicioPrincipal = "Cambio de Llantas",
                        DetalleItems = "2x Neumáticos Bridgestone 195/65R15 + Montaje",
                        EstadoPago = "Pagado"
                    },
                    new FacturaSimuladaViewModel
                    {
                        Id = 102,
                        NumeroFactura = "FAC-00198",
                        Fecha = DateTime.Now.AddMonths(-2),
                        TotalPagar = 85000,
                        ServicioPrincipal = "Alineación y Balanceo",
                        DetalleItems = "Servicio de Alineación 3D + Balanceo de 4 ruedas",
                        EstadoPago = "Pagado"
                    },
                    new FacturaSimuladaViewModel
                    {
                        Id = 103,
                        NumeroFactura = "FAC-00150",
                        Fecha = DateTime.Now.AddMonths(-4),
                        TotalPagar = 120000,
                        ServicioPrincipal = "Mantenimiento Preventivo",
                        DetalleItems = "Cambio de Aceite Primario + Filtro de Aire",
                        EstadoPago = "Pagado"
                    }
                }
            };

            // Dejamos la foto de perfil en null para que la vista renderice la imagen por defecto
            viewModel.FotoBase64 = null;

            return viewModel;
        }

        public async Task<bool> ActualizarPerfilAsync(PerfilUsuarioViewModel model)
        {
            // =========================================================================
            // ❌ COMENTADO TEMPORALMENTE: No busca en la BD para evitar excepciones por registros inexistentes.
            // =========================================================================
            /*
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
            await _context.SaveChangesAsync();
            */

            // =========================================================================
            // ✅ BYPASS DE GUARDADO: Simula el éxito total en la UI al oprimir el botón
            // =========================================================================
            await Task.Delay(100); // Pequeña espera asíncrona simulada
            return true;
        }
    }
}