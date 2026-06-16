using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class EstadoInventario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
