using Llanteria.Data;
using Llanteria.Models;

namespace Llanteria.Infrastructure;

/// <summary>
/// Catálogos que la aplicación necesita para funcionar y que el modelo no sembraba:
/// sin categorías de gasto no se podía registrar ningún gasto, y sin estados de inventario no se podía crear stock.
/// Solo agrega lo que falta; nunca modifica ni borra.
/// </summary>
public static class DatosIniciales
{
    public static void Asegurar(LlanteriaDbContext db)
    {
        foreach (var n in new[] { "Administrador", "Empleado", "Cliente" })
            if (!db.Roles.Any(r => r.NombreRol == n)) db.Roles.Add(new Role { NombreRol = n });

        foreach (var n in new[] { Estados.Disponible, Estados.Agotado, Estados.Danado })
            if (!db.EstadoInventarios.Any(e => e.Nombre == n)) db.EstadoInventarios.Add(new EstadoInventario { Nombre = n });

        foreach (var n in new[] { "Arriendo", "Servicios públicos", "Nómina", "Proveedores", "Impuestos", "Mantenimiento", "Otros" })
            if (!db.CategoriaGastos.Any(c => c.Nombre == n)) db.CategoriaGastos.Add(new CategoriaGasto { Nombre = n });

        db.SaveChanges();
    }

    public static class Estados
    {
        public const string Disponible = "Disponible";
        public const string Agotado = "Agotado";
        public const string Danado = "Dañado";
    }
}
