using Llanteria.Models;
using Llanteria.Data;
using System.Collections.Generic;
using System.Linq;

namespace Llanteria.Services;

public class ProveedoreService
{
    private readonly LlanteriaDbContext _context;

    public ProveedoreService(LlanteriaDbContext context)
    {
        _context = context;
    }

    // Obtener todos los proveedores (Empresas)
    public List<Proveedore> GetProveedores()
    {
        return _context.Proveedores.OrderBy(p => p.NombreEmpresa).ToList();
    }

    // Obtener un proveedor por ID
    public Proveedore GetProveedor(int id)
    {
        return _context.Proveedores.FirstOrDefault(p => p.Id == id);
    }

    // ✅ NUEVO: Agregar Proveedor
    public void AddProveedor(Proveedore p)
    {
        _context.Proveedores.Add(p);
        _context.SaveChanges(); // 👈 Al ejecutar esto, SQL genera el ID automáticamente y se lo asigna al objeto 'p'
    }

    // ✅ NUEVO: Actualizar Proveedor
    public void UpdateProveedor(Proveedore p)
    {
        _context.Proveedores.Update(p);
        _context.SaveChanges();
    }

    // ✅ NUEVO: Eliminar Proveedor
    public void DeleteProveedor(int id)
    {
        var prov = GetProveedor(id);
        if (prov != null)
        {
            _context.Proveedores.Remove(prov);
            _context.SaveChanges();
        }
    }
}