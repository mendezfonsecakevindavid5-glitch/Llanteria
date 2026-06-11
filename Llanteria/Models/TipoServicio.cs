using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class TipoServicio
{
    public int Id { get; set; }

    public string NombreServicio { get; set; } = null!;

    public decimal ValorServicio { get; set; }

    public string? IconoPath { get; set; }

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();
}
