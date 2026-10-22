using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

/// <summary>
/// Estado de un registro. Ningún registro se elimina de la base de datos: "eliminar" lo pasa a Deshabilitado
/// (queda oculto en todas las consultas, ver LlanteriaDbContext). Bloqueado queda visible pero no se puede usar.
/// </summary>
public class Estado
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Nombre { get; set; } = null!;
}

/// <summary>Estado de los registros que tienen que ver con dinero (facturas, gastos, pedidos, órdenes de trabajo).</summary>
public class EstadoFinanciero
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Nombre { get; set; } = null!;
}

/// <summary>Valores semilla de la tabla Estados.</summary>
public static class EstadoIds
{
    public const int Deshabilitado = 1;
    public const int Habilitado = 2;
    public const int Bloqueado = 3;

    public static readonly (int Id, string Nombre)[] Semilla =
    {
        (Deshabilitado, "Deshabilitado"), (Habilitado, "Habilitado"), (Bloqueado, "Bloqueado")
    };
}

/// <summary>Valores semilla de la tabla EstadosFinancieros. Anulado es el equivalente financiero de Deshabilitado (queda oculto).</summary>
public static class EstadoFinancieroIds
{
    public const int Anulado = 1;
    public const int Pendiente = 2;
    public const int Pagado = 3;

    public static readonly (int Id, string Nombre)[] Semilla =
    {
        (Anulado, "Anulado"), (Pendiente, "Pendiente"), (Pagado, "Pagado")
    };
}

/// <summary>Marca: el modelo tiene IdEstado (tabla Estados) y no se elimina, se deshabilita.</summary>
public interface IConEstado { }

/// <summary>Marca: el modelo está ligado a recursos financieros; su IdEstado apunta a EstadosFinancieros.</summary>
public interface IConEstadoFinanciero { }

/// <summary>Marca: el registro pertenece a una llantería (tabla Llanterias), para que el sistema sirva a varias.</summary>
public interface IDeLlanteria
{
    int? IdLlanteria { get; set; }
}

// ---- Modelos con estado normal (Deshabilitado / Habilitado / Bloqueado) ----
public partial class Bodega : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class CanjeIncentivo : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class CatalogoIncentivo : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class CategoriaGasto : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Cliente : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class DetalleProducto : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Empleado : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class EstadoInventario : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
// Inventario ya tenía IdEstado (estado del stock: disponible, dañado...), así que su estado de registro se llama IdEstadoRegistro.
public partial class Inventario : IConEstado { public int IdEstadoRegistro { get; set; } = EstadoIds.Habilitado; }
public partial class Marca : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Oferta : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class PerfilUsuario : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Producto : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class Proveedore : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class RecuperacionCuenta : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Role : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Sexo : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class TipoDocumento : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class TipoServicio : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Usuario : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Vehiculo : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Pais : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Articulo : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class ServicioWeb : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class Resenia : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Cita : IConEstado, IDeLlanteria { public int IdEstado { get; set; } = EstadoIds.Habilitado; public int? IdLlanteria { get; set; } }
public partial class Pqrs : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class SolicitudEmpresa : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class Sugerencia : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }
public partial class EditorContenido : IConEstado { public int IdEstado { get; set; } = EstadoIds.Habilitado; }

// ---- Modelos ligados a finanzas: su IdEstado apunta a EstadosFinancieros ----
public partial class Factura : IConEstadoFinanciero, IDeLlanteria { public int IdEstado { get; set; } = EstadoFinancieroIds.Pagado; public int? IdLlanteria { get; set; } }
public partial class DetalleFactura : IConEstadoFinanciero { public int IdEstado { get; set; } = EstadoFinancieroIds.Pagado; }
public partial class Gasto : IConEstadoFinanciero, IDeLlanteria { public int IdEstado { get; set; } = EstadoFinancieroIds.Pagado; public int? IdLlanteria { get; set; } }
public partial class Pedido : IConEstadoFinanciero, IDeLlanteria { public int IdEstado { get; set; } = EstadoFinancieroIds.Pendiente; public int? IdLlanteria { get; set; } }
public partial class PedidoItem : IConEstadoFinanciero { public int IdEstado { get; set; } = EstadoFinancieroIds.Pendiente; }
public partial class OrdenTrabajo : IConEstadoFinanciero { public int IdEstado { get; set; } = EstadoFinancieroIds.Pendiente; }
