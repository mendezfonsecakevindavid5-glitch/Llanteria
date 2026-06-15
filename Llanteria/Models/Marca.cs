using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Marca
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<DetalleProducto> DetalleProductos { get; set; } = new List<DetalleProducto>();
}
