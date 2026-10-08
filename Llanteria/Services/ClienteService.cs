using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;
namespace Llanteria.Services;

public class ClienteService

{

    private readonly LlanteriaDbContext _context;



    public ClienteService(LlanteriaDbContext context)

    {

        _context = context;

    }



    public List<Cliente> GetClientes()

    {

        return _context.Clientes

        .Include(c => c.IdDocumentoNavigation)

        .Include(c => c.IdSexoNavigation)

        .ToList();

    }



    public List<Cliente> GetClientesPorPuntos()

    {

        return _context.Clientes

        .Include(c => c.IdDocumentoNavigation)

        .Include(c => c.IdSexoNavigation)

        .OrderByDescending(c => c.PuntosAcumulados)

        .ToList();

    }



    public Cliente? GetCliente(int id)

    {

        return _context.Clientes

        .Include(c => c.IdDocumentoNavigation)

        .Include(c => c.IdSexoNavigation)

        .FirstOrDefault(c => c.Id == id);

    }



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



    public void AddCliente(Cliente obj)

    {

        _context.Clientes.Add(obj);

        _context.SaveChanges();

    }



    public void UpdateCliente(Cliente obj)

    {

        _context.Clientes.Update(obj);

        _context.SaveChanges();

    }



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