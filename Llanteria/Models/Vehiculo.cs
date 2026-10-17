using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

public partial class Vehiculo
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(10, ErrorMessage = "La placa no puede exceder los 10 caracteres.")]
    [Display(Name = "Placa")]
    public string Placa { get; set; } = null!;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, ErrorMessage = "La marca no puede exceder los 50 caracteres.")]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = null!;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El modelo no puede exceder los 50 caracteres.")]
    [Display(Name = "Modelo")]
    public string Modelo { get; set; } = null!;

    [Range(1950, 2100, ErrorMessage = "Año no válido.")]
    [Display(Name = "Año")]
    public int? Anio { get; set; }

    /// <summary>Último kilometraje conocido (se actualiza con las órdenes de trabajo).</summary>
    [Range(0, 2000000, ErrorMessage = "Kilometraje no válido.")]
    [Display(Name = "Kilometraje")]
    public int? Kilometraje { get; set; }

    [Required(ErrorMessage = "Debe asignar un cliente al vehículo.")]
    [Display(Name = "Propietario / Cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona el propietario.")]
    public int IdCliente { get; set; }

    // --- Propiedades de Navegación ---

    [ForeignKey("IdCliente")]
    public virtual Cliente Cliente { get; set; } = null!;

    // ❌ BORRADO: Se eliminó por completo la línea virtual Conductore IdConductorNavigation que causaba el error.
}