using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace Llanteria.Seguridad;

/// <summary>Extensiones para declarar en el modelo qué columnas se guardan cifradas.</summary>
public static class CifradoEf
{
    public static PropertyBuilder<string?> Cifrada(this PropertyBuilder<string?> p, string contexto)
    {
        var c = new ValueConverter<string?, string?>(
            (Expression<Func<string?, string?>>)(v => ServicioCifrado.Actual.Cifrar(v, contexto)),
            (Expression<Func<string?, string?>>)(v => ServicioCifrado.Actual.Descifrar(v, contexto)));
        return p.HasConversion(c);
    }

    public static PropertyBuilder<byte[]?> Cifrada(this PropertyBuilder<byte[]?> p, string contexto)
    {
        var c = new ValueConverter<byte[]?, byte[]?>(
            (Expression<Func<byte[]?, byte[]?>>)(v => ServicioCifrado.Actual.Cifrar(v, contexto)),
            (Expression<Func<byte[]?, byte[]?>>)(v => ServicioCifrado.Actual.Descifrar(v, contexto)));
        return p.HasConversion(c);
    }
}
