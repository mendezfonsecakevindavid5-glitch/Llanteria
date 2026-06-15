using System;
using System.Collections.Generic;

namespace Llanteria.Models;

public partial class LogActividad
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public string? TablaAfectada { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? DireccionIp { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
