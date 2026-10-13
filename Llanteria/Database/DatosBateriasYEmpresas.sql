/* ============================================================================
   Llantería - COMPLEMENTO: baterías y solicitudes de empresas  (base LlanteriaDB)
   Ejecutar DESPUÉS de DatosDePrueba.sql.
   - Agrega 5 marcas de baterías y 9 baterías (con la que ya existía suman 10) con su detalle e inventario,
     para que funcionen el filtro por marca y por precio de la página /Baterias.
   - Crea la tabla SolicitudesEmpresa si todavía no existe (la app también la crea al arrancar) y carga 10 solicitudes.
   - Sin imágenes.
   ============================================================================ */
USE [LlanteriaDB]
GO
SET NOCOUNT ON
GO
SET IDENTITY_INSERT [dbo].[Marcas] ON;
INSERT [dbo].[Marcas] ([Id],[Nombre]) VALUES (11, N'Varta');
INSERT [dbo].[Marcas] ([Id],[Nombre]) VALUES (12, N'Bosch');
INSERT [dbo].[Marcas] ([Id],[Nombre]) VALUES (13, N'Willard');
INSERT [dbo].[Marcas] ([Id],[Nombre]) VALUES (14, N'Exide');
INSERT [dbo].[Marcas] ([Id],[Nombre]) VALUES (15, N'Optima');
SET IDENTITY_INSERT [dbo].[Marcas] OFF;
GO
SET IDENTITY_INSERT [dbo].[Productos] ON;
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (13, N'Varta Blue Dynamic 12V 60Ah', N'Batería libre de mantenimiento para carros pequeños y medianos.', 310000, 395000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (14, N'Varta Silver Dynamic 12V 74Ah', N'Mayor capacidad de arranque, ideal para clima caliente.', 380000, 485000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (15, N'Bosch S4 12V 70Ah', N'Batería de uso general con bornes estándar.', 340000, 435000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (16, N'Bosch S5 12V 80Ah', N'Alta potencia para camionetas y SUV.', 420000, 540000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (17, N'Willard Titanio 12V 650A', N'Batería de uso diario, caja 34.', 300000, 385000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (18, N'Willard Extreme 12V 800A', N'Pensada para vehículos con muchos accesorios eléctricos.', 385000, 495000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (19, N'Exide Start-Stop EFB 12V 70Ah', N'Específica para vehículos con sistema start-stop.', 520000, 665000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (20, N'Exide Classic 12V 45Ah', N'Económica, para motores de baja cilindrada.', 215000, 279000, 4, N'Baterías', NULL, 8);
INSERT [dbo].[Productos] ([Id],[Nombre],[Descripcion],[PrecioCompra],[PrecioVenta],[IdProveedor],[Categoria],[RutaImagen],[IdBodega]) VALUES (21, N'Optima RedTop 12V 50Ah', N'Tecnología AGM de alto rendimiento, muy resistente a vibraciones.', 610000, 780000, 4, N'Baterías', NULL, 8);
SET IDENTITY_INSERT [dbo].[Productos] OFF;
GO
SET IDENTITY_INSERT [dbo].[DetalleProductos] ON;
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (12, 12, 13, 18);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (13, 13, 11, 18);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (14, 14, 11, 24);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (15, 15, 12, 18);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (16, 16, 12, 24);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (17, 17, 13, 18);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (18, 18, 13, 24);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (19, 19, 14, 24);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (20, 20, 14, 12);
INSERT [dbo].[DetalleProductos] ([Id],[IdProducto],[IdMarca],[GarantiaMeses]) VALUES (21, 21, 15, 36);
SET IDENTITY_INSERT [dbo].[DetalleProductos] OFF;
GO
SET IDENTITY_INSERT [dbo].[Inventario] ON;
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (16, 13, N'BA-VAR-60', 9, 3, 1, '2026-09-15', 9, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (17, 14, N'BA-VAR-74', 7, 3, 1, '2026-09-15', 7, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (18, 15, N'BA-BOS-70', 10, 3, 1, '2026-09-15', 10, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (19, 16, N'BA-BOS-80', 6, 3, 1, '2026-09-15', 6, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (20, 17, N'BA-WIL-650', 8, 3, 1, '2026-09-15', 8, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (21, 18, N'BA-WIL-800', 5, 3, 1, '2026-09-15', 5, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (22, 19, N'BA-EXI-EFB70', 4, 3, 1, '2026-09-15', 4, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (23, 20, N'BA-EXI-45', 12, 3, 1, '2026-09-15', 12, 8);
INSERT [dbo].[Inventario] ([Id],[IdProducto],[Codigo],[StockActual],[StockMinimo],[IdEstado],[FechaIngreso],[CantidadIngresada],[IdBodega]) VALUES (24, 21, N'BA-OPT-RED50', 3, 3, 1, '2026-09-15', 3, 8);
SET IDENTITY_INSERT [dbo].[Inventario] OFF;
GO

IF OBJECT_ID(N'dbo.SolicitudesEmpresa', N'U') IS NULL
CREATE TABLE dbo.SolicitudesEmpresa (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SolicitudesEmpresa PRIMARY KEY,
    Empresa nvarchar(120) NOT NULL, Nit nvarchar(20) NULL, Contacto nvarchar(100) NOT NULL,
    Telefono nvarchar(20) NOT NULL, Correo nvarchar(100) NOT NULL, Ciudad nvarchar(60) NULL,
    TipoFlota nvarchar(40) NOT NULL, NumeroVehiculos int NOT NULL, Mensaje nvarchar(500) NULL,
    Estado nvarchar(20) NOT NULL CONSTRAINT DF_SolEmp_Estado DEFAULT ('Pendiente'),
    FechaCreacion datetime NOT NULL CONSTRAINT DF_SolEmp_Fecha DEFAULT (GETDATE()));
GO
SET IDENTITY_INSERT [dbo].[SolicitudesEmpresa] ON;
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (1, N'Transportes Sierra Nevada S.A.S.', N'900123401-1', N'Rafael Peñaranda', N'3151000001', N'logistica@example.com', N'Valledupar', N'Transporte de carga', 18, N'Revisión de llantas de tractomulas cada mes.', N'Pendiente', '2026-10-01 09:10:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (2, N'Cooperativa de Taxis La Esperanza', N'900123402-2', N'Gloria Mejía', N'3151000002', N'gerencia.taxis@example.com', N'Valledupar', N'Taxis y plataformas', 40, N'Necesitamos cambio de aceite programado.', N'Contactada', '2026-10-02 09:11:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (3, N'Constructora Río Guatapurí', N'900123403-3', N'Luis Alfonso Cuello', N'3151000003', N'compras@example.com', N'Valledupar', N'Maquinaria y obra', 12, NULL, N'Pendiente', '2026-10-03 09:12:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (4, N'Colegio Campestre del Cesar', N'900123404-4', N'Ana Milena Daza', N'3151000004', N'admin.colegio@example.com', N'Valledupar', N'Transporte escolar', 8, N'Revisión previa al inicio del año escolar.', N'Contactada', '2026-10-04 09:13:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (5, N'Distribuidora Frutas del Valle', N'900123405-5', N'Jhon Fredy Mora', N'3151000005', N'operaciones@example.com', N'Valledupar', N'Transporte de carga', 15, N'Entregas diarias, muchas llantas se desgastan rápido.', N'Pendiente', '2026-10-05 09:14:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (6, N'Empresa de Buses Vallenatos', N'900123406-6', N'Carmen Rosa Ortiz', N'3151000006', N'flota@example.com', N'Valledupar', N'Transporte de pasajeros', 25, N'Alineación y balanceo por rutas.', N'Pendiente', '2026-10-06 09:15:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (7, N'Servicios Eléctricos del Caribe', N'900123407-7', N'Fabián Arzuaga', N'3151000007', N'logistica.sec@example.com', N'Aguachica', N'Vehículos de empresa', 10, NULL, N'Cerrada', '2026-10-07 09:16:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (8, N'Agroindustrial La Mesa', N'900123408-8', N'Diego Armando Pinto', N'3151000008', N'taller@example.com', N'Codazzi', N'Maquinaria y obra', 20, N'Camionetas y tractores en finca.', N'Pendiente', '2026-10-08 09:17:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (9, N'Mensajería Express Valledupar', N'900123409-9', N'Sandra Liliana Rueda', N'3151000009', N'gerencia.mensajeria@example.com', N'Valledupar', N'Vehículos de empresa', 30, N'Motos y vans de reparto.', N'Contactada', '2026-10-09 09:18:00');
INSERT [dbo].[SolicitudesEmpresa] ([Id],[Empresa],[Nit],[Contacto],[Telefono],[Correo],[Ciudad],[TipoFlota],[NumeroVehiculos],[Mensaje],[Estado],[FechaCreacion]) VALUES (10, N'Transportes Escolares Los Cardones', N'900123410-0', N'Wilmer Zuleta', N'3151000010', N'contacto.cardones@example.com', N'Valledupar', N'Transporte escolar', 6, NULL, N'Pendiente', '2026-10-10 09:19:00');
SET IDENTITY_INSERT [dbo].[SolicitudesEmpresa] OFF;
GO
SELECT 'Baterías' AS Categoria, COUNT(*) AS Registros FROM Productos WHERE Categoria = N'Baterías' UNION ALL SELECT 'Marcas', COUNT(*) FROM Marcas UNION ALL SELECT 'SolicitudesEmpresa', COUNT(*) FROM SolicitudesEmpresa;
GO
