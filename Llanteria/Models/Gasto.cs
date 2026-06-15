using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Gasto
{
    public int Id { get; set; }

    public int IdCategoria { get; set; }

    public string? Descripcion { get; set; }

    public decimal Monto { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public DateOnly? FechaPago { get; set; }

    public string? Estado { get; set; }

    public string? ArchivoSupport { get; set; }

    public virtual CategoriaGasto IdCategoriaNavigation { get; set; } = null!;
}
