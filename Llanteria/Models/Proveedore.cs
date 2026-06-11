using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Proveedore
{
    public int Id { get; set; }

    public string NombreEmpresa { get; set; } = null!;

    public string? Contacto { get; set; }

    public string? Telefono { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
