using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

public partial class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60)]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(60)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = null!;

    [Required(ErrorMessage = "Debe seleccionar un tipo de documento.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el tipo de documento.")]
    public int IdDocumento { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [StringLength(20)]
    public string NumeroDocumento { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30, ErrorMessage = "El teléfono no puede exceder los 30 caracteres.")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el sexo.")]
    public int IdSexo { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Foto { get; set; }

    public int PuntosAcumulados { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public string? PasswordHash { get; set; }

    [NotMapped]
    public string NombreCompleto => $"{Nombres} {Apellidos}";

    // --- Propiedades de Navegación ---
    public virtual TipoDocumento IdDocumentoNavigation { get; set; } = null!;
    public virtual Sexo IdSexoNavigation { get; set; } = null!;
    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}