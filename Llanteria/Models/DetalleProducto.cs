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

    // ---- Filtros nuevos del catálogo ----
    /// <summary>Voltaje de la batería: "6V", "12V", "24V". Solo baterías.</summary>
    public string? Voltaje { get; set; }

    /// <summary>Tecnología de la batería: "Plomo-ácido", "Libre de mantenimiento", "AGM", "Gel", "Litio". Solo baterías.</summary>
    public string? TecnologiaBateria { get; set; }

    /// <summary>Uso del lubricante: "Motor gasolina", "Motor diésel", "Motocicleta 4T", "Transmisión", "Hidráulico"... Solo lubricantes.</summary>
    public string? AplicacionLubricante { get; set; }

    /// <summary>Presentación del lubricante: "Litro", "Cuarto", "Galón (4 L)", "Balde (5 gal)"... Solo lubricantes.</summary>
    public string? PresentacionLubricante { get; set; }

    /// <summary>Tipo de accesorio: "Frenos", "Filtros", "Iluminación", "Limpiaparabrisas", "Seguridad", "Interior", "Exterior", "Herramientas". Solo accesorios.</summary>
    public string? TipoAccesorio { get; set; }

    /// <summary>Dónde va el accesorio: "Delantero", "Trasero" o "Universal". Solo accesorios.</summary>
    public string? UbicacionAccesorio { get; set; }

    /// <summary>Amperaje/capacidad de la batería (ej. "45Ah"). Solo aplica a baterías.</summary>
    public string? Amperaje { get; set; }

    /// <summary>Polaridad de la batería: "Izquierda" o "Derecha". Solo aplica a baterías.</summary>
    public string? Polaridad { get; set; }

    public virtual Marca? IdMarcaNavigation { get; set; } = null!;

    public virtual Producto? IdProductoNavigation { get; set; } = null!;
}


public static class TiposVehiculo
{
    // Los valores se guardan en DetalleProductos.TipoVehiculo (nvarchar(10)); por eso los códigos son cortos.
    public const string Auto = "Auto";
    public const string Moto = "Moto";
    public const string Pesada = "Pesada";     // Maquinaria pesada
    public const string Agricola = "Agricola"; // Maquinaria agrícola
    public static readonly string[] Todos = { Auto, Moto, Pesada, Agricola };

    /// <summary>Opciones para listas desplegables y chips: valor guardado + texto que ve la persona.</summary>
    public static readonly OpcionVehiculo[] Opciones =
    {
        new(Auto, "Automóviles"), new(Moto, "Motocicletas"), new(Pesada, "Maquinaria pesada"), new(Agricola, "Maquinaria agrícola")
    };

    public static string Etiqueta(string? v) => Opciones.FirstOrDefault(o => o.Valor == Normalizar(v))?.Texto ?? "Automóviles";

    /// <summary>Convierte cualquier texto (también el de un Excel: "Maquinaria agrícola", "pesada"...) a uno de los cuatro códigos. Lo desconocido es "Auto".</summary>
    public static string Normalizar(string? v)
    {
        var t = (v ?? "").Trim().ToLowerInvariant()
            .Replace("á", "a").Replace("í", "i");
        if (t.Length == 0) return Auto;
        if (t.StartsWith("moto")) return Moto;
        if (t.Contains("pesad")) return Pesada;
        if (t.Contains("agric")) return Agricola;
        return Auto;
    }
}

public sealed record OpcionVehiculo(string Valor, string Texto);
