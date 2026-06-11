using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class ClienteService
{
    private readonly LlanteriaDbContext _context;

    // Constructor con Inyección de Dependencias
    public ClienteService(LlanteriaDbContext context)
    {
        _context = context;
    }

    // Obtener todos los clientes (con sus relaciones)
    public List<Cliente> GetClientes()
    {
        return _context.Clientes
            .Include(c => c.IdDocumentoNavigation)
            .Include(c => c.IdSexoNavigation)
            .ToList();
    }

    // Obtener un cliente por su ID
    public Cliente? GetCliente(int id)
    {
        return _context.Clientes
            .Include(c => c.IdDocumentoNavigation)
            .Include(c => c.IdSexoNavigation)
            .FirstOrDefault(c => c.Id == id);
    }

    // Agregar un nuevo cliente
    public void AddCliente(Cliente obj)
    {
        _context.Clientes.Add(obj);
        _context.SaveChanges();
    }

    // Actualizar un cliente existente
    public void UpdateCliente(Cliente obj)
    {
        _context.Clientes.Update(obj);
        _context.SaveChanges();
    }

    // Eliminar un cliente por ID
    public void DeleteCliente(int id)
    {
        var obj = _context.Clientes.Find(id);
        if (obj != null)
        {
            _context.Clientes.Remove(obj);
            _context.SaveChanges();
        }
    }
}