using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Llanteria.Infrastructure;

namespace Llanteria.Seguridad;

/// <summary>
/// Comandos para administrar la bóveda de secretos. Se ejecutan desde la carpeta del proyecto (donde está Llanteria.csproj):
///
///   dotnet run -- boveda init            crea la bóveda (frase + autenticador) y la sella con el appsettings actual
///   dotnet run -- boveda importar        mueve llaves de appsettings.json a la bóveda y las borra del archivo
///   dotnet run -- boveda set Brevo:ApiKey     guarda o cambia un secreto (el valor se escribe oculto)
///   dotnet run -- boveda listar          muestra los NOMBRES guardados (nunca los valores)
///   dotnet run -- boveda mostrar Brevo:ApiKey  muestra un valor (úsalo con cuidado)
///   dotnet run -- boveda borrar Brevo:ApiKey
///   dotnet run -- boveda firmar          vuelve a sellar appsettings.json después de editarlo a propósito
///   dotnet run -- boveda cambiar-frase   cambia la frase
///   dotnet run -- boveda estado          dice si la bóveda existe y si appsettings coincide con su sello
///
/// Todo lo que modifica exige la frase Y el código de 6 dígitos de la app autenticadora.
/// </summary>
public static class BovedaCli
{
    private const int FallosMaximos = 5;
    private static string RutaIntentos => CarpetaPrivada.Archivo("boveda.intentos");

    public static int Ejecutar(string[] args)
    {
        CarpetaPrivada.Resolver(Directory.GetCurrentDirectory());
        var comando = args.Length > 0 ? args[0].ToLowerInvariant() : "ayuda";
        var parametro = args.Length > 1 ? args[1] : null;
        try
        {
            return comando switch
            {
                "init" => Init(),
                "importar" => Importar(),
                "set" => Establecer(parametro),
                "listar" => Listar(),
                "mostrar" => Mostrar(parametro),
                "borrar" => Borrar(parametro),
                "firmar" => Firmar(),
                "cambiar-frase" => CambiarFrase(),
                "estado" => Estado(),
                _ => Ayuda()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            return 1;
        }
    }

    private static int Ayuda()
    {
        Console.WriteLine("Uso: dotnet run -- boveda <init|importar|set <clave>|listar|mostrar <clave>|borrar <clave>|firmar|cambiar-frase|estado>");
        return 0;
    }

    // ------------------------------------------------------------------ entrada de datos
    private static string Pedir(string pregunta, bool oculto)
    {
        Console.Write(pregunta);
        if (!oculto || Console.IsInputRedirected) return Console.ReadLine() ?? "";
        var sb = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (tecla.Key == ConsoleKey.Backspace) { if (sb.Length > 0) sb.Length--; continue; }
            if (!char.IsControl(tecla.KeyChar)) sb.Append(tecla.KeyChar);
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------ verificación de identidad
    private static (int Fallos, long Hasta) LeerBloqueo()
    {
        try
        {
            if (!File.Exists(RutaIntentos)) return (0, 0);
            var p = File.ReadAllText(RutaIntentos).Split(';');
            return (int.Parse(p[0]), long.Parse(p[1]));
        }
        catch { return (0, 0); }
    }

    private static void GuardarBloqueo(int fallos, long hasta) => File.WriteAllText(RutaIntentos, $"{fallos};{hasta}");

    /// <summary>Frase + código del autenticador. Retraso creciente y bloqueo de 15 minutos tras 5 fallos.</summary>
    private static Boveda.Abierta? Autenticar()
    {
        if (!Boveda.Existe) { Console.Error.WriteLine("Todavía no hay bóveda. Crea una con: dotnet run -- boveda init"); return null; }

        var (fallos, hasta) = LeerBloqueo();
        var ahora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (hasta > ahora)
        {
            Console.Error.WriteLine($"Demasiados intentos fallidos. Inténtalo de nuevo en {(hasta - ahora + 59) / 60} minuto(s).");
            return null;
        }

        var abierta = Boveda.AbrirConFrase(Pedir("Frase de la bóveda: ", oculto: true));
        var ok = abierta != null;
        if (ok && abierta!.Secretos.TryGetValue("_totp", out var totp))
            ok = Boveda.VerificarTotp(totp, Pedir("Código de 6 dígitos de tu app autenticadora: ", oculto: false));

        if (!ok)
        {
            fallos++;
            GuardarBloqueo(fallos, fallos >= FallosMaximos ? ahora + 15 * 60 : 0);
            Thread.Sleep(Math.Min(fallos, 5) * 1500);
            Console.Error.WriteLine("Verificación incorrecta."); // no dice cuál de los dos datos falló
            return null;
        }
        if (File.Exists(RutaIntentos)) File.Delete(RutaIntentos);
        return abierta;
    }

    // ------------------------------------------------------------------ comandos
    private static int Init()
    {
        if (Boveda.Existe) { Console.Error.WriteLine("La bóveda ya existe. Para cambiar la frase usa: boveda cambiar-frase"); return 1; }

        var frase = Pedir("Elige una frase de al menos 12 caracteres: ", oculto: true);
        if (frase.Length < 12) { Console.Error.WriteLine("La frase es demasiado corta."); return 1; }
        if (frase != Pedir("Repite la frase: ", oculto: true)) { Console.Error.WriteLine("Las frases no coinciden."); return 1; }

        var totp = Boveda.NuevoSecretoTotp();
        Console.WriteLine();
        Console.WriteLine("Agrega esta cuenta a tu app autenticadora (Microsoft Authenticator, Google Authenticator, Authy...):");
        Console.WriteLine($"  Clave manual: {totp}");
        Console.WriteLine($"  Enlace:       otpauth://totp/Llanteria:desarrollador?secret={totp}&issuer=Llanteria");
        if (!Boveda.VerificarTotp(totp, Pedir("Escribe el código de 6 dígitos que muestra la app: ", oculto: false)))
        { Console.Error.WriteLine("El código no coincide; no se creó la bóveda."); return 1; }

        var a = Boveda.Crear(frase);
        a.Secretos["_totp"] = totp;
        Boveda.Sellar(a);
        Boveda.Guardar(a);
        Console.WriteLine("Bóveda creada. Ahora ejecuta: dotnet run -- boveda importar");
        Console.WriteLine("Guarda la frase y la clave del autenticador en un lugar seguro: sin ellas no se puede modificar la bóveda.");
        return 0;
    }

    private static int Importar()
    {
        var a = Autenticar(); if (a == null) return 1;
        var ruta = CarpetaPrivada.Archivo("appsettings.json");
        if (!File.Exists(ruta)) { Console.Error.WriteLine("No se encontró Privado/appsettings.json."); return 1; }

        var raiz = JsonNode.Parse(File.ReadAllText(ruta),
            documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })?.AsObject()
            ?? throw new InvalidOperationException("appsettings.json no es un JSON válido.");

        var movidos = new List<string>();
        foreach (var clave in Boveda.ClavesSensibles)
        {
            var partes = clave.Split(':');
            JsonNode? nodo = raiz;
            foreach (var parte in partes[..^1]) nodo = (nodo as JsonObject)?[parte];
            if (nodo is not JsonObject padre) continue;
            var valor = padre[partes[^1]]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(valor)) continue;
            a.Secretos[clave] = valor;
            padre[partes[^1]] = "";
            movidos.Add(clave);
        }

        File.WriteAllText(ruta, raiz.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), new UTF8Encoding(false));
        Boveda.Sellar(a);
        Boveda.Guardar(a);

        Console.WriteLine(movidos.Count == 0 ? "No había llaves con valor en appsettings.json." : "Movido a la bóveda: " + string.Join(", ", movidos));
        Console.WriteLine("appsettings.json quedó sin secretos y sellado.");
        Console.WriteLine("Pendiente a mano: Privado/clave.maestra y Privado/usuarios.json pueden tener datos sensibles en texto plano. Si ya no los necesitas, bórralos.");
        return 0;
    }

    private static int Establecer(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave)) { Console.Error.WriteLine("Indica la clave, por ejemplo: boveda set Brevo:ApiKey"); return 1; }
        if (clave.StartsWith('_')) { Console.Error.WriteLine("Los nombres que empiezan por _ están reservados."); return 1; }
        var a = Autenticar(); if (a == null) return 1;
        var valor = Pedir($"Valor de {clave} (no se muestra): ", oculto: true);
        if (valor.Length == 0) { Console.Error.WriteLine("Valor vacío; no se guardó."); return 1; }
        a.Secretos[clave] = valor;
        Boveda.Guardar(a);
        Console.WriteLine("Guardado.");
        return 0;
    }

    private static int Listar()
    {
        var a = Autenticar(); if (a == null) return 1;
        foreach (var k in a.Secretos.Keys.Where(k => !k.StartsWith('_')).OrderBy(k => k)) Console.WriteLine("  " + k);
        return 0;
    }

    private static int Mostrar(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave)) { Console.Error.WriteLine("Indica la clave."); return 1; }
        var a = Autenticar(); if (a == null) return 1;
        if (clave.StartsWith('_') || !a.Secretos.TryGetValue(clave, out var v)) { Console.Error.WriteLine("No existe esa clave."); return 1; }
        Console.WriteLine(v);
        return 0;
    }

    private static int Borrar(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.StartsWith('_')) { Console.Error.WriteLine("Indica una clave válida."); return 1; }
        var a = Autenticar(); if (a == null) return 1;
        Console.WriteLine(a.Secretos.Remove(clave) ? "Borrada." : "No existía.");
        Boveda.Guardar(a);
        return 0;
    }

    private static int Firmar()
    {
        var a = Autenticar(); if (a == null) return 1;
        Boveda.Sellar(a);
        Boveda.Guardar(a);
        Console.WriteLine("appsettings sellado con su contenido actual.");
        return 0;
    }

    private static int CambiarFrase()
    {
        var a = Autenticar(); if (a == null) return 1;
        var nueva = Pedir("Nueva frase (mínimo 12 caracteres): ", oculto: true);
        if (nueva.Length < 12) { Console.Error.WriteLine("La frase es demasiado corta."); return 1; }
        if (nueva != Pedir("Repite la nueva frase: ", oculto: true)) { Console.Error.WriteLine("Las frases no coinciden."); return 1; }
        Boveda.Guardar(a, nueva);
        Console.WriteLine("Frase cambiada.");
        return 0;
    }

    private static int Estado()
    {
        if (!Boveda.Existe) { Console.WriteLine("No hay bóveda (Privado/boveda.json)."); return 0; }
        try
        {
            var secretos = Boveda.CargarParaApp();
            Console.WriteLine($"Bóveda abierta correctamente. Secretos guardados: {secretos?.Count ?? 0}. appsettings coincide con su sello.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }
}
