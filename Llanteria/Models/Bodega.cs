using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Bodega
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]

    [StringLength(100)]

    public string NombreBodega { get; set; } = null!;

    public string? Ubicacion { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La capacidad no puede ser negativa.")]

    public int? CapacidadMaxima { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
