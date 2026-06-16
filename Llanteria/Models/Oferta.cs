using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Oferta
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal? PorcentajeDescuento { get; set; }

    public decimal? MontoDescuentoFijo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool? Estado { get; set; }

    public string? CodigoPromocional { get; set; }
}
