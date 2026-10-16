using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

// Parte escrita a mano de Producto: precio promocional opcional.
public partial class Producto
{
    [Range(0, 999999999, ErrorMessage = "El precio promocional no puede ser negativo.")]
    [Column(TypeName = "decimal(14, 2)")]
    [Display(Name = "Precio promocional")]
    public decimal? PrecioPromocion { get; set; }

    [NotMapped]
    public bool EnPromocion => PrecioPromocion is > 0 && PrecioPromocion < PrecioVenta;

    /// <summary>Lo que paga el cliente: el precio promocional si existe, si no el de venta.</summary>
    [NotMapped]
    public decimal PrecioActual => EnPromocion ? PrecioPromocion!.Value : PrecioVenta;
}
