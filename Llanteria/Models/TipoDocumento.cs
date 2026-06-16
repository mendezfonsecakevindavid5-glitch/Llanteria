using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class TipoDocumento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? SiglasPresentacion { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
