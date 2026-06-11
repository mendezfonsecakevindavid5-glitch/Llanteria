using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class CatalogoIncentivo
{
    public int Id { get; set; }

    public string NombrePremio { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int PuntosRequeridos { get; set; }

    public int? StockDisponible { get; set; }

    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();
}
