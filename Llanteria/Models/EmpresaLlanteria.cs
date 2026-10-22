using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

/// <summary>
/// Una llantería (negocio) que usa el sistema. La tabla permite que el mismo sistema funcione para varias:
/// los modelos principales (productos, bodegas, empleados, promociones, facturas...) guardan IdLlanteria.
/// El nombre de la clase evita chocar con el espacio de nombres "Llanteria"; la tabla se llama Llanterias.
/// </summary>
public partial class EmpresaLlanteria : IConEstado
{
    public int Id { get; set; }

    // ---- Información del negocio ----
    [Required(ErrorMessage = "El nombre del negocio es obligatorio."), StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)] public string? RazonSocial { get; set; }
    [StringLength(20)] public string? Nit { get; set; }
    [StringLength(200)] public string? Direccion { get; set; }
    [StringLength(60)] public string? Ciudad { get; set; }
    [StringLength(60)] public string? Departamento { get; set; }
    [StringLength(25)] public string? Telefono { get; set; }
    [StringLength(20)] public string? WhatsApp { get; set; }
    [StringLength(100)] public string? Correo { get; set; }
    [StringLength(200)] public string? SitioWeb { get; set; }
    [StringLength(200)] public string? Instagram { get; set; }
    [StringLength(200)] public string? Facebook { get; set; }
    [StringLength(120)] public string? Logo { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    [StringLength(40)] public string? HorarioSemana { get; set; }
    [StringLength(40)] public string? HorarioSabado { get; set; }
    [StringLength(40)] public string? HorarioDomingo { get; set; }

    // ---- Información del propietario ----
    [StringLength(120)] public string? PropietarioNombre { get; set; }
    [StringLength(20)] public string? PropietarioDocumento { get; set; }
    [StringLength(25)] public string? PropietarioTelefono { get; set; }
    [StringLength(100)] public string? PropietarioCorreo { get; set; }

    // ---- Lo que ofrece ----
    public bool OfreceLlantas { get; set; } = true;
    public bool OfreceLubricantes { get; set; } = true;
    public bool OfreceBaterias { get; set; } = true;
    public bool OfreceAccesorios { get; set; } = true;
    public bool OfreceMantenimientoFlotas { get; set; }
    public bool OfreceServicioADomicilio { get; set; }

    /// <summary>Servicios que presta, uno por línea (alineación, balanceo, cambio de aceite, frenos...).</summary>
    [StringLength(1000)] public string? ServiciosOfrecidos { get; set; }

    /// <summary>Marcas y productos destacados, uno por línea.</summary>
    [StringLength(1000)] public string? ProductosDestacados { get; set; }

    [StringLength(1000)] public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public int IdEstado { get; set; } = EstadoIds.Habilitado;
}
