using System;
using System.Collections.Generic;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Data;

public partial class LlanteriaDbContext : DbContext
{
    public LlanteriaDbContext()
    {
    }

    public LlanteriaDbContext(DbContextOptions<LlanteriaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bodega> Bodegas { get; set; }

    public virtual DbSet<CanjeIncentivo> CanjeIncentivos { get; set; }

    public virtual DbSet<CatalogoIncentivo> CatalogoIncentivos { get; set; }

    public virtual DbSet<CategoriaGasto> CategoriaGastos { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleFactura> DetalleFacturas { get; set; }

    public virtual DbSet<DetalleProducto> DetalleProductos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadoInventario> EstadoInventarios { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<LogActividad> LogActividads { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Oferta> Ofertas { get; set; }

    public virtual DbSet<PerfilUsuario> PerfilUsuarios { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<RecuperacionCuenta> RecuperacionCuentas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sexo> Sexos { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumentos { get; set; }

    public virtual DbSet<TipoServicio> TipoServicios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    // ❌ BORRADO: Se eliminó el DbSet<Conductore> de aquí.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LlanteriaDB;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bodegas__3214EC07D99D647E");

            entity.Property(e => e.NombreBodega).HasMaxLength(50);
            entity.Property(e => e.Ubicacion).HasMaxLength(100);
        });

        modelBuilder.Entity<CanjeIncentivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CanjeInc__3214EC0748C92D09");

            entity.Property(e => e.FechaCanje)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.EntregadoPorNavigation).WithMany(p => p.CanjeIncentivos)
                .HasForeignKey(d => d.EntregadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Canje_Usuario");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.CanjeIncentivos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Canje_Cliente");

            entity.HasOne(d => d.IdIncentivoNavigation).WithMany(p => p.CanjeIncentivos)
                .HasForeignKey(d => d.IdIncentivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Canje_Incentivo");
        });

        modelBuilder.Entity<CatalogoIncentivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Catalogo__3214EC07BD55A4C5");

            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.NombrePremio).HasMaxLength(100);
            entity.Property(e => e.StockDisponible).HasDefaultValue(0);
        });

        modelBuilder.Entity<CategoriaGasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07A3F49746");

            entity.ToTable("CategoriaGasto");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC070B0B0CAC");

            entity.Property(e => e.Apellidos).HasMaxLength(60);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Nombres).HasMaxLength(60);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
            entity.Property(e => e.PuntosAcumulados).HasDefaultValue(0);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_Documento");

            entity.HasOne(d => d.IdSexoNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdSexo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_Sexo");
        });

        // ❌ BORRADO: Se eliminó por completo el bloque modelBuilder.Entity<Conductore>() de aquí.

        modelBuilder.Entity<DetalleFactura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleF__3214EC07183BE331");

            entity.ToTable("DetalleFactura");

            entity.Property(e => e.CodigoItem).HasMaxLength(50);
            entity.Property(e => e.Descripción).HasMaxLength(260);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ValorUnitario).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.DetalleFacturas)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleF_Factura");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleFacturas)
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_DetalleF_Producto");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.DetalleFacturas)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK_DetalleF_Servicio");
        });

        modelBuilder.Entity<DetalleProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleP__3214EC077517B69D");

            entity.HasIndex(e => e.IdProducto, "UQ__DetalleP__09889211BCD1C267").IsUnique();

            entity.Property(e => e.Ancho).HasMaxLength(10);
            entity.Property(e => e.Diametro).HasMaxLength(10);
            entity.Property(e => e.IndiceCarga).HasMaxLength(10);
            entity.Property(e => e.IndiceVelocidad).HasMaxLength(10);
            entity.Property(e => e.Perfil).HasMaxLength(10);
            entity.Property(e => e.TipoAceite).HasMaxLength(30);
            entity.Property(e => e.Viscosidad).HasMaxLength(20);

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.DetalleProductos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleP_Marca");

            entity.HasOne(d => d.IdProductoNavigation).WithOne(p => p.DetalleProducto)
                .HasForeignKey<DetalleProducto>(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleP_Producto");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empleado__3214EC07434BD29A");

            entity.Property(e => e.Apellidos).HasMaxLength(60);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Horario).HasMaxLength(50);
            entity.Property(e => e.Nombres).HasMaxLength(60);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
            entity.Property(e => e.Salario).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleados_Documento");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleados_Roles");

            entity.HasOne(d => d.IdSexoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdSexo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleados_Sexo");
        });

        modelBuilder.Entity<EstadoInventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EstadoIn__3214EC075355F3A3");

            entity.ToTable("EstadoInventario");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facturas__3214EC070AD40338");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Iva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IVA");
            entity.Property(e => e.RteFte)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalBruto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPagar).HasColumnType("decimal(18, 2)");

            // ✅ CORREGIDO: Relación cambiada de Conductor a Cliente
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturas_Cliente");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Gastos__3214EC0735B1CBF7");

            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_Categoria");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC0732C7DFFF");

            entity.ToTable("Inventario");

            entity.HasIndex(e => e.Codigo, "UQ__Inventar__06370DAC43C451B2").IsUnique();

            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StockActual).HasDefaultValue(0);
            entity.Property(e => e.StockMinimo).HasDefaultValue(40);

            entity.HasOne(d => d.IdBodegaNavigation).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.IdBodega)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventario_Bodega");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventario_Estado");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventario_Producto");
        });

        modelBuilder.Entity<LogActividad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LogActiv__3214EC078ACEF574");

            entity.ToTable("LogActividad");

            entity.Property(e => e.Accion).HasMaxLength(100);
            entity.Property(e => e.DireccionIp)
                .HasMaxLength(45)
                .HasColumnName("DireccionIP");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TablaAfectada).HasMaxLength(50);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LogActividads)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_Usuario");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC07BCD148E3");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ofertas__3214EC078BED32FB");

            entity.HasIndex(e => e.CodigoPromocional, "UQ__Ofertas__172ECB7A9611BA9C").IsUnique();

            entity.Property(e => e.CodigoPromocional).HasMaxLength(20);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.MontoDescuentoFijo)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PorcentajeDescuento)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(100);
        });

        modelBuilder.Entity<PerfilUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PerfilUs__3214EC077EF3D8F5");

            entity.ToTable("PerfilUsuario");

            entity.HasIndex(e => e.IdUsuario, "UQ__PerfilUs__5B65BF964D23691B").IsUnique();

            entity.Property(e => e.Bio).HasMaxLength(255);
            entity.Property(e => e.NotificacionesActivas).HasDefaultValue(true);
            entity.Property(e => e.TemaPreferencia)
                .HasMaxLength(20)
                .HasDefaultValue("Light");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.PerfilUsuario)
                .HasForeignKey<PerfilUsuario>(d => d.IdUsuario)
                .HasConstraintName("FK_Perfil_Usuario");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07A32D7B2F");

            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(14, 2)");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Proveedor");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proveedo__3214EC0795DE5AE5");

            entity.Property(e => e.Contacto).HasMaxLength(100);
            entity.Property(e => e.NombreEmpresa).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<RecuperacionCuenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Recupera__3214EC078147DB11");

            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.TokenRecuperacion).HasMaxLength(100);
            entity.Property(e => e.Usado).HasDefaultValue(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RecuperacionCuenta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recuperacion_Usuario");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07D0A9A948");

            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<Sexo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sexo__3214EC0792275EF0");

            entity.ToTable("Sexo");

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoDocu__3214EC07071BF407");

            entity.ToTable("TipoDocumento");

            entity.Property(e => e.Nombre).HasMaxLength(40);
        });

        modelBuilder.Entity<TipoServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoServ__3214EC0792BBB127");

            entity.ToTable("TipoServicio");

            entity.Property(e => e.IconoPath).HasMaxLength(255);
            entity.Property(e => e.NombreServicio).HasMaxLength(50);
            entity.Property(e => e.ValorServicio).HasColumnType("decimal(16, 3)");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07440B1D54");

            entity.HasIndex(e => e.Username, "UQ__Usuarios__536C85E49A6A20E7").IsUnique();

            entity.HasIndex(e => e.IdEmpleado, "UQ__Usuarios__CE6D8B9F67398071").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaFinBaneo).HasColumnType("datetime");
            entity.Property(e => e.MotivoSancion).HasMaxLength(255);
            entity.Property(e => e.UltimaConexion).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.IdEmpleadoNavigation).WithOne(p => p.Usuario)
                .HasForeignKey<Usuario>(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Empleado");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Rol");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vehiculo__3214EC07EB4BB1DF");

            entity.Property(e => e.Marca).HasMaxLength(50);
            entity.Property(e => e.Modelo).HasMaxLength(45);
            entity.Property(e => e.Placa).HasMaxLength(10);

            // ✅ CORREGIDO: Relación cambiada de Conductor a Cliente
            entity.HasOne(d => d.Cliente).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehiculos_Cliente");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}