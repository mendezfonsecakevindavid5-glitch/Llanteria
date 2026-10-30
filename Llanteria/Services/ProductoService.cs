using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class ProductoService
{
    private readonly LlanteriaDbContext _context;

    // Constructor con Inyección de Dependencias
    public ProductoService(LlanteriaDbContext context)
    {
        _context = context;
    }

    // Obtener todos los productos (incluyendo Proveedor y DetalleProducto)
    public List<Producto> GetProductos()
    {
        return _context.Productos
            .Include(p => p.IdProveedorNavigation)
            .Include(p => p.DetalleProducto) // ← Incluimos el detalle para que cargue Ancho, Perfil y Diámetro
            .ToList();
    }

    // Obtener un producto por su ID
    public Producto? GetProducto(int id)
    {
        return _context.Productos
            .Include(p => p.IdProveedorNavigation)
            .Include(p => p.DetalleProducto)
            .FirstOrDefault(p => p.Id == id);
    }

    // Agregar un nuevo producto
    public void AddProducto(Producto obj)
    {
        NormalizarDescuento(obj, null);
        _context.Productos.Add(obj);
        _context.SaveChanges();
    }

    /// <summary>
    /// Deja coherentes los tres datos del descuento: sin porcentaje no hay duración ni inicio; y el conteo de días
    /// arranca hoy cuando se crea el descuento o se cambia su porcentaje o su duración.
    /// </summary>
    private static void NormalizarDescuento(Producto p, (decimal? Porcentaje, int? Duracion)? anterior)
    {
        if (p.PorcentajeDescuento is not > 0)
        {
            p.PorcentajeDescuento = null;
            p.DuracionDescuentoDias = null;
            p.FechaInicioDescuento = null;
            return;
        }

        var cambio = anterior is null || anterior.Value.Porcentaje != p.PorcentajeDescuento || anterior.Value.Duracion != p.DuracionDescuentoDias;
        if (cambio || p.FechaInicioDescuento is null) p.FechaInicioDescuento = DateTime.Now;
    }

    // Actualizar un producto existente
    public void UpdateProducto(Producto obj)
    {
        var existente = _context.Productos
            .Include(p => p.DetalleProducto)
            .FirstOrDefault(p => p.Id == obj.Id);

        if (existente == null) return;

        var anteriorPorcentaje = existente.PorcentajeDescuento;
        var anteriorDuracion = existente.DuracionDescuentoDias;

        // Actualizar datos del producto
        existente.Nombre = obj.Nombre;
        existente.Descripcion = obj.Descripcion;
        existente.PrecioCompra = obj.PrecioCompra;
        existente.PrecioVenta = obj.PrecioVenta;
        existente.IdProveedor = obj.IdProveedor;
        existente.IdBodega = obj.IdBodega;
        existente.Categoria = obj.Categoria;
        existente.RutaImagen = obj.RutaImagen;

        // Descuento (porcentaje y duración). El día de inicio lo fija el sistema.
        existente.PorcentajeDescuento = obj.PorcentajeDescuento;
        existente.DuracionDescuentoDias = obj.DuracionDescuentoDias;
        NormalizarDescuento(existente, (anteriorPorcentaje, anteriorDuracion));

        // Actualizar o crear DetalleProducto
        if (obj.DetalleProducto != null && obj.DetalleProducto.IdMarca > 0)
        {
            if (existente.DetalleProducto == null)
            {
                obj.DetalleProducto.IdProducto = existente.Id;
                existente.DetalleProducto = obj.DetalleProducto;
            }
            else
            {
                existente.DetalleProducto.IdMarca = obj.DetalleProducto.IdMarca;
                existente.DetalleProducto.Ancho = obj.DetalleProducto.Ancho;
                existente.DetalleProducto.Perfil = obj.DetalleProducto.Perfil;
                existente.DetalleProducto.Diametro = obj.DetalleProducto.Diametro;
                existente.DetalleProducto.Viscosidad = obj.DetalleProducto.Viscosidad;
                existente.DetalleProducto.TipoAceite = obj.DetalleProducto.TipoAceite;
                existente.DetalleProducto.IndiceCarga = obj.DetalleProducto.IndiceCarga;
                existente.DetalleProducto.IndiceVelocidad = obj.DetalleProducto.IndiceVelocidad;
                existente.DetalleProducto.GarantiaMeses = obj.DetalleProducto.GarantiaMeses;
                existente.DetalleProducto.TipoVehiculo = obj.DetalleProducto.TipoVehiculo;
                existente.DetalleProducto.Amperaje = obj.DetalleProducto.Amperaje;
                existente.DetalleProducto.Polaridad = obj.DetalleProducto.Polaridad;
                existente.DetalleProducto.Voltaje = obj.DetalleProducto.Voltaje;
                existente.DetalleProducto.TecnologiaBateria = obj.DetalleProducto.TecnologiaBateria;
                existente.DetalleProducto.AplicacionLubricante = obj.DetalleProducto.AplicacionLubricante;
                existente.DetalleProducto.PresentacionLubricante = obj.DetalleProducto.PresentacionLubricante;
                existente.DetalleProducto.TipoAccesorio = obj.DetalleProducto.TipoAccesorio;
                existente.DetalleProducto.UbicacionAccesorio = obj.DetalleProducto.UbicacionAccesorio;
            }
        }

        _context.SaveChanges();
    }

    // Eliminar un producto por ID
    public void DeleteProducto(int id)
    {
        var obj = _context.Productos.Find(id);
        if (obj != null)
        {
            _context.Productos.Remove(obj);
            _context.SaveChanges();
        }
    }
}