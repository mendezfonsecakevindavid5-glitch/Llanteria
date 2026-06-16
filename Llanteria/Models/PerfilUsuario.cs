using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class PerfilUsuario
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string? Bio { get; set; }

    public string? TemaPreferencia { get; set; }

    public byte[]? FotoCircular { get; set; }

    public bool? NotificacionesActivas { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
