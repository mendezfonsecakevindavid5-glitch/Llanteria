using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Inventario
{
    public int Id { get; set; }

    public int IdProducto { get; set; }

    public string Codigo { get; set; } = null!;

    public int? StockActual { get; set; }

    public int? StockMinimo { get; set; }

    public int IdEstado { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public int CantidadIngresada { get; set; }

    public int IdBodega { get; set; }

    public virtual Bodega IdBodegaNavigation { get; set; } = null!;

    public virtual EstadoInventario IdEstadoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
