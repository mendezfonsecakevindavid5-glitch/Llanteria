using System.Data;
using Llanteria.Data;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Carga los datos de ejemplo (los INSERT INTO de todas las tablas) al iniciar la aplicación, para que una base
/// recién creada ya salga con productos, facturas, clientes, gastos, citas, etc. y no haya que ejecutar ningún script a mano.
///
/// El script vive en Scripts/DatosEjemploInicial.sql y viaja DENTRO del proyecto como recurso incrustado.
///
/// Reglas de seguridad (para no duplicar ni pisar datos reales):
///  - Solo se carga si la base está "recién creada": existen únicamente las 3 cuentas iniciales (administrador,
///    empleado y usuario registrado) y todas las tablas de ejemplo están vacías. Si ya hay datos, no hace nada.
///  - Todo el script corre en una sola transacción: si algo falla se revierte y la aplicación sigue arrancando.
///  - Se controla con la clave "DatosEjemplo:CargarAlIniciar" (true/false). Si no está definida, se carga solo
///    en el entorno Development; en producción hay que activarla a propósito.
///
/// Debe ejecutarse DESPUÉS de crear los catálogos y las cuentas iniciales, y ANTES de LlanteriaInicial y del cifrado
/// (para que lo que se inserte en texto plano quede cifrado en ese mismo arranque).
/// </summary>
public static class DatosEjemplo
{
    private const string NombreRecurso = "DatosEjemploInicial.sql";

    /// <summary>Tablas que el script llena con Id fijos: deben estar vacías para que no haya choques de llave primaria.</summary>
    private static readonly string[] TablasQueDebenEstarVacias =
    {
        "Llanterias", "Eventos", "Ofertas", "Proveedores", "Marcas", "TipoServicio", "Bodegas", "CatalogoIncentivos",
        "EditoresContenido", "Articulos", "ServiciosWeb", "Productos", "DetalleProductos", "Inventario", "Vehiculos",
        "OrdenesTrabajo", "Facturas", "DetalleFactura", "Pedidos", "PedidoItems", "Citas", "CanjeIncentivos",
        "Resenias", "Sugerencias", "Pqrs", "SolicitudesEmpresa", "Gastos"
    };

    public static void Asegurar(LlanteriaDbContext db, ILogger logger, IConfiguration config, IHostEnvironment entorno)
    {
        var ajuste = config["DatosEjemplo:CargarAlIniciar"];
        var cargar = bool.TryParse(ajuste, out var valor) ? valor : entorno.IsDevelopment();
        if (!cargar)
        {
            logger.LogInformation("Datos de ejemplo desactivados (DatosEjemplo:CargarAlIniciar).");
            return;
        }

        var conexion = db.Database.GetDbConnection();
        var estabaAbierta = conexion.State == ConnectionState.Open;
        if (!estabaAbierta) conexion.Open();
        try
        {
            if (!BaseRecienCreada(conexion))
            {
                logger.LogInformation("Datos de ejemplo omitidos: la base ya tiene datos (solo se cargan en una base recién creada).");
                return;
            }

            using var comando = conexion.CreateCommand();
            comando.CommandText = LeerScript();
            comando.CommandTimeout = 180;
            comando.ExecuteNonQuery();
            logger.LogInformation("Datos de ejemplo cargados correctamente.");
        }
        finally
        {
            if (!estabaAbierta) conexion.Close();
        }
    }

    private static bool BaseRecienCreada(System.Data.Common.DbConnection conexion)
    {
        var vacias = string.Join(" + ", TablasQueDebenEstarVacias.Select(t => $"(SELECT COUNT(*) FROM dbo.{t})"));
        var sql = $@"
SELECT CASE WHEN
        (SELECT COUNT(*) FROM dbo.Usuarios) = 3
    AND (SELECT COUNT(*) FROM dbo.Clientes) = 1
    AND (SELECT COUNT(*) FROM dbo.Empleados) = 2
    AND (SELECT COUNT(*) FROM dbo.Roles WHERE (Id = 1 AND NombreRol = N'{RolesApp.Administrador}') OR (Id = 2 AND NombreRol = N'{RolesApp.Secretario}')
            OR (Id = 3 AND NombreRol = N'{RolesApp.Empleado}') OR (Id = 4 AND NombreRol = N'{RolesApp.Cliente}')
            OR (Id = 5 AND NombreRol = N'{RolesApp.UsuarioRegistrado}')) = 5
    AND (SELECT COUNT(*) FROM dbo.Usuarios u JOIN dbo.Roles r ON r.Id = u.IdRol
          WHERE (u.Id = 1 AND r.NombreRol = N'{RolesApp.Administrador}' AND u.IdEmpleado = 1)
             OR (u.Id = 2 AND r.NombreRol = N'{RolesApp.Empleado}' AND u.IdEmpleado = 2)
             OR (u.Id = 3 AND r.NombreRol = N'{RolesApp.UsuarioRegistrado}' AND u.IdCliente = 1)) = 3
    AND ({vacias}) = 0
THEN 1 ELSE 0 END;";

        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }

    private static string LeerScript()
    {
        var ensamblado = typeof(DatosEjemplo).Assembly;
        var nombre = ensamblado.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(NombreRecurso, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No se encontró el recurso incrustado {NombreRecurso}. Revisa el EmbeddedResource en Llanteria.csproj.");

        using var flujo = ensamblado.GetManifestResourceStream(nombre)!;
        using var lector = new StreamReader(flujo); // detecta el BOM UTF-8
        return lector.ReadToEnd();
    }
}
