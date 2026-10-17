using Llanteria.Data;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// El proyecto crea la base con EnsureCreated, que NO modifica una base que ya existe.
/// Este paso agrega, de forma idempotente, lo que se añadió después de la primera versión
/// (si la base es nueva, EnsureCreated ya lo creó y esto no hace nada).
/// </summary>
public static class EsquemaUpdater
{
    public static void Aplicar(LlanteriaDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.Citas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Citas (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Citas PRIMARY KEY,
        Slug nvarchar(60) NOT NULL,
        Servicio nvarchar(100) NOT NULL,
        Fecha date NOT NULL,
        Hora nvarchar(10) NOT NULL,
        Vehiculo nvarchar(150) NOT NULL,
        Direccion nvarchar(200) NULL,
        FormaPago nvarchar(20) NOT NULL,
        Notas nvarchar(500) NULL,
        Solicitante nvarchar(100) NOT NULL,
        Telefono nvarchar(20) NULL,
        Correo nvarchar(100) NULL,
        IdCliente int NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_Citas_Estado DEFAULT ('Pendiente'),
        FechaCreacion datetime NOT NULL CONSTRAINT DF_Citas_Fecha DEFAULT (GETDATE())
    );
END;

IF OBJECT_ID(N'dbo.SolicitudesEmpresa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudesEmpresa (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SolicitudesEmpresa PRIMARY KEY,
        Empresa nvarchar(120) NOT NULL,
        Nit nvarchar(20) NULL,
        Contacto nvarchar(100) NOT NULL,
        Telefono nvarchar(20) NOT NULL,
        Correo nvarchar(100) NOT NULL,
        Ciudad nvarchar(60) NULL,
        TipoFlota nvarchar(40) NOT NULL,
        NumeroVehiculos int NOT NULL,
        Mensaje nvarchar(500) NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_SolEmp_Estado DEFAULT ('Pendiente'),
        FechaCreacion datetime NOT NULL CONSTRAINT DF_SolEmp_Fecha DEFAULT (GETDATE())
    );
END;

IF OBJECT_ID(N'dbo.Pqrs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pqrs (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pqrs PRIMARY KEY,
        Radicado nvarchar(20) NOT NULL,
        Tipo nvarchar(20) NOT NULL,
        Nombre nvarchar(100) NOT NULL,
        NumeroDocumento nvarchar(20) NULL,
        Correo nvarchar(100) NOT NULL,
        Telefono nvarchar(25) NULL,
        Asunto nvarchar(120) NOT NULL,
        Mensaje nvarchar(1500) NOT NULL,
        AutorizaDatos bit NOT NULL,
        IdCliente int NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_Pqrs_Estado DEFAULT ('Recibida'),
        Respuesta nvarchar(1500) NULL,
        FechaCreacion datetime NOT NULL CONSTRAINT DF_Pqrs_Fecha DEFAULT (GETDATE()),
        FechaRespuesta datetime NULL
    );
    CREATE UNIQUE INDEX IX_Pqrs_Radicado ON dbo.Pqrs(Radicado);
END;

IF COL_LENGTH('dbo.DetalleProductos', 'TipoVehiculo') IS NULL
    ALTER TABLE dbo.DetalleProductos ADD TipoVehiculo nvarchar(10) NOT NULL CONSTRAINT DF_DetalleProductos_TipoVehiculo DEFAULT ('Auto');

IF COL_LENGTH('dbo.Vehiculos', 'Anio') IS NULL
    ALTER TABLE dbo.Vehiculos ADD Anio int NULL;
IF COL_LENGTH('dbo.Vehiculos', 'Kilometraje') IS NULL
    ALTER TABLE dbo.Vehiculos ADD Kilometraje int NULL;

IF OBJECT_ID(N'dbo.OrdenesTrabajo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrdenesTrabajo (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrdenesTrabajo PRIMARY KEY,
        Numero nvarchar(20) NOT NULL,
        IdVehiculo int NOT NULL CONSTRAINT FK_OrdenesTrabajo_Vehiculos REFERENCES dbo.Vehiculos(Id),
        IdEmpleado int NULL,
        Tipo nvarchar(40) NOT NULL,
        Descripcion nvarchar(150) NOT NULL,
        Kilometraje int NULL,
        Observaciones nvarchar(500) NULL,
        Total decimal(14,2) NOT NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_OrdenesTrabajo_Estado DEFAULT ('Recibido'),
        FechaIngreso datetime NOT NULL,
        FechaEntrega datetime NULL,
        ProximaFecha date NULL,
        ProximoKm int NULL,
        RecordatorioEnviado bit NOT NULL CONSTRAINT DF_OrdenesTrabajo_Recordatorio DEFAULT (0)
    );
    CREATE UNIQUE INDEX IX_OrdenesTrabajo_Numero ON dbo.OrdenesTrabajo(Numero);
    CREATE INDEX IX_OrdenesTrabajo_IdVehiculo ON dbo.OrdenesTrabajo(IdVehiculo);
END;

IF OBJECT_ID(N'dbo.Resenias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Resenias (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Resenias PRIMARY KEY,
        IdCliente int NOT NULL,
        Nombre nvarchar(100) NOT NULL,
        Calificacion int NOT NULL,
        Comentario nvarchar(400) NOT NULL,
        Visible bit NOT NULL CONSTRAINT DF_Resenias_Visible DEFAULT (1),
        Fecha datetime NOT NULL CONSTRAINT DF_Resenias_Fecha DEFAULT (GETDATE())
    );
    CREATE UNIQUE INDEX IX_Resenias_IdCliente ON dbo.Resenias(IdCliente);
END;

IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
        Numero nvarchar(20) NOT NULL,
        Token nvarchar(32) NOT NULL,
        IdCliente int NULL,
        Nombres nvarchar(60) NOT NULL,
        Apellidos nvarchar(60) NOT NULL,
        TipoDocumento nvarchar(10) NOT NULL,
        NumeroDocumento nvarchar(20) NOT NULL,
        Correo nvarchar(100) NOT NULL,
        Telefono nvarchar(25) NOT NULL,
        Pais nvarchar(40) NOT NULL,
        Departamento nvarchar(60) NOT NULL,
        Ciudad nvarchar(60) NOT NULL,
        Direccion nvarchar(200) NOT NULL,
        Complemento nvarchar(100) NULL,
        CodigoPostal nvarchar(12) NOT NULL,
        Notas nvarchar(400) NULL,
        MetodoPago nvarchar(20) NOT NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_Pedidos_Estado DEFAULT ('Pendiente'),
        CodigoCupon nvarchar(20) NULL,
        Subtotal decimal(18,2) NOT NULL,
        Descuento decimal(18,2) NOT NULL,
        BaseGravable decimal(18,2) NOT NULL,
        Iva decimal(18,2) NOT NULL,
        Envio decimal(18,2) NOT NULL,
        Total decimal(18,2) NOT NULL,
        Fecha datetime NOT NULL
    );
    CREATE UNIQUE INDEX IX_Pedidos_Token ON dbo.Pedidos(Token);
END;

IF OBJECT_ID(N'dbo.PedidoItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PedidoItems (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PedidoItems PRIMARY KEY,
        IdPedido int NOT NULL CONSTRAINT FK_PedidoItems_Pedidos REFERENCES dbo.Pedidos(Id) ON DELETE CASCADE,
        IdProducto int NULL,
        Descripcion nvarchar(260) NOT NULL,
        Cantidad int NOT NULL,
        ValorUnitario decimal(18,2) NOT NULL,
        Subtotal decimal(18,2) NOT NULL
    );
    CREATE INDEX IX_PedidoItems_IdPedido ON dbo.PedidoItems(IdPedido);
END;

IF COL_LENGTH('dbo.Citas', 'RecordatorioEnviado') IS NULL
    ALTER TABLE dbo.Citas ADD RecordatorioEnviado bit NOT NULL CONSTRAINT DF_Citas_Recordatorio DEFAULT (0);

IF COL_LENGTH('dbo.Clientes', 'PasswordHash') IS NULL
    ALTER TABLE dbo.Clientes ADD PasswordHash nvarchar(200) NULL;

IF COL_LENGTH('dbo.Productos', 'PrecioPromocion') IS NULL
    ALTER TABLE dbo.Productos ADD PrecioPromocion decimal(14,2) NULL;

-- Índice único de Usuarios.IdEmpleado sin filtro: solo dejaba un usuario sin empleado (el segundo cliente que se registraba fallaba).
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ__Usuarios__CE6D8B9F67398071' AND object_id = OBJECT_ID(N'dbo.Usuarios') AND has_filter = 0)
BEGIN
    DROP INDEX UQ__Usuarios__CE6D8B9F67398071 ON dbo.Usuarios;
    CREATE UNIQUE INDEX UQ__Usuarios__CE6D8B9F67398071 ON dbo.Usuarios(IdEmpleado) WHERE IdEmpleado IS NOT NULL;
END;

-- Columnas que ahora se guardan cifradas: el texto cifrado es más largo, así que pasan a nvarchar(max).
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PerfilUsuario') AND name = 'Bio' AND max_length <> -1)
    ALTER TABLE dbo.PerfilUsuario ALTER COLUMN Bio nvarchar(max) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Usuarios') AND name = 'MotivoSancion' AND max_length <> -1)
    ALTER TABLE dbo.Usuarios ALTER COLUMN MotivoSancion nvarchar(max) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Clientes') AND name = 'Foto' AND max_length <> -1)
    ALTER TABLE dbo.Clientes ALTER COLUMN Foto nvarchar(max) NULL;");
    }
}
