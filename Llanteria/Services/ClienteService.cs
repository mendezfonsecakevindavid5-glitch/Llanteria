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

    // ✅ NUEVO: Obtener todos los clientes ordenados por puntos (para el panel de administración)
    public List<Cliente> GetClientesPorPuntos()
    {
        return _context.Clientes
            .Include(c => c.IdDocumentoNavigation)
            .Include(c => c.IdSexoNavigation)
            .OrderByDescending(c => c.PuntosAcumulados)
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

    // ✅ NUEVO: Actualizar únicamente el balance de puntos de un cliente de forma rápida
    public void ActualizarPuntos(int clienteId, int nuevosPuntos)
    {
        var cliente = _context.Clientes.Find(clienteId);
        if (cliente != null)
        {
            cliente.PuntosAcumulados = nuevosPuntos;
            _context.Clientes.Update(cliente);
            _context.SaveChanges();
        }
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