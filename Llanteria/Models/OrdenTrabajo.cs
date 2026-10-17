using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

/// <summary>
/// Orden de trabajo: el seguimiento de un vehículo dentro del taller (desde que entra hasta que se entrega).
/// Cuando se entrega, deja el historial del vehículo y programa el próximo mantenimiento.
/// </summary>
public class OrdenTrabajo
{
    public const string Recibido = "Recibido";
    public const string EnProceso = "En proceso";
    public const string Listo = "Listo";
    public const string Entregado = "Entregado";
    public const string Cancelado = "Cancelado";
    public static readonly string[] Estados = { Recibido, EnProceso, Listo, Entregado, Cancelado };

    public int Id { get; set; }

    /// <summary>Número visible para el cliente, p. ej. OT-26-000015.</summary>
    public string Numero { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el vehículo.")]
    [Display(Name = "Vehículo")]
    public int IdVehiculo { get; set; }

    /// <summary>Empleado responsable (opcional). Sin llave foránea: borrar un empleado no debe bloquearse por órdenes antiguas.</summary>
    [Display(Name = "Técnico responsable")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "Indica el tipo de trabajo.")]
    [StringLength(40)]
    [Display(Name = "Tipo de trabajo")]
    public string Tipo { get; set; } = Mantenimientos.Otro;

    [Required(ErrorMessage = "Describe el trabajo a realizar.")]
    [StringLength(150, ErrorMessage = "La descripción no puede superar 150 caracteres.")]
    [Display(Name = "Trabajo a realizar")]
    public string Descripcion { get; set; } = "";

    [Range(0, 2000000, ErrorMessage = "Kilometraje no válido.")]
    [Display(Name = "Kilometraje al ingresar")]
    public int? Kilometraje { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres.")]
    public string? Observaciones { get; set; }

    [Range(0, 999999999, ErrorMessage = "El valor no puede ser negativo.")]
    [Display(Name = "Valor estimado")]
    public decimal Total { get; set; }

    public string Estado { get; set; } = Recibido;

    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }

    /// <summary>Cuándo toca el próximo servicio de este tipo (por fecha). Si se deja vacío, se calcula al entregar.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Próximo servicio (fecha)")]
    public DateTime? ProximaFecha { get; set; }

    [Range(0, 2000000, ErrorMessage = "Kilometraje no válido.")]
    [Display(Name = "Próximo servicio (km)")]
    public int? ProximoKm { get; set; }

    public bool RecordatorioEnviado { get; set; }

    public virtual Vehiculo Vehiculo { get; set; } = null!;
}

/// <summary>Tipos de mantenimiento y cada cuánto se repiten (valores de referencia; el taller puede ajustarlos al crear la orden).</summary>
public static class Mantenimientos
{
    public const string Otro = "Otro";

    /// <summary>Tipo → (días hasta el próximo servicio, kilómetros hasta el próximo servicio). Null = no se repite.</summary>
    public static readonly Dictionary<string, (int? Dias, int? Km)> Intervalos = new()
    {
        ["Cambio de aceite y filtro"] = (180, 5000),
        ["Rotación de llantas"] = (180, 10000),
        ["Alineación y balanceo"] = (365, 10000),
        ["Cambio de llantas"] = (null, null),
        ["Batería"] = (730, null),
        ["Frenos"] = (365, 20000),
        ["Suspensión y amortiguadores"] = (null, 40000),
        ["Revisión general"] = (365, null),
        [Otro] = (null, null)
    };

    public static IEnumerable<string> Tipos => Intervalos.Keys;
}
