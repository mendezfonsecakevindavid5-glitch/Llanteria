using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Llanteria.Infrastructure;
using Microsoft.AspNetCore.DataProtection;

namespace Llanteria.Seguridad;

/// <summary>
/// Bóveda de secretos (Privado/boveda.json). Guarda cifrados con AES-256-GCM los valores que NO deben estar en texto plano:
/// la llave de API de Brevo, la llave SMTP, la clave maestra de cifrado de datos y el secreto de Power BI.
///
/// Cómo se protege:
///  - Los secretos van cifrados con una "llave de datos" aleatoria (AES-256-GCM: si alguien cambia un solo byte, no abre).
///  - Esa llave de datos se guarda dos veces: (1) cifrada con tu FRASE (PBKDF2-SHA256, 600.000 iteraciones), que solo se
///    pide para MODIFICAR la bóveda, y (2) protegida por el sistema (Windows DPAPI del usuario que ejecuta la app), para que
///    la aplicación arranque sola. El archivo copiado a otro equipo u otro usuario de Windows no se puede abrir.
///  - Para modificar la bóveda (comandos "boveda ..."), además de la frase se pide un código de 6 dígitos de una app
///    autenticadora (TOTP). Hay retraso creciente y bloqueo temporal tras fallos seguidos.
///  - La bóveda también guarda el "sello" (SHA-256) de appsettings.json: si el archivo se edita a mano, la aplicación NO arranca
///    hasta que se vuelva a firmar con "boveda firmar" (que exige frase + código).
///
/// En un servidor Linux, o en producción en general, define LLANTERIA_BOVEDA_FRASE en el entorno del servicio (o usa el almacén de secretos del hosting).
/// </summary>
public static class Boveda
{
    public const string VariableFrase = "LLANTERIA_BOVEDA_FRASE";
    private const string Aad = "llanteria-boveda-v1";
    private const int Iteraciones = 600_000;
    private const string PrefijoSello = "_sello:";
    private static readonly string[] ArchivosSellables = { "appsettings.json", "appsettings.Development.json", "appsettings.Production.json" };

    /// <summary>Valores de appsettings.json que "importar" mueve a la bóveda.</summary>
    public static readonly string[] ClavesSensibles = { "Brevo:ApiKey", "Brevo:SmtpKey", "Cifrado:Clave", "PowerBI:ClientSecret" };

    private sealed class Sobre
    {
        public int V { get; set; } = 1;
        public int Iteraciones { get; set; }
        public string Sal { get; set; } = "";
        public string ClavePorFrase { get; set; } = "";
        public string? ClavePorSistema { get; set; }
        public string Datos { get; set; } = "";
    }

    public sealed class Abierta
    {
        internal byte[] Clave = Array.Empty<byte>();
        internal byte[] Sal = Array.Empty<byte>();
        internal int Iter;
        internal string? ClavePorFrase;
        public Dictionary<string, string> Secretos { get; internal set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public static string Ruta => CarpetaPrivada.Archivo("boveda.json");
    public static bool Existe => File.Exists(Ruta);

    // ---------------------------------------------------------------- criptografía
    private static byte[] Cifrar(byte[] clave, byte[] datos)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ct = new byte[datos.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(clave, 16);
        aes.Encrypt(nonce, datos, ct, tag, Encoding.UTF8.GetBytes(Aad));
        return nonce.Concat(tag).Concat(ct).ToArray();
    }

    private static byte[] Descifrar(byte[] clave, byte[] paquete)
    {
        if (paquete.Length < 28) throw new CryptographicException("La bóveda está dañada.");
        var nonce = paquete[..12];
        var tag = paquete[12..28];
        var ct = paquete[28..];
        var plano = new byte[ct.Length];
        using var aes = new AesGcm(clave, 16);
        aes.Decrypt(nonce, ct, tag, plano, Encoding.UTF8.GetBytes(Aad));
        return plano;
    }

    private static byte[] DerivarFrase(string frase, byte[] sal, int iter)
        => Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(frase), sal, iter, HashAlgorithmName.SHA256, 32);

    private static IDataProtector Protector()
    {
        var proveedor = DataProtectionProvider.Create(
            new DirectoryInfo(Path.Combine(CarpetaPrivada.Ruta, "boveda-llaves")),
            b =>
            {
                b.SetApplicationName("LlanteriaBoveda");
                if (OperatingSystem.IsWindows()) b.ProtectKeysWithDpapi();
            });
        return proveedor.CreateProtector("Llanteria.Boveda.v1");
    }

    public static string Hash(byte[] contenido) => Convert.ToHexString(SHA256.HashData(contenido));

    // ---------------------------------------------------------------- lectura y escritura
    private static Sobre Leer() =>
        JsonSerializer.Deserialize<Sobre>(File.ReadAllText(Ruta, Encoding.UTF8)) ?? throw new InvalidOperationException("La bóveda está vacía o dañada.");

    private static Abierta Completar(Sobre s, byte[] clave)
    {
        var json = Descifrar(clave, Convert.FromBase64String(s.Datos));
        var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        return new Abierta
        {
            Clave = clave, Sal = Convert.FromBase64String(s.Sal), Iter = s.Iteraciones, ClavePorFrase = s.ClavePorFrase,
            Secretos = new Dictionary<string, string>(dic, StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>Abre con la frase; null si la frase es incorrecta.</summary>
    public static Abierta? AbrirConFrase(string frase)
    {
        var s = Leer();
        try
        {
            var kek = DerivarFrase(frase, Convert.FromBase64String(s.Sal), s.Iteraciones);
            return Completar(s, Descifrar(kek, Convert.FromBase64String(s.ClavePorFrase)));
        }
        catch (CryptographicException) { return null; }
    }

    /// <summary>Abre con la protección del sistema (DPAPI); null si no se puede (otro usuario, otro equipo).</summary>
    public static Abierta? AbrirConSistema()
    {
        var s = Leer();
        if (string.IsNullOrEmpty(s.ClavePorSistema)) return null;
        try { return Completar(s, Protector().Unprotect(Convert.FromBase64String(s.ClavePorSistema))); }
        catch (CryptographicException) { return null; }
    }

    /// <summary>Crea la bóveda desde cero.</summary>
    public static Abierta Crear(string frase)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        var a = new Abierta { Clave = RandomNumberGenerator.GetBytes(32), Sal = sal, Iter = Iteraciones };
        a.ClavePorFrase = Convert.ToBase64String(Cifrar(DerivarFrase(frase, sal, Iteraciones), a.Clave));
        return a;
    }

    /// <summary>Guarda. Si se pasa nuevaFrase, se vuelve a envolver la llave de datos con ella.</summary>
    public static void Guardar(Abierta a, string? nuevaFrase = null)
    {
        if (nuevaFrase != null)
        {
            a.Sal = RandomNumberGenerator.GetBytes(16);
            a.Iter = Iteraciones;
            a.ClavePorFrase = Convert.ToBase64String(Cifrar(DerivarFrase(nuevaFrase, a.Sal, a.Iter), a.Clave));
        }
        var sobre = new Sobre
        {
            Iteraciones = a.Iter,
            Sal = Convert.ToBase64String(a.Sal),
            ClavePorFrase = a.ClavePorFrase ?? "",
            ClavePorSistema = Convert.ToBase64String(Protector().Protect(a.Clave)),
            Datos = Convert.ToBase64String(Cifrar(a.Clave, JsonSerializer.SerializeToUtf8Bytes(a.Secretos)))
        };
        var tmp = Ruta + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(sobre, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
        File.Move(tmp, Ruta, overwrite: true);
    }

    // ---------------------------------------------------------------- sello de appsettings
    /// <summary>Calcula y guarda el sello de los appsettings que existan (en memoria; luego hay que llamar Guardar).</summary>
    public static void Sellar(Abierta a)
    {
        foreach (var nombre in ArchivosSellables)
        {
            var ruta = CarpetaPrivada.Archivo(nombre);
            if (File.Exists(ruta)) a.Secretos[PrefijoSello + nombre] = Hash(File.ReadAllBytes(ruta));
            else a.Secretos.Remove(PrefijoSello + nombre);
        }
    }

    private static void VerificarSellos(Dictionary<string, string> secretos)
    {
        foreach (var kv in secretos.Where(k => k.Key.StartsWith(PrefijoSello, StringComparison.OrdinalIgnoreCase)))
        {
            var nombre = kv.Key[PrefijoSello.Length..];
            var ruta = CarpetaPrivada.Archivo(nombre);
            var actual = File.Exists(ruta) ? Hash(File.ReadAllBytes(ruta)) : "";
            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(actual), Encoding.ASCII.GetBytes(kv.Value)))
                throw new InvalidOperationException(
                    $"{nombre} fue modificado (o borrado) fuera del procedimiento autorizado, por eso la aplicación no arranca. " +
                    "Si el cambio fue tuyo, firma de nuevo: dotnet run --project Llanteria -- boveda firmar (pide tu frase y el código de verificación).");
        }
    }

    // ---------------------------------------------------------------- carga para la aplicación
    /// <summary>
    /// Devuelve los secretos para mezclarlos con la configuración (null si todavía no hay bóveda: el proyecto sigue funcionando como antes).
    /// Verifica los sellos de appsettings.
    /// </summary>
    public static Dictionary<string, string?>? CargarParaApp()
    {
        if (!Existe) return null;
        Abierta? a = null;
        var frase = Environment.GetEnvironmentVariable(VariableFrase);
        if (!string.IsNullOrEmpty(frase)) a = AbrirConFrase(frase);
        a ??= AbrirConSistema();
        if (a == null)
            throw new InvalidOperationException(
                "No se pudo abrir Privado/boveda.json. La bóveda solo la abre el mismo usuario de Windows que la creó, " +
                $"o quien defina la variable de entorno {VariableFrase} con la frase correcta.");
        VerificarSellos(a.Secretos);
        return a.Secretos.Where(kv => !kv.Key.StartsWith('_')).ToDictionary(kv => kv.Key, kv => (string?)kv.Value);
    }

    // ---------------------------------------------------------------- TOTP (RFC 6238) para la verificación de identidad
    private const string Alfabeto32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string NuevoSecretoTotp() => ABase32(RandomNumberGenerator.GetBytes(20));

    private static string ABase32(byte[] datos)
    {
        var sb = new StringBuilder();
        int buffer = 0, bits = 0;
        foreach (var b in datos)
        {
            buffer = (buffer << 8) | b; bits += 8;
            while (bits >= 5) { sb.Append(Alfabeto32[(buffer >> (bits - 5)) & 31]); bits -= 5; }
        }
        if (bits > 0) sb.Append(Alfabeto32[(buffer << (5 - bits)) & 31]);
        return sb.ToString();
    }

    private static byte[] DeBase32(string texto)
    {
        var salida = new List<byte>();
        int buffer = 0, bits = 0;
        foreach (var ch in texto.Trim().Replace(" ", "").ToUpperInvariant())
        {
            var v = Alfabeto32.IndexOf(ch);
            if (v < 0) continue;
            buffer = (buffer << 5) | v; bits += 5;
            if (bits >= 8) { salida.Add((byte)((buffer >> (bits - 8)) & 0xFF)); bits -= 8; }
        }
        return salida.ToArray();
    }

    private static string Totp(byte[] secreto, long paso)
    {
        var msg = BitConverter.GetBytes(paso);
        if (BitConverter.IsLittleEndian) Array.Reverse(msg);
        using var h = new HMACSHA1(secreto);
        var hash = h.ComputeHash(msg);
        var off = hash[^1] & 0x0F;
        var bin = ((hash[off] & 0x7F) << 24) | (hash[off + 1] << 16) | (hash[off + 2] << 8) | hash[off + 3];
        return (bin % 1_000_000).ToString("D6");
    }

    public static bool VerificarTotp(string secretoBase32, string? codigo)
    {
        var limpio = (codigo ?? "").Trim().Replace(" ", "");
        if (limpio.Length != 6) return false;
        var secreto = DeBase32(secretoBase32);
        var paso = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        for (var d = -1; d <= 1; d++)
            if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Totp(secreto, paso + d)), Encoding.ASCII.GetBytes(limpio))) return true;
        return false;
    }
}
