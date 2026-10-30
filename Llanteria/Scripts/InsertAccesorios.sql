/* =====================================================================================================
   LLANDUPAR - INSERT DE ACCESORIOS (frenos, filtros, iluminación, limpiaparabrisas, seguridad, interior, exterior y herramientas)
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

/* 1) Marcas (se agregan las que falten, por nombre) */
INSERT INTO dbo.Marcas (Nombre)
SELECT m.N FROM (VALUES (N'Bosch'), (N'Brembo'), (N'Philips'), (N'Osram'), (N'Mann-Filter'), (N'Fram'), (N'Genérico')) m(N)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Marcas x WHERE x.Nombre = m.N);
GO

/* 2) Accesorios: 34 productos con ficha técnica propia (TipoAccesorio y UbicacionAccesorio) y existencias.
      TipoVehiculo: Auto, Moto, Pesada (maquinaria pesada) o Agricola (maquinaria agrícola). */
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
(N'Pastillas de freno delanteras Bosch BP1234',N'Accesorios',N'Bosch',N'Juego de pastillas cerámicas de baja emisión de polvo para frenado firme y silencioso.',150000,195000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Frenos',N'Delantero'),
(N'Pastillas de freno traseras Bosch BP5678',N'Accesorios',N'Bosch',N'Juego trasero con excelente resistencia al calor y larga duración.',120000,160000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Frenos',N'Trasero'),
(N'Disco de freno delantero Brembo 280 mm',N'Accesorios',N'Brembo',N'Disco ventilado de alto rendimiento que disipa mejor el calor en frenadas exigentes.',260000,330000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,NULL,NULL,10,30,NULL,NULL,NULL,NULL,N'Frenos',N'Delantero'),
(N'Líquido de frenos Bosch DOT 4 500 ml',N'Accesorios',N'Bosch',N'Líquido de alto punto de ebullición para sistemas con ABS.',22000,32000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Frenos',N'Universal'),
(N'Pastillas de freno moto Brembo delanteras',N'Accesorios',N'Brembo',N'Pastillas sinterizadas para motos de media y alta cilindrada.',70000,95000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Frenos',N'Delantero'),
(N'Pastillas de freno Bosch para maquinaria pesada',N'Accesorios',N'Bosch',N'Pastillas reforzadas para cargadores y montacargas de uso intensivo.',380000,480000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Frenos',N'Universal'),
(N'Filtro de aceite Mann-Filter W 712/95',N'Accesorios',N'Mann-Filter',N'Filtro enroscable de alta capacidad de retención para motores a gasolina y diésel.',28000,39000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Filtro de aire Fram CA10242',N'Accesorios',N'Fram',N'Filtro de aire de motor con medio de alta eficiencia.',35000,48000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Filtro de combustible Bosch para diésel',N'Accesorios',N'Bosch',N'Separa agua y partículas para proteger la bomba y los inyectores.',55000,74000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Filtro de aceite Fram para moto',N'Accesorios',N'Fram',N'Filtro de aceite compacto para motos de 4 tiempos.',18000,26000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Filtro de aire Mann-Filter para tractor',N'Accesorios',N'Mann-Filter',N'Filtro de aire de dos etapas para ambientes con mucho polvo y trabajo en campo.',120000,155000,NULL,NULL,NULL,N'Agricola',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Filtro hidráulico Mann-Filter para maquinaria',N'Accesorios',N'Mann-Filter',N'Elemento filtrante para sistemas hidráulicos de excavadoras y cargadores.',210000,270000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Filtros',N'Universal'),
(N'Bombillos LED H7 Philips Ultinon 6500K (par)',N'Accesorios',N'Philips',N'Luz blanca intensa con mayor alcance y bajo consumo.',120000,155000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,NULL,NULL,15,20,NULL,NULL,NULL,NULL,N'Iluminación',N'Delantero'),
(N'Bombillo halógeno H4 Osram Night Breaker',N'Accesorios',N'Osram',N'Hasta 150 % más de luz y 20 % más de alcance que un bombillo estándar.',45000,62000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Iluminación',N'Delantero'),
(N'Stop LED trasero universal para moto',N'Accesorios',N'Genérico',N'Luz de freno y posición con LED, instalación sencilla.',28000,40000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,3,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Iluminación',N'Trasero'),
(N'Faro de trabajo LED 48 W para maquinaria',N'Accesorios',N'Genérico',N'Faro de haz amplio resistente al agua y a la vibración.',95000,130000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Iluminación',N'Universal'),
(N'Barra LED agrícola 120 W para tractor',N'Accesorios',N'Genérico',N'Barra de luz para labores nocturnas en el campo.',150000,195000,NULL,NULL,NULL,N'Agricola',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Iluminación',N'Delantero'),
(N'Plumillas limpiaparabrisas Bosch Aerotwin (juego)',N'Accesorios',N'Bosch',N'Plumillas planas que barren sin ruido y con presión uniforme.',55000,78000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Limpiaparabrisas',N'Delantero'),
(N'Plumilla limpiaparabrisas trasera Bosch 14 in',N'Accesorios',N'Bosch',N'Plumilla trasera de caucho natural resistente al sol.',18000,28000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Limpiaparabrisas',N'Trasero'),
(N'Casco integral certificado DOT',N'Accesorios',N'Genérico',N'Casco integral con visera antirrayones y ventilación frontal.',140000,190000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,6,NULL,NULL,10,30,NULL,NULL,NULL,NULL,N'Seguridad',N'Universal'),
(N'Kit de carretera (extintor, triángulos y botiquín)',N'Accesorios',N'Genérico',N'Kit completo exigido para circular: extintor, dos triángulos, botiquín y linterna.',65000,95000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Seguridad',N'Universal'),
(N'Cadena antirrobo reforzada para moto',N'Accesorios',N'Genérico',N'Cadena de acero templado con candado de alta seguridad.',45000,65000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,3,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Seguridad',N'Universal'),
(N'Extintor ABC de 10 lb para maquinaria',N'Accesorios',N'Genérico',N'Extintor multipropósito con soporte para cabinas y compartimentos de motor.',70000,98000,NULL,NULL,NULL,N'Pesada',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Seguridad',N'Universal'),
(N'Tapetes de caucho universales (4 piezas)',N'Accesorios',N'Genérico',N'Tapetes antideslizantes y lavables, recortables a la medida.',60000,85000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Interior',N'Universal'),
(N'Cubreasientos de tela universales (juego)',N'Accesorios',N'Genérico',N'Fundas transpirables para asientos delanteros y traseros.',90000,130000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Interior',N'Universal'),
(N'Soporte de celular para moto con carga USB',N'Accesorios',N'Genérico',N'Soporte con agarre firme y puerto USB para cargar en ruta.',25000,38000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,3,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Interior',N'Delantero'),
(N'Cubre carro impermeable talla L',N'Accesorios',N'Genérico',N'Protege del sol, la lluvia y el polvo; con correas de sujeción.',55000,78000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Exterior',N'Universal'),
(N'Barra portaequipajes universal para techo',N'Accesorios',N'Genérico',N'Par de barras de aluminio con soportes regulables y llave antirrobo.',180000,240000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Exterior',N'Universal'),
(N'Protector de tanque para moto',N'Accesorios',N'Genérico',N'Adhesivo transparente que evita rayones por la ropa y la cremallera.',30000,45000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Exterior',N'Universal'),
(N'Gato hidráulico tipo botella 2 toneladas',N'Accesorios',N'Genérico',N'Gato compacto de acero para cambio de llantas en autos pequeños y medianos.',95000,130000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Herramientas',N'Universal'),
(N'Compresor de aire portátil 12 V',N'Accesorios',N'Genérico',N'Infla una llanta en minutos y mide la presión con manómetro digital.',80000,115000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,6,NULL,NULL,8,25,NULL,NULL,NULL,NULL,N'Herramientas',N'Universal'),
(N'Kit de herramientas para moto de 24 piezas',N'Accesorios',N'Genérico',N'Llaves, destornilladores y dados en estuche compacto para llevar bajo el asiento.',40000,58000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Herramientas',N'Universal'),
(N'Engrasadora manual de 500 cc para maquinaria',N'Accesorios',N'Genérico',N'Engrasadora de palanca con manguera flexible y acople universal.',48000,68000,NULL,NULL,NULL,N'Agricola',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Herramientas',N'Universal'),
(N'Cables pasacorriente 600 A',N'Accesorios',N'Genérico',N'Cables de cobre con pinzas aisladas para arrancar vehículos con la batería descargada.',45000,68000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,N'Herramientas',N'Universal');

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

-- Existencias: cada producto nuevo entra con 30 unidades disponibles (mínimo 5).
INSERT INTO dbo.Inventario (IdProducto, Codigo, StockActual, StockMinimo, IdEstado, FechaIngreso, CantidadIngresada, IdBodega)
SELECT pr.Id, N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5), 30, 5, @Disp, CAST(GETDATE() AS date), 30, @Bod
FROM @P p
JOIN dbo.Productos pr ON pr.Nombre = p.Nombre
WHERE @Disp IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i WHERE i.IdProducto = pr.Id)
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i2 WHERE i2.Codigo = N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5));
GO

SELECT COUNT(*) AS AccesoriosEnCatalogo FROM dbo.Productos WHERE Categoria = N'Accesorios';
GO
