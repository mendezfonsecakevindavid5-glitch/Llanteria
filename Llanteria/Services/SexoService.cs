using Llanteria.Models;
using Llanteria.Data;

namespace Llanteria.Services;

public class SexoService
{
    private readonly LlanteriaDbContext _context;
    public SexoService(LlanteriaDbContext context) => _context = context;
    public List<Sexo> GetSexos() => _context.Sexos.ToList();
}