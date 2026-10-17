using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Llanteria.Models;

public partial class DetalleProducto
{
    public int Id { get; set; }

    public int IdProducto { get; set; }

    public int IdMarca { get; set; }

    public string? Ancho { get; set; }

    public string? Perfil { get; set; }

    public string? Diametro { get; set; }

    /// <summary>Tipo de vehículo al que sirve la llanta: "Auto" o "Moto" (buscador del inicio).</summary>
    public string TipoVehiculo { get; set; } = TiposVehiculo.Auto;

    public string? IndiceCarga { get; set; }


    public string? IndiceVelocidad { get; set; }

    public string? Viscosidad { get; set; }

    public string? TipoAceite { get; set; }

    public int? GarantiaMeses { get; set; }

    public virtual Marca? IdMarcaNavigation { get; set; } = null!;

    public virtual Producto? IdProductoNavigation { get; set; } = null!;
}


public static class TiposVehiculo
{
    public const string Auto = "Auto";
    public const string Moto = "Moto";
    public static readonly string[] Todos = { Auto, Moto };
    public static string Normalizar(string? v) => string.Equals(v?.Trim(), Moto, StringComparison.OrdinalIgnoreCase) ? Moto : Auto;
}
