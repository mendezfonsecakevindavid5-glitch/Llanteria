using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public int IdDocumento { get; set; }

    public string NumeroDocumento { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int IdSexo { get; set; }

    public string? Foto { get; set; }

    public int PuntosAcumulados { get; set; }

    public virtual TipoDocumento IdDocumentoNavigation { get; set; } = null!;
    public virtual Sexo IdSexoNavigation { get; set; } = null!;
    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}