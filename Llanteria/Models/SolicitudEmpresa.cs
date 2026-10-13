namespace Llanteria.Models;

/// <summary>Solicitud de una empresa que quiere atender su flota de vehículos en el taller.</summary>
public class SolicitudEmpresa
{
    public const string Pendiente = "Pendiente";
    public const string Contactada = "Contactada";
    public const string Cerrada = "Cerrada";
    public static readonly string[] Estados = { Pendiente, Contactada, Cerrada };
    public static readonly string[] TiposFlota =
    {
        "Transporte de carga", "Transporte de pasajeros", "Taxis y plataformas", "Vehículos de empresa",
        "Maquinaria y obra", "Transporte escolar", "Otro"
    };

    public int Id { get; set; }
    public string Empresa { get; set; } = "";
    public string? Nit { get; set; }
    public string Contacto { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public string? Ciudad { get; set; }
    public string TipoFlota { get; set; } = "";
    public int NumeroVehiculos { get; set; }
    public string? Mensaje { get; set; }
    public string Estado { get; set; } = Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
