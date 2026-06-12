using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models
{
    public class PerfilUsuarioViewModel
    {
        public int Id { get; set; }

        public string? Bio { get; set; }

        public string? TemaPreferencia { get; set; }

        public bool NotificacionesActivas { get; set; }

        public IFormFile? NuevaFoto { get; set; }

        public string? FotoBase64 { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = null!;

        public string? Direccion { get; set; }

        public int Puntos { get; set; }

        // ============================================================
        // 📄 HISTORIAL DE FACTURAS Y SERVICIOS (DATOS SIMULADOS)
        // ============================================================
        public List<FacturaSimuladaViewModel> FacturasRecientes { get; set; } = new List<FacturaSimuladaViewModel>();
    }

    /// <summary>
    /// Modelo auxiliar para simular la estructura de tus tablas reales:
    /// Factura, DetalleFactura y TipoServicio.
    /// </summary>
    public class FacturaSimuladaViewModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public decimal TotalPagar { get; set; }
        public string ServicioPrincipal { get; set; } = null!;
        public string DetalleItems { get; set; } = null!;
        public string EstadoPago { get; set; } = null!;
    }
}