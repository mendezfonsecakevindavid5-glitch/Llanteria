using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Ya NO crea una llantería de ejemplo con datos quemados de appsettings: eso lo hace ahora el script de datos de
/// prueba (INSERT INTO) o el propio administrador desde el panel. Esta clase solo reasigna, a la primera llantería
/// que YA exista en la base, los registros que se hayan quedado sin llantería (IdLlanteria NULL); si todavía no hay
/// ninguna llantería creada, no hace nada.
/// </summary>
public static class LlanteriaInicial
{
    public static void Asegurar(LlanteriaDbContext db, NegocioInfo n)
    {
        var primera = db.Llanterias.IgnoreQueryFilters().OrderBy(l => l.Id).FirstOrDefault();
        if (primera == null) return; // nada que asignar: todavía no existe ninguna llantería

        // Los registros existentes (productos, bodegas, facturas...) pasan a ser de la primera llantería.
        foreach (var et in db.Model.GetEntityTypes().Where(t => typeof(IDeLlanteria).IsAssignableFrom(t.ClrType)))
        {
            var tabla = et.GetTableName();
            if (tabla == null) continue;
            db.Database.ExecuteSqlRaw($"UPDATE [{et.GetSchema() ?? "dbo"}].[{tabla}] SET [IdLlanteria] = {primera.Id} WHERE [IdLlanteria] IS NULL;");
        }
    }
}
