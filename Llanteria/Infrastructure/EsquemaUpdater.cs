using Llanteria.Data;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// El proyecto crea la base con EnsureCreated, que NO modifica una base que ya existe.
/// Este paso agrega, de forma idempotente, lo que se añadió después de la primera versión
/// (si la base es nueva, EnsureCreated ya lo creó y esto no hace nada).
/// Si prefieres migraciones de EF, mira la sección correspondiente en LEEME_CAMBIOS.md.
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

IF COL_LENGTH('dbo.Clientes', 'PasswordHash') IS NULL
    ALTER TABLE dbo.Clientes ADD PasswordHash nvarchar(200) NULL;");
    }
}
