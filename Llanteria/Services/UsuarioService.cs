using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Services;

public class UsuarioService
{
    private readonly LlanteriaDbContext _context;

    // Constructor con Inyección de Dependencias
    public UsuarioService(LlanteriaDbContext context)
    {
        _context = context;
    }

    // Obtener todos los usuarios (incluyendo Rol y Empleado)
    public List<Usuario> GetUsuarios()
    {
        return _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .Include(u => u.IdEmpleadoNavigation)
            .ToList();
    }

    // Obtener un usuario por ID
    public Usuario? GetUsuario(int id)
    {
        return _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .Include(u => u.IdEmpleadoNavigation)
            .FirstOrDefault(u => u.Id == id);
    }

    // Agregar un nuevo usuario
    public void AddUsuario(Usuario obj)
    {
        _context.Usuarios.Add(obj);
        _context.SaveChanges();
    }

    // Actualizar un usuario existente
    public void UpdateUsuario(Usuario obj)
    {
        _context.Usuarios.Update(obj);
        _context.SaveChanges();
    }

    // Eliminar un usuario
    public void DeleteUsuario(int id)
    {
        var obj = _context.Usuarios.Find(id);
        if (obj != null)
        {
            _context.Usuarios.Remove(obj);
            _context.SaveChanges();
        }
    }
}