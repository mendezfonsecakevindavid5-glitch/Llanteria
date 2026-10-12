using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Cliente
{
    public int Id { get; set; }

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

    [Required(ErrorMessage = "El teléfono es obligatorio.")]

    [StringLength(20)]

    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")]

    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]

    [StringLength(100)]

    public string Correo { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el sexo.")]

    public int IdSexo { get; set; }

    public string? Foto { get; set; }

    public int PuntosAcumulados { get; set; }

    // Hash de la contraseña del club de puntos. Nunca se enlaza desde formularios.
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public string? PasswordHash { get; set; }

    public virtual TipoDocumento IdDocumentoNavigation { get; set; } = null!;
    public virtual Sexo IdSexoNavigation { get; set; } = null!;
    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}