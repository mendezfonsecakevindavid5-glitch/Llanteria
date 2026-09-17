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

    // Obtener todos los productos (incluyendo el Proveedor si es necesario)
    public List<Producto> GetProductos()
    {
        return _context.Productos
            .Include(p => p.IdProveedorNavigation)
            .ToList();
    }

    // Obtener un producto por su ID
    public Producto? GetProducto(int id)
    {
        return _context.Productos
            .Include(p => p.IdProveedorNavigation)
            .Include(p => p.DetalleProducto)          // ← AGREGAR ESTA LÍNEA
            .FirstOrDefault(p => p.Id == id);
    }

    // Agregar un nuevo producto
    public void AddProducto(Producto obj)
    {
        _context.Productos.Add(obj);
        _context.SaveChanges();
    }

    // Actualizar un producto existente
    public void UpdateProducto(Producto obj)
    {
        var existente = _context.Productos
            .Include(p => p.DetalleProducto)
            .FirstOrDefault(p => p.Id == obj.Id);

        if (existente == null) return;

        // Actualizar datos del producto
        existente.Nombre = obj.Nombre;
        existente.Descripcion = obj.Descripcion;
        existente.PrecioCompra = obj.PrecioCompra;
        existente.PrecioVenta = obj.PrecioVenta;
        existente.IdProveedor = obj.IdProveedor;
        existente.IdBodega = obj.IdBodega;
        existente.Categoria = obj.Categoria;
        existente.RutaImagen = obj.RutaImagen;

        // Actualizar o crear DetalleProducto
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
                existente.DetalleProducto.GarantiaMeses = obj.DetalleProducto.GarantiaMeses;
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