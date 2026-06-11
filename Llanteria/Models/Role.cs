using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Role
{
    public int Id { get; set; }

    public string NombreRol { get; set; } = null!;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
