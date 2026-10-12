using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Marca
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]

    [StringLength(100)]

    public string Nombre { get; set; } = null!;

    public virtual ICollection<DetalleProducto> DetalleProductos { get; set; } = new List<DetalleProducto>();
}
