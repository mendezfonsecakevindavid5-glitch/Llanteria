using System.Linq.Expressions;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Data;

// Estados de registro, borrado lógico y multi-llantería. Parte escrita a mano del contexto.
public partial class LlanteriaDbContext
{
    public virtual DbSet<Estado> Estados { get; set; }
    public virtual DbSet<EstadoFinanciero> EstadosFinancieros { get; set; }
    public virtual DbSet<EmpresaLlanteria> Llanterias { get; set; }
    public virtual DbSet<Evento> Eventos { get; set; }

    /// <summary>Nombre de la columna de estado: en Inventario "IdEstado" ya significa otra cosa (estado del stock).</summary>
    public static string ColumnaEstado(Type tipo) => tipo == typeof(Inventario) ? "IdEstadoRegistro" : "IdEstado";

    private static void ConfigurarEstadosYLlanterias(ModelBuilder modelBuilder)
    {
        // Semillas: van en el modelo para que una base nueva ya las tenga cuando se insertan los países, etc.
        // (en una base existente las inserta EsquemaUpdater).
        modelBuilder.Entity<Estado>(e =>
        {
            e.ToTable("Estados");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasData(EstadoIds.Semilla.Select(s => new Estado { Id = s.Id, Nombre = s.Nombre }));
        });
        modelBuilder.Entity<EstadoFinanciero>(e =>
        {
            e.ToTable("EstadosFinancieros");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasData(EstadoFinancieroIds.Semilla.Select(s => new EstadoFinanciero { Id = s.Id, Nombre = s.Nombre }));
        });

        modelBuilder.Entity<EmpresaLlanteria>(e =>
        {
            e.ToTable("Llanterias");
            e.Property(x => x.FechaCreacion).HasColumnType("datetime");
        });

        modelBuilder.Entity<Evento>(e =>
        {
            e.ToTable("Eventos");
            e.Property(x => x.FechaInicio).HasColumnType("date");
            e.Property(x => x.FechaFin).HasColumnType("date");
            e.HasMany(x => x.Ofertas).WithOne(o => o.EventoNavigation).HasForeignKey(o => o.IdEvento).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.OfertaNavigation).WithMany().HasForeignKey(p => p.IdOferta).OnDelete(DeleteBehavior.NoAction);

        foreach (var tipo in modelBuilder.Model.GetEntityTypes().Select(t => t.ClrType).Where(t => !t.IsAbstract).ToList())
        {
            var estandar = typeof(IConEstado).IsAssignableFrom(tipo);
            var financiero = typeof(IConEstadoFinanciero).IsAssignableFrom(tipo);
            var deLlanteria = typeof(IDeLlanteria).IsAssignableFrom(tipo);
            if (!estandar && !financiero && !deLlanteria) continue;

            var tb = modelBuilder.Entity(tipo);

            if (estandar || financiero)
            {
                var columna = ColumnaEstado(tipo);
                var p = Expression.Parameter(tipo, "e");
                var valor = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(int) }, p, Expression.Constant(columna));
                var oculto = estandar ? EstadoIds.Deshabilitado : EstadoFinancieroIds.Anulado;

                // Lo deshabilitado (o anulado) no aparece en ninguna consulta: "queda oculto".
                tb.HasQueryFilter(Expression.Lambda(Expression.NotEqual(valor, Expression.Constant(oculto)), p));
                tb.Property(columna).HasDefaultValue(estandar ? EstadoIds.Habilitado : EstadoFinancieroIds.Pagado);

                if (estandar) tb.HasOne(typeof(Estado)).WithMany().HasForeignKey(columna).OnDelete(DeleteBehavior.NoAction);
                else tb.HasOne(typeof(EstadoFinanciero)).WithMany().HasForeignKey(columna).OnDelete(DeleteBehavior.NoAction);
            }

            if (deLlanteria)
                tb.HasOne(typeof(EmpresaLlanteria)).WithMany().HasForeignKey("IdLlanteria").OnDelete(DeleteBehavior.NoAction);
        }
    }

    /// <summary>
    /// Ningún registro se elimina de la base de datos: lo que se manda a eliminar pasa a "Deshabilitado"
    /// (o "Anulado" en los financieros) y queda oculto por el filtro global.
    /// </summary>
    private void DeshabilitarEnLugarDeEliminar()
    {
        foreach (var entrada in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).ToList())
        {
            var tipo = entrada.Metadata.ClrType;
            var estandar = typeof(IConEstado).IsAssignableFrom(tipo);
            var financiero = typeof(IConEstadoFinanciero).IsAssignableFrom(tipo);
            if (!estandar && !financiero) continue;

            entrada.State = EntityState.Unchanged;
            var propiedad = entrada.Property(ColumnaEstado(tipo));
            propiedad.CurrentValue = estandar ? EstadoIds.Deshabilitado : EstadoFinancieroIds.Anulado;
            propiedad.IsModified = true;
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        DeshabilitarEnLugarDeEliminar();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        DeshabilitarEnLugarDeEliminar();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
