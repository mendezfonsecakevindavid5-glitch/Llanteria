using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class DetalleProducto
{
    public int Id { get; set; }

    public int IdProducto { get; set; }

    public int IdMarca { get; set; }

    public string? Ancho { get; set; }

    public string? Perfil { get; set; }

    public string? Diametro { get; set; }

    public string? IndiceCarga { get; set; }

    public string? IndiceVelocidad { get; set; }

    public string? Viscosidad { get; set; }

    public string? TipoAceite { get; set; }

    public int? GarantiaMeses { get; set; }

    public virtual Marca IdMarcaNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
