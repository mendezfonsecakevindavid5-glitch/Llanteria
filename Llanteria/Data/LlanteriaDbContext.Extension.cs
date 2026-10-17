using Llanteria.Models;
using Llanteria.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Data;

// Parte del contexto escrita a mano: así el archivo generado (scaffold) no se toca.
public partial class LlanteriaDbContext
{
    public virtual DbSet<Cita> Citas { get; set; }
    public virtual DbSet<SolicitudEmpresa> SolicitudesEmpresa { get; set; }
    public virtual DbSet<Resenia> Resenias { get; set; }
    public virtual DbSet<Pedido> Pedidos { get; set; }
    public virtual DbSet<PedidoItem> PedidoItems { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Columnas guardadas cifradas (AES-256-GCM). El texto cifrado es más largo que el original:
        // por eso estas columnas pasan a nvarchar(max) (EsquemaUpdater lo aplica en bases ya creadas).
        modelBuilder.Entity<Cliente>().Property(e => e.Foto).HasColumnType("nvarchar(max)").Cifrada("Cliente.Foto");
        modelBuilder.Entity<Empleado>().Property(e => e.Foto).Cifrada("Empleado.Foto");
        modelBuilder.Entity<PerfilUsuario>().Property(e => e.Bio).HasColumnType("nvarchar(max)").Cifrada("PerfilUsuario.Bio");
        modelBuilder.Entity<PerfilUsuario>().Property(e => e.FotoCircular).Cifrada("PerfilUsuario.FotoCircular");
        modelBuilder.Entity<Usuario>().Property(e => e.MotivoSancion).HasColumnType("nvarchar(max)").Cifrada("Usuario.MotivoSancion");

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

        modelBuilder.Entity<SolicitudEmpresa>(e =>
        {
            e.ToTable("SolicitudesEmpresa");
            e.Property(x => x.Empresa).HasMaxLength(120);
            e.Property(x => x.Nit).HasMaxLength(20);
            e.Property(x => x.Contacto).HasMaxLength(100);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Ciudad).HasMaxLength(60);
            e.Property(x => x.TipoFlota).HasMaxLength(40);
            e.Property(x => x.Mensaje).HasMaxLength(500);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
            e.Property(x => x.FechaCreacion).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Resenia>(e =>
        {
            e.ToTable("Resenias");
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.Comentario).HasMaxLength(400);
            e.Property(x => x.Fecha).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            e.HasIndex(x => x.IdCliente).IsUnique();
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.ToTable("Pedidos");
            e.Property(x => x.Numero).HasMaxLength(20);
            e.Property(x => x.Token).HasMaxLength(32);
            e.Property(x => x.Nombres).HasMaxLength(60);
            e.Property(x => x.Apellidos).HasMaxLength(60);
            e.Property(x => x.TipoDocumento).HasMaxLength(10);
            e.Property(x => x.NumeroDocumento).HasMaxLength(20);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Telefono).HasMaxLength(25);
            e.Property(x => x.Pais).HasMaxLength(40);
            e.Property(x => x.Departamento).HasMaxLength(60);
            e.Property(x => x.Ciudad).HasMaxLength(60);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.Complemento).HasMaxLength(100);
            e.Property(x => x.CodigoPostal).HasMaxLength(12);
            e.Property(x => x.Notas).HasMaxLength(400);
            e.Property(x => x.MetodoPago).HasMaxLength(20);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
            e.Property(x => x.CodigoCupon).HasMaxLength(20);
            e.Property(x => x.Subtotal).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Descuento).HasColumnType("decimal(18, 2)");
            e.Property(x => x.BaseGravable).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Iva).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Envio).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Total).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Fecha).HasColumnType("datetime");
            e.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<PedidoItem>(e =>
        {
            e.ToTable("PedidoItems");
            e.Property(x => x.Descripcion).HasMaxLength(260);
            e.Property(x => x.ValorUnitario).HasColumnType("decimal(18, 2)");
            e.Property(x => x.Subtotal).HasColumnType("decimal(18, 2)");
            e.HasOne(x => x.Pedido).WithMany(p => p.Items).HasForeignKey(x => x.IdPedido).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cliente>().Property(c => c.PasswordHash).HasMaxLength(200);
    }
}
