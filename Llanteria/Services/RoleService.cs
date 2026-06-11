using Llanteria.Data;
using Llanteria.Models;

namespace Llanteria.Services
{
    public class RoleService
    {
        private readonly LlanteriaDbContext _context;
        public RoleService(LlanteriaDbContext context) => _context = context;
        public List<Role> GetRoles() => _context.Roles.ToList();
    }
}
