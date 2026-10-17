namespace Llanteria.Models;

/// <summary>Petición, queja, reclamo o sugerencia radicada por un cliente o visitante.</summary>
public class Pqrs
{
    public const string Recibida = "Recibida";
    public const string EnTramite = "En trámite";
    public const string Resuelta = "Resuelta";
    public static readonly string[] Estados = { Recibida, EnTramite, Resuelta };

    public static readonly string[] Tipos = { "Petición", "Queja", "Reclamo", "Sugerencia" };

    /// <summary>Días hábiles que el taller se compromete a tardar en responder.</summary>
    public const int DiasHabilesRespuesta = 15;

    public int Id { get; set; }
    /// <summary>Número de radicado visible para el cliente, p. ej. PQ-26-000012.</summary>
    public string Radicado { get; set; } = "";
    public string Tipo { get; set; } = "Petición";
    public string Nombre { get; set; } = "";
    public string? NumeroDocumento { get; set; }
    public string Correo { get; set; } = "";
    public string? Telefono { get; set; }
    public string Asunto { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public bool AutorizaDatos { get; set; }
    public int? IdCliente { get; set; }
    public string Estado { get; set; } = Recibida;
    public string? Respuesta { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaRespuesta { get; set; }

    /// <summary>Fecha límite de respuesta: suma días hábiles (lunes a viernes) a la fecha de radicación.</summary>
    public DateTime FechaLimite
    {
        get
        {
            var f = FechaCreacion.Date;
            var dias = 0;
            while (dias < DiasHabilesRespuesta)
            {
                f = f.AddDays(1);
                if (f.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) dias++;
            }
            return f;
        }
    }
}
