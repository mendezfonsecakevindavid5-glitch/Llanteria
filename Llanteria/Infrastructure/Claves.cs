using Microsoft.AspNetCore.Identity;

namespace Llanteria.Infrastructure;

/// <summary>
/// Cifrado de contraseñas con PBKDF2 (PasswordHasher de ASP.NET Core). Se usa un solo punto de entrada
/// para que Account, Usuario y Perfil no repitan la lógica.
/// Las contraseñas que seguían en texto plano se aceptan UNA vez y se reemplazan por su hash al iniciar sesión.
/// </summary>
public static class Claves
{
    private static readonly PasswordHasher<object> Hasher = new();

    public static string Hash(string clave) => Hasher.HashPassword(null!, clave);

    // El formato v3 del hasher siempre empieza con el byte 0x01, que en Base64 es "AQAAAA".
    public static bool EsHash(string? valor) => !string.IsNullOrEmpty(valor) && valor.StartsWith("AQAAAA", StringComparison.Ordinal);

    /// <param name="debeActualizar">true si hay que volver a guardar el hash (venía en texto plano o con parámetros viejos).</param>
    public static bool Verificar(string? almacenado, string? ingresada, out bool debeActualizar)
    {
        debeActualizar = false;
        if (string.IsNullOrEmpty(almacenado) || ingresada == null) return false;

        if (EsHash(almacenado))
        {
            var r = Hasher.VerifyHashedPassword(null!, almacenado, ingresada);
            debeActualizar = r == PasswordVerificationResult.SuccessRehashNeeded;
            return r != PasswordVerificationResult.Failed;
        }

        // Contraseña heredada en texto plano.
        var ok = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(almacenado), System.Text.Encoding.UTF8.GetBytes(ingresada));
        debeActualizar = ok;
        return ok;
    }
}
