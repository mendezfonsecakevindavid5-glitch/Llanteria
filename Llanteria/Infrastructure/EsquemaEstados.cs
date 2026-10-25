using Llanteria.Data;
using Llanteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>
/// Parte de EsquemaUpdater para lo que se añadió con los estados de registro: tablas Estados y EstadosFinancieros,
/// IdEstado en todos los modelos, tablas Llanterias y Eventos, y las columnas nuevas de promociones, reseñas y blog.
/// Todo es idempotente: si la base es nueva, EnsureCreated ya lo creó y esto no cambia nada.
/// </summary>
public static partial class EsquemaUpdater
{
    private static void AplicarEstadosYNuevasTablas(LlanteriaDbContext db)
    {
        var errores = new List<string>();
        void Ejecutar(string sql)
        {
            try { db.Database.ExecuteSqlRaw(sql); }
            catch (Exception ex) { errores.Add(ex.Message); }
        }

        // 1) Tablas de estados con sus valores semilla.
        Ejecutar("IF OBJECT_ID(N'dbo.Estados', N'U') IS NULL CREATE TABLE dbo.Estados (Id int NOT NULL CONSTRAINT PK_Estados PRIMARY KEY, Nombre nvarchar(30) NOT NULL);");
        Ejecutar("IF OBJECT_ID(N'dbo.EstadosFinancieros', N'U') IS NULL CREATE TABLE dbo.EstadosFinancieros (Id int NOT NULL CONSTRAINT PK_EstadosFinancieros PRIMARY KEY, Nombre nvarchar(30) NOT NULL);");
        foreach (var (id, nombre) in EstadoIds.Semilla)
            Ejecutar($"IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Id = {id}) INSERT INTO dbo.Estados (Id, Nombre) VALUES ({id}, N'{nombre}');");
        foreach (var (id, nombre) in EstadoFinancieroIds.Semilla)
            Ejecutar($"IF NOT EXISTS (SELECT 1 FROM dbo.EstadosFinancieros WHERE Id = {id}) INSERT INTO dbo.EstadosFinancieros (Id, Nombre) VALUES ({id}, N'{nombre}');");

        // 2) Tablas nuevas.
        Ejecutar(@"
IF OBJECT_ID(N'dbo.Llanterias', N'U') IS NULL
CREATE TABLE dbo.Llanterias (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Llanterias PRIMARY KEY,
    Nombre nvarchar(120) NOT NULL,
    RazonSocial nvarchar(150) NULL, Nit nvarchar(20) NULL, Direccion nvarchar(200) NULL, Ciudad nvarchar(60) NULL,
    Departamento nvarchar(60) NULL, Telefono nvarchar(25) NULL, WhatsApp nvarchar(20) NULL, Correo nvarchar(100) NULL,
    SitioWeb nvarchar(200) NULL, Instagram nvarchar(200) NULL, Facebook nvarchar(200) NULL, Logo nvarchar(120) NULL,
    Latitud float NULL, Longitud float NULL,
    HorarioSemana nvarchar(40) NULL, HorarioSabado nvarchar(40) NULL, HorarioDomingo nvarchar(40) NULL,
    PropietarioNombre nvarchar(120) NULL, PropietarioDocumento nvarchar(20) NULL, PropietarioTelefono nvarchar(25) NULL, PropietarioCorreo nvarchar(100) NULL,
    OfreceLlantas bit NOT NULL CONSTRAINT DF_Llanterias_Llantas DEFAULT (1),
    OfreceLubricantes bit NOT NULL CONSTRAINT DF_Llanterias_Lubricantes DEFAULT (1),
    OfreceBaterias bit NOT NULL CONSTRAINT DF_Llanterias_Baterias DEFAULT (1),
    OfreceAccesorios bit NOT NULL CONSTRAINT DF_Llanterias_Accesorios DEFAULT (1),
    OfreceMantenimientoFlotas bit NOT NULL CONSTRAINT DF_Llanterias_Flotas DEFAULT (0),
    OfreceServicioADomicilio bit NOT NULL CONSTRAINT DF_Llanterias_Domicilio DEFAULT (0),
    ServiciosOfrecidos nvarchar(1000) NULL, ProductosDestacados nvarchar(1000) NULL, Observaciones nvarchar(1000) NULL,
    FechaCreacion datetime NOT NULL CONSTRAINT DF_Llanterias_Fecha DEFAULT (GETDATE())
);");
        Ejecutar(@"
IF OBJECT_ID(N'dbo.Eventos', N'U') IS NULL
CREATE TABLE dbo.Eventos (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Eventos PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Descripcion nvarchar(300) NULL,
    Fecha date NOT NULL CONSTRAINT DF_Eventos_Fecha DEFAULT (CONVERT(date, GETDATE())),
    FechaInicio date NOT NULL,
    FechaFin date NOT NULL,
    Recurrente bit NOT NULL CONSTRAINT DF_Eventos_Recurrente DEFAULT (0)
);");

        // 3) IdEstado (y su llave foránea) en todos los modelos; IdLlanteria en los que pertenecen a una llantería.
        foreach (var et in db.Model.GetEntityTypes())
        {
            var tipo = et.ClrType;
            var tabla = et.GetTableName();
            if (tabla == null) continue;
            var esquema = et.GetSchema() ?? "dbo";
            var completo = $"[{esquema}].[{tabla}]";
            var nombreObjeto = $"{esquema}.{tabla}";

            var estandar = typeof(IConEstado).IsAssignableFrom(tipo);
            var financiero = typeof(IConEstadoFinanciero).IsAssignableFrom(tipo);
            if (estandar || financiero)
            {
                var col = LlanteriaDbContext.ColumnaEstado(tipo);
                var predeterminado = estandar ? EstadoIds.Habilitado
                    : tipo == typeof(Pedido) || tipo == typeof(PedidoItem) || tipo == typeof(OrdenTrabajo) ? EstadoFinancieroIds.Pendiente : EstadoFinancieroIds.Pagado;
                var referencia = estandar ? "[dbo].[Estados]" : "[dbo].[EstadosFinancieros]";
                Ejecutar($"IF OBJECT_ID(N'{nombreObjeto}', N'U') IS NOT NULL AND COL_LENGTH(N'{nombreObjeto}', N'{col}') IS NULL " +
                         $"ALTER TABLE {completo} ADD [{col}] int NOT NULL CONSTRAINT [DF_{tabla}_{col}] DEFAULT ({predeterminado});");
                Ejecutar(SqlLlaveForanea(nombreObjeto, completo, col, referencia, "FK_" + tabla + "_" + col));
            }

            if (typeof(IDeLlanteria).IsAssignableFrom(tipo))
            {
                Ejecutar($"IF OBJECT_ID(N'{nombreObjeto}', N'U') IS NOT NULL AND COL_LENGTH(N'{nombreObjeto}', N'IdLlanteria') IS NULL ALTER TABLE {completo} ADD [IdLlanteria] int NULL;");
                Ejecutar(SqlLlaveForanea(nombreObjeto, completo, "IdLlanteria", "[dbo].[Llanterias]", "FK_" + tabla + "_IdLlanteria"));
            }
        }

        // 4) Promociones ligadas a eventos y productos ligados a una promoción.
        Ejecutar("IF COL_LENGTH(N'dbo.Ofertas', N'IdEvento') IS NULL ALTER TABLE dbo.Ofertas ADD IdEvento int NULL;");
        Ejecutar(SqlLlaveForanea("dbo.Ofertas", "[dbo].[Ofertas]", "IdEvento", "[dbo].[Eventos]", "FK_Ofertas_IdEvento"));
        Ejecutar("IF COL_LENGTH(N'dbo.Productos', N'IdOferta') IS NULL ALTER TABLE dbo.Productos ADD IdOferta int NULL;");
        Ejecutar(SqlLlaveForanea("dbo.Productos", "[dbo].[Productos]", "IdOferta", "[dbo].[Ofertas]", "FK_Productos_IdOferta"));

        // 4b) Eventos: día en que ocurre (se muestra solo ese día). En bases anteriores se copia de FechaInicio.
        Ejecutar(@"
IF OBJECT_ID(N'dbo.Eventos', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Eventos', N'Fecha') IS NULL
BEGIN
    ALTER TABLE dbo.Eventos ADD Fecha date NULL;
    EXEC(N'UPDATE dbo.Eventos SET Fecha = FechaInicio');
    EXEC(N'ALTER TABLE dbo.Eventos ALTER COLUMN Fecha date NOT NULL');
    EXEC(N'ALTER TABLE dbo.Eventos ADD CONSTRAINT DF_Eventos_Fecha DEFAULT (CONVERT(date, GETDATE())) FOR Fecha');
END;");

        // 4c) Descuentos de productos (llantas, baterías y lubricantes): porcentaje, duración en días y día de inicio.
        Ejecutar("IF COL_LENGTH(N'dbo.Productos', N'PorcentajeDescuento') IS NULL ALTER TABLE dbo.Productos ADD PorcentajeDescuento decimal(5,2) NULL;");
        Ejecutar("IF COL_LENGTH(N'dbo.Productos', N'DuracionDescuentoDias') IS NULL ALTER TABLE dbo.Productos ADD DuracionDescuentoDias int NULL;");
        Ejecutar("IF COL_LENGTH(N'dbo.Productos', N'FechaInicioDescuento') IS NULL ALTER TABLE dbo.Productos ADD FechaInicioDescuento datetime NULL;");

        // 5) Reseñas: usuario y foto del cliente que opina.
        Ejecutar("IF COL_LENGTH(N'dbo.Resenias', N'IdUsuario') IS NULL ALTER TABLE dbo.Resenias ADD IdUsuario int NULL;");
        Ejecutar("IF COL_LENGTH(N'dbo.Resenias', N'Usuario') IS NULL ALTER TABLE dbo.Resenias ADD Usuario nvarchar(50) NULL;");

        // 6) Blog: autor, imagen de presentación, tabla de contenido y categoría opcional.
        Ejecutar("IF COL_LENGTH(N'dbo.Articulos', N'IdAutor') IS NULL ALTER TABLE dbo.Articulos ADD IdAutor int NULL;");
        Ejecutar("IF COL_LENGTH(N'dbo.Articulos', N'ImagenPresentacion') IS NULL ALTER TABLE dbo.Articulos ADD ImagenPresentacion nvarchar(120) NULL;");
        Ejecutar("IF COL_LENGTH(N'dbo.Articulos', N'TablaContenido') IS NULL ALTER TABLE dbo.Articulos ADD TablaContenido nvarchar(200) NULL;");
        Ejecutar("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Articulos') AND name = N'Categoria' AND is_nullable = 0) ALTER TABLE dbo.Articulos ALTER COLUMN Categoria nvarchar(40) NULL;");

        if (errores.Count > 0)
            throw new InvalidOperationException("Algunos cambios del esquema (estados y tablas nuevas) no se pudieron aplicar: " + string.Join(" | ", errores.Distinct()));
    }

    /// <summary>Agrega la llave foránea solo si esa columna todavía no tiene ninguna (EF ya la crea con otro nombre en una base nueva).</summary>
    private static string SqlLlaveForanea(string nombreObjeto, string completo, string columna, string referencia, string nombreFk)
        => $"IF OBJECT_ID(N'{nombreObjeto}', N'U') IS NOT NULL AND COL_LENGTH(N'{nombreObjeto}', N'{columna}') IS NOT NULL " +
           $"AND NOT EXISTS (SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id " +
           $"WHERE fkc.parent_object_id = OBJECT_ID(N'{nombreObjeto}') AND c.name = N'{columna}') " +
           $"ALTER TABLE {completo} ADD CONSTRAINT [{nombreFk}] FOREIGN KEY ([{columna}]) REFERENCES {referencia}(Id);";
}
