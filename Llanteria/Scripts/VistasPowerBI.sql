/* =====================================================================================================
   LLANDUPAR - VISTAS PARA POWER BI (esquema [bi], solo lectura)
   Power BI se conecta a estas vistas y NO a las tablas: así el informe no ve contraseñas, tokens ni datos personales
   (los clientes aparecen solo por su Id), y puedes cambiar las tablas sin romper el informe.

   Cómo usarlas:
     1. Ejecuta este script en la base de la aplicación (SSMS / Azure Data Studio).
     2. Crea un usuario SOLO de lectura para Power BI (ver paso 3) y úsalo al conectar. Nunca uses el usuario de la aplicación.
     3. En Power BI Desktop: Obtener datos > SQL Server > servidor y base > modo "Importar" (o DirectQuery si necesitas datos en vivo)
        > selecciona las 4 vistas [bi].* > Cargar.
     4. Relaciones sugeridas: vw_Ventas[IdProducto] -> vw_Productos[IdProducto]; vw_Inventario[IdProducto] -> vw_Productos[IdProducto].
        Crea una tabla de fechas (Calendario) y relaciónala con vw_Ventas[Fecha] y vw_Gastos[FechaRegistro].
   ===================================================================================================== */
-- USE [LlanteriaDB];
-- GO
IF SCHEMA_ID(N'bi') IS NULL EXEC(N'CREATE SCHEMA bi');
GO

CREATE OR ALTER VIEW bi.vw_Productos AS
SELECT p.Id AS IdProducto, p.Nombre, p.Categoria, m.Nombre AS Marca, d.TipoVehiculo,
       CASE d.TipoVehiculo WHEN N'Moto' THEN N'Motocicletas' WHEN N'Pesada' THEN N'Maquinaria pesada' WHEN N'Agricola' THEN N'Maquinaria agrícola' ELSE N'Automóviles' END AS TipoVehiculoEtiqueta,
       d.TipoAccesorio, d.UbicacionAccesorio, d.Voltaje, d.TecnologiaBateria, d.AplicacionLubricante, d.PresentacionLubricante,
       p.PrecioCompra, p.PrecioVenta, p.PorcentajeDescuento,
       p.PrecioVenta - p.PrecioCompra AS MargenUnitario
FROM dbo.Productos p
LEFT JOIN dbo.DetalleProductos d ON d.IdProducto = p.Id
LEFT JOIN dbo.Marcas m ON m.Id = d.IdMarca;
GO

CREATE OR ALTER VIEW bi.vw_Ventas AS
SELECT f.Id AS IdFactura, f.Fecha, f.IdCliente,
       df.IdProducto, df.IdServicio, df.CodigoItem, df.[Descripción] AS Descripcion, df.Cantidad, df.ValorUnitario, df.Subtotal,
       COALESCE(p.Categoria, N'Servicios') AS Categoria,
       f.TotalBruto, f.Iva, f.TotalPagar
FROM dbo.Facturas f
JOIN dbo.DetalleFactura df ON df.IdFactura = f.Id
LEFT JOIN dbo.Productos p ON p.Id = df.IdProducto;
GO

CREATE OR ALTER VIEW bi.vw_Gastos AS
SELECT g.Id AS IdGasto, c.Nombre AS Categoria, g.Descripcion, g.Monto, g.FechaRegistro, g.FechaVencimiento, g.FechaPago, g.Estado
FROM dbo.Gastos g
LEFT JOIN dbo.CategoriaGasto c ON c.Id = g.IdCategoria;
GO

CREATE OR ALTER VIEW bi.vw_Inventario AS
SELECT i.Id AS IdInventario, i.IdProducto, i.Codigo, b.NombreBodega AS Bodega, e.Nombre AS Estado,
       i.StockActual, i.StockMinimo, i.CantidadIngresada, i.FechaIngreso,
       CASE WHEN i.StockActual <= i.StockMinimo THEN 1 ELSE 0 END AS StockBajo
FROM dbo.Inventario i
LEFT JOIN dbo.Bodegas b ON b.Id = i.IdBodega
LEFT JOIN dbo.EstadoInventario e ON e.Id = i.IdEstado;
GO

/* 3) Usuario de solo lectura para Power BI. CAMBIA la contraseña y guárdala en un gestor de contraseñas.
      (Descomenta, ajusta y ejecuta una sola vez. Si la base está en Azure SQL, crea el usuario con CREATE USER ... WITH PASSWORD.) */
-- CREATE LOGIN bi_lector WITH PASSWORD = N'CAMBIA-ESTA-CONTRASEÑA-LARGA-Y-UNICA';
-- CREATE USER bi_lector FOR LOGIN bi_lector;
-- GRANT SELECT ON SCHEMA::bi TO bi_lector;   -- solo las vistas del esquema bi; ninguna tabla
-- GO
