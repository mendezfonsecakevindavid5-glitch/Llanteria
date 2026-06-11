using Llanteria.Models;
using Llanteria.Data;

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
}