using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Data;

// Parte del contexto escrita a mano: así el archivo generado (scaffold) no se toca.
public partial class LlanteriaDbContext
{
    public virtual DbSet<Cita> Citas { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(e =>
        {
            e.ToTable("Citas");
            e.Property(x => x.Slug).HasMaxLength(60);
            e.Property(x => x.Servicio).HasMaxLength(100);
            e.Property(x => x.Fecha).HasColumnType("date");
            e.Property(x => x.Hora).HasMaxLength(10);
            e.Property(x => x.Vehiculo).HasMaxLength(150);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.FormaPago).HasMaxLength(20);
            e.Property(x => x.Notas).HasMaxLength(500);
            e.Property(x => x.Solicitante).HasMaxLength(100);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
            e.Property(x => x.FechaCreacion).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Cliente>().Property(c => c.PasswordHash).HasMaxLength(200);
    }
}
