using System.Net.Mail;

namespace Llanteria.Infrastructure;

/// <summary>Reglas de validación compartidas por los controladores (una sola fuente de verdad).</summary>
public static class Validaciones
{
    public const int LongitudMaximaCorreo = 100;

    /// <summary>Un correo es válido si se puede interpretar y la dirección resultante es exactamente lo escrito.</summary>
    public static bool CorreoValido(string? correo)
    {
        var texto = correo?.Trim();
        return !string.IsNullOrEmpty(texto)
            && texto.Length <= LongitudMaximaCorreo
            && MailAddress.TryCreate(texto, out var direccion)
            && direccion.Address == texto;
    }
}
