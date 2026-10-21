using Llanteria.Infrastructure;
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
    public virtual DbSet<Pqrs> RegistrosPqrs { get; set; }
    public virtual DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
    public virtual DbSet<Articulo> Articulos { get; set; }
    public virtual DbSet<ServicioWeb> ServiciosWeb { get; set; }
    public virtual DbSet<EditorContenido> EditoresContenido { get; set; }
    public virtual DbSet<Sugerencia> Sugerencias { get; set; }

    /// <summary>¿El usuario fue invitado como editor de este recurso? (El administrador no necesita invitación.)</summary>
    public bool EsEditor(int idUsuario, string recurso)
        => EditoresContenido.AsNoTracking().Any(e => e.IdUsuario == idUsuario && e.Recurso == recurso);

    /// <summary>Servicios publicados en el sitio, en el orden que definió el administrador.</summary>
    public List<ServicioViewModel> ServiciosPublicados()
        => ServiciosWeb.AsNoTracking().Where(s => s.Publicado).OrderBy(s => s.Orden).ThenBy(s => s.Titulo)
            .AsEnumerable().Select(s => s.ComoVista()).ToList();

    /// <summary>Un servicio publicado por su dirección; null si no existe o está oculto.</summary>
    public ServicioViewModel? ServicioPublicado(string slug)
        => ServiciosWeb.AsNoTracking().FirstOrDefault(s => s.Slug == slug && s.Publicado)?.ComoVista();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetalleProducto>().Property(e => e.TipoVehiculo).HasMaxLength(10).HasDefaultValue(TiposVehiculo.Auto);
        modelBuilder.Entity<DetalleProducto>().Property(e => e.Amperaje).HasMaxLength(20);
        modelBuilder.Entity<DetalleProducto>().Property(e => e.Polaridad).HasMaxLength(20);

        // Columnas guardadas cifradas (AES-256-GCM). El texto cifrado es más largo que el original:
        // por eso estas columnas pasan a nvarchar(max) (EsquemaUpdater lo aplica en bases ya creadas).
        modelBuilder.Entity<Cliente>().Property(e => e.Foto).HasColumnType("nvarchar(max)").Cifrada("Cliente.Foto");
        modelBuilder.Entity<Empleado>().Property(e => e.Foto).Cifrada("Empleado.Foto");
        modelBuilder.Entity<Empleado>().Property(e => e.FotoDocumento).Cifrada("Empleado.FotoDocumento");
        modelBuilder.Entity<Empleado>().Property(e => e.FotoRostro).Cifrada("Empleado.FotoRostro");
        modelBuilder.Entity<PerfilUsuario>().Property(e => e.Bio).HasColumnType("nvarchar(max)").Cifrada("PerfilUsuario.Bio");
        modelBuilder.Entity<PerfilUsuario>().Property(e => e.FotoCircular).Cifrada("PerfilUsuario.FotoCircular");
        modelBuilder.Entity<Usuario>().Property(e => e.MotivoSancion).HasColumnType("nvarchar(max)").Cifrada("Usuario.MotivoSancion");

        modelBuilder.Entity<EditorContenido>(e =>
        {
            e.ToTable("EditoresContenido");
            e.Property(x => x.Recurso).HasMaxLength(20);
            e.HasIndex(x => new { x.IdUsuario, x.Recurso }).IsUnique();
            e.HasOne(x => x.IdUsuarioNavigation).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sugerencia>(e =>
        {
            e.ToTable("Sugerencias");
            e.Property(x => x.Area).HasMaxLength(40);
            e.Property(x => x.Mensaje).HasMaxLength(1000);
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue(Sugerencia.Nueva);
        });

        modelBuilder.Entity<ServicioWeb>(e =>
        {
            e.ToTable("ServiciosWeb");
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Ventajas).HasColumnType("nvarchar(max)");
            e.Property(x => x.InfoCita).HasColumnType("nvarchar(max)");
            e.Property(x => x.Publicado).HasDefaultValue(true);
        });

        modelBuilder.Entity<Articulo>(e =>
        {
            e.ToTable("Articulos");
            e.Property(x => x.Titulo).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(120);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Resumen).HasMaxLength(400);
            e.Property(x => x.Categoria).HasMaxLength(40);
            e.Property(x => x.Fecha).HasColumnType("date");
            e.Property(x => x.Autor).HasMaxLength(100);
            e.Property(x => x.Imagen).HasMaxLength(120);
            e.Property(x => x.Contenido).HasColumnType("nvarchar(max)");
            e.Property(x => x.Referencias).HasColumnType("nvarchar(max)");
            e.Property(x => x.Publicado).HasDefaultValue(true);
        });

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

        modelBuilder.Entity<OrdenTrabajo>(e =>
        {
            e.ToTable("OrdenesTrabajo");
            e.Property(x => x.Numero).HasMaxLength(20);
            e.HasIndex(x => x.Numero).IsUnique();
            e.HasIndex(x => x.IdVehiculo);
            e.Property(x => x.Tipo).HasMaxLength(40);
            e.Property(x => x.Descripcion).HasMaxLength(150);
            e.Property(x => x.Observaciones).HasMaxLength(500);
            e.Property(x => x.Total).HasColumnType("decimal(14,2)");
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue(OrdenTrabajo.Recibido);
            e.Property(x => x.FechaIngreso).HasColumnType("datetime");
            e.Property(x => x.FechaEntrega).HasColumnType("datetime");
            e.Property(x => x.ProximaFecha).HasColumnType("date");
            e.HasOne(x => x.Vehiculo).WithMany().HasForeignKey(x => x.IdVehiculo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pqrs>(e =>
        {
            e.ToTable("Pqrs");
            e.Property(x => x.Radicado).HasMaxLength(20);
            e.HasIndex(x => x.Radicado).IsUnique();
            e.Property(x => x.Tipo).HasMaxLength(20);
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.NumeroDocumento).HasMaxLength(20);
            e.Property(x => x.Correo).HasMaxLength(100);
            e.Property(x => x.Telefono).HasMaxLength(25);
            e.Property(x => x.Asunto).HasMaxLength(120);
            e.Property(x => x.Mensaje).HasMaxLength(1500);
            e.Property(x => x.Respuesta).HasMaxLength(1500);
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue(Pqrs.Recibida);
            e.Property(x => x.FechaCreacion).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            e.Property(x => x.FechaRespuesta).HasColumnType("datetime");
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
