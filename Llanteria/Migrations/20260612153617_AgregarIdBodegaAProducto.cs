using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Llanteria.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIdBodegaAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bodegas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreBodega = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Bodegas__3214EC07D99D647E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoIncentivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePremio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PuntosRequeridos = table.Column<int>(type: "int", nullable: false),
                    StockDisponible = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Catalogo__3214EC07BD55A4C5", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaGasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__3214EC07A3F49746", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadoInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EstadoIn__3214EC075355F3A3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Marcas__3214EC07BCD148E3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ofertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    MontoDescuentoFijo = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    FechaInicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CodigoPromocional = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ofertas__3214EC078BED32FB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Proveedo__3214EC0795DE5AE5", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC07D0A9A948", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sexo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sexo__3214EC0792275EF0", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoDocu__3214EC07071BF407", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreServicio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValorServicio = table.Column<decimal>(type: "decimal(16,3)", nullable: false),
                    IconoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoServ__3214EC0792BBB127", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Pendiente"),
                    ArchivoSupport = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Gastos__3214EC0735B1CBF7", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gastos_Categoria",
                        column: x => x.IdCategoria,
                        principalTable: "CategoriaGasto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdBodega = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Producto__3214EC07A32D7B2F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Bodegas_IdBodega",
                        column: x => x.IdBodega,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_Proveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdDocumento = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdSexo = table.Column<int>(type: "int", nullable: false),
                    PuntosAcumulados = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Clientes__3214EC070B0B0CAC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Documento",
                        column: x => x.IdDocumento,
                        principalTable: "TipoDocumento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Clientes_Sexo",
                        column: x => x.IdSexo,
                        principalTable: "Sexo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdDocumento = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Salario = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Horario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdSexo = table.Column<int>(type: "int", nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Empleado__3214EC07434BD29A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Documento",
                        column: x => x.IdDocumento,
                        principalTable: "TipoDocumento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Empleados_Roles",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Empleados_Sexo",
                        column: x => x.IdSexo,
                        principalTable: "Sexo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetalleProductos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdMarca = table.Column<int>(type: "int", nullable: false),
                    Ancho = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Perfil = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Diametro = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IndiceCarga = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IndiceVelocidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Viscosidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoAceite = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GarantiaMeses = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DetalleP__3214EC077517B69D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleP_Marca",
                        column: x => x.IdMarca,
                        principalTable: "Marcas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetalleP_Producto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Inventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    StockMinimo = table.Column<int>(type: "int", nullable: true, defaultValue: 40),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())"),
                    CantidadIngresada = table.Column<int>(type: "int", nullable: false),
                    IdBodega = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventar__3214EC0732C7DFFF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventario_Bodega",
                        column: x => x.IdBodega,
                        principalTable: "Bodegas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inventario_Estado",
                        column: x => x.IdEstado,
                        principalTable: "EstadoInventario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inventario_Producto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    TotalBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RteFte = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    TotalPagar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Facturas__3214EC070AD40338", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_Cliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Placa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vehiculo__3214EC07EB4BB1DF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Cliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Activo"),
                    MotivoSancion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FechaFinBaneo = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UltimaConexion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__3214EC07440B1D54", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Usuarios_Rol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetalleFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFactura = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: true),
                    IdServicio = table.Column<int>(type: "int", nullable: true),
                    CodigoItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripción = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DetalleF__3214EC07183BE331", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleF_Factura",
                        column: x => x.IdFactura,
                        principalTable: "Facturas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetalleF_Producto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetalleF_Servicio",
                        column: x => x.IdServicio,
                        principalTable: "TipoServicio",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CanjeIncentivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdIncentivo = table.Column<int>(type: "int", nullable: false),
                    FechaCanje = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    EntregadoPor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CanjeInc__3214EC0748C92D09", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Canje_Cliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Canje_Incentivo",
                        column: x => x.IdIncentivo,
                        principalTable: "CatalogoIncentivos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Canje_Usuario",
                        column: x => x.EntregadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogActividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TablaAfectada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    DireccionIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LogActiv__3214EC078ACEF574", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Log_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PerfilUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TemaPreferencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Light"),
                    FotoCircular = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    NotificacionesActivas = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PerfilUs__3214EC077EF3D8F5", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perfil_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecuperacionCuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    TokenRecuperacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Recupera__3214EC078147DB11", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recuperacion_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanjeIncentivos_EntregadoPor",
                table: "CanjeIncentivos",
                column: "EntregadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_CanjeIncentivos_IdCliente",
                table: "CanjeIncentivos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_CanjeIncentivos_IdIncentivo",
                table: "CanjeIncentivos",
                column: "IdIncentivo");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdDocumento",
                table: "Clientes",
                column: "IdDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdSexo",
                table: "Clientes",
                column: "IdSexo");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdFactura",
                table: "DetalleFactura",
                column: "IdFactura");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdProducto",
                table: "DetalleFactura",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdServicio",
                table: "DetalleFactura",
                column: "IdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleProductos_IdMarca",
                table: "DetalleProductos",
                column: "IdMarca");

            migrationBuilder.CreateIndex(
                name: "UQ__DetalleP__09889211BCD1C267",
                table: "DetalleProductos",
                column: "IdProducto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IdDocumento",
                table: "Empleados",
                column: "IdDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IdRol",
                table: "Empleados",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IdSexo",
                table: "Empleados",
                column: "IdSexo");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdCliente",
                table: "Facturas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_IdCategoria",
                table: "Gastos",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_IdBodega",
                table: "Inventario",
                column: "IdBodega");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_IdEstado",
                table: "Inventario",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_IdProducto",
                table: "Inventario",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "UQ__Inventar__06370DAC43C451B2",
                table: "Inventario",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogActividad_IdUsuario",
                table: "LogActividad",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "UQ__Ofertas__172ECB7A9611BA9C",
                table: "Ofertas",
                column: "CodigoPromocional",
                unique: true,
                filter: "[CodigoPromocional] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__PerfilUs__5B65BF964D23691B",
                table: "PerfilUsuario",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdBodega",
                table: "Productos",
                column: "IdBodega");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdProveedor",
                table: "Productos",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacionCuentas_IdUsuario",
                table: "RecuperacionCuentas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdRol",
                table: "Usuarios",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__536C85E49A6A20E7",
                table: "Usuarios",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__CE6D8B9F67398071",
                table: "Usuarios",
                column: "IdEmpleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_IdCliente",
                table: "Vehiculos",
                column: "IdCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanjeIncentivos");

            migrationBuilder.DropTable(
                name: "DetalleFactura");

            migrationBuilder.DropTable(
                name: "DetalleProductos");

            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "Inventario");

            migrationBuilder.DropTable(
                name: "LogActividad");

            migrationBuilder.DropTable(
                name: "Ofertas");

            migrationBuilder.DropTable(
                name: "PerfilUsuario");

            migrationBuilder.DropTable(
                name: "RecuperacionCuentas");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "CatalogoIncentivos");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "TipoServicio");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "CategoriaGasto");

            migrationBuilder.DropTable(
                name: "EstadoInventario");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Bodegas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "TipoDocumento");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sexo");
        }
    }
}
