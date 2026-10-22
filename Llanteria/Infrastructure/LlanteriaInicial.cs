using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Crea la primera llantería a partir de los datos del negocio (appsettings, sección "Negocio") si la tabla está vacía
/// y le asigna los registros que todavía no pertenecen a ninguna. Así el sistema sigue funcionando igual con una sola llantería.
/// </summary>
public static class LlanteriaInicial
{
    public static void Asegurar(LlanteriaDbContext db, NegocioInfo n)
    {
        var primera = db.Llanterias.IgnoreQueryFilters().OrderBy(l => l.Id).FirstOrDefault();
        if (primera == null)
        {
            primera = new EmpresaLlanteria
            {
                Nombre = n.Nombre,
                Nit = string.IsNullOrWhiteSpace(n.Nit) ? null : n.Nit,
                Direccion = n.Direccion,
                Ciudad = n.Ciudad,
                Telefono = n.Telefono,
                WhatsApp = n.WhatsApp,
                Correo = n.Correo,
                Instagram = n.Instagram,
                Facebook = n.Facebook,
                Latitud = n.Latitud,
                Longitud = n.Longitud,
                HorarioSemana = $"{n.SemanaAbre} - {n.SemanaCierra}",
                HorarioSabado = $"{n.SabadoAbre} - {n.SabadoCierra}",
                ServiciosOfrecidos = "Alineación y balanceo\nCambio de aceite\nFrenos\nMecánica general"
            };
            db.Llanterias.Add(primera);
            db.SaveChanges();
        }

        // Los registros existentes (productos, bodegas, facturas...) pasan a ser de la primera llantería.
        foreach (var et in db.Model.GetEntityTypes().Where(t => typeof(IDeLlanteria).IsAssignableFrom(t.ClrType)))
        {
            var tabla = et.GetTableName();
            if (tabla == null) continue;
            db.Database.ExecuteSqlRaw($"UPDATE [{et.GetSchema() ?? "dbo"}].[{tabla}] SET [IdLlanteria] = {primera.Id} WHERE [IdLlanteria] IS NULL;");
        }
    }
}
