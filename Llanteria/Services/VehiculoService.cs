using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class VehiculoService
{
    private readonly LlanteriaDbContext _context;

    public VehiculoService(LlanteriaDbContext context)
    {
        _context = context;
    }

    public List<Vehiculo> GetVehiculos()
    {
        return _context.Vehiculos
            // ✅ CORREGIDO: Ahora incluye la propiedad de navegación Cliente
            .Include(v => v.Cliente)
            .ToList();
    }

    public Vehiculo? GetVehiculo(int id)
    {
        return _context.Vehiculos
            // ✅ CORREGIDO: Ahora incluye la propiedad de navegación Cliente
            .Include(v => v.Cliente)
            .FirstOrDefault(v => v.Id == id);
    }

    public void AddVehiculo(Vehiculo obj)
    {
        _context.Vehiculos.Add(obj);
        _context.SaveChanges();
    }

    public void UpdateVehiculo(Vehiculo obj)
    {
        _context.Vehiculos.Update(obj);
        _context.SaveChanges();
    }

    public void DeleteVehiculo(int id)
    {
        var obj = _context.Vehiculos.Find(id);
        if (obj != null)
        {
            _context.Vehiculos.Remove(obj);
            _context.SaveChanges();
        }
    }
}