using System.Diagnostics;

namespace Llanteria.Infrastructure;

/// <summary>
/// Carpeta privada de la aplicación. Aquí viven los archivos que NUNCA deben salir del servidor:
/// appsettings.json (cadena de conexión, llaves de correo), la clave maestra de cifrado y las llaves
/// que protegen las cookies de sesión.
///
/// Ubicación (en orden de prioridad):
///   1. Variable de entorno LLANTERIA_CARPETA_PRIVADA (recomendado en producción: una ruta FUERA del proyecto,
///      p. ej. C:\ProgramData\Llanteria\Privado o /etc/llanteria).
///   2. La carpeta "Privado" junto al proyecto.
///
/// Esta carpeta no está dentro de wwwroot (nunca se sirve por HTTP), no se copia a la publicación,
/// está excluida de Git y, al iniciar, se restringe el acceso solo al usuario que ejecuta la aplicación.
/// </summary>
public static class CarpetaPrivada
{
    public const string VariableEntorno = "LLANTERIA_CARPETA_PRIVADA";

    public static string Ruta { get; private set; } = "";

    public static string Resolver(string contentRoot)
    {
        var ruta = Environment.GetEnvironmentVariable(VariableEntorno);
        if (string.IsNullOrWhiteSpace(ruta)) ruta = Path.Combine(contentRoot, "Privado");
        ruta = Path.GetFullPath(ruta);

        Directory.CreateDirectory(ruta);
        Restringir(ruta);
        Ruta = ruta;
        return ruta;
    }

    public static string Archivo(string nombre) => Path.Combine(Ruta, nombre);

    private static void Restringir(string ruta)
    {
        try
        {
            if (OperatingSystem.IsWindows()) RestringirWindows(ruta);
            else File.SetUnixFileMode(ruta, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        catch
        {
            // Si el sistema no permite cambiar permisos (p. ej. unidad de red) no se impide el arranque:
            // los permisos de la carpeta se deben fijar manualmente en el despliegue.
        }
    }

    private static void RestringirWindows(string ruta)
    {
        // Quita la herencia y deja control total solo a: el usuario que ejecuta la app, SYSTEM y Administradores (por SID, sin depender del idioma).
        var usuario = $"{Environment.UserDomainName}\\{Environment.UserName}";
        var args = $"\"{ruta}\" /inheritance:r /grant:r \"{usuario}:(OI)(CI)F\" *S-1-5-18:(OI)(CI)F *S-1-5-32-544:(OI)(CI)F";
        using var p = Process.Start(new ProcessStartInfo("icacls", args) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true });
        p?.WaitForExit(5000);
    }
}
