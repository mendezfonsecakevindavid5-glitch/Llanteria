using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Llanteria.Infrastructure;

/// <summary>
/// Aplica a TODOS los formularios del sitio la misma protección que ya tienen Login y Registro:
/// sin autocompletado del navegador ni de gestores de contraseñas, y sin copiar, cortar, pegar ni arrastrar texto en los campos.
/// Se ejecuta en el servidor (los atributos ya vienen en el HTML) y se complementa con wwwroot/js/form-guard.js
/// para los campos que se crean después con JavaScript.
/// Para excluir un campo concreto (p. ej. un buscador donde sí se quiera pegar): data-guard="off".
/// </summary>
[HtmlTargetElement("form")]
[HtmlTargetElement("input")]
[HtmlTargetElement("textarea")]
[HtmlTargetElement("select")]
public class FormGuardTagHelper : TagHelper
{
    // Corre al final, después de los tag helpers de asp-for, asp-action, etc.
    public override int Order => int.MaxValue;

    private static readonly HashSet<string> SinProteccion = new(StringComparer.OrdinalIgnoreCase)
        { "hidden", "checkbox", "radio", "file", "submit", "button", "reset", "image", "range", "color" };

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (output.Attributes.TryGetAttribute("data-guard", out var g) && string.Equals(g.Value?.ToString(), "off", StringComparison.OrdinalIgnoreCase))
            return;

        var tag = output.TagName.ToLowerInvariant();
        if (tag == "form")
        {
            output.Attributes.SetAttribute("autocomplete", "off");
            return;
        }

        var tipo = output.Attributes.TryGetAttribute("type", out var t) ? t.Value?.ToString() ?? "text" : "text";
        if (tag == "input" && SinProteccion.Contains(tipo)) return;

        // Las contraseñas usan "new-password": es lo único que los navegadores respetan para no sugerir claves guardadas.
        output.Attributes.SetAttribute("autocomplete", tipo.Equals("password", StringComparison.OrdinalIgnoreCase) ? "new-password" : "off");
        output.Attributes.SetAttribute("data-lpignore", "true");   // LastPass
        output.Attributes.SetAttribute("data-1p-ignore", "true");  // 1Password
        output.Attributes.SetAttribute("data-form-type", "other"); // Dashlane
        output.Attributes.SetAttribute("spellcheck", "false");

        if (tag == "select") return;
        foreach (var ev in new[] { "onpaste", "oncopy", "oncut", "ondrop" })
            if (!output.Attributes.ContainsName(ev)) output.Attributes.SetAttribute(ev, "return false;");
    }
}
