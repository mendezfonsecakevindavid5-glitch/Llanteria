using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Cifra, una sola vez, los datos que ya estaban guardados sin cifrar en las columnas protegidas.
/// Es idempotente: solo toca las filas que todavía no empiezan con la marca del cifrado.
/// </summary>
public static class CifradoMigrador
{
    public static void Aplicar(LlanteriaDbContext db, ILogger logger)
    {
        // 1) Las columnas deben ser de longitud máxima: el dato cifrado es más largo que el original.
        foreach (var sql in Ampliaciones)
        {
            try { db.Database.ExecuteSqlRaw(sql); }
            catch (Exception ex) { logger.LogWarning(ex, "No se pudo ampliar una columna cifrada: {Sql}", sql); }
        }

        // 2) Cifra lo que haya en texto plano.
        Texto<Cliente>(db, logger, "Clientes", "Foto");
        Binario<Empleado>(db, logger, "Empleados", "Foto");
        Binario<PerfilUsuario>(db, logger, "PerfilUsuario", "FotoCircular");
        Texto<PerfilUsuario>(db, logger, "PerfilUsuario", "Bio");
        Texto<Usuario>(db, logger, "Usuarios", "MotivoSancion");
        Texto<Cita>(db, logger, "Citas", "Direccion");
        Texto<Cita>(db, logger, "Citas", "Notas");
        Texto<Pedido>(db, logger, "Pedidos", "Direccion");
        Texto<Pedido>(db, logger, "Pedidos", "Complemento");
        Texto<Pedido>(db, logger, "Pedidos", "CodigoPostal");
        Texto<Pedido>(db, logger, "Pedidos", "Notas");
        Texto<SolicitudEmpresa>(db, logger, "SolicitudesEmpresa", "Mensaje");
        Texto<LogActividad>(db, logger, "LogActividad", "DireccionIp");
    }

    private static string Ampliar(string tabla, string columna, string tipo, bool nulo)
        => $"IF COL_LENGTH('dbo.{tabla}', '{columna}') IS NOT NULL AND COL_LENGTH('dbo.{tabla}', '{columna}') <> -1 " +
           $"ALTER TABLE dbo.{tabla} ALTER COLUMN {columna} {tipo} {(nulo ? "NULL" : "NOT NULL")};";

    private static readonly string[] Ampliaciones =
    {
        Ampliar("Clientes", "Foto", "nvarchar(max)", true),
        Ampliar("Empleados", "Foto", "varbinary(max)", true),
        Ampliar("PerfilUsuario", "FotoCircular", "varbinary(max)", true),
        Ampliar("PerfilUsuario", "Bio", "nvarchar(max)", true),
        Ampliar("Usuarios", "MotivoSancion", "nvarchar(max)", true),
        Ampliar("Citas", "Direccion", "nvarchar(max)", true),
        Ampliar("Citas", "Notas", "nvarchar(max)", true),
        Ampliar("Pedidos", "Direccion", "nvarchar(max)", false),
        Ampliar("Pedidos", "Complemento", "nvarchar(max)", true),
        Ampliar("Pedidos", "CodigoPostal", "nvarchar(max)", false),
        Ampliar("Pedidos", "Notas", "nvarchar(max)", true),
        Ampliar("SolicitudesEmpresa", "Mensaje", "nvarchar(max)", true),
        Ampliar("LogActividad", "DireccionIp", "nvarchar(max)", true),
    };

    private static void Texto<T>(LlanteriaDbContext db, ILogger logger, string tabla, string columna) where T : class
        => Cifrar<T>(db, logger, tabla, columna,
            $"SELECT Id AS Value FROM dbo.{tabla} WHERE {columna} IS NOT NULL AND {columna} NOT LIKE 'e1:%'");

    private static void Binario<T>(LlanteriaDbContext db, ILogger logger, string tabla, string columna) where T : class
        => Cifrar<T>(db, logger, tabla, columna,
            $"SELECT Id AS Value FROM dbo.{tabla} WHERE {columna} IS NOT NULL AND SUBSTRING({columna}, 1, 4) <> 0x4C4C4531");

    private static void Cifrar<T>(LlanteriaDbContext db, ILogger logger, string tabla, string columna, string sqlIds) where T : class
    {
        try
        {
            var ids = db.Database.SqlQueryRaw<int>(sqlIds).ToList();
            if (ids.Count == 0) return;

            foreach (var lote in ids.Chunk(100))
            {
                var filas = db.Set<T>().Where(e => lote.Contains(EF.Property<int>(e, "Id"))).ToList();
                foreach (var fila in filas) db.Entry(fila).Property(columna).IsModified = true; // fuerza a reescribir el valor, ya cifrado
                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
            logger.LogInformation("Cifrado {Tabla}.{Columna}: {N} filas.", tabla, columna, ids.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo cifrar {Tabla}.{Columna}. Los datos siguen legibles y se reintentará al próximo inicio.", tabla, columna);
            db.ChangeTracker.Clear();
        }
    }
}
