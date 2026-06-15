using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Bodega
{
    public int Id { get; set; }

    public string NombreBodega { get; set; } = null!;

    public string? Ubicacion { get; set; }

    public int? CapacidadMaxima { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
