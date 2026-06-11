using Llanteria.Models;
using Llanteria.Data;

namespace Llanteria.Services;

public class TipoDocumentoService
{
    private readonly LlanteriaDbContext _context;
    public TipoDocumentoService(LlanteriaDbContext context) => _context = context;
    public List<TipoDocumento> GetTipoDocumentos() => _context.TipoDocumentos.ToList();
}