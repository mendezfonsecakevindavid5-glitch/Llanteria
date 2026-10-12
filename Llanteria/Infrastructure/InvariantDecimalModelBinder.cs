using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Llanteria.Infrastructure;

/// <summary>
/// Los campos numéricos del HTML (type="number") siempre envían el punto como separador decimal.
/// Con la cultura es-CO, el punto sería un separador de miles y "1234.50" se leería como 123450.
/// Este binder interpreta siempre los decimales con la cultura invariante.
/// </summary>
public class InvariantDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var tipo = context.Metadata.ModelType;
        return tipo == typeof(decimal) || tipo == typeof(decimal?) ? new InvariantDecimalModelBinder() : null;
    }
}

public class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valor = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valor == ValueProviderResult.None) return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valor);
        var texto = valor.FirstValue?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            if (bindingContext.ModelType == typeof(decimal?)) bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero))
            bindingContext.Result = ModelBindingResult.Success(numero);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "El valor numérico no es válido.");

        return Task.CompletedTask;
    }
}
