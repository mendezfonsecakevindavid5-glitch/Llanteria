using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Empleado
{
    public int Id { get; set; }

    public byte[]? Foto { get; set; }

    /// <summary>Foto del documento de identidad (cédula). Se guarda cifrada, igual que la foto del rostro.</summary>
    public byte[]? FotoDocumento { get; set; }

    /// <summary>Foto del rostro del empleado para verificación, distinta de la foto de perfil (<see cref="Foto"/>).</summary>
    public byte[]? FotoRostro { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]

    [StringLength(100)]

    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]

    [StringLength(100)]

    public string Apellidos { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el tipo de documento.")]

    public int IdDocumento { get; set; }

    [Required(ErrorMessage = "El documento es obligatorio.")]

    [StringLength(20)]

    public string NumeroDocumento { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    [Range(0, 999999999, ErrorMessage = "El salario no puede ser negativo.")]

    public decimal Salario { get; set; }

    public string? Horario { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el sexo.")]

    public int IdSexo { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el rol.")]

    public int IdRol { get; set; }

    public virtual TipoDocumento IdDocumentoNavigation { get; set; } = null!;

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual Sexo IdSexoNavigation { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
