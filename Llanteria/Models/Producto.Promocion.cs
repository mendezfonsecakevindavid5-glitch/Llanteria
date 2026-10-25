using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

// Parte escrita a mano de Producto: descuentos. Aplica igual a llantas, baterías y lubricantes.
public partial class Producto
{
    [Range(0, 999999999, ErrorMessage = "El precio promocional no puede ser negativo.")]
    [Column(TypeName = "decimal(14, 2)")]
    [Display(Name = "Precio promocional")]
    public decimal? PrecioPromocion { get; set; }

    /// <summary>Porcentaje de descuento sobre el precio de venta (0 a 100). Vacío = sin descuento porcentual.</summary>
    [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe estar entre 0 y 100.")]
    [Column(TypeName = "decimal(5, 2)")]
    [Display(Name = "Porcentaje de descuento")]
    public decimal? PorcentajeDescuento { get; set; }

    /// <summary>Cuántos días dura el descuento, contados desde <see cref="FechaInicioDescuento"/>. Vacío = sin vencimiento.</summary>
    [Range(1, 3650, ErrorMessage = "La duración debe ser de 1 a 3650 días.")]
    [Display(Name = "Duración del descuento (días)")]
    public int? DuracionDescuentoDias { get; set; }

    /// <summary>Día en que empezó el descuento. Lo fija el sistema al guardar o cambiar el descuento.</summary>
    [Column(TypeName = "datetime")]
    [Display(Name = "Inicio del descuento")]
    public DateTime? FechaInicioDescuento { get; set; }

    /// <summary>Primer instante en que el descuento ya no aplica (a las 00:00 del día siguiente al último día); null si no vence.</summary>
    [NotMapped]
    public DateTime? FechaFinDescuento =>
        FechaInicioDescuento.HasValue && DuracionDescuentoDias.HasValue
            ? FechaInicioDescuento.Value.Date.AddDays(DuracionDescuentoDias.Value)
            : null;

    /// <summary>¿Hay un descuento por porcentaje que todavía no venció?</summary>
    [NotMapped]
    public bool DescuentoPorcentualActivo =>
        PorcentajeDescuento is > 0 and <= 100 && (FechaFinDescuento is null || DateTime.Now < FechaFinDescuento);

    [NotMapped]
    private bool PrecioPromocionActivo => PrecioPromocion is > 0 && PrecioPromocion < PrecioVenta;

    /// <summary>Lo que paga el cliente: el precio con el porcentaje descontado si hay descuento vigente; si no, el precio promocional; si no, el de venta.</summary>
    [NotMapped]
    public decimal PrecioActual =>
        DescuentoPorcentualActivo
            ? Math.Round(PrecioVenta * (1 - PorcentajeDescuento!.Value / 100m), 0, MidpointRounding.AwayFromZero)
            : PrecioPromocionActivo ? PrecioPromocion!.Value : PrecioVenta;

    [NotMapped]
    public bool EnPromocion => PrecioActual < PrecioVenta;

    /// <summary>Número entero que se dibuja en el círculo rojo de la tarjeta (0 si no hay descuento).</summary>
    [NotMapped]
    public int PorcentajeMostrado =>
        !EnPromocion || PrecioVenta <= 0 ? 0
        : DescuentoPorcentualActivo ? (int)Math.Round(PorcentajeDescuento!.Value, MidpointRounding.AwayFromZero)
        : (int)Math.Round((1 - PrecioActual / PrecioVenta) * 100, MidpointRounding.AwayFromZero);
}
