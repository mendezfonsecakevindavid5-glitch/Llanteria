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
        foreach (var n in new[] { RolesApp.Administrador, RolesApp.Secretario, RolesApp.Empleado, RolesApp.Cliente, RolesApp.UsuarioRegistrado })
            if (!db.Roles.Any(r => r.NombreRol == n)) db.Roles.Add(new Role { NombreRol = n });

        foreach (var n in new[] { Estados.Disponible, Estados.Agotado, Estados.Danado })
            if (!db.EstadoInventarios.Any(e => e.Nombre == n)) db.EstadoInventarios.Add(new EstadoInventario { Nombre = n });

        foreach (var n in new[] { "Arriendo", "Servicios públicos", "Nómina", "Proveedores", "Impuestos", "Mantenimiento", "Otros" })
            if (!db.CategoriaGastos.Any(c => c.Nombre == n)) db.CategoriaGastos.Add(new CategoriaGasto { Nombre = n });

        db.SaveChanges();
        CorregirRolDeClientesAntiguos(db);
        SembrarBlog(db);
        SembrarServicios(db);
        SembrarAdministrador(db);
        SembrarAdministradorKevin(db);
        SembrarCatalogoDemo(db);
    }

    /// <summary>
    /// Productos de ejemplo (llantas, aceites, baterías) solo para tener algo con qué probar la búsqueda
    /// y los filtros mientras se cargan los productos reales. Solo se ejecuta si no hay ningún producto todavía.
    /// </summary>
    private static void SembrarCatalogoDemo(LlanteriaDbContext db)
    {
        if (db.Productos.Any()) return;

        var proveedor = db.Proveedores.FirstOrDefault();
        if (proveedor == null)
        {
            proveedor = new Proveedore { NombreEmpresa = "Distribuidora Valledupar S.A.S.", Contacto = "Ventas", Telefono = "3000000000" };
            db.Proveedores.Add(proveedor);
        }

        var bodega = db.Bodegas.FirstOrDefault();
        if (bodega == null)
        {
            bodega = new Bodega { NombreBodega = "Bodega Principal", Ubicacion = "Valledupar", CapacidadMaxima = 1000 };
            db.Bodegas.Add(bodega);
        }

        var nombresMarca = new[] { "Michelin", "Pirelli", "Bridgestone", "Goodyear", "Mobil", "Castrol", "Varta" };
        var marcas = nombresMarca.ToDictionary(n => n, n => db.Marcas.FirstOrDefault(m => m.Nombre == n));
        foreach (var n in nombresMarca)
            if (marcas[n] == null) { marcas[n] = new Marca { Nombre = n }; db.Marcas.Add(marcas[n]!); }

        db.SaveChanges(); // para tener los Id antes de armar los productos

        Producto Llanta(string nombre, string marca, string ancho, string perfil, string diametro, string tipoVehiculo, decimal compra, decimal venta) => new()
        {
            Nombre = nombre,
            Categoria = "Llantas",
            Descripcion = $"Llanta {ancho}/{perfil} R{diametro}.",
            PrecioCompra = compra,
            PrecioVenta = venta,
            IdProveedor = proveedor.Id,
            IdBodega = bodega.Id,
            DetalleProducto = new DetalleProducto { IdMarca = marcas[marca]!.Id, Ancho = ancho, Perfil = perfil, Diametro = diametro, TipoVehiculo = tipoVehiculo }
        };

        Producto Aceite(string nombre, string marca, string viscosidad, string tipo, int garantiaMeses, decimal compra, decimal venta) => new()
        {
            Nombre = nombre,
            Categoria = "Aceites",
            Descripcion = $"Aceite {tipo.ToLowerInvariant()} {viscosidad}.",
            PrecioCompra = compra,
            PrecioVenta = venta,
            IdProveedor = proveedor.Id,
            IdBodega = bodega.Id,
            DetalleProducto = new DetalleProducto { IdMarca = marcas[marca]!.Id, Viscosidad = viscosidad, TipoAceite = tipo, GarantiaMeses = garantiaMeses }
        };

        Producto Bateria(string nombre, string marca, string amperaje, string polaridad, int garantiaMeses, decimal compra, decimal venta) => new()
        {
            Nombre = nombre,
            Categoria = "Baterías",
            Descripcion = "Batería para automóvil.",
            PrecioCompra = compra,
            PrecioVenta = venta,
            IdProveedor = proveedor.Id,
            IdBodega = bodega.Id,
            DetalleProducto = new DetalleProducto { IdMarca = marcas[marca]!.Id, Amperaje = amperaje, Polaridad = polaridad, GarantiaMeses = garantiaMeses }
        };

        db.Productos.AddRange(
            Llanta("Michelin Energy XM2", "Michelin", "185", "65", "15", TiposVehiculo.Auto, 180000, 260000),
            Llanta("Pirelli Cinturato P1", "Pirelli", "195", "55", "16", TiposVehiculo.Auto, 210000, 295000),
            Llanta("Bridgestone Turanza T005", "Bridgestone", "205", "55", "16", TiposVehiculo.Auto, 230000, 320000),
            Llanta("Goodyear Assurance", "Goodyear", "175", "70", "13", TiposVehiculo.Auto, 150000, 215000),
            Llanta("Pirelli Angel GT", "Pirelli", "110", "70", "17", TiposVehiculo.Moto, 160000, 230000),
            Aceite("Mobil Super 20W-50", "Mobil", "20W-50", "Mineral", 3, 28000, 42000),
            Aceite("Mobil 1 5W-30", "Mobil", "5W-30", "Sintético", 6, 55000, 79000),
            Aceite("Castrol GTX 15W-40", "Castrol", "15W-40", "Semisintético", 3, 32000, 47000),
            Aceite("Castrol Edge 5W-40", "Castrol", "5W-40", "Sintético", 6, 58000, 84000),
            Bateria("Varta Blue Dynamic 34R", "Varta", "34Ah", "Derecha", 12, 240000, 340000),
            Bateria("Varta Silver Dynamic 47R", "Varta", "47Ah", "Izquierda", 18, 310000, 430000)
        );
        db.SaveChanges();
    }

    /// <summary>
    /// Crea un administrador inicial solo si ese correo todavía no tiene cuenta (no toca nada si ya existe).
    /// La contraseña queda documentada aquí mismo porque, una vez guardada, el hash no se puede revertir:
    /// si se pierde, hay que restablecerla por el flujo normal de "Olvidé mi contraseña".
    /// </summary>
    private static void SembrarAdministrador(LlanteriaDbContext db)
    {
        const string correo = "lsantiagolgame@gmail.com";
        const string username = "lsantiagolgame";
        const string passwordInicial = "Ta#Llanteria2026$18"; // Cámbiala después de iniciar sesión por primera vez.

        if (db.Usuarios.Any(u => u.Correo == correo)) return;

        var idAdmin = db.Roles.Where(r => r.NombreRol == RolesApp.Administrador).Select(r => r.Id).FirstOrDefault();
        if (idAdmin == 0) return;

        db.Usuarios.Add(new Usuario
        {
            Username = username,
            Correo = correo,
            PasswordHash = Llanteria.Infrastructure.Claves.Hash(passwordInicial),
            IdRol = idAdmin,
            Estado = "Activo",
            FechaCreacion = DateTime.Now
        });
        db.SaveChanges();
    }

    /// <summary>
    /// Administrador de pruebas del panel (Dashboard + AdminLayout). Solo se crea si ese correo aún no existe;
    /// si ya existe, no se toca (no se pisa su rol ni su contraseña). Cambia la contraseña al iniciar sesión.
    /// </summary>
    private static void SembrarAdministradorKevin(LlanteriaDbContext db)
    {
        const string correo = "mendezfonsecakevindavid5@gmail.com";
        const string username = "mendezfonsecakevindavid5";
        const string passwordInicial = "Kv#Llanteria2026$51";

        if (db.Usuarios.Any(u => u.Correo == correo || u.Username == username)) return;

        var idAdmin = db.Roles.Where(r => r.NombreRol == RolesApp.Administrador).Select(r => r.Id).FirstOrDefault();
        if (idAdmin == 0) return;

        db.Usuarios.Add(new Usuario
        {
            Username = username,
            Correo = correo,
            PasswordHash = Llanteria.Infrastructure.Claves.Hash(passwordInicial),
            IdRol = idAdmin,
            Estado = "Activo",
            FechaCreacion = DateTime.Now
        });
        db.SaveChanges();
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
