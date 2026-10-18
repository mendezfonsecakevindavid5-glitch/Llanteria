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
        CorregirRolDeClientesAntiguos(db);
        SembrarBlog(db);
        SembrarServicios(db);
    }

    /// <summary>
    /// Antes el registro fijaba el rol con Id 2, que en la base es "Empleado": quien se registraba como cliente entraba al panel.
    /// Los usuarios ligados a un cliente (y no a un empleado) que quedaron con ese rol pasan a "Cliente", que conserva
    /// MiCuenta, puntos y reseñas pero no da acceso al panel. Es idempotente: si no hay casos, no cambia nada.
    /// </summary>
    private static void CorregirRolDeClientesAntiguos(LlanteriaDbContext db)
    {
        var idEmpleado = db.Roles.Where(r => r.NombreRol == RolesApp.Empleado).Select(r => r.Id).FirstOrDefault();
        var idCliente = db.Roles.Where(r => r.NombreRol == RolesApp.Cliente).Select(r => r.Id).FirstOrDefault();
        if (idEmpleado == 0 || idCliente == 0) return;

        var afectados = db.Usuarios.Where(u => u.IdRol == idEmpleado && u.IdCliente != null && u.IdEmpleado == null).ToList();
        if (afectados.Count == 0) return;

        foreach (var u in afectados) u.IdRol = idCliente;
        db.SaveChanges();
    }

    /// <summary>Carga los servicios de ejemplo solo si la tabla está vacía: después, el administrador los maneja desde el panel.</summary>
    private static void SembrarServicios(LlanteriaDbContext db)
    {
        if (db.ServiciosWeb.Any()) return;

        db.ServiciosWeb.AddRange(ServiciosCatalogo.Semilla.Select((v, i) => ServicioWeb.DesdeVista(v, i + 1)));
        db.SaveChanges();
    }

    /// <summary>Carga los artículos de ejemplo solo si el blog está vacío: después, el administrador los maneja desde el panel.</summary>
    private static void SembrarBlog(LlanteriaDbContext db)
    {
        if (db.Articulos.Any()) return;

        db.Articulos.AddRange(BlogCatalogo.Semilla.Select(p => new Articulo
        {
            Slug = p.Slug,
            Titulo = p.Titulo,
            Resumen = p.Resumen,
            Categoria = p.Categoria,
            Fecha = p.Fecha.ToDateTime(TimeOnly.MinValue),
            Autor = p.Autor,
            Imagen = p.Imagen,
            Contenido = p.Contenido,
            Publicado = true
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
