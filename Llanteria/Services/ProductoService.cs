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
        _context.Productos.Update(obj);
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