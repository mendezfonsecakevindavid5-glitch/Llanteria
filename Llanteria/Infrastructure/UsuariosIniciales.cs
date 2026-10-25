using System.Security.Cryptography;
using System.Text.Json;
using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// "Secreto de usuarios": garantiza que SIEMPRE existan un administrador, un empleado y un usuario registrado,
/// aunque se borre la base de datos y se vuelva a crear.
///
/// También reúne los catálogos mínimos que la aplicación necesita para funcionar (roles, estados de inventario y
/// categorías de gasto): antes vivían en la clase DatosIniciales, que se eliminó. Ver <see cref="AsegurarCatalogos"/>.
///
/// Las cuentas se definen en Privado/usuarios.json (carpeta privada: fuera de Git y de wwwroot).
/// Si el archivo no existe, se crea solo la primera vez con contraseñas aleatorias que puedes leer y cambiar en ese archivo.
/// Al iniciar, cada cuenta que falte en la base se crea; las que ya existen no se tocan (no se pisa una contraseña cambiada).
/// </summary>
public static class UsuariosIniciales
{
    public const string NombreArchivo = "usuarios.json";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>Nombres de los estados del stock (tabla EstadoInventario). Antes: DatosIniciales.Estados.</summary>
    public static class Estados
    {
        public const string Disponible = "Disponible";
        public const string Agotado = "Agotado";
        public const string Danado = "Dañado";
    }

    /// <summary>
    /// Catálogos que la aplicación necesita para funcionar y que el modelo no sembraba:
    /// sin categorías de gasto no se podía registrar ningún gasto, y sin estados de inventario no se podía crear stock.
    /// Solo agrega lo que falta; nunca modifica ni borra. Debe ejecutarse ANTES de <see cref="Asegurar"/> (que necesita los roles).
    /// </summary>
    public static void AsegurarCatalogos(LlanteriaDbContext db)
    {
        foreach (var n in new[] { RolesApp.Administrador, RolesApp.Secretario, RolesApp.Empleado, RolesApp.Cliente, RolesApp.UsuarioRegistrado })
            if (!db.Roles.Any(r => r.NombreRol == n)) db.Roles.Add(new Role { NombreRol = n });

        foreach (var n in new[] { Estados.Disponible, Estados.Agotado, Estados.Danado })
            if (!db.EstadoInventarios.Any(e => e.Nombre == n)) db.EstadoInventarios.Add(new EstadoInventario { Nombre = n });

        foreach (var n in new[] { "Arriendo", "Servicios públicos", "Nómina", "Proveedores", "Impuestos", "Mantenimiento", "Otros" })
            if (!db.CategoriaGastos.Any(c => c.Nombre == n)) db.CategoriaGastos.Add(new CategoriaGasto { Nombre = n });

        db.SaveChanges();
        CorregirRolDeClientesAntiguos(db);
    }

    // Antes aquí (en DatosIniciales) se sembraban datos de ejemplo quemados en el código. Se quitaron: ahora esos datos
    // los pone el script de datos de prueba (INSERT INTO) o el administrador desde el panel.

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

    private sealed class Archivo { public List<Cuenta> Usuarios { get; set; } = new(); }

    private sealed class Cuenta
    {
        public string Rol { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Telefono { get; set; } = "";
    }

    public static void Asegurar(LlanteriaDbContext db, ILogger logger)
    {
        var ruta = CarpetaPrivada.Archivo(NombreArchivo);
        if (!File.Exists(ruta))
        {
            File.WriteAllText(ruta, JsonSerializer.Serialize(Plantilla(), Json));
            logger.LogWarning("Se creó {Ruta} con las cuentas iniciales (administrador, empleado y usuario). Ábrelo para ver o cambiar las contraseñas.", ruta);
        }

        var cuentas = JsonSerializer.Deserialize<Archivo>(File.ReadAllText(ruta), Json)?.Usuarios ?? new();

        // Catálogos que las cuentas necesitan (los crea el modelo; aquí solo se leen).
        var idDocumento = db.TipoDocumentos.OrderBy(t => t.Id).Select(t => t.Id).FirstOrDefault();
        var idSexo = db.Sexos.OrderBy(x => x.Id).Select(x => x.Id).FirstOrDefault();
        if (idDocumento == 0 || idSexo == 0)
        {
            logger.LogError("No se crearon los usuarios iniciales: faltan los catálogos de tipo de documento o sexo.");
            return;
        }

        foreach (var c in cuentas)
        {
            if (string.IsNullOrWhiteSpace(c.Username) || string.IsNullOrWhiteSpace(c.Password) || string.IsNullOrWhiteSpace(c.Correo))
            {
                logger.LogWarning("Usuario inicial omitido: Username, Password y Correo son obligatorios ({Rol}).", c.Rol);
                continue;
            }

            var username = c.Username.Trim();
            if (db.Usuarios.Any(u => u.Username == username)) continue;

            // El correo puede repetirse entre cuentas (Usuarios.Correo no tiene restricción única en la base):
            // usuarios.json puede traer, a propósito, el mismo correo en varias cuentas de prueba.
            var correo = c.Correo.Trim();

            var idRol = db.Roles.Where(r => r.NombreRol == c.Rol).Select(r => r.Id).FirstOrDefault();
            if (idRol == 0)
            {
                logger.LogWarning("Usuario inicial {Username} omitido: el rol \"{Rol}\" no existe.", username, c.Rol);
                continue;
            }

            var hash = Claves.Hash(c.Password);
            var usuario = new Usuario
            {
                Username = username,
                Correo = correo,
                PasswordHash = hash,
                IdRol = idRol,
                Estado = "Activo",
                FechaCreacion = DateTime.Now,
                IdTipoDocumento = idDocumento
            };

            if (c.Rol is RolesApp.Administrador or RolesApp.Empleado)
            {
                usuario.IdEmpleadoNavigation = new Empleado
                {
                    Nombres = c.Nombres,
                    Apellidos = c.Apellidos,
                    IdDocumento = idDocumento,
                    NumeroDocumento = c.Documento,
                    Correo = correo,
                    Telefono = c.Telefono,
                    Salario = 0,
                    Horario = "Lunes a viernes",
                    IdSexo = idSexo,
                    FechaNacimiento = new DateOnly(1990, 1, 1),
                    IdRol = idRol
                };
            }
            else
            {
                usuario.IdClienteNavigation = new Cliente
                {
                    Nombres = c.Nombres,
                    Apellidos = c.Apellidos,
                    IdDocumento = idDocumento,
                    NumeroDocumento = c.Documento,
                    Correo = correo,
                    Telefono = c.Telefono,
                    IdSexo = idSexo,
                    PuntosAcumulados = 0,
                    PasswordHash = hash
                };
            }

            try
            {
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                logger.LogInformation("Usuario inicial creado: {Username} ({Rol}).", username, c.Rol);
            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                logger.LogError(ex, "No se pudo crear el usuario inicial {Username}.", username);
            }
        }
    }

    private static Archivo Plantilla() => new()
    {
        Usuarios =
        {
            Nueva(RolesApp.Administrador, "kevin_90_1", "Kevin", "Méndez", "900000001"),
            Nueva(RolesApp.Empleado, "santiago_10_2", "Santiago", "Empleado", "900000002"),
            Nueva(RolesApp.UsuarioRegistrado, "ana_12_3", "Ana", "Usuaria", "900000003")
        }
    };

    private const string CorreoPruebas = "mendezfonsecakevindavid5@gmail.com";

    private static Cuenta Nueva(string rol, string username, string nombres, string apellidos, string documento) => new()
    {
        Rol = rol,
        Username = username,
        Password = ClaveAleatoria(),
        // Las 3 cuentas de prueba comparten el mismo correo a propósito (Usuarios.Correo no es único en la base).
        Correo = CorreoPruebas,
        Nombres = nombres,
        Apellidos = apellidos,
        Documento = documento,
        Telefono = "+57 3000000000"
    };

    // 14 caracteres con mayúscula, minúscula, número y símbolo; sin letras ambiguas (0/O, 1/l/I).
    private static string ClaveAleatoria()
    {
        const string May = "ABCDEFGHJKLMNPQRSTUVWXYZ", Min = "abcdefghijkmnpqrstuvwxyz", Num = "23456789", Sim = "#$%&*+-=?@";
        var todos = May + Min + Num + Sim;
        var c = new List<char> { Tomar(May), Tomar(Min), Tomar(Num), Tomar(Sim) };
        while (c.Count < 14) c.Add(Tomar(todos));
        return new string(c.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());

        static char Tomar(string s) => s[RandomNumberGenerator.GetInt32(s.Length)];
    }
}
