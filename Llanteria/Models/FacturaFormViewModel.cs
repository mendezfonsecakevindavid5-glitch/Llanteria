using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

public class FacturaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly? Fecha { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente.")]
    public int IdCliente { get; set; }

    [Range(0, 999999999, ErrorMessage = "El IVA no puede ser negativo.")]
    public decimal Iva { get; set; }

    [Range(0, 999999999, ErrorMessage = "La retención no puede ser negativa.")]
    public decimal? RteFte { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres.")]
    public string? Observaciones { get; set; }

    public List<LineaFacturaVM> Lineas { get; set; } = new();
}

public class LineaFacturaVM
{
    /// <summary>"P" = producto, "S" = servicio.</summary>
    public string Tipo { get; set; } = "P";
    public int IdItem { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
}
