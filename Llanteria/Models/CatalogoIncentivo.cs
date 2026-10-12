using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class CatalogoIncentivo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del premio es obligatorio.")]

    [StringLength(100)]

    public string NombrePremio { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Indica los puntos requeridos.")]

    public int PuntosRequeridos { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Las unidades no pueden ser negativas.")]

    public int? StockDisponible { get; set; }

    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();
}
