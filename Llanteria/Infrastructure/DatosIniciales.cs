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
        foreach (var n in new[] { RolesApp.Administrador, RolesApp.Empleado, RolesApp.Cliente, RolesApp.UsuarioRegistrado })
            if (!db.Roles.Any(r => r.NombreRol == n)) db.Roles.Add(new Role { NombreRol = n });

        foreach (var n in new[] { Estados.Disponible, Estados.Agotado, Estados.Danado })
            if (!db.EstadoInventarios.Any(e => e.Nombre == n)) db.EstadoInventarios.Add(new EstadoInventario { Nombre = n });

        foreach (var n in new[] { "Arriendo", "Servicios públicos", "Nómina", "Proveedores", "Impuestos", "Mantenimiento", "Otros" })
            if (!db.CategoriaGastos.Any(c => c.Nombre == n)) db.CategoriaGastos.Add(new CategoriaGasto { Nombre = n });

        db.SaveChanges();
        SembrarBlog(db);
    }

    /// <summary>Carga los artículos de ejemplo solo si el blog está vacío: después, el administrador los maneja desde el panel.</summary>
    private static void SembrarBlog(LlanteriaDbContext db)
    {
        if (db.Articulos.Any()) return;

        db.Articulos.AddRange(BlogCatalogo.Semilla.Select(p => new Articulo
        {
            Slug = p.Slug, Titulo = p.Titulo, Resumen = p.Resumen, Categoria = p.Categoria,
            Fecha = p.Fecha.ToDateTime(TimeOnly.MinValue), Autor = p.Autor, Imagen = p.Imagen,
            Contenido = p.Contenido, Publicado = true
        }));
        db.SaveChanges();
    }

    public static class Estados
    {
        public const string Disponible = "Disponible";
        public const string Agotado = "Agotado";
        public const string Danado = "Dañado";
    }
}
