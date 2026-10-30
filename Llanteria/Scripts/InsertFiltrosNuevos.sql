/* =====================================================================================================
   LLANDUPAR - DATOS PARA LOS FILTROS NUEVOS (maquinaria pesada y agrícola, baterías y lubricantes)
   SQL Server. Idempotente: se puede ejecutar varias veces (cada producto/marca se agrega solo si no existe, por nombre).
   Requisitos: la base ya creada por la aplicación y con al menos un proveedor, una bodega y la llantería (Scripts/DatosDePrueba.sql los crea).
   Las fotos usan default-producto.png: cámbialas desde el panel (Productos > Editar).
   Usa GO: ejecútalo en SSMS / Azure Data Studio / sqlcmd.
   ===================================================================================================== */
-- USE [LlanteriaDB];
-- GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* 0) Columnas nuevas (la aplicación también las crea al iniciar; se repiten por si ejecutas el script antes). */
IF COL_LENGTH(N'dbo.DetalleProductos', N'Voltaje') IS NULL ALTER TABLE dbo.DetalleProductos ADD Voltaje nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'TecnologiaBateria') IS NULL ALTER TABLE dbo.DetalleProductos ADD TecnologiaBateria nvarchar(30) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'AplicacionLubricante') IS NULL ALTER TABLE dbo.DetalleProductos ADD AplicacionLubricante nvarchar(30) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'PresentacionLubricante') IS NULL ALTER TABLE dbo.DetalleProductos ADD PresentacionLubricante nvarchar(30) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'TipoAccesorio') IS NULL ALTER TABLE dbo.DetalleProductos ADD TipoAccesorio nvarchar(40) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'UbicacionAccesorio') IS NULL ALTER TABLE dbo.DetalleProductos ADD UbicacionAccesorio nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.DetalleProductos', N'TipoVehiculo') IS NULL ALTER TABLE dbo.DetalleProductos ADD TipoVehiculo nvarchar(10) NOT NULL CONSTRAINT DF_DetalleProductos_TipoVehiculo DEFAULT ('Auto');
IF COL_LENGTH(N'dbo.Productos', N'PorcentajeDescuento') IS NULL ALTER TABLE dbo.Productos ADD PorcentajeDescuento decimal(5,2) NULL;
IF COL_LENGTH(N'dbo.Productos', N'DuracionDescuentoDias') IS NULL ALTER TABLE dbo.Productos ADD DuracionDescuentoDias int NULL;
IF COL_LENGTH(N'dbo.Productos', N'FechaInicioDescuento') IS NULL ALTER TABLE dbo.Productos ADD FechaInicioDescuento datetime NULL;
GO

/* 1) Marcas que falten */
INSERT INTO dbo.Marcas (Nombre)
SELECT m.N FROM (VALUES (N'Titan'), (N'Trelleborg'), (N'Alliance'), (N'Michelin'), (N'Bridgestone'), (N'Goodyear'), (N'Firestone'), (N'Bosch'), (N'Willard'), (N'MAC'), (N'Varta'), (N'Mobil'), (N'Shell'), (N'Castrol'), (N'Valvoline')) m(N)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Marcas x WHERE x.Nombre = m.N);
GO

/* 2) Completa los filtros nuevos en los productos que YA tienes (solo donde el campo está vacío).
      Deduce el valor del nombre y la descripción; revísalo y ajústalo desde el panel (Productos > Editar) si algo no coincide. */
-- Baterías: voltaje y tecnología
UPDATE d SET Voltaje = CASE WHEN p.Nombre LIKE N'%24V%' THEN N'24V' WHEN p.Nombre LIKE N'% 6V %' THEN N'6V' ELSE N'12V' END
FROM dbo.DetalleProductos d JOIN dbo.Productos p ON p.Id = d.IdProducto
WHERE p.Categoria = N'Baterías' AND d.Voltaje IS NULL;

UPDATE d SET TecnologiaBateria = CASE
        WHEN p.Nombre LIKE N'%AGM%' OR p.Nombre LIKE N'%Optima%' THEN N'AGM'
        WHEN p.Descripcion LIKE N'%libre%mantenimiento%' OR p.Descripcion LIKE N'%sellad%' THEN N'Libre de mantenimiento'
        ELSE N'Plomo-ácido' END
FROM dbo.DetalleProductos d JOIN dbo.Productos p ON p.Id = d.IdProducto
WHERE p.Categoria = N'Baterías' AND d.TecnologiaBateria IS NULL;

-- Lubricantes: aplicación y presentación
UPDATE d SET AplicacionLubricante = CASE
        WHEN d.TipoVehiculo = N'Moto' OR p.Nombre LIKE N'%Moto%' THEN N'Motocicleta 4T'
        WHEN p.Nombre LIKE N'%Diésel%' OR p.Nombre LIKE N'%Diesel%' OR p.Nombre LIKE N'%Delvac%' OR p.Nombre LIKE N'%Rimula%' THEN N'Motor diésel'
        ELSE N'Motor gasolina' END
FROM dbo.DetalleProductos d JOIN dbo.Productos p ON p.Id = d.IdProducto
WHERE p.Categoria = N'Aceites' AND d.AplicacionLubricante IS NULL;

UPDATE d SET PresentacionLubricante = CASE
        WHEN p.Nombre LIKE N'%55 gal%' THEN N'Caneca (55 gal)'
        WHEN p.Nombre LIKE N'%5 gal%' THEN N'Balde (5 gal)'
        WHEN p.Nombre LIKE N'% 4 L%' THEN N'Galón (4 L)'
        WHEN p.Nombre LIKE N'% 1 L%' THEN N'Litro'
        ELSE NULL END
FROM dbo.DetalleProductos d JOIN dbo.Productos p ON p.Id = d.IdProducto
WHERE p.Categoria = N'Aceites' AND d.PresentacionLubricante IS NULL;
GO

/* 3) Productos de MAQUINARIA PESADA y AGRÍCOLA (llantas, baterías y lubricantes) para que los chips "Maquinaria pesada" y
      "Maquinaria agrícola" muestren resultados. TipoVehiculo: Pesada o Agricola. */
DECLARE @Prov int = (SELECT TOP 1 Id FROM dbo.Proveedores ORDER BY Id);
DECLARE @Bod  int = (SELECT TOP 1 Id FROM dbo.Bodegas ORDER BY Id);
DECLARE @Lla  int = (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);
DECLARE @Disp int = (SELECT TOP 1 Id FROM dbo.EstadoInventario WHERE Nombre = N'Disponible');
IF @Prov IS NULL OR @Bod IS NULL
BEGIN
    RAISERROR(N'Faltan proveedores o bodegas. Ejecuta primero Scripts/DatosDePrueba.sql (bloques 1 a 4) o crea al menos un proveedor y una bodega desde el panel.', 16, 1);
    RETURN;
END;

DECLARE @P TABLE (Nombre nvarchar(100), Cat nvarchar(50), Marca nvarchar(50), Descr nvarchar(255), Compra decimal(14,2), Venta decimal(14,2),
    Ancho nvarchar(10), Perfil nvarchar(10), Diam nvarchar(10), Veh nvarchar(10), IC nvarchar(10), IV nvarchar(10),
    Visc nvarchar(20), TipoAc nvarchar(30), Gar int, Amp nvarchar(20), Pol nvarchar(20), Dto decimal(5,2), Dias int,
    Volt nvarchar(10), Tec nvarchar(30), Apl nvarchar(30), Pres nvarchar(30), TipoAcc nvarchar(40), Ubic nvarchar(20));
INSERT INTO @P VALUES
(N'Michelin XHA2 17.5 R25 para cargador',N'Llantas',N'Michelin',N'Radial para cargadores frontales: gran resistencia a cortes y buena tracción.',4200000,5300000,N'17.5',NULL,N'25',N'Pesada',N'170',N'A2',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Bridgestone VJT 23.5 R25 para maquinaria',N'Llantas',N'Bridgestone',N'Radial para cargadores y volquetas articuladas en minería y obra civil.',6800000,8500000,N'23.5',NULL,N'25',N'Pesada',N'190',N'B',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Goodyear RL-4+ 20.5 R25 para maquinaria',N'Llantas',N'Goodyear',N'Banda profunda para trabajo en roca y terreno abrasivo.',5200000,6600000,N'20.5',NULL,N'25',N'Pesada',N'176',N'A2',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Titan Hard Surface 14.00-24 para motoniveladora',N'Llantas',N'Titan',N'Diagonal para motoniveladoras y trabajo sobre superficies duras.',3600000,4600000,N'14.00',NULL,N'24',N'Pesada',N'148',N'B',NULL,NULL,12,NULL,NULL,8,30,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Titan Farm 18.4-34 trasera para tractor',N'Llantas',N'Titan',N'Llanta trasera agrícola de barras altas para máxima tracción en suelo húmedo.',2900000,3700000,N'18.4',NULL,N'34',N'Agricola',N'141',N'A8',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Trelleborg TM600 600/65 R38 para tractor',N'Llantas',N'Trelleborg',N'Radial agrícola de alta flotación que reduce la compactación del suelo.',7500000,9400000,N'600',N'65',N'38',N'Agricola',N'159',N'D',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Alliance Agristar 14.9-24 para tractor',N'Llantas',N'Alliance',N'Llanta agrícola diagonal económica para tractores medianos.',1150000,1500000,N'14.9',NULL,N'24',N'Agricola',N'124',N'A6',NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Firestone Radial All Traction 420/85 R30',N'Llantas',N'Firestone',N'Radial agrícola con buena autolimpieza y confort en carretera.',2600000,3300000,N'420',N'85',N'30',N'Agricola',N'144',N'A8',NULL,NULL,12,NULL,NULL,10,30,NULL,NULL,NULL,NULL,NULL,NULL),
(N'Batería Bosch T4 12V 180Ah para camión',N'Baterías',N'Bosch',N'Batería libre de mantenimiento para camiones y maquinaria de arranque exigente.',720000,910000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,18,N'180Ah',N'Izquierda',NULL,NULL,N'12V',N'Libre de mantenimiento',NULL,NULL,NULL,NULL),
(N'Batería Willard 4D 12V 200Ah para maquinaria',N'Baterías',N'Willard',N'Gran reserva de energía para retroexcavadoras y cargadores.',780000,980000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,12,N'200Ah',N'Izquierda',NULL,NULL,N'12V',N'Plomo-ácido',NULL,NULL,NULL,NULL),
(N'Batería MAC 8D 12V 225Ah para maquinaria',N'Baterías',N'MAC',N'Capacidad extra para equipos de arranque diésel pesado.',880000,1100000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,12,N'225Ah',N'Derecha',NULL,NULL,N'12V',N'Plomo-ácido',NULL,NULL,NULL,NULL),
(N'Batería Varta ProMotive 12V 180Ah para tractor',N'Baterías',N'Varta',N'Resistente a vibraciones y arranques en frío; ideal para trabajo agrícola.',760000,950000,NULL,NULL,NULL,N'Agricola',NULL,NULL,NULL,NULL,24,N'180Ah',N'Izquierda',NULL,NULL,N'12V',N'Libre de mantenimiento',NULL,NULL,NULL,NULL),
(N'Batería Willard 12V 100Ah para tractor',N'Baterías',N'Willard',N'Batería de uso agrícola para tractores medianos y motobombas.',420000,530000,NULL,NULL,NULL,N'Agricola',NULL,NULL,NULL,NULL,12,N'100Ah',N'Derecha',10,30,N'12V',N'Plomo-ácido',NULL,NULL,NULL,NULL),
(N'Batería Bosch 6V 225Ah para maquinaria',N'Baterías',N'Bosch',N'Par de baterías de 6 V en serie (24 V) para equipos de servicio pesado.',520000,650000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,12,N'225Ah',N'Izquierda',NULL,NULL,N'6V',N'Plomo-ácido',NULL,NULL,NULL,NULL),
(N'Mobil Delvac 1300 Super 15W-40 Balde 5 gal',N'Aceites',N'Mobil',N'Aceite diésel de servicio pesado para motores de maquinaria y camiones.',420000,530000,NULL,NULL,NULL,N'Pesada',NULL,NULL,N'15W-40',N'Mineral',6,NULL,NULL,NULL,NULL,NULL,NULL,N'Motor diésel',N'Balde (5 gal)',NULL,NULL),
(N'Shell Rimula R4 L 15W-40 Caneca 55 gal',N'Aceites',N'Shell',N'Aceite diésel de alto rendimiento para flotas y maquinaria en obra.',3900000,4800000,NULL,NULL,NULL,N'Pesada',NULL,NULL,N'15W-40',N'Mineral',6,NULL,NULL,NULL,NULL,NULL,NULL,N'Motor diésel',N'Caneca (55 gal)',NULL,NULL),
(N'Mobil DTE 10 Excel 46 Hidráulico Balde 5 gal',N'Aceites',N'Mobil',N'Aceite hidráulico de alto índice de viscosidad para excavadoras y cargadores.',390000,490000,NULL,NULL,NULL,N'Pesada',NULL,NULL,N'ISO 46',N'Mineral',6,NULL,NULL,NULL,NULL,NULL,NULL,N'Hidráulico',N'Balde (5 gal)',NULL,NULL),
(N'Castrol Agri Trans Plus 10W-30 UTTO Balde 5 gal',N'Aceites',N'Castrol',N'Aceite de transmisión e hidráulico para tractores (UTTO).',410000,520000,NULL,NULL,NULL,N'Agricola',NULL,NULL,N'10W-30',N'Mineral',6,NULL,NULL,8,25,NULL,NULL,N'Transmisión',N'Balde (5 gal)',NULL,NULL),
(N'Shell Spirax S4 TXM UTTO Balde 5 gal',N'Aceites',N'Shell',N'Fluido para transmisión, frenos húmedos e hidráulica de tractores.',430000,540000,NULL,NULL,NULL,N'Agricola',NULL,NULL,N'10W-30',N'Mineral',6,NULL,NULL,NULL,NULL,NULL,NULL,N'Transmisión',N'Balde (5 gal)',NULL,NULL),
(N'Valvoline Premium Blue 15W-40 diésel 4 L',N'Aceites',N'Valvoline',N'Aceite diésel para tractores y motobombas con exigencia de campo.',125000,160000,NULL,NULL,NULL,N'Agricola',NULL,NULL,N'15W-40',N'Mineral',6,NULL,NULL,NULL,NULL,NULL,NULL,N'Motor diésel',N'Galón (4 L)',NULL,NULL);

INSERT INTO dbo.Productos (Nombre, Descripcion, PrecioCompra, PrecioVenta, IdProveedor, Categoria, RutaImagen, IdBodega, IdLlanteria, PorcentajeDescuento, DuracionDescuentoDias, FechaInicioDescuento)
SELECT p.Nombre, p.Descr, p.Compra, p.Venta, @Prov, p.Cat, N'default-producto.png', @Bod, @Lla, p.Dto, p.Dias, CASE WHEN p.Dto IS NOT NULL THEN GETDATE() END
FROM @P p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Productos x WHERE x.Nombre = p.Nombre);

INSERT INTO dbo.DetalleProductos (IdProducto, IdMarca, Ancho, Perfil, Diametro, TipoVehiculo, IndiceCarga, IndiceVelocidad, Viscosidad, TipoAceite, GarantiaMeses, Amperaje, Polaridad,
    Voltaje, TecnologiaBateria, AplicacionLubricante, PresentacionLubricante, TipoAccesorio, UbicacionAccesorio)
SELECT pr.Id, m.Id, p.Ancho, p.Perfil, p.Diam, p.Veh, p.IC, p.IV, p.Visc, p.TipoAc, p.Gar, p.Amp, p.Pol, p.Volt, p.Tec, p.Apl, p.Pres, p.TipoAcc, p.Ubic
FROM @P p
JOIN dbo.Productos pr ON pr.Nombre = p.Nombre
JOIN dbo.Marcas m ON m.Nombre = p.Marca
WHERE NOT EXISTS (SELECT 1 FROM dbo.DetalleProductos d WHERE d.IdProducto = pr.Id);

-- Existencias: cada producto nuevo entra con 12 unidades disponibles (mínimo 5).
INSERT INTO dbo.Inventario (IdProducto, Codigo, StockActual, StockMinimo, IdEstado, FechaIngreso, CantidadIngresada, IdBodega)
SELECT pr.Id, N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5), 12, 5, @Disp, CAST(GETDATE() AS date), 12, @Bod
FROM @P p
JOIN dbo.Productos pr ON pr.Nombre = p.Nombre
WHERE @Disp IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i WHERE i.IdProducto = pr.Id)
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i2 WHERE i2.Codigo = N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5));
GO

SELECT d.TipoVehiculo, p.Categoria, COUNT(*) AS Productos
FROM dbo.DetalleProductos d JOIN dbo.Productos p ON p.Id = d.IdProducto
WHERE d.TipoVehiculo IN (N'Pesada', N'Agricola')
GROUP BY d.TipoVehiculo, p.Categoria ORDER BY d.TipoVehiculo, p.Categoria;
GO
