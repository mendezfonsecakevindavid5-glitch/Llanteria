using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

public class EmpleadoService
{
    private readonly LlanteriaDbContext _context;

    public EmpleadoService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<Empleado> GetEmpleados()
    {
        return _context.Empleados
            .Include(e => e.IdRolNavigation)
            .Include(e => e.IdSexoNavigation)
            .Include(e => e.IdDocumentoNavigation)
            .ToList();
    }

    public Empleado? GetEmpleado(int id)
    {
        return _context.Empleados
            .Include(e => e.IdRolNavigation)
            .Include(e => e.IdSexoNavigation)
            .Include(e => e.IdDocumentoNavigation)
            .FirstOrDefault(e => e.Id == id);
    }

    public void AddEmpleado(Empleado obj)
    {
        _context.Empleados.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateEmpleado(Empleado obj)
    {
        _context.Empleados.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteEmpleado(int id)
    {
        var obj = _context.Empleados.Find(id);
        if (obj != null)
        {
            _context.Empleados.Remove(obj);
            _context.SaveChanges();
        }
    }
}