using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class CanjeIncentivo
{
    public int Id { get; set; }

    public int IdCliente { get; set; }

    public int IdIncentivo { get; set; }

    public DateTime? FechaCanje { get; set; }

    public int EntregadoPor { get; set; }

    public virtual Usuario EntregadoPorNavigation { get; set; } = null!;

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual CatalogoIncentivo IdIncentivoNavigation { get; set; } = null!;
}
