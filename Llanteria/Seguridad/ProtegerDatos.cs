using Llanteria.Data;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Seguridad;

/// <summary>
/// Cifra los datos que ya estaban guardados en texto claro antes de activar el cifrado.
/// Se ejecuta al iniciar solo si "Cifrado:MigrarDatosExistentes" es true; es idempotente
/// (lo que ya está cifrado no se vuelve a cifrar). Cuando termine, ponlo en false.
/// </summary>
public static class ProtegerDatos
{
    public static int Ejecutar(LlanteriaDbContext db)
    {
        var cambios = 0;

        foreach (var c in db.Clientes.Where(c => c.Foto != null).ToList())
        { db.Entry(c).Property(x => x.Foto).IsModified = true; cambios++; }

        foreach (var e in db.Empleados.Where(e => e.Foto != null).ToList())
        { db.Entry(e).Property(x => x.Foto).IsModified = true; cambios++; }

        foreach (var p in db.PerfilUsuarios.ToList())
        {
            if (p.Bio != null) { db.Entry(p).Property(x => x.Bio).IsModified = true; cambios++; }
            if (p.FotoCircular != null) { db.Entry(p).Property(x => x.FotoCircular).IsModified = true; cambios++; }
        }

        foreach (var u in db.Usuarios.Where(u => u.MotivoSancion != null).ToList())
        { db.Entry(u).Property(x => x.MotivoSancion).IsModified = true; cambios++; }

        if (cambios > 0) db.SaveChanges();
        return cambios;
    }
}
