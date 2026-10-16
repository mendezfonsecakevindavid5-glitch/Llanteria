namespace Llanteria.Models;

/// <summary>Pedido hecho desde la tienda en línea (carrito + checkout).</summary>
public class Pedido
{
    public const string Pendiente = "Pendiente";
    public const string Pagado = "Pagado";
    public const string Enviado = "Enviado";
    public const string Entregado = "Entregado";
    public const string Cancelado = "Cancelado";
    public static readonly string[] Estados = { Pendiente, Pagado, Enviado, Entregado, Cancelado };

    public const string Efectivo = "Efectivo";
    public const string Transferencia = "Transferencia";

    public int Id { get; set; }
    /// <summary>Número visible para el cliente, p. ej. LV-26-000015.</summary>
    public string Numero { get; set; } = "";
    /// <summary>Código aleatorio para ver la confirmación sin exponer el Id.</summary>
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public int? IdCliente { get; set; }

    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";

    public string Pais { get; set; } = "Colombia";
    public string Departamento { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string? Complemento { get; set; }
    public string CodigoPostal { get; set; } = "";
    public string? Notas { get; set; }

    public string MetodoPago { get; set; } = Efectivo;
    public string Estado { get; set; } = Pendiente;
    public string? CodigoCupon { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal BaseGravable { get; set; }
    public decimal Iva { get; set; }
    public decimal Envio { get; set; }
    public decimal Total { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public List<PedidoItem> Items { get; set; } = new();

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}

public class PedidoItem
{
    public int Id { get; set; }
    public int IdPedido { get; set; }
    /// <summary>Sin llave foránea a propósito: borrar un producto no debe bloquearse por pedidos antiguos.</summary>
    public int? IdProducto { get; set; }
    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Pedido? Pedido { get; set; }
}
