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

    [Required(ErrorMessage = "Debe asignar un cliente al vehículo.")]
    [Display(Name = "Propietario / Cliente")]
    public int IdCliente { get; set; }

    // --- Propiedades de Navegación ---

    [ForeignKey("IdCliente")]
    public virtual Cliente Cliente { get; set; } = null!;

    // ❌ BORRADO: Se eliminó por completo la línea virtual Conductore IdConductorNavigation que causaba el error.
}