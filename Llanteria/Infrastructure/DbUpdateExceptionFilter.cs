using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Cuando la base de datos rechaza una operación (por ejemplo borrar una marca que tiene
/// productos) se muestra un aviso en lugar de una pantalla de error 500.
/// </summary>
public class DbUpdateExceptionFilter : IExceptionFilter
{
    private readonly ITempDataDictionaryFactory _tempData;

    public DbUpdateExceptionFilter(ITempDataDictionaryFactory tempData) => _tempData = tempData;

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DbUpdateException) return;

        _tempData.GetTempData(context.HttpContext)["Error"] =
            "La operación no se pudo realizar: el registro está siendo usado por otros datos o la información no es válida.";

        var volver = context.HttpContext.Request.Headers.Referer.ToString();
        var esLocal = Uri.TryCreate(volver, UriKind.Absolute, out var uri)
                      && uri.Host == context.HttpContext.Request.Host.Host;

        context.Result = new RedirectResult(esLocal ? volver : "/");
        context.ExceptionHandled = true;
    }
}
