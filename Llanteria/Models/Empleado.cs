using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Empleado
{
    public int Id { get; set; }

    public byte[]? Foto { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public int IdDocumento { get; set; }

    public string NumeroDocumento { get; set; } = null!;

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public decimal Salario { get; set; }

    public string? Horario { get; set; }

    public int IdSexo { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public int IdRol { get; set; }

    public virtual TipoDocumento IdDocumentoNavigation { get; set; } = null!;

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual Sexo IdSexoNavigation { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
