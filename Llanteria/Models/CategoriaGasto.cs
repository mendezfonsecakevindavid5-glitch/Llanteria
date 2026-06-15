using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class CategoriaGasto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
}
