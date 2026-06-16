using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

public partial class Factura
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha de la factura es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Emisión")]
    public DateOnly? Fecha { get; set; }

    [Required(ErrorMessage = "Debe asignar un cliente a la factura.")]
    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Required]
    [Range(0.01, 9999999999.99, ErrorMessage = "El Total Bruto debe ser mayor a cero.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Subtotal (Bruto)")]
    public decimal TotalBruto { get; set; }

    [Required]
    [Range(0, 9999999999.99, ErrorMessage = "El valor del IVA no puede ser negativo.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Valor IVA")]
    public decimal Iva { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "La Retención no puede ser negativa.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Rte. Fuente")]
    public decimal? RteFte { get; set; } = 0;

    [Required]
    [Range(0.01, 9999999999.99, ErrorMessage = "El total a pagar debe ser mayor a cero.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Total Neto a Pagar")]
    public decimal TotalPagar { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [DataType(DataType.MultilineText)]
    public string? Observaciones { get; set; }

    // --- Propiedades de Navegación ---

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    // ✅ CORREGIDO: Ahora apunta correctamente al Cliente usando su IdCliente real
    [ForeignKey("IdCliente")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    // --- Lógica Extra para la UI ---

    [NotMapped]
    public string ResumenFactura => $"Factura #{Id} - Total: {TotalPagar:C2}";
}