/* =====================================================================================================
   LLANDUPAR - DATOS DE PRUEBA (SQL Server)
   Rellena SOLO las tablas que están vacías y agrega más productos al catálogo (llantas, baterías y lubricantes).

   - Es idempotente: se puede ejecutar varias veces; cada bloque revisa antes de insertar.
   - Todas las llaves foráneas se buscan por nombre (no por número), así que sirve con cualquier base ya creada.
   - NO crea cuentas de acceso (Usuarios): las contraseñas se guardan con hash y las crea la aplicación al iniciar
     (administrador, empleado y usuario, ver Privado/usuarios.json). Los bloques que dependen de un usuario
     (PerfilUsuario, LogActividad, RecuperacionCuentas, CanjeIncentivos, EditoresContenido) se llenan si ya existen.
   - Ejecútalo con la base ya creada por la aplicación (EnsureCreated). Usa GO, por lo que va en SSMS / Azure Data Studio / sqlcmd.
   - Cambia el nombre de la base en la línea USE.
   ===================================================================================================== */
-- USE [Llanteria];
-- GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* 0) Columnas nuevas (la app también las crea al iniciar; aquí se repiten por si ejecutas el script antes). */
IF OBJECT_ID(N'dbo.Eventos', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Eventos', N'Fecha') IS NULL
BEGIN
    ALTER TABLE dbo.Eventos ADD Fecha date NULL;
    EXEC(N'UPDATE dbo.Eventos SET Fecha = FechaInicio');
    EXEC(N'ALTER TABLE dbo.Eventos ALTER COLUMN Fecha date NOT NULL');
    EXEC(N'ALTER TABLE dbo.Eventos ADD CONSTRAINT DF_Eventos_Fecha DEFAULT (CONVERT(date, GETDATE())) FOR Fecha');
END;
IF COL_LENGTH(N'dbo.Productos', N'PorcentajeDescuento') IS NULL ALTER TABLE dbo.Productos ADD PorcentajeDescuento decimal(5,2) NULL;
IF COL_LENGTH(N'dbo.Productos', N'DuracionDescuentoDias') IS NULL ALTER TABLE dbo.Productos ADD DuracionDescuentoDias int NULL;
IF COL_LENGTH(N'dbo.Productos', N'FechaInicioDescuento') IS NULL ALTER TABLE dbo.Productos ADD FechaInicioDescuento datetime NULL;
GO

/* 1) Llanterias (la sucursal) */
IF NOT EXISTS (SELECT 1 FROM dbo.Llanterias)
INSERT INTO dbo.Llanterias (Nombre, RazonSocial, Nit, Direccion, Ciudad, Departamento, Telefono, WhatsApp, Correo, HorarioSemana, HorarioSabado, HorarioDomingo,
    Latitud, Longitud, PropietarioNombre, PropietarioDocumento, PropietarioTelefono, PropietarioCorreo,
    OfreceLlantas, OfreceLubricantes, OfreceBaterias, OfreceAccesorios, OfreceMantenimientoFlotas, OfreceServicioADomicilio, ServiciosOfrecidos, ProductosDestacados, Observaciones)
VALUES (N'Llandupar', N'Llandupar S.A.S.', N'900123456-7', N'Valledupar, Cesar', N'Valledupar', N'Cesar', N'+57 311 772 3978', N'573117723978', N'superllantasmedellinsas@hotmail.com',
    N'7:30 a. m. a 5:30 p. m.', N'8:00 a. m. a 4:00 p. m.', N'Cerrado', 10.4631, -73.2532, N'Propietario de prueba', N'1065000000', N'+57 3000000000', N'propietario@example.com',
    1, 1, 1, 1, 1, 1, N'Montaje y balanceo, alineación 3D, mantenimiento express, baterías, suspensión y serviteca móvil.', N'Llantas, baterías y lubricantes de las marcas líderes.', N'Datos de prueba.');
GO

/* 2) Catálogos mínimos (la app también los crea al iniciar) */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
    INSERT INTO dbo.Roles (NombreRol) VALUES (N'Administrador'), (N'Secretario'), (N'Empleado'), (N'Cliente'), (N'Usuario registrado');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoInventario)
    INSERT INTO dbo.EstadoInventario (Nombre) VALUES (N'Disponible'), (N'Agotado'), (N'Dañado');
IF NOT EXISTS (SELECT 1 FROM dbo.CategoriaGasto)
    INSERT INTO dbo.CategoriaGasto (Nombre) VALUES (N'Arriendo'), (N'Servicios públicos'), (N'Nómina'), (N'Proveedores'), (N'Impuestos'), (N'Mantenimiento'), (N'Otros');
-- Sexo, TipoDocumento, Pais, Estados y EstadosFinancieros ya los siembra el modelo (HasData): no están vacías.
GO

/* 3) Bodegas y proveedores */
IF NOT EXISTS (SELECT 1 FROM dbo.Bodegas)
    INSERT INTO dbo.Bodegas (NombreBodega, Ubicacion, CapacidadMaxima, IdLlanteria)
    SELECT v.N, v.U, v.C, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
    FROM (VALUES (N'Bodega principal', N'Valledupar, sede principal', 1500), (N'Bodega de exhibición', N'Valledupar, sala de ventas', 300)) v(N, U, C);

IF NOT EXISTS (SELECT 1 FROM dbo.Proveedores)
    INSERT INTO dbo.Proveedores (NombreEmpresa, Contacto, Telefono) VALUES
    (N'Distribuidora de Llantas del Caribe', N'Sandra Barros', N'+57 300 555 0111'),
    (N'Baterías y Lubricantes del Norte', N'Hernán Daza', N'+57 300 555 0122'),
    (N'Importadora de Neumáticos Andina', N'Paola Vergara', N'+57 300 555 0133');
GO

/* 4) Marcas (se agregan las que falten, por nombre) */
INSERT INTO dbo.Marcas (Nombre)
SELECT m.N FROM (VALUES (N'Michelin'), (N'Bridgestone'), (N'Goodyear'), (N'Pirelli'), (N'Continental'), (N'Hankook'), (N'Yokohama'), (N'Dunlop'), (N'Kumho'), (N'Maxxis'), (N'Bosch'), (N'Varta'), (N'Willard'), (N'MAC'), (N'Yuasa'), (N'Mobil'), (N'Castrol'), (N'Valvoline'), (N'Shell'), (N'Terpel'), (N'Havoline'), (N'Motul')) m(N)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Marcas x WHERE x.Nombre = m.N);
GO

/* 5) MÁS PRODUCTOS para las vistas Llantas, Baterías y Lubricantes (se agregan solo los que no existan, por nombre).
      Las fotos usan default-producto.png: cámbialas desde el panel (Productos > Editar) o copia las imágenes a wwwroot/images/productoos.
      Algunos llevan descuento (porcentaje + duración en días) para ver el círculo rojo en la tarjeta. */
DECLARE @Prov int = (SELECT TOP 1 Id FROM dbo.Proveedores ORDER BY Id);
DECLARE @Bod  int = (SELECT TOP 1 Id FROM dbo.Bodegas ORDER BY Id);
DECLARE @Lla  int = (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);
DECLARE @Disp int = (SELECT TOP 1 Id FROM dbo.EstadoInventario WHERE Nombre = N'Disponible');

DECLARE @P TABLE (Nombre nvarchar(100), Cat nvarchar(50), Marca nvarchar(50), Descr nvarchar(255), Compra decimal(14,2), Venta decimal(14,2),
    Ancho nvarchar(10), Perfil nvarchar(10), Diam nvarchar(10), Veh nvarchar(10), IC nvarchar(10), IV nvarchar(10),
    Visc nvarchar(20), TipoAc nvarchar(30), Gar int, Amp nvarchar(20), Pol nvarchar(20), Dto decimal(5,2), Dias int);
INSERT INTO @P VALUES
(N'Michelin Primacy 4 205/55 R16 91V',N'Llantas',N'Michelin',N'Llanta de turismo con excelente agarre en mojado y larga duración.',410000,520000,N'205',N'55',N'16',N'Auto',N'91',N'V',NULL,NULL,60,NULL,NULL,10,30),
(N'Michelin Pilot Sport 4 225/45 R17 94Y',N'Llantas',N'Michelin',N'Llanta deportiva de alto desempeño para conducción dinámica.',620000,780000,N'225',N'45',N'17',N'Auto',N'94',N'Y',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Bridgestone Turanza T005 205/60 R16 92H',N'Llantas',N'Bridgestone',N'Confort y seguridad en carretera, baja resistencia a la rodadura.',380000,480000,N'205',N'60',N'16',N'Auto',N'92',N'H',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Bridgestone Ecopia EP150 195/65 R15 91H',N'Llantas',N'Bridgestone',N'Ahorro de combustible para uso diario en ciudad.',280000,360000,N'195',N'65',N'15',N'Auto',N'91',N'H',NULL,NULL,60,NULL,NULL,12,20),
(N'Goodyear EfficientGrip 185/65 R15 88H',N'Llantas',N'Goodyear',N'Llanta eficiente y silenciosa para autos compactos.',265000,340000,N'185',N'65',N'15',N'Auto',N'88',N'H',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Goodyear Assurance MaxGuard 215/60 R16 95V',N'Llantas',N'Goodyear',N'Frenado seguro sobre piso mojado y buen kilometraje.',355000,455000,N'215',N'60',N'16',N'Auto',N'95',N'V',NULL,NULL,60,NULL,NULL,15,45),
(N'Pirelli Cinturato P1 175/65 R14 82T',N'Llantas',N'Pirelli',N'Agarre equilibrado y baja resistencia, ideal para ciudad.',225000,290000,N'175',N'65',N'14',N'Auto',N'82',N'T',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Pirelli P Zero 235/40 R18 95Y',N'Llantas',N'Pirelli',N'Ultra alto desempeño para vehículos deportivos.',730000,920000,N'235',N'40',N'18',N'Auto',N'95',N'Y',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Continental ComfortContact CC6 195/55 R16 87H',N'Llantas',N'Continental',N'Comodidad de marcha y baja sonoridad.',335000,430000,N'195',N'55',N'16',N'Auto',N'87',N'H',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Hankook Kinergy Eco2 175/70 R13 82T',N'Llantas',N'Hankook',N'Opción económica y duradera para autos pequeños.',200000,260000,N'175',N'70',N'13',N'Auto',N'82',N'T',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Yokohama BluEarth AE50 205/55 R16 91V',N'Llantas',N'Yokohama',N'Eficiencia y confort con excelente respuesta de dirección.',365000,470000,N'205',N'55',N'16',N'Auto',N'91',N'V',NULL,NULL,60,NULL,NULL,8,30),
(N'Dunlop SP Sport LM705 225/50 R17 94W',N'Llantas',N'Dunlop',N'Deportiva de uso diario con buen agarre en curvas.',395000,510000,N'225',N'50',N'17',N'Auto',N'94',N'W',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Kumho Solus TA31 215/55 R17 94V',N'Llantas',N'Kumho',N'Llanta de turismo con muy buena relación precio-calidad.',385000,495000,N'215',N'55',N'17',N'Auto',N'94',N'V',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Hankook Dynapro HT 265/65 R17 112H',N'Llantas',N'Hankook',N'Para camionetas y SUV: resistente en carretera y trocha.',520000,680000,N'265',N'65',N'17',N'Auto',N'112',N'H',NULL,NULL,60,NULL,NULL,NULL,NULL),
(N'Michelin City Grip 2 90/90-18 Moto',N'Llantas',N'Michelin',N'Llanta para moto urbana, agarre seguro en piso mojado.',150000,195000,N'90',N'90',N'18',N'Moto',N'51',N'P',NULL,NULL,24,NULL,NULL,NULL,NULL),
(N'Pirelli Diablo Rosso III 120/70 ZR17 Moto',N'Llantas',N'Pirelli',N'Llanta delantera deportiva para moto de alto cilindraje.',380000,480000,N'120',N'70',N'17',N'Moto',N'58',N'W',NULL,NULL,24,NULL,NULL,10,30),
(N'Pirelli Diablo Rosso III 180/55 ZR17 Moto',N'Llantas',N'Pirelli',N'Llanta trasera deportiva con gran tracción.',470000,590000,N'180',N'55',N'17',N'Moto',N'73',N'W',NULL,NULL,24,NULL,NULL,NULL,NULL),
(N'Bridgestone Battlax BT-46 110/90-17 Moto',N'Llantas',N'Bridgestone',N'Doble propósito, larga duración para uso diario.',180000,235000,N'110',N'90',N'17',N'Moto',N'60',N'P',NULL,NULL,24,NULL,NULL,NULL,NULL),
(N'Maxxis Victra 80/100-17 Moto',N'Llantas',N'Maxxis',N'Llanta económica para moto de trabajo y ciudad.',95000,125000,N'80',N'100',N'17',N'Moto',N'46',N'P',NULL,NULL,12,NULL,NULL,NULL,NULL),
(N'Dunlop Sportmax D404 130/70-17 Moto',N'Llantas',N'Dunlop',N'Llanta trasera para moto media con buen agarre.',250000,320000,N'130',N'70',N'17',N'Moto',N'62',N'P',NULL,NULL,24,NULL,NULL,5,15),
(N'Batería Bosch S4 12V 45Ah',N'Baterías',N'Bosch',N'Batería sellada de libre mantenimiento para autos pequeños.',230000,295000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,N'45Ah',N'Derecha',NULL,NULL),
(N'Batería Bosch S5 12V 60Ah',N'Baterías',N'Bosch',N'Mayor potencia de arranque para autos medianos.',320000,410000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,N'60Ah',N'Derecha',10,30),
(N'Batería Varta Blue Dynamic 12V 50Ah',N'Baterías',N'Varta',N'Arranque confiable en clima caliente.',260000,335000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,N'50Ah',N'Izquierda',NULL,NULL),
(N'Batería Varta Silver Dynamic 12V 74Ah',N'Baterías',N'Varta',N'Alta capacidad para camionetas y vehículos con muchos accesorios.',390000,495000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,18,N'74Ah',N'Derecha',NULL,NULL),
(N'Batería Willard 36HP 12V 55Ah',N'Baterías',N'Willard',N'Batería robusta de uso general, garantía local.',250000,320000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,N'55Ah',N'Izquierda',NULL,NULL),
(N'Batería MAC Gold 12V 65Ah',N'Baterías',N'MAC',N'Larga vida útil y alta reserva de energía.',300000,385000,NULL,NULL,NULL,N'Auto',NULL,NULL,NULL,NULL,12,N'65Ah',N'Derecha',12,25),
(N'Batería Yuasa YTX7L-BS 12V 6Ah Moto',N'Baterías',N'Yuasa',N'Batería sellada para motos de hasta 250 cc.',85000,115000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,12,N'6Ah',N'Izquierda',NULL,NULL),
(N'Batería Yuasa YTX9-BS 12V 8Ah Moto',N'Baterías',N'Yuasa',N'Para motos de media cilindrada, arranque firme.',110000,145000,NULL,NULL,NULL,N'Moto',NULL,NULL,NULL,NULL,12,N'8Ah',N'Izquierda',NULL,NULL),
(N'Mobil 1 5W-30 Sintético 4 L',N'Aceites',N'Mobil',N'Aceite 100 % sintético para motores modernos, máxima protección.',150000,195000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'5W-30',N'Sintético',12,NULL,NULL,NULL,NULL),
(N'Castrol GTX 20W-50 Mineral 4 L',N'Aceites',N'Castrol',N'Aceite mineral para motores con kilometraje alto.',85000,115000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'20W-50',N'Mineral',6,NULL,NULL,NULL,NULL),
(N'Castrol Magnatec 10W-40 Semisintético 4 L',N'Aceites',N'Castrol',N'Protege el motor desde el arranque en frío.',115000,150000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'10W-40',N'Semisintético',6,NULL,NULL,10,30),
(N'Valvoline MaxLife 10W-40 4 L',N'Aceites',N'Valvoline',N'Formulado para motores de más de 100.000 km.',105000,138000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'10W-40',N'Semisintético',6,NULL,NULL,NULL,NULL),
(N'Shell Helix HX7 10W-40 4 L',N'Aceites',N'Shell',N'Tecnología de limpieza activa para menos depósitos.',110000,142000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'10W-40',N'Semisintético',6,NULL,NULL,NULL,NULL),
(N'Shell Helix Ultra 5W-40 Sintético 4 L',N'Aceites',N'Shell',N'Rendimiento superior para alta exigencia.',165000,215000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'5W-40',N'Sintético',12,NULL,NULL,NULL,NULL),
(N'Terpel Celerity 20W-50 4 L',N'Aceites',N'Terpel',N'Lubricante nacional de excelente relación precio-calidad.',75000,99000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'20W-50',N'Mineral',6,NULL,NULL,8,20),
(N'Havoline ProDS 5W-30 Sintético 4 L',N'Aceites',N'Havoline',N'Sintético para bajo consumo de combustible.',140000,182000,NULL,NULL,NULL,N'Auto',NULL,NULL,N'5W-30',N'Sintético',12,NULL,NULL,NULL,NULL),
(N'Motul 7100 4T 10W-40 Sintético 1 L Moto',N'Aceites',N'Motul',N'Aceite 100 % sintético para motos 4 tiempos.',48000,64000,NULL,NULL,NULL,N'Moto',NULL,NULL,N'10W-40',N'Sintético',6,NULL,NULL,NULL,NULL),
(N'Mobil Super Moto 20W-50 1 L Moto',N'Aceites',N'Mobil',N'Para motos de trabajo y ciudad.',28000,38000,NULL,NULL,NULL,N'Moto',NULL,NULL,N'20W-50',N'Mineral',6,NULL,NULL,NULL,NULL),
(N'Castrol Power1 4T 10W-40 1 L Moto',N'Aceites',N'Castrol',N'Semisintético para motos de media y alta cilindrada.',42000,56000,NULL,NULL,NULL,N'Moto',NULL,NULL,N'10W-40',N'Semisintético',6,NULL,NULL,10,30),
(N'Valvoline 4T 20W-50 1 L Moto',N'Aceites',N'Valvoline',N'Lubricante mineral de uso diario para motos.',26000,35000,NULL,NULL,NULL,N'Moto',NULL,NULL,N'20W-50',N'Mineral',6,NULL,NULL,NULL,NULL);

INSERT INTO dbo.Productos (Nombre, Descripcion, PrecioCompra, PrecioVenta, IdProveedor, Categoria, RutaImagen, IdBodega, IdLlanteria, PorcentajeDescuento, DuracionDescuentoDias, FechaInicioDescuento)
SELECT p.Nombre, p.Descr, p.Compra, p.Venta, @Prov, p.Cat, N'default-producto.png', @Bod, @Lla, p.Dto, p.Dias, CASE WHEN p.Dto IS NOT NULL THEN GETDATE() END
FROM @P p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Productos x WHERE x.Nombre = p.Nombre);

INSERT INTO dbo.DetalleProductos (IdProducto, IdMarca, Ancho, Perfil, Diametro, TipoVehiculo, IndiceCarga, IndiceVelocidad, Viscosidad, TipoAceite, GarantiaMeses, Amperaje, Polaridad)
SELECT pr.Id, m.Id, p.Ancho, p.Perfil, p.Diam, p.Veh, p.IC, p.IV, p.Visc, p.TipoAc, p.Gar, p.Amp, p.Pol
FROM @P p
JOIN dbo.Productos pr ON pr.Nombre = p.Nombre
JOIN dbo.Marcas m ON m.Nombre = p.Marca
WHERE NOT EXISTS (SELECT 1 FROM dbo.DetalleProductos d WHERE d.IdProducto = pr.Id);

-- Existencias: cada producto nuevo entra con unidades disponibles (el catálogo marca "Agotado" si el stock llega a 0).
INSERT INTO dbo.Inventario (IdProducto, Codigo, StockActual, StockMinimo, IdEstado, FechaIngreso, CantidadIngresada, IdBodega)
SELECT pr.Id, N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5), 24, 5, @Disp, CAST(GETDATE() AS date), 24, @Bod
FROM @P p
JOIN dbo.Productos pr ON pr.Nombre = p.Nombre
WHERE @Disp IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i WHERE i.IdProducto = pr.Id)
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventario i2 WHERE i2.Codigo = N'LLD-' + RIGHT(N'00000' + CAST(pr.Id AS nvarchar(10)), 5));
GO

/* 6) Clientes de prueba (se agregan por número de documento; los necesitan reseñas, facturas, citas, vehículos, etc.) */
DECLARE @Doc int = (SELECT TOP 1 Id FROM dbo.TipoDocumento WHERE SiglasPresentacion = N'C.C.');
IF @Doc IS NULL SET @Doc = (SELECT TOP 1 Id FROM dbo.TipoDocumento ORDER BY Id);
INSERT INTO dbo.Clientes (Nombres, Apellidos, IdDocumento, NumeroDocumento, Telefono, Correo, IdSexo, PuntosAcumulados)
SELECT v.N, v.A, @Doc, v.D, v.T, v.C, ISNULL((SELECT TOP 1 Id FROM dbo.Sexo WHERE Nombre = v.S), (SELECT TOP 1 Id FROM dbo.Sexo ORDER BY Id)), 120
FROM (VALUES
    (N'1065100101',N'Carlos',N'Mendoza Pérez',N'Masculino',N'+57 3015550101',N'carlos_mendoza@example.com'),
    (N'1065100102',N'Laura',N'Fonseca Daza',N'Femenino',N'+57 3015550102',N'laura_fonseca@example.com'),
    (N'1065100103',N'Andrés',N'Maestre Ruiz',N'Masculino',N'+57 3015550103',N'andres_maestre@example.com'),
    (N'1065100104',N'Valentina',N'Ochoa Jiménez',N'Femenino',N'+57 3015550104',N'valentina_ochoa@example.com'),
    (N'1065100105',N'Jhon',N'Cuello Arzuaga',N'Masculino',N'+57 3015550105',N'jhon_cuello@example.com'),
    (N'1065100106',N'María',N'Gutiérrez Rojas',N'Femenino',N'+57 3015550106',N'maria_gutierrez@example.com'),
    (N'1065100107',N'Luis',N'Pimienta Mejía',N'Masculino',N'+57 3015550107',N'luis_pimienta@example.com'),
    (N'1065100108',N'Daniela',N'Araújo Brito',N'Femenino',N'+57 3015550108',N'daniela_araujo@example.com')
) v(D, N, A, S, T, C)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Clientes c WHERE c.NumeroDocumento = v.D);
GO

/* 7) Reseñas (para ver el diseño del carrusel del inicio) */
IF NOT EXISTS (SELECT 1 FROM dbo.Resenias)
INSERT INTO dbo.Resenias (IdCliente, Nombre, Calificacion, Comentario, Visible, IdUsuario, Usuario, Fecha)
SELECT c.Id, ISNULL(NULLIF(c.Nombres, N''), v.U), v.Cal, v.Com, 1, NULL, v.U, DATEADD(DAY, -v.Dias, GETDATE())
FROM (VALUES
    (N'1065100101',N'carlos_mendoza',5,N'Cambié las cuatro llantas de mi camioneta y el montaje fue el mismo día. Muy buena atención y precios claros.',2),
    (N'1065100102',N'laura_fonseca',5,N'Excelente servicio de alineación y balanceo, el carro quedó firme en carretera. Totalmente recomendados.',5),
    (N'1065100103',N'andres_maestre',4,N'Buen surtido de baterías y la instalación fue rápida. Solo esperé un poco porque había mucha gente.',9),
    (N'1065100104',N'valentina_ochoa',5,N'Me asesoraron para elegir el aceite correcto de mi motor. Se nota que saben del tema.',14),
    (N'1065100105',N'jhon_cuello',4,N'Compré llantas para mi moto a buen precio y me las montaron en minutos. Volveré.',21),
    (N'1065100106',N'maria_gutierrez',5,N'La serviteca móvil me salvó un domingo con la batería descargada. Llegaron rápido y fueron muy amables.',30),
    (N'1065100107',N'luis_pimienta',3,N'Buen producto y garantía por escrito. Mejoraría la información de disponibilidad en la página.',41),
    (N'1065100108',N'daniela_araujo',5,N'Agendé mi cita desde la página y todo salió como lo pactado. Atención ágil y profesional.',52)
) v(Doc, U, Cal, Com, Dias)
JOIN dbo.Clientes c ON c.NumeroDocumento = v.Doc;
GO

/* 8) Eventos, promociones (Ofertas) e incentivos
      Un evento solo se muestra el día de su Fecha (de 12:00 a. m. a 11:59 p. m.). El primero usa la fecha de HOY para que lo veas en Promociones. */
IF NOT EXISTS (SELECT 1 FROM dbo.Eventos)
INSERT INTO dbo.Eventos (Nombre, Descripcion, Fecha, FechaInicio, FechaFin, Recurrente, IdLlanteria)
SELECT v.N, v.D, v.F, v.F, v.F, v.R, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'Jornada de revisión gratuita (prueba)', N'Revisión de presión y desgaste de llantas sin costo durante todo el día.', CONVERT(date, GETDATE()), 0),
    (N'Navidad', N'Promociones de temporada en llantas y baterías para viajar seguro.', DATEFROMPARTS(YEAR(GETDATE()), 12, 25), 1),
    (N'Año Nuevo', N'Arranca el año con tu carro listo: descuentos en mantenimiento.', DATEFROMPARTS(YEAR(GETDATE()), 1, 1), 1),
    (N'Día del Trabajo', N'Ofertas especiales en lubricantes y servicios.', DATEFROMPARTS(YEAR(GETDATE()), 5, 1), 1)
) v(N, D, F, R);

IF NOT EXISTS (SELECT 1 FROM dbo.Ofertas)
INSERT INTO dbo.Ofertas (Titulo, Descripcion, PorcentajeDescuento, MontoDescuentoFijo, FechaInicio, FechaFin, Estado, CodigoPromocional, IdEvento, IdLlanteria)
SELECT v.T, v.D, v.P, v.M, v.I, v.F, 1, v.C, (SELECT TOP 1 Id FROM dbo.Eventos WHERE Nombre = v.E ORDER BY Id), (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'Bienvenida', N'10 % de descuento en tu primera compra.', 10, 0, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, 120, GETDATE()), N'BIENVENIDA10', NULL),
    (N'Ahorra $20.000', N'Descuento fijo en compras desde $300.000.', 0, 20000, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, 90, GETDATE()), N'AHORRA20K', NULL),
    (N'Promo del evento de hoy', N'15 % de descuento solo durante el evento de hoy.', 15, 0, CAST(CAST(GETDATE() AS date) AS datetime), DATEADD(DAY, 365, GETDATE()), N'EVENTOHOY15', N'Jornada de revisión gratuita (prueba)'),
    (N'Navidad llantera', N'12 % en llantas y baterías el día de Navidad.', 12, 0, CAST(CAST(GETDATE() AS date) AS datetime), DATEADD(DAY, 365, GETDATE()), N'NAVIDAD12', N'Navidad')
) v(T, D, P, M, I, F, C, E);

IF NOT EXISTS (SELECT 1 FROM dbo.CatalogoIncentivos)
INSERT INTO dbo.CatalogoIncentivos (NombrePremio, Descripcion, PuntosRequeridos, StockDisponible) VALUES
    (N'Balanceo gratis', N'Balanceo de las cuatro ruedas sin costo.', 200, 20),
    (N'Cambio de aceite con descuento', N'30 % de descuento en mano de obra del cambio de aceite.', 350, 15),
    (N'Alineación 3D gratis', N'Una alineación 3D sin costo.', 500, 10),
    (N'Bono de $50.000', N'Bono redimible en cualquier compra de la tienda.', 800, 5);

IF NOT EXISTS (SELECT 1 FROM dbo.CanjeIncentivos)
   AND EXISTS (SELECT 1 FROM dbo.Usuarios) AND EXISTS (SELECT 1 FROM dbo.CatalogoIncentivos)
INSERT INTO dbo.CanjeIncentivos (IdCliente, IdIncentivo, EntregadoPor)
SELECT (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100101'),
       (SELECT TOP 1 Id FROM dbo.CatalogoIncentivos ORDER BY PuntosRequeridos),
       (SELECT TOP 1 Id FROM dbo.Usuarios ORDER BY Id)
WHERE EXISTS (SELECT 1 FROM dbo.Clientes WHERE NumeroDocumento = N'1065100101');
GO

/* 9) Tarifas de servicio, vehículos y empleados */
IF NOT EXISTS (SELECT 1 FROM dbo.TipoServicio)
INSERT INTO dbo.TipoServicio (NombreServicio, ValorServicio) VALUES
    (N'Montaje y balanceo', 40000), (N'Alineación 3D', 60000), (N'Mantenimiento express', 120000),
    (N'Instalación de batería', 0), (N'Diagnóstico de suspensión', 45000), (N'Serviteca móvil (desplazamiento)', 35000), (N'Rotación de llantas', 30000);

IF NOT EXISTS (SELECT 1 FROM dbo.Vehiculos)
INSERT INTO dbo.Vehiculos (Placa, Marca, Modelo, Anio, Kilometraje, IdCliente)
SELECT v.P, v.M, v.Mo, v.A, v.K, c.Id
FROM (VALUES
    (N'KJH123', N'Toyota', N'Hilux', 2021, 48000, N'1065100101'),
    (N'LPQ456', N'Chevrolet', N'Onix', 2020, 62000, N'1065100102'),
    (N'MNB789', N'Renault', N'Duster', 2019, 85000, N'1065100103'),
    (N'ABC12D', N'Yamaha', N'FZ 150', 2022, 15000, N'1065100105'),
    (N'NRT321', N'Mazda', N'3', 2018, 97000, N'1065100106')
) v(P, M, Mo, A, K, D)
JOIN dbo.Clientes c ON c.NumeroDocumento = v.D;

IF NOT EXISTS (SELECT 1 FROM dbo.Empleados)
INSERT INTO dbo.Empleados (Nombres, Apellidos, IdDocumento, NumeroDocumento, Correo, Telefono, Salario, Horario, IdSexo, FechaNacimiento, IdRol, IdLlanteria)
SELECT v.N, v.A, (SELECT TOP 1 Id FROM dbo.TipoDocumento ORDER BY Id), v.D, v.C, N'+57 3000000000', v.S, N'Lunes a viernes',
       (SELECT TOP 1 Id FROM dbo.Sexo WHERE Nombre = v.Sx), v.F, (SELECT TOP 1 Id FROM dbo.Roles WHERE NombreRol = N'Empleado'), (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'Santiago', N'Barros Cuello', N'1065200201', N'santiago.barros@example.com', 1800000, N'Masculino', CAST('1992-04-12' AS date)),
    (N'Karen', N'Peñaloza Mora', N'1065200202', N'karen.penaloza@example.com', 1800000, N'Femenino', CAST('1995-09-03' AS date))
) v(N, A, D, C, S, Sx, F)
WHERE EXISTS (SELECT 1 FROM dbo.Roles WHERE NombreRol = N'Empleado');
GO

/* 10) Ventas: facturas con sus detalles (IVA 19 %) */
IF NOT EXISTS (SELECT 1 FROM dbo.Facturas)
BEGIN
    DECLARE @Lla int = (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);
    DECLARE @F1 int, @F2 int, @F3 int;
    DECLARE @C1 int = (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100101');
    DECLARE @C2 int = (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100102');
    DECLARE @C3 int = (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100104');
    DECLARE @PrLlanta int = (SELECT TOP 1 Id FROM dbo.Productos WHERE Nombre = N'Michelin Primacy 4 205/55 R16 91V');
    DECLARE @PrBat int = (SELECT TOP 1 Id FROM dbo.Productos WHERE Nombre = N'Batería Bosch S4 12V 45Ah');
    DECLARE @PrAceite int = (SELECT TOP 1 Id FROM dbo.Productos WHERE Nombre = N'Mobil 1 5W-30 Sintético 4 L');
    DECLARE @SvAli int = (SELECT TOP 1 Id FROM dbo.TipoServicio WHERE NombreServicio = N'Alineación 3D');
    DECLARE @SvMon int = (SELECT TOP 1 Id FROM dbo.TipoServicio WHERE NombreServicio = N'Montaje y balanceo');

    IF @C1 IS NOT NULL AND @C2 IS NOT NULL AND @C3 IS NOT NULL
    BEGIN
        -- Factura 1: 4 llantas + montaje y balanceo
        INSERT INTO dbo.Facturas (Fecha, IdCliente, TotalBruto, IVA, RteFte, TotalPagar, Observaciones, IdEstado, IdLlanteria)
        VALUES (DATEADD(DAY, -20, CAST(GETDATE() AS date)), @C1, 2240000, 425600, 0, 2665600, N'Venta de prueba', 3, @Lla);
        SET @F1 = SCOPE_IDENTITY();
        INSERT INTO dbo.DetalleFactura (IdFactura, IdProducto, IdServicio, CodigoItem, [Descripción], Cantidad, ValorUnitario, Subtotal, IdEstado) VALUES
            (@F1, @PrLlanta, NULL, N'P-LLANTA', N'Michelin Primacy 4 205/55 R16 91V', 4, 520000, 2080000, 3),
            (@F1, NULL, @SvMon, N'S-MONTAJE', N'Montaje y balanceo', 4, 40000, 160000, 3);

        -- Factura 2: batería
        INSERT INTO dbo.Facturas (Fecha, IdCliente, TotalBruto, IVA, RteFte, TotalPagar, Observaciones, IdEstado, IdLlanteria)
        VALUES (DATEADD(DAY, -9, CAST(GETDATE() AS date)), @C2, 295000, 56050, 0, 351050, N'Venta de prueba', 3, @Lla);
        SET @F2 = SCOPE_IDENTITY();
        INSERT INTO dbo.DetalleFactura (IdFactura, IdProducto, IdServicio, CodigoItem, [Descripción], Cantidad, ValorUnitario, Subtotal, IdEstado) VALUES
            (@F2, @PrBat, NULL, N'P-BATERIA', N'Batería Bosch S4 12V 45Ah', 1, 295000, 295000, 3);

        -- Factura 3: aceite + alineación
        INSERT INTO dbo.Facturas (Fecha, IdCliente, TotalBruto, IVA, RteFte, TotalPagar, Observaciones, IdEstado, IdLlanteria)
        VALUES (CAST(GETDATE() AS date), @C3, 255000, 48450, 0, 303450, N'Venta de prueba', 3, @Lla);
        SET @F3 = SCOPE_IDENTITY();
        INSERT INTO dbo.DetalleFactura (IdFactura, IdProducto, IdServicio, CodigoItem, [Descripción], Cantidad, ValorUnitario, Subtotal, IdEstado) VALUES
            (@F3, @PrAceite, NULL, N'P-ACEITE', N'Mobil 1 5W-30 Sintético 4 L', 1, 195000, 195000, 3),
            (@F3, NULL, @SvAli, N'S-ALINEACION', N'Alineación 3D', 1, 60000, 60000, 3);
    END
END;

/* Gastos */
IF NOT EXISTS (SELECT 1 FROM dbo.Gastos)
INSERT INTO dbo.Gastos (IdCategoria, Descripcion, Monto, FechaRegistro, FechaVencimiento, FechaPago, Estado, IdEstado, IdLlanteria)
SELECT (SELECT TOP 1 Id FROM dbo.CategoriaGasto WHERE Nombre = v.C), v.D, v.M, DATEADD(DAY, -v.Dias, GETDATE()), DATEADD(DAY, -v.Dias + 5, CAST(GETDATE() AS date)),
       CASE WHEN v.E = N'Pagado' THEN DATEADD(DAY, -v.Dias + 2, CAST(GETDATE() AS date)) END, v.E, 3, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'Arriendo', N'Arriendo del local', 2500000, 25, N'Pagado'),
    (N'Servicios públicos', N'Energía y agua', 640000, 15, N'Pagado'),
    (N'Proveedores', N'Compra de llantas al distribuidor', 4800000, 8, N'Pagado'),
    (N'Mantenimiento', N'Mantenimiento de la balanceadora', 350000, 3, N'Pendiente')
) v(C, D, M, Dias, E)
WHERE EXISTS (SELECT 1 FROM dbo.CategoriaGasto WHERE Nombre = v.C);
GO

/* 11) Citas, órdenes de trabajo, pedidos, PQRS, solicitudes de empresas y sugerencias */
IF NOT EXISTS (SELECT 1 FROM dbo.Citas)
INSERT INTO dbo.Citas (Slug, Servicio, Fecha, Hora, Vehiculo, Direccion, FormaPago, Notas, Solicitante, Telefono, Correo, IdCliente, Estado, RecordatorioEnviado, IdLlanteria)
SELECT v.Sl, v.Sv, DATEADD(DAY, v.D, CAST(GETDATE() AS date)), v.H, v.Ve, v.Di, v.Fp, v.No, c.Nombres + N' ' + c.Apellidos, c.Telefono, c.Correo, c.Id, v.Es, 0, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'montaje-balanceo', N'Montaje y balanceo', 2, N'09:00', N'KJH123 / Toyota Hilux', NULL, N'Efectivo', N'Cuatro llantas nuevas.', N'Confirmada', N'1065100101'),
    (N'alineacion-3d', N'Alineación 3D', 3, N'10:30', N'LPQ456 / Chevrolet Onix', NULL, N'Transferencia', NULL, N'Pendiente', N'1065100102'),
    (N'serviteca-movil', N'Serviteca móvil', 1, N'15:00', N'MNB789 / Renault Duster', N'Calle 16 # 9-45, barrio Centro', N'Efectivo', N'Batería descargada.', N'Pendiente', N'1065100103'),
    (N'mantenimiento-express', N'Mantenimiento express', -5, N'08:30', N'NRT321 / Mazda 3', NULL, N'Efectivo', NULL, N'Completada', N'1065100106')
) v(Sl, Sv, D, H, Ve, Di, Fp, No, Es, Doc)
JOIN dbo.Clientes c ON c.NumeroDocumento = v.Doc;

IF NOT EXISTS (SELECT 1 FROM dbo.OrdenesTrabajo)
INSERT INTO dbo.OrdenesTrabajo (Numero, IdVehiculo, IdEmpleado, Tipo, Descripcion, Kilometraje, Observaciones, Total, Estado, FechaIngreso, FechaEntrega, ProximaFecha, ProximoKm, RecordatorioEnviado, IdEstado)
SELECT v.Nu, ve.Id, (SELECT TOP 1 Id FROM dbo.Empleados ORDER BY Id), v.Ti, v.De, ve.Kilometraje, v.Ob, v.To, v.Es, DATEADD(DAY, -v.Dias, GETDATE()),
       CASE WHEN v.Es = N'Entregado' THEN DATEADD(DAY, -v.Dias, GETDATE()) END, DATEADD(DAY, 180 - v.Dias, GETDATE()), NULL, 0, CASE WHEN v.Es = N'Entregado' THEN 3 ELSE 2 END
FROM (VALUES
    (N'OT-0001', N'KJH123', N'Cambio de llantas', N'Cambio de 4 llantas y balanceo', N'Se entregó con la alineación revisada.', 2400000, N'Entregado', 20),
    (N'OT-0002', N'LPQ456', N'Alineación y balanceo', N'Alineación 3D y balanceo', NULL, 100000, N'En proceso', 1),
    (N'OT-0003', N'NRT321', N'Cambio de aceite y filtro', N'Aceite sintético 5W-30 y filtro', NULL, 195000, N'Recibido', 0)
) v(Nu, Pl, Ti, De, Ob, To, Es, Dias)
JOIN dbo.Vehiculos ve ON ve.Placa = v.Pl;

IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos)
BEGIN
    DECLARE @Lla2 int = (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);
    DECLARE @C1 int = (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100104');
    DECLARE @C2 int = (SELECT TOP 1 Id FROM dbo.Clientes WHERE NumeroDocumento = N'1065100107');
    DECLARE @PA int = (SELECT TOP 1 Id FROM dbo.Productos WHERE Nombre = N'Goodyear EfficientGrip 185/65 R15 88H');
    DECLARE @PB int = (SELECT TOP 1 Id FROM dbo.Productos WHERE Nombre = N'Castrol Magnatec 10W-40 Semisintético 4 L');
    DECLARE @Ped1 int, @Ped2 int;
    IF @C1 IS NOT NULL AND @C2 IS NOT NULL
    BEGIN
        INSERT INTO dbo.Pedidos (Numero, Token, IdCliente, Nombres, Apellidos, TipoDocumento, NumeroDocumento, Correo, Telefono, Pais, Departamento, Ciudad, Direccion, Complemento, CodigoPostal, Notas, MetodoPago, Estado, CodigoCupon,
                                 Subtotal, Descuento, BaseGravable, Iva, Envio, Total, Fecha, IdEstado, IdLlanteria)
        VALUES (N'PED-0001', LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'')), @C1, N'Valentina', N'Ochoa Jiménez', N'C.C.', N'1065100104', N'valentina_ochoa@example.com', N'+57 3015550104',
                N'Colombia', N'Cesar', N'Valledupar', N'Carrera 9 # 16-20', NULL, N'200001', N'Entregar en horas de la tarde.', N'Efectivo', N'Pendiente', NULL,
                1020000, 0, 857143, 162857, 0, 1020000, DATEADD(DAY, -2, GETDATE()), 2, @Lla2);
        SET @Ped1 = SCOPE_IDENTITY();
        INSERT INTO dbo.PedidoItems (IdPedido, IdProducto, Descripcion, Cantidad, ValorUnitario, Subtotal, IdEstado) VALUES
            (@Ped1, @PA, N'Goodyear EfficientGrip 185/65 R15 88H', 3, 340000, 1020000, 2);

        INSERT INTO dbo.Pedidos (Numero, Token, IdCliente, Nombres, Apellidos, TipoDocumento, NumeroDocumento, Correo, Telefono, Pais, Departamento, Ciudad, Direccion, Complemento, CodigoPostal, Notas, MetodoPago, Estado, CodigoCupon,
                                 Subtotal, Descuento, BaseGravable, Iva, Envio, Total, Fecha, IdEstado, IdLlanteria)
        VALUES (N'PED-0002', LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'')), @C2, N'Luis', N'Pimienta Mejía', N'C.C.', N'1065100107', N'luis_pimienta@example.com', N'+57 3015550107',
                N'Colombia', N'Cesar', N'Valledupar', N'Calle 12 # 5-30', N'Apto 301', N'200001', NULL, N'Transferencia', N'Pagado', N'BIENVENIDA10',
                300000, 30000, 226891, 43109, 0, 270000, DATEADD(DAY, -6, GETDATE()), 3, @Lla2);
        SET @Ped2 = SCOPE_IDENTITY();
        INSERT INTO dbo.PedidoItems (IdPedido, IdProducto, Descripcion, Cantidad, ValorUnitario, Subtotal, IdEstado) VALUES
            (@Ped2, @PB, N'Castrol Magnatec 10W-40 Semisintético 4 L', 2, 150000, 300000, 3);
    END
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Pqrs)
INSERT INTO dbo.Pqrs (Radicado, Tipo, Nombre, NumeroDocumento, Correo, Telefono, Asunto, Mensaje, AutorizaDatos, IdCliente, Estado, Respuesta, FechaCreacion, FechaRespuesta)
SELECT v.R, v.T, c.Nombres + N' ' + c.Apellidos, c.NumeroDocumento, c.Correo, c.Telefono, v.A, v.M, 1, c.Id, v.E, v.Re,
       DATEADD(DAY, -v.D, GETDATE()), CASE WHEN v.Re IS NOT NULL THEN DATEADD(DAY, -v.D + 1, GETDATE()) END
FROM (VALUES
    (N'PQRS-0001', N'Petición', N'Factura electrónica', N'Solicito copia de mi factura del mes pasado.', N'Resuelta', N'Enviamos la factura a tu correo registrado.', 12, N'1065100101'),
    (N'PQRS-0002', N'Queja', N'Demora en la atención', N'Esperé más de una hora para el montaje de mis llantas.', N'En trámite', NULL, 4, N'1065100103'),
    (N'PQRS-0003', N'Sugerencia', N'Más medios de pago', N'Sería bueno poder pagar con tarjeta desde la página.', N'Recibida', NULL, 1, N'1065100108')
) v(R, T, A, M, E, Re, D, Doc)
JOIN dbo.Clientes c ON c.NumeroDocumento = v.Doc;

IF NOT EXISTS (SELECT 1 FROM dbo.SolicitudesEmpresa)
INSERT INTO dbo.SolicitudesEmpresa (Empresa, Nit, Contacto, Telefono, Correo, Ciudad, TipoFlota, NumeroVehiculos, Mensaje, Estado) VALUES
    (N'Transportes del Cesar S.A.S.', N'900456789-1', N'Ricardo Pacheco', N'+57 300 555 0201', N'compras@transcesar.example.com', N'Valledupar', N'Camiones', 18, N'Necesitamos cotización de llantas para la flota y mantenimiento mensual.', N'Pendiente'),
    (N'Mensajería Rápida Valle', N'901234567-8', N'Sofía Naranjo', N'+57 300 555 0202', N'sofia@mensajeriavalle.example.com', N'Valledupar', N'Motocicletas', 25, N'Buscamos llantas y baterías para 25 motos de reparto.', N'Contactada');

IF NOT EXISTS (SELECT 1 FROM dbo.Sugerencias)
INSERT INTO dbo.Sugerencias (Area, Calificacion, Mensaje, Nombre, Correo, IdUsuario, Estado) VALUES
    (N'Catálogo y precios', 4, N'Agregar fotos reales de cada llanta ayudaría a decidir.', N'Carlos Mendoza', N'carlos_mendoza@example.com', NULL, N'Nueva'),
    (N'Facilidad de uso', 5, N'El buscador por marca está muy cómodo.', N'Laura Fonseca', N'laura_fonseca@example.com', NULL, N'Revisada'),
    (N'Servicios y citas', 4, N'Me gustaría recibir un recordatorio por WhatsApp de mi cita.', NULL, NULL, NULL, N'Nueva');
GO

/* 12) Servicios del sitio (las tarjetas de servicios del inicio y sus páginas) */
IF NOT EXISTS (SELECT 1 FROM dbo.ServiciosWeb)
INSERT INTO dbo.ServiciosWeb (Slug, Titulo, Subtitulo, Icono, Descripcion, Ventajas, InfoCita, EtiquetaPrecio, Precio, EsDomicilio, Publicado, Orden, IdLlanteria)
SELECT v.Sl, v.T, v.St, v.Ic, v.D, v.Ve, v.Inf, v.El, v.Pr, v.Dom, 1, v.O, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'montaje-balanceo',N'Montaje y balanceo',N'Precisión computarizada para una conducción suave',N'fa-compact-disc',N'El montaje profesional garantiza el acople perfecto entre la llanta y el rin sin dañar las pestañas ni los sensores. Con balanceo computarizado distribuimos el peso de forma equitativa para una rodadura suave.',N'fa-car-side | Conducción suave | Elimina vibraciones en el volante y el pedal.' + NCHAR(10) + N'fa-arrows-spin | Desgaste uniforme | Evita deformaciones en la banda de rodamiento.',N'Tiempo estimado | 45 a 60 minutos.' + NCHAR(10) + N'Garantía | 30 días o 1.000 km de alineación/balanceo.',N'Total estimado:',N'$40.000 COP',0,1),
    (N'alineacion-3d',N'Alineación 3D',N'Ajuste técnico milimétrico con tecnología óptica',N'fa-arrows-to-dot',N'Usamos sensores ópticos de alta definición para ajustar los ángulos de las ruedas (camber, caster y toe) según las especificaciones del fabricante.',N'fa-compass | Trayectoria recta | Evita que el vehículo se desvíe en recta.' + NCHAR(10) + N'fa-leaf | Ahorro de combustible | Reduce la fricción innecesaria del neumático.',N'Tiempo estimado | 30 a 45 minutos.' + NCHAR(10) + N'Garantía | 30 días o 1.000 km de alineación técnica.',N'Total estimado:',N'$60.000 COP',0,2),
    (N'mantenimiento-express',N'Mantenimiento express',N'Cambio de aceite, filtros y revisión de puntos de seguridad',N'fa-oil-can',N'Servicio rápido para mantener la vida útil de tu motor: aceite homologado, filtro de aceite, filtro de aire e inspección preventiva de 15 puntos.',N'fa-shield-halved | Insumos homologados | Aceites y filtros de calidad garantizada.' + NCHAR(10) + N'fa-bolt | Servicio rápido | Atención express de menos de 45 minutos.',N'Tiempo estimado | 30 a 45 minutos.' + NCHAR(10) + N'Incluye | Revisión de 15 puntos de seguridad.',N'Total estimado:',N'$120.000 COP',0,3),
    (N'baterias',N'Venta e instalación de baterías',N'Diagnóstico de carga e instalación profesional con garantía',N'fa-car-battery',N'Revisamos el estado de la batería y del sistema de carga (alternador y motor de arranque). Baterías selladas y de libre mantenimiento de las marcas líderes.',N'fa-award | Garantía escrita | Respaldo directo ante cualquier eventualidad.' + NCHAR(10) + N'fa-screwdriver-wrench | Instalación gratis | Montaje y comprobación de voltaje sin costo.',N'Tiempo de instalación | 15 a 20 minutos.' + NCHAR(10) + N'Batería usada | Recibimos la anterior y te damos descuento de retoma.',N'Total estimado:',N'$240.000 COP',0,4),
    (N'suspension-amortiguadores',N'Suspensión y amortiguadores',N'Diagnóstico técnico para garantizar estabilidad y confort',N'fa-truck-monster',N'Inspeccionamos amortiguadores, espirales, bujes, terminales, tijeras y rótulas para asegurar el agarre del vehículo y prevenir el desgaste irregular de las llantas.',N'fa-shield-cat | Seguridad en curvas | Mantiene el vehículo firme y mejora la adherencia.' + NCHAR(10) + N'fa-road | Absorción de impactos | Filtra las irregularidades del camino.',N'Tiempo de diagnóstico | 45 a 60 minutos.' + NCHAR(10) + N'Garantía | Repuestos homologados con respaldo en mano de obra.',N'Total estimado:',N'$60.000 COP',0,5),
    (N'serviteca-movil',N'Serviteca móvil',N'Atención especializada a domicilio en Valledupar',N'fa-truck-pickup',N'Llevamos nuestro taller móvil hasta tu casa u oficina en Valledupar: montaje, balanceo, despinche, recarga y cambio de baterías sin que tengas que moverte.',N'fa-house | Comodidad total | Atención en la puerta de tu hogar o trabajo.' + NCHAR(10) + N'fa-clock | Ahorro de tiempo | Sigues con tus actividades mientras te atendemos.',N'Zona de cobertura | Toda la zona urbana de Valledupar.' + NCHAR(10) + N'Requisito de espacio | Terreno plano y acceso libre.',N'Desplazamiento desde:',N'$35.000 COP',1,6)
) v(Sl, T, St, Ic, D, Ve, Inf, El, Pr, Dom, O);
GO

/* 13) Artículos del blog (con secciones numeradas con "## "; la tabla de contenido las numera sin repetir) */
IF NOT EXISTS (SELECT 1 FROM dbo.Articulos)
INSERT INTO dbo.Articulos (Titulo, Slug, Resumen, Categoria, Fecha, Autor, Imagen, Contenido, Publicado, IdLlanteria)
SELECT v.T, v.Sl, v.R, v.C, DATEADD(DAY, v.D, CAST(GETDATE() AS date)), N'Equipo técnico Llandupar', v.Im, v.Co, 1, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'cuando-cambiar-las-llantas',N'Cuándo cambiar las llantas',N'Señales claras de que tus llantas ya cumplieron su ciclo.',N'Rueda seguro',N'cuando-cambiar-las-llantas.svg',-30,N'Una llanta gastada es una de las principales causas de accidentes en carretera mojada. Estas señales te ayudan a decidir a tiempo.' + NCHAR(10) + N'' + NCHAR(10) + N'## Revisa la profundidad del dibujo' + NCHAR(10) + N'' + NCHAR(10) + N'Si la banda está cerca del indicador de desgaste, cambia las llantas.' + NCHAR(10) + N'' + NCHAR(10) + N'## Busca grietas y bultos' + NCHAR(10) + N'' + NCHAR(10) + N'Las grietas en los costados o los bultos indican daños internos. No esperes a que fallen.' + NCHAR(10) + N'' + NCHAR(10) + N'## Considera la edad' + NCHAR(10) + N'' + NCHAR(10) + N'Aunque parezcan buenas, una llanta de más de seis años pierde agarre.'),
    (N'presion-de-las-llantas',N'Presión de las llantas: la revisión de dos minutos que te ahorra dinero',N'Mantener la presión correcta alarga la vida de la llanta y baja el consumo.',N'Tips de compra',N'presion-de-las-llantas.svg',-18,N'Una presión baja desgasta los bordes y gasta más combustible. Revisarla toma dos minutos.' + NCHAR(10) + N'' + NCHAR(10) + N'## Dónde está la presión correcta' + NCHAR(10) + N'' + NCHAR(10) + N'La recomendada por el fabricante está en el marco de la puerta o en el manual, no en el costado de la llanta.' + NCHAR(10) + N'' + NCHAR(10) + N'## Cuándo medirla' + NCHAR(10) + N'' + NCHAR(10) + N'Con las llantas frías, al menos una vez al mes y antes de viajes largos.' + NCHAR(10) + N'' + NCHAR(10) + N'## Qué pasa si te pasas' + NCHAR(10) + N'' + NCHAR(10) + N'Demasiada presión reduce el agarre y desgasta el centro de la banda.'),
    (N'temporada-de-lluvias-prepara-tu-carro',N'Temporada de lluvias en el Cesar: prepara tu carro para manejar con seguridad',N'Llantas, frenos, plumillas y luces: la revisión que conviene hacer antes de que lleguen los aguaceros.',N'Actualidad',N'temporada-de-lluvias-prepara-tu-carro.svg',-10,N'Con la llegada de las lluvias cambian las condiciones de la vía: el pavimento se vuelve resbaloso y la visibilidad baja. Un poco de preparación hace una gran diferencia.' + NCHAR(10) + N'' + NCHAR(10) + N'## Revisa el dibujo de tus llantas' + NCHAR(10) + N'' + NCHAR(10) + N'En piso mojado, el dibujo es lo único que evacúa el agua entre la llanta y el pavimento.' + NCHAR(10) + N'' + NCHAR(10) + N'## Frenos en buen estado' + NCHAR(10) + N'' + NCHAR(10) + N'Con el piso mojado la distancia de frenado se alarga. Si tus frenos chillan o el pedal se siente esponjoso, revísalos.' + NCHAR(10) + N'' + NCHAR(10) + N'## Limpiabrisas y luces' + NCHAR(10) + N'' + NCHAR(10) + N'Cambia las plumillas resecas y verifica que funcionen todas las luces.' + NCHAR(10) + N'' + NCHAR(10) + N'## Batería y sistema eléctrico' + NCHAR(10) + N'' + NCHAR(10) + N'La humedad exige más a la batería: revisa bornes y carga.' + NCHAR(10) + N'' + NCHAR(10) + N'## Consejos para manejar bajo la lluvia' + NCHAR(10) + N'' + NCHAR(10) + N'Baja la velocidad, aumenta la distancia y evita frenar de golpe.')
) v(Sl, T, R, C, Im, D, Co);
GO

/* 14) Tablas que dependen de usuarios con sesión (se llenan solo si la app ya creó los usuarios iniciales) */
IF NOT EXISTS (SELECT 1 FROM dbo.PerfilUsuario) AND EXISTS (SELECT 1 FROM dbo.Usuarios)
INSERT INTO dbo.PerfilUsuario (IdUsuario, TemaPreferencia, NotificacionesActivas)
SELECT Id, N'Light', 1 FROM dbo.Usuarios;

IF NOT EXISTS (SELECT 1 FROM dbo.LogActividad) AND EXISTS (SELECT 1 FROM dbo.Usuarios)
INSERT INTO dbo.LogActividad (IdUsuario, Accion, TablaAfectada, FechaRegistro, DireccionIP)
SELECT u.Id, v.A, v.T, DATEADD(HOUR, -v.H, GETDATE()), N'127.0.0.1'
FROM (SELECT TOP 1 Id FROM dbo.Usuarios ORDER BY Id) u
CROSS JOIN (VALUES (N'Inicio de sesión', N'Usuarios', 30), (N'Creó un producto', N'Productos', 20), (N'Registró una factura', N'Facturas', 5)) v(A, T, H);

IF NOT EXISTS (SELECT 1 FROM dbo.RecuperacionCuentas) AND EXISTS (SELECT 1 FROM dbo.Usuarios)
INSERT INTO dbo.RecuperacionCuentas (IdUsuario, TokenRecuperacion, FechaExpiracion, Usado)
SELECT TOP 1 Id, N'DEMO-' + LEFT(REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N''), 24), DATEADD(DAY, -3, GETDATE()), 1 FROM dbo.Usuarios ORDER BY Id DESC;

IF NOT EXISTS (SELECT 1 FROM dbo.EditoresContenido)
   AND EXISTS (SELECT 1 FROM dbo.Usuarios u JOIN dbo.Roles r ON r.Id = u.IdRol WHERE r.NombreRol = N'Administrador')
   AND EXISTS (SELECT 1 FROM dbo.Usuarios u JOIN dbo.Roles r ON r.Id = u.IdRol WHERE r.NombreRol = N'Empleado')
INSERT INTO dbo.EditoresContenido (IdUsuario, Recurso, IdInvitadoPor, Fecha)
SELECT (SELECT TOP 1 u.Id FROM dbo.Usuarios u JOIN dbo.Roles r ON r.Id = u.IdRol WHERE r.NombreRol = N'Empleado' ORDER BY u.Id), N'Blog',
       (SELECT TOP 1 u.Id FROM dbo.Usuarios u JOIN dbo.Roles r ON r.Id = u.IdRol WHERE r.NombreRol = N'Administrador' ORDER BY u.Id), GETDATE();
GO

PRINT N'Datos de prueba cargados: se llenaron solo las tablas que estaban vacías y se agregaron los productos que faltaban.';
GO

