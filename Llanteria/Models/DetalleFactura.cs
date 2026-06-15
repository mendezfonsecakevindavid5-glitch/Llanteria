using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class DetalleFactura
{
    public int Id { get; set; }

    public int IdFactura { get; set; }

    public int? IdProducto { get; set; }

    public int? IdServicio { get; set; }

    public string CodigoItem { get; set; } = null!;

    public string Descripción { get; set; } = null!;

    public int Cantidad { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual Producto? IdProductoNavigation { get; set; }

    public virtual TipoServicio? IdServicioNavigation { get; set; }
}
