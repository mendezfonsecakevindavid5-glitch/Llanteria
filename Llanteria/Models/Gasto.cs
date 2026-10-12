using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Gasto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]

    public int IdCategoria { get; set; }

    [StringLength(200)]

    public string? Descripcion { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a cero.")]

    public decimal Monto { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public DateOnly? FechaPago { get; set; }

    public string? Estado { get; set; }

    public string? ArchivoSupport { get; set; }

    public virtual CategoriaGasto IdCategoriaNavigation { get; set; } = null!;
}
