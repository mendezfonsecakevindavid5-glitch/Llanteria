namespace Llanteria.Models;

/// <summary>Cita solicitada desde la página de un servicio. Antes solo se enviaba por correo y no quedaba registro.</summary>
public class Cita
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Completada = "Completada";
    public const string Cancelada = "Cancelada";
    public static readonly string[] Estados = { Pendiente, Confirmada, Completada, Cancelada };

    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Servicio { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Hora { get; set; } = "";
    public string Vehiculo { get; set; } = "";
    public string? Direccion { get; set; }
    public string FormaPago { get; set; } = "Efectivo";
    public string? Notas { get; set; }
    public string Solicitante { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public int? IdCliente { get; set; }
    public string Estado { get; set; } = Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
