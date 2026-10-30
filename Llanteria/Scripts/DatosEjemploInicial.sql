/* =====================================================================================================
   LLANDUPAR - DATOS DE EJEMPLO QUE SE CARGAN AL INICIAR LA APLICACIÓN
   Lo ejecuta Infrastructure/DatosEjemplo.cs (recurso incrustado en el proyecto); no hay que correrlo a mano.

   - Solo se carga en una base recién creada: la app comprueba que las tablas de ejemplo estén vacías y que
     existan únicamente las 3 cuentas iniciales (administrador, empleado y usuario registrado).
   - Todo va en una sola transacción: si algo falla, se revierte y la app sigue arrancando.
   - Las fechas se calculan a partir de HOY (@hoy), así el panel siempre muestra los últimos 6 meses con datos.
   - Los usuarios nuevos usan la misma contraseña de la cuenta "usuario registrado" (Privado/usuarios.json).
   - Se omiten los catálogos que la app ya crea (Estados, Roles, Sexo, TipoDocumento, EstadoInventario, CategoriaGasto...).
   ===================================================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @hoy date = CAST(GETDATE() AS date);
DECLARE @hash nvarchar(max) = (SELECT PasswordHash FROM dbo.Usuarios WHERE Id = 3);

BEGIN TRY
BEGIN TRANSACTION;

-- ------------------------------------------------------------
-- Pais (3 filas)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[Pais] WHERE [Id] >= 3)
BEGIN
SET IDENTITY_INSERT [dbo].[Pais] ON;
INSERT INTO [dbo].[Pais] ([Id], [IdEstado], [Nombre], [Prefijo], [CodigoISO])
VALUES
    (3, 2, N'Venezuela', N'+58', N'VE'),
    (4, 2, N'Ecuador', N'+593', N'EC'),
    (5, 2, N'México', N'+52', N'MX');
SET IDENTITY_INSERT [dbo].[Pais] OFF;
END;

-- ------------------------------------------------------------
-- Llanterias (2 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Llanterias] ON;
INSERT INTO [dbo].[Llanterias] ([Id], [Nombre], [RazonSocial], [Nit], [Direccion], [Ciudad], [Departamento], [Telefono], [WhatsApp], [Correo], [SitioWeb], [Instagram], [Facebook], [Logo], [Latitud], [Longitud], [HorarioSemana], [HorarioSabado], [HorarioDomingo], [PropietarioNombre], [PropietarioDocumento], [PropietarioTelefono], [PropietarioCorreo], [OfreceLlantas], [OfreceLubricantes], [OfreceBaterias], [OfreceAccesorios], [OfreceMantenimientoFlotas], [OfreceServicioADomicilio], [ServiciosOfrecidos], [ProductosDestacados], [Observaciones], [FechaCreacion], [IdEstado])
VALUES
    (1, N'Llandupar Centro', N'Llandupar S.A.S.', N'901234567-8', N'Calle 15 # 12-34, Centro', N'Valledupar', N'Cesar', N'+57 605 5801234', N'+57 3001234567', N'centro@example.com', N'https://llandupar.example.com', N'https://instagram.com/llandupar', N'https://facebook.com/llandupar', NULL, 10.4631, -73.2532, N'7:00 a. m. - 6:00 p. m.', N'8:00 a. m. - 4:00 p. m.', N'Cerrado', N'Kevin Méndez', N'900000001', N'+57 3000000000', N'propietario@example.com', 1, 1, 1, 1, 1, 1, N'Alineación y balanceo, montaje y desmontaje, cambio de aceite, revisión de baterías, rotación de llantas y reparación de pinchazos', N'Llantas Michelin, Bridgestone y Goodyear; lubricantes Mobil y Castrol; baterías MAC y Willard', N'Sede principal con bodega de lubricantes', DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -293, @hoy) AS DateTime)), 2),
    (2, N'Llandupar Norte', N'Llandupar S.A.S.', N'901234567-8', N'Carrera 19 # 6-50, Zona Norte', N'Valledupar', N'Cesar', N'+57 605 5805678', N'+57 3009876543', N'norte@example.com', N'https://llandupar.example.com', N'https://instagram.com/llandupar', N'https://facebook.com/llandupar', NULL, 10.4912, -73.2605, N'7:00 a. m. - 6:00 p. m.', N'8:00 a. m. - 3:00 p. m.', N'Cerrado', N'Kevin Méndez', N'900000001', N'+57 3000000000', N'propietario@example.com', 1, 1, 1, 1, 0, 0, N'Montaje y desmontaje, cambio de aceite, revisión de baterías y venta de accesorios', N'Llantas Hankook, lubricantes Terpel, baterías para motocicleta', N'Sucursal con enfoque en motocicletas y vehículos pequeños', DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -293, @hoy) AS DateTime)), 2);
SET IDENTITY_INSERT [dbo].[Llanterias] OFF;

-- ------------------------------------------------------------
-- Eventos (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Eventos] ON;
INSERT INTO [dbo].[Eventos] ([Id], [Nombre], [Descripcion], [Fecha], [FechaInicio], [FechaFin], [Recurrente], [IdEstado], [IdLlanteria])
VALUES
    (1, N'Black Friday', N'Descuentos especiales en llantas durante el fin de semana de Black Friday', CAST(N'2026-11-27' AS Date), CAST(N'2026-11-25' AS Date), CAST(N'2026-11-30' AS Date), 1, 2, NULL),
    (2, N'Navidad y Año Nuevo', N'Promociones en lubricantes y mantenimiento para los viajes de fin de año', CAST(N'2026-12-24' AS Date), CAST(N'2026-12-01' AS Date), CAST(N'2026-12-31' AS Date), 1, 2, NULL),
    (3, N'Amor y Amistad', N'Descuento fijo en servicios para compartir con amigos y familia', CAST(N'2026-09-19' AS Date), CAST(N'2026-09-14' AS Date), CAST(N'2026-09-20' AS Date), 1, 2, NULL),
    (4, N'Festival de la Leyenda Vallenata', N'Revisión gratuita de presión y estado de llantas para quienes viajan al festival', CAST(N'2027-04-28' AS Date), CAST(N'2027-04-25' AS Date), CAST(N'2027-05-02' AS Date), 1, 2, NULL),
    (5, N'Puente festivo de noviembre', N'Promoción en baterías antes del puente festivo', CAST(N'2026-11-16' AS Date), CAST(N'2026-11-13' AS Date), CAST(N'2026-11-16' AS Date), 0, 2, 1);
SET IDENTITY_INSERT [dbo].[Eventos] OFF;

-- ------------------------------------------------------------
-- Ofertas (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Ofertas] ON;
INSERT INTO [dbo].[Ofertas] ([Id], [IdEstado], [IdLlanteria], [IdEvento], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional])
VALUES
    (1, 2, NULL, 1, N'Black Friday en llantas', N'15% de descuento en llantas seleccionadas', 15.00, 0.00, CAST(N'2026-11-25T00:00:00.000' AS DateTime), CAST(N'2026-11-30T23:59:59.000' AS DateTime), 1, N'BLACK15'),
    (2, 2, NULL, 2, N'Lubricantes para viajar en Navidad', N'10% de descuento en aceites y lubricantes', 10.00, 0.00, CAST(N'2026-12-01T00:00:00.000' AS DateTime), CAST(N'2026-12-31T23:59:59.000' AS DateTime), 1, N'NAVIDAD10'),
    (3, 2, NULL, 3, N'Amor y Amistad', N'$20.000 de descuento en servicios de taller', 0.00, 20000.00, CAST(N'2026-09-14T00:00:00.000' AS DateTime), CAST(N'2026-09-20T23:59:59.000' AS DateTime), 0, N'AMIGO20'),
    (4, 2, NULL, 5, N'Baterías para el puente festivo', N'8% de descuento en baterías para auto y camioneta', 8.00, 0.00, CAST(N'2026-11-13T00:00:00.000' AS DateTime), CAST(N'2026-11-16T23:59:59.000' AS DateTime), 1, N'FESTIVO8'),
    (5, 2, 1, NULL, N'Descuento en compras en línea', N'$15.000 de descuento en pedidos de la sede Centro', 0.00, 15000.00, CAST(N'2026-10-01T00:00:00.000' AS DateTime), CAST(N'2026-12-31T23:59:59.000' AS DateTime), 1, N'MONTAJE15');
SET IDENTITY_INSERT [dbo].[Ofertas] OFF;

-- ------------------------------------------------------------
-- Proveedores (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Proveedores] ON;
INSERT INTO [dbo].[Proveedores] ([Id], [IdEstado], [NombreEmpresa], [Contacto], [Telefono])
VALUES
    (1, 2, N'Distribuidora Andina de Llantas S.A.S.', N'Marcela Rojas', N'+57 3101112233'),
    (2, 2, N'Lubricantes del Caribe Ltda.', N'Ricardo Barros', N'+57 3102223344'),
    (3, 2, N'Baterías Energía Total S.A.S.', N'Paola Cantillo', N'+57 3103334455'),
    (4, 2, N'Accesorios y Repuestos del Cesar', N'Hernán Zabaleta', N'+57 3104445566'),
    (5, 2, N'Importadora Rueda Sur S.A.S.', N'Natalia Pumarejo', N'+57 3105556677');
SET IDENTITY_INSERT [dbo].[Proveedores] OFF;

-- ------------------------------------------------------------
-- Marcas (12 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Marcas] ON;
INSERT INTO [dbo].[Marcas] ([Id], [IdEstado], [Nombre])
VALUES
    (1, 2, N'Michelin'),
    (2, 2, N'Bridgestone'),
    (3, 2, N'Goodyear'),
    (4, 2, N'Pirelli'),
    (5, 2, N'Hankook'),
    (6, 2, N'Mobil'),
    (7, 2, N'Castrol'),
    (8, 2, N'Terpel'),
    (9, 2, N'MAC'),
    (10, 2, N'Bosch'),
    (11, 2, N'Willard'),
    (12, 2, N'Genérica');
SET IDENTITY_INSERT [dbo].[Marcas] OFF;

-- ------------------------------------------------------------
-- TipoServicio (8 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[TipoServicio] ON;
INSERT INTO [dbo].[TipoServicio] ([Id], [IdEstado], [NombreServicio], [ValorServicio], [IconoPath])
VALUES
    (1, 2, N'Alineación', 60000.000, NULL),
    (2, 2, N'Balanceo', 40000.000, NULL),
    (3, 2, N'Montaje y desmontaje de llanta', 20000.000, NULL),
    (4, 2, N'Cambio de aceite', 35000.000, NULL),
    (5, 2, N'Revisión de batería', 15000.000, NULL),
    (6, 2, N'Rotación de llantas', 30000.000, NULL),
    (7, 2, N'Reparación de pinchazo', 15000.000, NULL),
    (8, 2, N'Servicio a domicilio', 50000.000, NULL);
SET IDENTITY_INSERT [dbo].[TipoServicio] OFF;

-- ------------------------------------------------------------
-- Bodegas (3 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Bodegas] ON;
INSERT INTO [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima], [IdEstado], [IdLlanteria])
VALUES
    (1, N'Bodega Principal', N'Sede Centro - primer piso', 2000, 2, 1),
    (2, N'Bodega Lubricantes', N'Sede Centro - patio trasero', 800, 2, 1),
    (3, N'Bodega Norte', N'Sede Norte - bodega general', 1200, 2, 2);
SET IDENTITY_INSERT [dbo].[Bodegas] OFF;

-- ------------------------------------------------------------
-- CatalogoIncentivos (6 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[CatalogoIncentivos] ON;
INSERT INTO [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible], [IdEstado])
VALUES
    (1, N'Descuento de $20.000 en montaje', N'Válido para el montaje y desmontaje de llantas', 300, 99, 2),
    (2, N'Cambio de aceite gratis', N'Incluye mano de obra; el aceite se paga aparte', 800, 24, 2),
    (3, N'Alineación gratis', N'Alineación para un vehículo liviano', 1000, 19, 2),
    (4, N'Kit de limpieza para auto', N'Incluye paño, shampoo y brillallantas', 500, 15, 2),
    (5, N'Gorra Llandupar', N'Gorra oficial de la marca', 250, 39, 2),
    (6, N'Bono de $100.000 en productos', N'Redimible en llantas, lubricantes, baterías o accesorios', 2000, 5, 2);
SET IDENTITY_INSERT [dbo].[CatalogoIncentivos] OFF;

-- ------------------------------------------------------------
-- Clientes (6 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Clientes] ON;
INSERT INTO [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash], [IdEstado])
VALUES
    (2, N'Carlos', N'Pérez Mendoza', 1, N'1065123402', N'+57 3001234502', N'carlos.perez@example.com', 1, NULL, 450, @hash, 2),
    (3, N'Laura', N'Gómez Rincón', 1, N'1065987403', N'+57 3001234503', N'laura.gomez@example.com', 2, NULL, 1200, @hash, 2),
    (4, N'Andrés', N'Villazón Castro', 1, N'77012404', N'+57 3001234504', N'andres.villazon@example.com', 1, NULL, 80, @hash, 2),
    (5, N'María', N'Fernández Daza', 1, N'49765405', N'+57 3001234505', N'maria.fernandez@example.com', 2, NULL, 150, @hash, 2),
    (6, N'Jorge', N'Arzuaga Maestre', 1, N'77123406', N'+57 3001234506', N'jorge.arzuaga@example.com', 1, NULL, 2300, @hash, 2),
    (7, N'Sofía', N'Ospino Brito', 1, N'1065456407', N'+57 3001234507', N'sofia.ospino@example.com', 2, NULL, 150, @hash, 2);
SET IDENTITY_INSERT [dbo].[Clientes] OFF;

-- ------------------------------------------------------------
-- Empleados (3 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Empleados] ON;
INSERT INTO [dbo].[Empleados] ([Id], [Foto], [FotoDocumento], [FotoRostro], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol], [IdEstado], [IdLlanteria])
VALUES
    (3, NULL, NULL, NULL, N'Daniela', N'Orozco Peñaranda', 1, N'1065222303', N'daniela.orozco@example.com', N'+57 3002223303', 1800000.00, N'Lunes a sábado', 2, CAST(N'1995-03-14' AS Date), 2, 2, 1),
    (4, NULL, NULL, NULL, N'Luis', N'Hernández Torres', 1, N'1065222304', N'luis.hernandez@example.com', N'+57 3002223304', 1500000.00, N'Lunes a viernes', 1, CAST(N'1992-07-22' AS Date), 3, 2, 1),
    (5, NULL, NULL, NULL, N'Pedro', N'Maestre Ustariz', 1, N'1065222305', N'pedro.maestre@example.com', N'+57 3002223305', 1500000.00, N'Martes a sábado', 1, CAST(N'1989-11-05' AS Date), 3, 2, 2);
SET IDENTITY_INSERT [dbo].[Empleados] OFF;

-- ------------------------------------------------------------
-- Usuarios (11 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Usuarios] ON;
INSERT INTO [dbo].[Usuarios] ([Id], [IdEstado], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdCliente], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento])
VALUES
    (4, 2, N'daniela_41_4', N'daniela.orozco@example.com', @hash, 3, NULL, 2, N'Activo', NULL, NULL, DATEADD(SECOND, 32400, CAST(DATEADD(DAY, -58, @hoy) AS DateTime)), DATEADD(SECOND, 27900, CAST(@hoy AS DateTime)), 1),
    (5, 2, N'luis_58_5', N'luis.hernandez@example.com', @hash, 4, NULL, 3, N'Activo', NULL, NULL, DATEADD(SECOND, 33000, CAST(DATEADD(DAY, -58, @hoy) AS DateTime)), DATEADD(SECOND, 63000, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), 1),
    (6, 2, N'pedro_23_6', N'pedro.maestre@example.com', @hash, 5, NULL, 3, N'Activo', NULL, NULL, DATEADD(SECOND, 30600, CAST(DATEADD(DAY, -44, @hoy) AS DateTime)), DATEADD(SECOND, 64500, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), 1),
    (7, 2, N'carlos_35_7', N'carlos.perez@example.com', @hash, NULL, 2, 4, N'Activo', NULL, NULL, DATEADD(SECOND, 65700, CAST(DATEADD(DAY, -40, @hoy) AS DateTime)), DATEADD(SECOND, 72600, CAST(DATEADD(DAY, -2, @hoy) AS DateTime)), 1),
    (8, 2, N'laura_62_8', N'laura.gomez@example.com', @hash, NULL, 3, 4, N'Activo', NULL, NULL, DATEADD(SECOND, 36300, CAST(DATEADD(DAY, -38, @hoy) AS DateTime)), DATEADD(SECOND, 42000, CAST(DATEADD(DAY, -5, @hoy) AS DateTime)), 1),
    (9, 2, N'andres_19_9', N'andres.villazon@example.com', @hash, NULL, 4, 4, N'Activo', NULL, NULL, DATEADD(SECOND, 51600, CAST(DATEADD(DAY, -35, @hoy) AS DateTime)), DATEADD(SECOND, 33300, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), 1),
    (10, 2, N'maria_47_10', N'maria.fernandez@example.com', @hash, NULL, 5, 4, N'Activo', NULL, NULL, DATEADD(SECOND, 60300, CAST(DATEADD(DAY, -32, @hoy) AS DateTime)), DATEADD(SECOND, 70200, CAST(DATEADD(DAY, -10, @hoy) AS DateTime)), 1),
    (11, 2, N'jorge_84_11', N'jorge.arzuaga@example.com', @hash, NULL, 6, 5, N'Activo', NULL, NULL, DATEADD(SECOND, 31800, CAST(DATEADD(DAY, -29, @hoy) AS DateTime)), DATEADD(SECOND, 43500, CAST(DATEADD(DAY, -6, @hoy) AS DateTime)), 1),
    (12, 2, N'sofia_26_12', N'sofia.ospino@example.com', @hash, NULL, 7, 5, N'Activo', NULL, NULL, DATEADD(SECOND, 48300, CAST(DATEADD(DAY, -27, @hoy) AS DateTime)), DATEADD(SECOND, 55200, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), 1),
    (13, 2, N'luisa_77_13', N'luisa.cantillo@example.com', @hash, NULL, NULL, 5, N'Activo', NULL, NULL, DATEADD(SECOND, 68400, CAST(DATEADD(DAY, -20, @hoy) AS DateTime)), DATEADD(SECOND, 75600, CAST(DATEADD(DAY, -4, @hoy) AS DateTime)), 1),
    (14, 3, N'hector_63_14', N'hector.mejia@example.com', @hash, NULL, NULL, 5, N'Bloqueado', N'Comentarios ofensivos en la sección de reseñas', CAST(DATEADD(DAY, 16, @hoy) AS DateTime), DATEADD(SECOND, 34200, CAST(DATEADD(DAY, -18, @hoy) AS DateTime)), DATEADD(SECOND, 79800, CAST(DATEADD(DAY, -8, @hoy) AS DateTime)), 1);
SET IDENTITY_INSERT [dbo].[Usuarios] OFF;

-- ------------------------------------------------------------
-- PerfilUsuario (14 filas)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 1)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 1, N'Administrador de la plataforma', N'Dark', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 2)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 2, N'Empleado de la sede Centro', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 3)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 3, N'Cliente frecuente', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 4)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 4, N'Secretaria de la sede Centro', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 5)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 5, N'Técnico de montaje y alineación', N'Dark', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 6)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 6, N'Técnico de la sede Norte', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 7)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 7, N'Me gusta cuidar mi carro', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 8)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 8, NULL, N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 9)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 9, NULL, N'Dark', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 10)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 10, N'Viajo seguido a Santa Marta', N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 11)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 11, N'Encargado de una flota de camionetas', N'Dark', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 12)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 12, NULL, N'Light', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 13)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 13, NULL, N'Light', NULL, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PerfilUsuario] WHERE [IdUsuario] = 14)
INSERT INTO [dbo].[PerfilUsuario] ([IdEstado], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas])
VALUES
    (2, 14, NULL, N'Light', NULL, 0);

-- ------------------------------------------------------------
-- RecuperacionCuentas (3 filas)
-- ------------------------------------------------------------
INSERT INTO [dbo].[RecuperacionCuentas] ([IdEstado], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado])
VALUES
    (2, 7, N'3c9b9036ea0d8eb5faab08080bd83b71ed79bff893c5a46733e16d94f8183a08', DATEADD(SECOND, 36900, CAST(DATEADD(DAY, -12, @hoy) AS DateTime)), 1),
    (2, 9, N'8a2e426205aca5ffe7b2fa9605da926de0600f6ed0d7beb72fc662ab7e0a5d4c', DATEADD(SECOND, 43200, CAST(@hoy AS DateTime)), 0),
    (2, 12, N'09b6d5b228bebaeede12bb0b1a3390e09687afc3cac727285286c2d9fc0cadf5', DATEADD(SECOND, 67200, CAST(DATEADD(DAY, -3, @hoy) AS DateTime)), 1);

-- ------------------------------------------------------------
-- LogActividad (12 filas)
-- ------------------------------------------------------------
INSERT INTO [dbo].[LogActividad] ([IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP])
VALUES
    (1, N'Inicio de sesión', N'Usuarios', DATEADD(SECOND, 29429, CAST(@hoy AS DateTime)), N'192.168.1.10'),
    (1, N'Creación de producto', N'Productos', DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -40, @hoy) AS DateTime)), N'192.168.1.10'),
    (1, N'Creación de usuario', N'Usuarios', DATEADD(SECOND, 32400, CAST(DATEADD(DAY, -58, @hoy) AS DateTime)), N'192.168.1.10'),
    (4, N'Registro de empleado', N'Empleados', DATEADD(SECOND, 30000, CAST(DATEADD(DAY, -44, @hoy) AS DateTime)), N'192.168.1.21'),
    (4, N'Actualización de inventario', N'Inventario', DATEADD(SECOND, 41400, CAST(DATEADD(DAY, -29, @hoy) AS DateTime)), N'192.168.1.21'),
    (4, N'Registro de factura', N'Facturas', DATEADD(SECOND, 39000, CAST(DATEADD(DAY, -25, @hoy) AS DateTime)), N'192.168.1.21'),
    (5, N'Orden de trabajo finalizada', N'OrdenesTrabajo', DATEADD(SECOND, 38700, CAST(DATEADD(DAY, -25, @hoy) AS DateTime)), N'192.168.1.22'),
    (5, N'Registro de factura', N'Facturas', DATEADD(SECOND, 45300, CAST(DATEADD(DAY, -16, @hoy) AS DateTime)), N'192.168.1.22'),
    (6, N'Actualización de inventario', N'Inventario', DATEADD(SECOND, 58200, CAST(DATEADD(DAY, -8, @hoy) AS DateTime)), N'192.168.1.31'),
    (1, N'Bloqueo de usuario', N'Usuarios', DATEADD(SECOND, 81000, CAST(DATEADD(DAY, -8, @hoy) AS DateTime)), N'192.168.1.10'),
    (7, N'Inicio de sesión', N'Usuarios', DATEADD(SECOND, 72600, CAST(DATEADD(DAY, -2, @hoy) AS DateTime)), N'190.250.14.77'),
    (12, N'Solicitud de recuperación de contraseña', N'RecuperacionCuentas', DATEADD(SECOND, 66300, CAST(DATEADD(DAY, -3, @hoy) AS DateTime)), N'190.250.31.45');

-- ------------------------------------------------------------
-- EditoresContenido (3 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[EditoresContenido] ON;
INSERT INTO [dbo].[EditoresContenido] ([Id], [IdUsuario], [Recurso], [IdInvitadoPor], [Fecha], [IdEstado])
VALUES
    (1, 4, N'Articulos', 1, DATEADD(SECOND, 34200, CAST(DATEADD(DAY, -55, @hoy) AS DateTime2(7))), 2),
    (2, 2, N'Articulos', 1, DATEADD(SECOND, 34500, CAST(DATEADD(DAY, -55, @hoy) AS DateTime2(7))), 2),
    (3, 4, N'Servicios', 1, DATEADD(SECOND, 54000, CAST(DATEADD(DAY, -54, @hoy) AS DateTime2(7))), 2);
SET IDENTITY_INSERT [dbo].[EditoresContenido] OFF;

-- ------------------------------------------------------------
-- Articulos (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Articulos] ON;
INSERT INTO [dbo].[Articulos] ([Id], [Titulo], [Slug], [Resumen], [Categoria], [Fecha], [Autor], [Imagen], [Contenido], [Referencias], [Publicado], [IdAutor], [ImagenPresentacion], [TablaContenido], [IdEstado], [IdLlanteria])
VALUES
    (1, N'Cómo saber cuándo cambiar tus llantas', N'como-saber-cuando-cambiar-tus-llantas', N'Tres revisiones rápidas que puedes hacer en casa para saber si tus llantas todavía son seguras.', N'Llantas', DATEADD(DAY, -50, @hoy), N'Kevin Méndez', N'cuando-cambiar-las-llantas.svg', N'1. Revisa el labrado
El labrado es lo que mantiene la llanta pegada al pavimento, sobre todo en lluvia. Si el indicador de desgaste (TWI) queda al ras de la banda de rodadura, es momento de cambiarla.

2. Busca grietas y deformaciones
Inspecciona los costados. Cortes, grietas o bultos son señal de daño interno y no se deben ignorar.

3. Mira la fecha de fabricación
El código DOT indica la semana y el año de fabricación. Aunque se vea bien, una llanta muy antigua pierde flexibilidad y agarre.', NULL, 1, 1, N'cuando-cambiar-las-llantas.svg', N'Revisa el labrado;Busca grietas y deformaciones;Mira la fecha de fabricación', 2, NULL),
    (2, N'La presión correcta, el hábito que más ahorra', N'la-presion-correcta-el-habito-que-mas-ahorra', N'Mantener la presión adecuada alarga la vida de las llantas, mejora el consumo y hace más segura la conducción.', N'Llantas', DATEADD(DAY, -36, @hoy), N'Daniela Orozco', N'presion-de-las-llantas.svg', N'1. Por qué importa
Una llanta con poco aire se desgasta por los bordes y aumenta el consumo de combustible. Con exceso de aire se desgasta por el centro y pierde agarre.

2. Cómo medirla
Hazlo con las llantas frías, antes de rodar, y usa un manómetro confiable.

3. Qué valor usar
La presión recomendada aparece en la etiqueta de la puerta del conductor o en el manual del vehículo, no en el costado de la llanta.', N'Manual del propietario de cada vehículo', 1, 4, N'presion-de-las-llantas.svg', N'Por qué importa;Cómo medirla;Qué valor usar', 2, NULL),
    (3, N'Aceite sintético, semisintético o mineral', N'aceite-sintetico-semisintetico-o-mineral', N'Diferencias básicas entre los tres tipos de aceite y cómo elegir el que pide tu motor.', N'Lubricantes', DATEADD(DAY, -28, @hoy), N'Kevin Méndez', N'como-elegir-el-aceite-de-tu-motor.svg', N'1. Aceite mineral
Es el más económico y se usa en motores sencillos o con mucho kilometraje.

2. Aceite semisintético
Mezcla bases minerales y sintéticas y ofrece un equilibrio entre precio y protección.

3. Aceite sintético
Mantiene mejor su viscosidad con el calor y protege motores modernos.

4. Cada cuánto cambiarlo
Sigue siempre el intervalo y la viscosidad que indica el manual del fabricante.', NULL, 1, 1, N'como-elegir-el-aceite-de-tu-motor.svg', N'Aceite mineral;Aceite semisintético;Aceite sintético;Cada cuánto cambiarlo', 2, NULL),
    (4, N'Cuidados de la batería con el calor de Valledupar', N'cuidados-de-la-bateria-con-el-calor-de-valledupar', N'Las altas temperaturas aceleran el desgaste de la batería. Estos cuidados ayudan a que dure más.', N'Baterías', DATEADD(DAY, -15, @hoy), N'Daniela Orozco', N'bateria-del-carro-en-clima-caliente.svg', N'1. Limpia los bornes
El sulfato en los bornes dificulta el arranque. Límpialos con un cepillo y revisa que estén bien apretados.

2. Evita trayectos muy cortos
Los viajes cortos no dejan que el alternador recargue la batería por completo.

3. Haz una revisión periódica
Una prueba de carga en el taller permite detectar una batería débil antes de quedarte varado.', NULL, 1, 4, N'bateria-del-carro-en-clima-caliente.svg', N'Limpia los bornes;Evita trayectos muy cortos;Haz una revisión periódica', 2, NULL),
    (5, N'Lista de chequeo antes de un viaje largo', N'lista-de-chequeo-antes-de-un-viaje-largo', N'Borrador: puntos a revisar en el vehículo antes de salir a carretera.', N'Mantenimiento', DATEADD(DAY, -2, @hoy), N'Kevin Méndez', NULL, N'1. Llantas
Presión, labrado y llanta de repuesto.

2. Fluidos
Aceite, refrigerante y líquido de frenos.

3. Batería y luces
Prueba de carga y revisión de todas las luces.', NULL, 0, 1, NULL, N'Llantas;Fluidos;Batería y luces', 2, NULL);
SET IDENTITY_INSERT [dbo].[Articulos] OFF;

-- ------------------------------------------------------------
-- ServiciosWeb (6 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[ServiciosWeb] ON;
INSERT INTO [dbo].[ServiciosWeb] ([Id], [IdEstado], [IdLlanteria], [Slug], [Titulo], [Subtitulo], [Icono], [Imagen], [Descripcion], [Ventajas], [InfoCita], [TituloVisitante], [TextoVisitante], [EtiquetaPrecio], [Precio], [PlaceholderVehiculo], [PlaceholderNotas], [PlaceholderMensaje], [EsDomicilio], [Publicado], [Orden])
VALUES
    (1, 2, NULL, N'alineacion-y-balanceo', N'Alineación y balanceo', N'Dirección estable y llantas que se desgastan parejo', N'bi-gear', NULL, N'Ajustamos los ángulos de tu vehículo y balanceamos cada llanta para evitar vibraciones y desgaste irregular.', N'Menos vibración en el volante;Mayor vida útil de las llantas;Mejor consumo de combustible', N'Agenda tu cita y te confirmamos por WhatsApp. El servicio dura cerca de dos horas.', N'Primera vez con nosotros', N'Si no tienes cuenta puedes agendar con tu nombre y teléfono.', N'Desde', N'$100.000', N'Ej: Mazda 3 2018 - KLM123', N'¿Notas vibraciones o el carro se va hacia un lado?', N'Cuéntanos qué necesitas', 0, 1, 1),
    (2, 2, NULL, N'cambio-de-aceite', N'Cambio de aceite', N'Lubricación adecuada para tu motor', N'bi-droplet', NULL, N'Cambiamos el aceite y el filtro con el lubricante que recomienda el fabricante de tu vehículo.', N'Mayor protección del motor;Aceites de marcas reconocidas;Servicio en menos de una hora', N'Puedes traer tu propio aceite o elegir entre los que tenemos en bodega.', NULL, NULL, N'Desde', N'$35.000', N'Ej: Chevrolet Spark - UVX456', N'¿Qué aceite usa tu vehículo?', N'Cuéntanos qué necesitas', 0, 1, 2),
    (3, 2, NULL, N'montaje-de-llantas', N'Montaje y desmontaje de llantas', N'Instalación segura con equipos especializados', N'bi-circle', NULL, N'Montamos y desmontamos llantas de autos, camionetas y motocicletas sin dañar tus rines.', N'Equipos de montaje sin maltrato al rin;Incluye válvula y balanceo básico;Para autos, camionetas y motos', N'Si compras las llantas con nosotros, el montaje se agenda en el mismo momento.', NULL, NULL, N'Por llanta', N'$20.000', N'Ej: Toyota Hilux 2019 - SZB321', N'¿Cuántas llantas vas a cambiar?', N'Cuéntanos qué necesitas', 0, 1, 3),
    (4, 2, NULL, N'revision-de-baterias', N'Revisión de baterías', N'Diagnóstico de carga y estado del sistema eléctrico', N'bi-battery-charging', NULL, N'Medimos la carga de tu batería y revisamos el alternador y el motor de arranque para evitar que te quedes varado.', N'Diagnóstico en pocos minutos;Revisión del alternador;Recomendación honesta de cambio', N'No requiere cita previa en horario de atención, pero puedes reservar para evitar esperas.', NULL, NULL, N'Desde', N'$15.000', N'Ej: Kia Picanto 2021 - TGH987', N'¿Tu carro demora en encender?', N'Cuéntanos qué necesitas', 0, 1, 4),
    (5, 2, NULL, N'mantenimiento-de-flotas', N'Mantenimiento de flotas', N'Planes para empresas de transporte y distribución', N'bi-truck', NULL, N'Diseñamos un plan de revisión periódica para las llantas, lubricantes y baterías de tu flota, con control por placa.', N'Control por vehículo;Precios por volumen;Recordatorios de próximo mantenimiento', N'Escríbenos con el tamaño de tu flota y te enviamos una propuesta.', N'Para empresas', N'Completa el formulario de empresas y un asesor te contactará.', N'Cotización', N'A convenir', N'Ej: 12 camionetas Nissan Frontier', N'Cuéntanos sobre tu flota', N'Describe tu necesidad', 0, 1, 5),
    (6, 2, NULL, N'servicio-a-domicilio', N'Servicio a domicilio', N'Vamos hasta donde estés en Valledupar', N'bi-geo-alt', NULL, N'Un técnico llega hasta tu casa u oficina para revisar baterías, cambiar llantas pinchadas y atender emergencias básicas.', N'Atención donde te encuentres;Técnicos con herramienta propia;Cobertura en Valledupar y alrededores', N'Indica la dirección exacta al agendar. El costo del desplazamiento se suma al servicio.', NULL, NULL, N'Desplazamiento', N'$50.000', N'Ej: Nissan Frontier - WQE258', N'¿Qué problema presenta tu vehículo?', N'Cuéntanos qué necesitas', 1, 1, 6);
SET IDENTITY_INSERT [dbo].[ServiciosWeb] OFF;

-- ------------------------------------------------------------
-- Productos (16 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Productos] ON;
INSERT INTO [dbo].[Productos] ([Id], [IdEstado], [IdLlanteria], [IdOferta], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega], [PrecioPromocion], [PorcentajeDescuento], [DuracionDescuentoDias], [FechaInicioDescuento])
VALUES
    (1, 2, 1, 1, N'Michelin Primacy 4 205/55 R16', N'Llanta de turismo con muy buen agarre en mojado y larga duración', 380000.00, 520000.00, 1, N'Llantas', N'default-producto.png', 1, 442000.00, 15.00, 6, CAST(N'2026-11-25T00:00:00.000' AS DateTime)),
    (2, 2, 1, 1, N'Bridgestone Turanza T005 195/65 R15', N'Llanta confortable y silenciosa para ciudad y carretera', 290000.00, 395000.00, 1, N'Llantas', N'default-producto.png', 1, 335750.00, 15.00, 6, CAST(N'2026-11-25T00:00:00.000' AS DateTime)),
    (3, 2, 1, NULL, N'Goodyear EfficientGrip 225/45 R17', N'Llanta de alto desempeño con bajo consumo de combustible', 410000.00, 560000.00, 1, N'Llantas', N'default-producto.png', 1, NULL, NULL, NULL, NULL),
    (4, 2, 1, NULL, N'Pirelli Cinturato P1 185/60 R15', N'Llanta para autos compactos, equilibrada entre confort y durabilidad', 270000.00, 365000.00, 1, N'Llantas', N'default-producto.png', 1, NULL, NULL, NULL, NULL),
    (5, 2, 1, NULL, N'Hankook Dynapro HT 265/65 R17', N'Llanta para camionetas y SUV, resistente en carretera y trocha', 520000.00, 710000.00, 5, N'Llantas', N'default-producto.png', 1, NULL, NULL, NULL, NULL),
    (6, 2, 1, NULL, N'Michelin City Grip 90/90-18 Moto', N'Llanta trasera para motocicleta de uso urbano', 130000.00, 185000.00, 5, N'Llantas', N'default-producto.png', 1, NULL, NULL, NULL, NULL),
    (7, 2, 2, NULL, N'Hankook Kinergy Eco 175/70 R13', N'Llanta económica para vehículos pequeños', 210000.00, 290000.00, 1, N'Llantas', N'default-producto.png', 3, NULL, NULL, NULL, NULL),
    (8, 2, 1, 2, N'Mobil Super 2000 10W-40 Galón', N'Aceite semisintético para motores a gasolina', 82000.00, 115000.00, 2, N'Lubricantes', N'default-producto.png', 2, 103500.00, 10.00, 31, CAST(N'2026-12-01T00:00:00.000' AS DateTime)),
    (9, 2, 1, 2, N'Castrol GTX 20W-50 Galón', N'Aceite mineral multigrado para motores con alto kilometraje', 76000.00, 105000.00, 2, N'Lubricantes', N'default-producto.png', 2, 94500.00, 10.00, 31, CAST(N'2026-12-01T00:00:00.000' AS DateTime)),
    (10, 2, 2, NULL, N'Terpel Celerity 5W-30 Cuarto', N'Aceite sintético para motores modernos', 32000.00, 48000.00, 2, N'Lubricantes', N'default-producto.png', 3, NULL, NULL, NULL, NULL),
    (11, 2, 1, 2, N'Mobil Delvac 15W-40 Galón', N'Aceite para motores diésel de servicio pesado', 98000.00, 135000.00, 2, N'Lubricantes', N'default-producto.png', 2, 121500.00, 10.00, 31, CAST(N'2026-12-01T00:00:00.000' AS DateTime)),
    (12, 2, 1, 4, N'Batería MAC Silver 12V 65Ah', N'Batería libre de mantenimiento para automóviles', 330000.00, 450000.00, 3, N'Baterías', N'default-producto.png', 1, 414000.00, 8.00, 4, CAST(N'2026-11-13T00:00:00.000' AS DateTime)),
    (13, 2, 1, 4, N'Batería Willard Extrema 12V 80Ah', N'Batería de alto rendimiento para camionetas y SUV', 410000.00, 560000.00, 3, N'Baterías', N'default-producto.png', 1, 515200.00, 8.00, 4, CAST(N'2026-11-13T00:00:00.000' AS DateTime)),
    (14, 2, 2, NULL, N'Batería Bosch 12V 9Ah Moto', N'Batería sellada para motocicleta', 95000.00, 140000.00, 3, N'Baterías', N'default-producto.png', 3, NULL, NULL, NULL, NULL),
    (15, 2, 2, NULL, N'Juego de tapas de válvula x4', N'Tapas de válvula antipolvo en aluminio', 6000.00, 15000.00, 4, N'Accesorios', N'default-producto.png', 3, NULL, NULL, NULL, NULL),
    (16, 2, 2, NULL, N'Sensor de presión de llantas TPMS', N'Kit de 4 sensores externos con pantalla', 85000.00, 130000.00, 4, N'Accesorios', N'default-producto.png', 3, NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT [dbo].[Productos] OFF;

-- ------------------------------------------------------------
-- DetalleProductos (16 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[DetalleProductos] ON;
INSERT INTO [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [TipoVehiculo], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses], [Voltaje], [TecnologiaBateria], [AplicacionLubricante], [PresentacionLubricante], [TipoAccesorio], [UbicacionAccesorio], [Amperaje], [Polaridad], [IdEstado])
VALUES
    (1, 1, 1, N'205', N'55', N'16', N'Auto', N'91', N'V', NULL, NULL, 60, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (2, 2, 2, N'195', N'65', N'15', N'Auto', N'91', N'H', NULL, NULL, 36, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (3, 3, 3, N'225', N'45', N'17', N'Auto', N'94', N'W', NULL, NULL, 48, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (4, 4, 4, N'185', N'60', N'15', N'Auto', N'84', N'H', NULL, NULL, 36, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (5, 5, 5, N'265', N'65', N'17', N'Camioneta', N'112', N'H', NULL, NULL, 36, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (6, 6, 1, N'90', N'90', N'18', N'Moto', N'51', N'P', NULL, NULL, 12, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (7, 7, 5, N'175', N'70', N'13', N'Auto', N'82', N'T', NULL, NULL, 24, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2),
    (8, 8, 6, NULL, NULL, NULL, N'Auto', NULL, NULL, N'10W-40', N'Semisintético', NULL, NULL, NULL, N'Motor a gasolina', N'Galón', NULL, NULL, NULL, NULL, 2),
    (9, 9, 7, NULL, NULL, NULL, N'Auto', NULL, NULL, N'20W-50', N'Mineral', NULL, NULL, NULL, N'Motor a gasolina', N'Galón', NULL, NULL, NULL, NULL, 2),
    (10, 10, 8, NULL, NULL, NULL, N'Auto', NULL, NULL, N'5W-30', N'Sintético', NULL, NULL, NULL, N'Motor a gasolina', N'Cuarto', NULL, NULL, NULL, NULL, 2),
    (11, 11, 6, NULL, NULL, NULL, N'Camión', NULL, NULL, N'15W-40', N'Mineral', NULL, NULL, NULL, N'Motor diésel', N'Galón', NULL, NULL, NULL, NULL, 2),
    (12, 12, 9, NULL, NULL, NULL, N'Auto', NULL, NULL, NULL, NULL, 18, N'12V', N'Libre de mantenimiento', NULL, NULL, NULL, NULL, N'65Ah', N'Derecha', 2),
    (13, 13, 11, NULL, NULL, NULL, N'Camioneta', NULL, NULL, NULL, NULL, 24, N'12V', N'Calcio-calcio', NULL, NULL, NULL, NULL, N'80Ah', N'Izquierda', 2),
    (14, 14, 10, NULL, NULL, NULL, N'Moto', NULL, NULL, NULL, NULL, 12, N'12V', N'Sellada AGM', NULL, NULL, NULL, NULL, N'9Ah', N'Izquierda', 2),
    (15, 15, 12, NULL, NULL, NULL, N'Auto', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'Tapas de válvula', N'Llanta', NULL, NULL, 2),
    (16, 16, 10, NULL, NULL, NULL, N'Auto', NULL, NULL, NULL, NULL, 6, NULL, NULL, NULL, NULL, N'Sensor TPMS', N'Llanta', NULL, NULL, 2);
SET IDENTITY_INSERT [dbo].[DetalleProductos] OFF;

-- ------------------------------------------------------------
-- Inventario (16 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Inventario] ON;
INSERT INTO [dbo].[Inventario] ([Id], [IdEstadoRegistro], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega])
VALUES
    (1, 2, 1, N'LLA-0001', 24, 8, 1, DATEADD(DAY, -40, @hoy), 24, 1),
    (2, 2, 2, N'LLA-0002', 18, 8, 1, DATEADD(DAY, -40, @hoy), 20, 1),
    (3, 2, 3, N'LLA-0003', 9, 8, 1, DATEADD(DAY, -40, @hoy), 12, 1),
    (4, 2, 4, N'LLA-0004', 14, 8, 1, DATEADD(DAY, -40, @hoy), 20, 1),
    (5, 2, 5, N'LLA-0005', 10, 8, 1, DATEADD(DAY, -40, @hoy), 20, 1),
    (6, 2, 6, N'LLA-0006', 7, 8, 1, DATEADD(DAY, -40, @hoy), 8, 1),
    (7, 2, 7, N'LLA-0007', 14, 8, 1, DATEADD(DAY, -35, @hoy), 20, 3),
    (8, 2, 8, N'LUB-0001', 55, 20, 1, DATEADD(DAY, -40, @hoy), 60, 2),
    (9, 2, 9, N'LUB-0002', 41, 20, 1, DATEADD(DAY, -40, @hoy), 48, 2),
    (10, 2, 10, N'LUB-0003', 79, 20, 1, DATEADD(DAY, -35, @hoy), 80, 3),
    (11, 2, 11, N'LUB-0004', 35, 20, 1, DATEADD(DAY, -40, @hoy), 36, 2),
    (12, 2, 12, N'BAT-0001', 13, 5, 1, DATEADD(DAY, -40, @hoy), 15, 1),
    (13, 2, 13, N'BAT-0002', 0, 5, 2, DATEADD(DAY, -40, @hoy), 1, 1),
    (14, 2, 14, N'BAT-0003', 10, 5, 1, DATEADD(DAY, -35, @hoy), 10, 3),
    (15, 2, 15, N'ACC-0001', 98, 10, 1, DATEADD(DAY, -35, @hoy), 100, 3),
    (16, 2, 16, N'ACC-0002', 19, 10, 1, DATEADD(DAY, -35, @hoy), 20, 3);
SET IDENTITY_INSERT [dbo].[Inventario] OFF;

-- ------------------------------------------------------------
-- Vehiculos (8 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Vehiculos] ON;
INSERT INTO [dbo].[Vehiculos] ([Id], [IdEstado], [Placa], [Marca], [Modelo], [Anio], [Kilometraje], [IdCliente])
VALUES
    (1, 2, N'KLM123', N'Mazda', N'3 Touring', 2018, 66200, 2),
    (2, 2, N'UVX456', N'Chevrolet', N'Spark GT', 2016, 98300, 3),
    (3, 2, N'FTR789', N'Chevrolet', N'Sail', 2019, 42000, 4),
    (4, 2, N'SZB321', N'Toyota', N'Hilux 4x4', 2019, 112500, 5),
    (5, 2, N'ABC12D', N'Honda', N'CB 190R', 2022, 18600, 6),
    (6, 2, N'TGH987', N'Kia', N'Picanto', 2021, 33000, 7),
    (7, 2, N'WQE258', N'Nissan', N'Frontier', 2017, 120800, 6),
    (8, 2, N'KPL741', N'Chevrolet', N'Onix', 2022, 12600, 1);
SET IDENTITY_INSERT [dbo].[Vehiculos] OFF;

-- ------------------------------------------------------------
-- OrdenesTrabajo (9 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[OrdenesTrabajo] ON;
INSERT INTO [dbo].[OrdenesTrabajo] ([Id], [IdEstado], [Numero], [IdVehiculo], [IdEmpleado], [Tipo], [Descripcion], [Kilometraje], [Observaciones], [Total], [Estado], [FechaIngreso], [FechaEntrega], [ProximaFecha], [ProximoKm], [RecordatorioEnviado])
VALUES
    (1, 3, N'OT-2026-0001', 1, 4, N'Alineación y balanceo', N'Alineación y balanceo de las cuatro llantas', 64500, N'Sin novedades', 100000.00, N'Entregado', DATEADD(SECOND, 30600, CAST(DATEADD(DAY, -25, @hoy) AS DateTime)), DATEADD(SECOND, 38700, CAST(DATEADD(DAY, -25, @hoy) AS DateTime)), DATEADD(DAY, 157, @hoy), 74500, 0),
    (2, 3, N'OT-2026-0002', 2, 4, N'Cambio de aceite', N'Cambio de aceite y filtro con Mobil Super 2000 10W-40', 97500, NULL, 35000.00, N'Entregado', DATEADD(SECOND, 32400, CAST(DATEADD(DAY, -23, @hoy) AS DateTime)), DATEADD(SECOND, 35400, CAST(DATEADD(DAY, -23, @hoy) AS DateTime)), DATEADD(DAY, 69, @hoy), 102500, 0),
    (3, 3, N'OT-2026-0003', 3, 4, N'Montaje y desmontaje', N'Montaje de 4 llantas Pirelli 185/60 R15', 41200, NULL, 80000.00, N'Entregado', DATEADD(SECOND, 50400, CAST(DATEADD(DAY, -20, @hoy) AS DateTime)), DATEADD(SECOND, 58200, CAST(DATEADD(DAY, -20, @hoy) AS DateTime)), DATEADD(DAY, 162, @hoy), 51200, 0),
    (4, 1, N'OT-2026-0004', 8, 5, N'Cambio de aceite', N'Cambio de aceite y filtro', 11800, N'Cancelada por el cliente', 35000.00, N'Cancelado', DATEADD(SECOND, 39600, CAST(DATEADD(DAY, -18, @hoy) AS DateTime)), NULL, NULL, NULL, 0),
    (5, 3, N'OT-2026-0005', 4, 4, N'Montaje y alineación', N'Montaje de 4 llantas Hankook y alineación', 111800, N'Se recomendó rotación en 10.000 km', 140000.00, N'Entregado', DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -16, @hoy) AS DateTime)), DATEADD(SECOND, 45000, CAST(DATEADD(DAY, -16, @hoy) AS DateTime)), DATEADD(DAY, 166, @hoy), 121800, 0),
    (6, 3, N'OT-2026-0006', 5, 4, N'Montaje y desmontaje', N'Cambio de llanta trasera de motocicleta', 18300, NULL, 20000.00, N'Entregado', DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -14, @hoy) AS DateTime)), DATEADD(SECOND, 38400, CAST(DATEADD(DAY, -14, @hoy) AS DateTime)), DATEADD(DAY, 168, @hoy), 28300, 0),
    (7, 3, N'OT-2026-0007', 7, 4, N'Alineación y balanceo', N'Alineación y balanceo de camioneta de flota', 120300, NULL, 100000.00, N'Entregado', DATEADD(SECOND, 29700, CAST(DATEADD(DAY, -10, @hoy) AS DateTime)), DATEADD(SECOND, 39600, CAST(DATEADD(DAY, -10, @hoy) AS DateTime)), DATEADD(DAY, 172, @hoy), 130300, 0),
    (8, 3, N'OT-2026-0008', 8, 5, N'Cambio de aceite', N'Cambio de aceite sintético Terpel 5W-30', 12400, NULL, 35000.00, N'Entregado', DATEADD(SECOND, 54000, CAST(DATEADD(DAY, -8, @hoy) AS DateTime)), DATEADD(SECOND, 56700, CAST(DATEADD(DAY, -8, @hoy) AS DateTime)), DATEADD(DAY, 84, @hoy), 17400, 0),
    (9, 2, N'OT-2026-0009', 6, 4, N'Revisión de batería', N'Diagnóstico de carga y cambio de batería MAC Silver', 32800, N'Pendiente de entrega al cliente', 15000.00, N'En proceso', DATEADD(SECOND, 54000, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), NULL, NULL, NULL, 0);
SET IDENTITY_INSERT [dbo].[OrdenesTrabajo] OFF;

-- ------------------------------------------------------------
-- Facturas (22 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Facturas] ON;
INSERT INTO [dbo].[Facturas] ([Id], [IdEstado], [IdLlanteria], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones])
VALUES
    (1, 3, 1, DATEADD(DAY, -169, @hoy), 2, 830000.00, 157700.00, 0.00, 987700.00, NULL),
    (2, 3, 1, DATEADD(DAY, -155, @hoy), 3, 150000.00, 28500.00, 0.00, 178500.00, NULL),
    (3, 3, 1, DATEADD(DAY, -140, @hoy), 2, 100000.00, 19000.00, 0.00, 119000.00, NULL),
    (4, 3, 1, DATEADD(DAY, -127, @hoy), 3, 140000.00, 26600.00, 0.00, 166600.00, NULL),
    (5, 3, 1, DATEADD(DAY, -114, @hoy), 4, 770000.00, 146300.00, 0.00, 916300.00, NULL),
    (6, 3, 1, DATEADD(DAY, -100, @hoy), 5, 465000.00, 88350.00, 0.00, 553350.00, NULL),
    (7, 3, 1, DATEADD(DAY, -86, @hoy), 6, 1520000.00, 288800.00, 0.00, 1808800.00, NULL),
    (8, 3, 1, DATEADD(DAY, -72, @hoy), 7, 300000.00, 57000.00, 0.00, 357000.00, NULL),
    (9, 3, 2, DATEADD(DAY, -64, @hoy), 1, 620000.00, 117800.00, 0.00, 737800.00, NULL),
    (10, 3, 1, DATEADD(DAY, -51, @hoy), 3, 620000.00, 117800.00, 0.00, 737800.00, NULL),
    (11, 3, 1, DATEADD(DAY, -43, @hoy), 2, 135000.00, 25650.00, 0.00, 160650.00, NULL),
    (12, 3, 1, DATEADD(DAY, -32, @hoy), 6, 1690000.00, 321100.00, 42250.00, 1968850.00, N'Cliente empresarial: se aplica retención en la fuente'),
    (13, 3, 1, DATEADD(DAY, -25, @hoy), 2, 100000.00, 19000.00, 0.00, 119000.00, NULL),
    (14, 3, 1, DATEADD(DAY, -23, @hoy), 3, 150000.00, 28500.00, 0.00, 178500.00, NULL),
    (15, 3, 1, DATEADD(DAY, -20, @hoy), 4, 1540000.00, 292600.00, 0.00, 1832600.00, NULL),
    (16, 1, 1, DATEADD(DAY, -18, @hoy), 3, 105000.00, 19950.00, 0.00, 124950.00, N'Anulada por error en los datos del cliente'),
    (17, 3, 1, DATEADD(DAY, -16, @hoy), 5, 2980000.00, 566200.00, 0.00, 3546200.00, NULL),
    (18, 3, 1, DATEADD(DAY, -14, @hoy), 6, 205000.00, 38950.00, 0.00, 243950.00, NULL),
    (19, 3, 1, DATEADD(DAY, -10, @hoy), 6, 1730000.00, 328700.00, 43250.00, 2015450.00, N'Cliente empresarial: se aplica retención en la fuente'),
    (20, 3, 2, DATEADD(DAY, -8, @hoy), 1, 83000.00, 15770.00, 0.00, 98770.00, NULL),
    (21, 3, 1, DATEADD(DAY, -2, @hoy), 2, 575000.00, 109250.00, 0.00, 684250.00, NULL),
    (22, 2, 1, DATEADD(DAY, -1, @hoy), 7, 465000.00, 88350.00, 0.00, 553350.00, N'Pendiente de pago');
SET IDENTITY_INSERT [dbo].[Facturas] OFF;

-- ------------------------------------------------------------
-- DetalleFactura (48 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[DetalleFactura] ON;
INSERT INTO [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal], [IdEstado])
VALUES
    (1, 1, 2, NULL, N'LLA-0002', N'Bridgestone Turanza T005 195/65 R15', 2, 395000.00, 790000.00, 3),
    (2, 1, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 2, 20000.00, 40000.00, 3),
    (3, 2, NULL, 4, N'SRV-0004', N'Cambio de aceite', 1, 35000.00, 35000.00, 3),
    (4, 2, 8, NULL, N'LUB-0001', N'Mobil Super 2000 10W-40 Galón', 1, 115000.00, 115000.00, 3),
    (5, 3, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (6, 3, NULL, 2, N'SRV-0002', N'Balanceo', 1, 40000.00, 40000.00, 3),
    (7, 4, 9, NULL, N'LUB-0002', N'Castrol GTX 20W-50 Galón', 1, 105000.00, 105000.00, 3),
    (8, 4, NULL, 4, N'SRV-0004', N'Cambio de aceite', 1, 35000.00, 35000.00, 3),
    (9, 5, 4, NULL, N'LLA-0004', N'Pirelli Cinturato P1 185/60 R15', 2, 365000.00, 730000.00, 3),
    (10, 5, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 2, 20000.00, 40000.00, 3),
    (11, 6, 12, NULL, N'BAT-0001', N'Batería MAC Silver 12V 65Ah', 1, 450000.00, 450000.00, 3),
    (12, 6, NULL, 5, N'SRV-0005', N'Revisión de batería', 1, 15000.00, 15000.00, 3),
    (13, 7, 5, NULL, N'LLA-0005', N'Hankook Dynapro HT 265/65 R17', 2, 710000.00, 1420000.00, 3),
    (14, 7, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 2, 20000.00, 40000.00, 3),
    (15, 7, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (16, 8, 8, NULL, N'LUB-0001', N'Mobil Super 2000 10W-40 Galón', 2, 115000.00, 230000.00, 3),
    (17, 8, NULL, 4, N'SRV-0004', N'Cambio de aceite', 2, 35000.00, 70000.00, 3),
    (18, 9, 7, NULL, N'LLA-0007', N'Hankook Kinergy Eco 175/70 R13', 2, 290000.00, 580000.00, 3),
    (19, 9, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 2, 20000.00, 40000.00, 3),
    (20, 10, 3, NULL, N'LLA-0003', N'Goodyear EfficientGrip 225/45 R17', 1, 560000.00, 560000.00, 3),
    (21, 10, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 1, 20000.00, 20000.00, 3),
    (22, 10, NULL, 2, N'SRV-0002', N'Balanceo', 1, 40000.00, 40000.00, 3),
    (23, 11, 11, NULL, N'LUB-0004', N'Mobil Delvac 15W-40 Galón', 1, 135000.00, 135000.00, 3),
    (24, 12, 5, NULL, N'LLA-0005', N'Hankook Dynapro HT 265/65 R17', 2, 710000.00, 1420000.00, 3),
    (25, 12, 9, NULL, N'LUB-0002', N'Castrol GTX 20W-50 Galón', 2, 105000.00, 210000.00, 3),
    (26, 12, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (27, 13, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (28, 13, NULL, 2, N'SRV-0002', N'Balanceo', 1, 40000.00, 40000.00, 3),
    (29, 14, 8, NULL, N'LUB-0001', N'Mobil Super 2000 10W-40 Galón', 1, 115000.00, 115000.00, 3),
    (30, 14, NULL, 4, N'SRV-0004', N'Cambio de aceite', 1, 35000.00, 35000.00, 3),
    (31, 15, 4, NULL, N'LLA-0004', N'Pirelli Cinturato P1 185/60 R15', 4, 365000.00, 1460000.00, 3),
    (32, 15, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 4, 20000.00, 80000.00, 3),
    (33, 16, 9, NULL, N'LUB-0002', N'Castrol GTX 20W-50 Galón', 1, 105000.00, 105000.00, 1),
    (34, 17, 5, NULL, N'LLA-0005', N'Hankook Dynapro HT 265/65 R17', 4, 710000.00, 2840000.00, 3),
    (35, 17, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 4, 20000.00, 80000.00, 3),
    (36, 17, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (37, 18, 6, NULL, N'LLA-0006', N'Michelin City Grip 90/90-18 Moto', 1, 185000.00, 185000.00, 3),
    (38, 18, NULL, 3, N'SRV-0003', N'Montaje y desmontaje de llanta', 1, 20000.00, 20000.00, 3),
    (39, 19, 5, NULL, N'LLA-0005', N'Hankook Dynapro HT 265/65 R17', 2, 710000.00, 1420000.00, 3),
    (40, 19, 9, NULL, N'LUB-0002', N'Castrol GTX 20W-50 Galón', 2, 105000.00, 210000.00, 3),
    (41, 19, NULL, 1, N'SRV-0001', N'Alineación', 1, 60000.00, 60000.00, 3),
    (42, 19, NULL, 2, N'SRV-0002', N'Balanceo', 1, 40000.00, 40000.00, 3),
    (43, 20, 10, NULL, N'LUB-0003', N'Terpel Celerity 5W-30 Cuarto', 1, 48000.00, 48000.00, 3),
    (44, 20, NULL, 4, N'SRV-0004', N'Cambio de aceite', 1, 35000.00, 35000.00, 3),
    (45, 21, 13, NULL, N'BAT-0002', N'Batería Willard Extrema 12V 80Ah', 1, 560000.00, 560000.00, 3),
    (46, 21, NULL, 5, N'SRV-0005', N'Revisión de batería', 1, 15000.00, 15000.00, 3),
    (47, 22, 12, NULL, N'BAT-0001', N'Batería MAC Silver 12V 65Ah', 1, 450000.00, 450000.00, 2),
    (48, 22, NULL, 5, N'SRV-0005', N'Revisión de batería', 1, 15000.00, 15000.00, 2);
SET IDENTITY_INSERT [dbo].[DetalleFactura] OFF;

-- ------------------------------------------------------------
-- Pedidos (4 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Pedidos] ON;
INSERT INTO [dbo].[Pedidos] ([Id], [IdEstado], [IdLlanteria], [Numero], [Token], [IdCliente], [Nombres], [Apellidos], [TipoDocumento], [NumeroDocumento], [Correo], [Telefono], [Pais], [Departamento], [Ciudad], [Direccion], [Complemento], [CodigoPostal], [Notas], [MetodoPago], [Estado], [CodigoCupon], [Subtotal], [Descuento], [BaseGravable], [Iva], [Envio], [Total], [Fecha])
VALUES
    (1, 3, 1, N'PED-2026-0001', N'18bb857bda99dc77bb6f33d1dba4acf0', 2, N'Carlos', N'Pérez Mendoza', N'C.C.', N'1065123402', N'carlos.perez@example.com', N'+57 3001234502', N'Colombia', N'Cesar', N'Valledupar', N'Carrera 9 # 20-41, Valledupar', N'Casa 1', N'200001', NULL, N'PSE', N'Entregado', NULL, 1120000.00, 0.00, 1120000.00, 212800.00, 15000.00, 1347800.00, DATEADD(SECOND, 69600, CAST(DATEADD(DAY, -12, @hoy) AS DateTime))),
    (2, 2, 1, N'PED-2026-0002', N'083466f71e1e95d791934ffc2a6dd930', 3, N'Laura', N'Gómez Rincón', N'C.C.', N'1065987403', N'laura.gomez@example.com', N'+57 3001234503', N'Colombia', N'Cesar', N'Valledupar', N'Carrera 9 # 20-42, Valledupar', NULL, N'200001', N'Llamar antes de entregar', N'Contra entrega', N'Pendiente', N'MONTAJE15', 325000.00, 15000.00, 310000.00, 58900.00, 12000.00, 380900.00, DATEADD(SECOND, 39900, CAST(DATEADD(DAY, -5, @hoy) AS DateTime))),
    (3, 3, 2, N'PED-2026-0003', N'6e1372b3f95b4fe55d0fade47733a710', NULL, N'Rafael', N'Zuleta Pimienta', N'C.C.', N'77888909', N'rafael.zuleta@example.com', N'+57 3112223344', N'Colombia', N'Cesar', N'Valledupar', N'Carrera 9 # 20-43, Valledupar', N'Casa 3', N'200001', NULL, N'Tarjeta', N'Enviado', NULL, 160000.00, 0.00, 160000.00, 30400.00, 10000.00, 200400.00, DATEADD(SECOND, 60000, CAST(DATEADD(DAY, -3, @hoy) AS DateTime))),
    (4, 2, 2, N'PED-2026-0004', N'4ac31b4f20a27fbdd1882fbe431c167e', 4, N'Andrés', N'Villazón Castro', N'C.C.', N'77012404', N'andres.villazon@example.com', N'+57 3001234504', N'Colombia', N'Cesar', N'Valledupar', N'Carrera 9 # 20-44, Valledupar', NULL, N'200001', NULL, N'Transferencia', N'Pendiente', NULL, 1160000.00, 0.00, 1160000.00, 220400.00, 0.00, 1380400.00, DATEADD(SECOND, 35400, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)));
SET IDENTITY_INSERT [dbo].[Pedidos] OFF;

-- ------------------------------------------------------------
-- PedidoItems (6 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[PedidoItems] ON;
INSERT INTO [dbo].[PedidoItems] ([Id], [IdEstado], [IdPedido], [IdProducto], [Descripcion], [Cantidad], [ValorUnitario], [Subtotal])
VALUES
    (1, 3, 1, 3, N'Goodyear EfficientGrip 225/45 R17', 2, 560000.00, 1120000.00),
    (2, 2, 2, 9, N'Castrol GTX 20W-50 Galón', 2, 105000.00, 210000.00),
    (3, 2, 2, 8, N'Mobil Super 2000 10W-40 Galón', 1, 115000.00, 115000.00),
    (4, 3, 3, 15, N'Juego de tapas de válvula x4', 2, 15000.00, 30000.00),
    (5, 3, 3, 16, N'Sensor de presión de llantas TPMS', 1, 130000.00, 130000.00),
    (6, 2, 4, 7, N'Hankook Kinergy Eco 175/70 R13', 4, 290000.00, 1160000.00);
SET IDENTITY_INSERT [dbo].[PedidoItems] OFF;

-- ------------------------------------------------------------
-- Citas (7 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Citas] ON;
INSERT INTO [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion], [RecordatorioEnviado], [IdEstado], [IdLlanteria])
VALUES
    (1, N'alineacion-y-balanceo', N'Alineación y balanceo', DATEADD(DAY, 4, @hoy), N'08:00', N'Mazda 3 Touring - KLM123', NULL, N'Efectivo', N'Revisar vibración en el volante', N'Carlos Pérez Mendoza', N'+57 3001234502', N'carlos.perez@example.com', 2, N'Confirmada', DATEADD(SECOND, 58800, CAST(DATEADD(DAY, -2, @hoy) AS DateTime)), 0, 2, 1),
    (2, N'cambio-de-aceite', N'Cambio de aceite', DATEADD(DAY, 6, @hoy), N'10:30', N'Chevrolet Spark GT - UVX456', NULL, N'Transferencia', NULL, N'Laura Gómez Rincón', N'+57 3001234503', N'laura.gomez@example.com', 3, N'Pendiente', DATEADD(SECOND, 29100, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), 0, 2, 1),
    (3, N'servicio-a-domicilio', N'Servicio a domicilio', DATEADD(DAY, 7, @hoy), N'14:00', N'Toyota Hilux 4x4 - SZB321', N'Carrera 5 # 14-20, Valledupar', N'Efectivo', N'Revisión de batería en casa', N'María Fernández Daza', N'+57 3001234505', N'maria.fernandez@example.com', 5, N'Confirmada', DATEADD(SECOND, 45600, CAST(DATEADD(DAY, -3, @hoy) AS DateTime)), 0, 2, 1),
    (4, N'revision-de-baterias', N'Revisión de baterías', DATEADD(DAY, -1, @hoy), N'15:00', N'Kia Picanto - TGH987', NULL, N'Tarjeta', N'El carro demora en encender', N'Sofía Ospino Brito', N'+57 3001234507', N'sofia.ospino@example.com', 7, N'Completada', DATEADD(SECOND, 33000, CAST(DATEADD(DAY, -3, @hoy) AS DateTime)), 1, 2, 1),
    (5, N'montaje-de-llantas', N'Montaje y desmontaje de llantas', DATEADD(DAY, -16, @hoy), N'08:00', N'Toyota Hilux 4x4 - SZB321', NULL, N'Tarjeta', N'Cuatro llantas nuevas', N'María Fernández Daza', N'+57 3001234505', N'maria.fernandez@example.com', 5, N'Completada', DATEADD(SECOND, 63000, CAST(DATEADD(DAY, -20, @hoy) AS DateTime)), 1, 2, 1),
    (6, N'alineacion-y-balanceo', N'Alineación y balanceo', DATEADD(DAY, 10, @hoy), N'09:00', N'Renault Logan - ZXC159', NULL, N'Efectivo', NULL, N'Rafael Zuleta Pimienta', N'+57 3112223344', N'rafael.zuleta@example.com', NULL, N'Pendiente', DATEADD(SECOND, 27000, CAST(@hoy AS DateTime)), 0, 2, 2),
    (7, N'cambio-de-aceite', N'Cambio de aceite', DATEADD(DAY, -18, @hoy), N'11:00', N'Chevrolet Onix - KPL741', NULL, N'Efectivo', NULL, N'Ana Usuaria', N'+57 3000000000', N'ana.usuaria@example.com', 1, N'Cancelada', DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -21, @hoy) AS DateTime)), 1, 2, 2);
SET IDENTITY_INSERT [dbo].[Citas] OFF;

-- ------------------------------------------------------------
-- CanjeIncentivos (4 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[CanjeIncentivos] ON;
INSERT INTO [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor], [IdEstado])
VALUES
    (1, 3, 1, DATEADD(SECOND, 37200, CAST(DATEADD(DAY, -15, @hoy) AS DateTime)), 4, 2),
    (2, 5, 2, DATEADD(SECOND, 57900, CAST(DATEADD(DAY, -10, @hoy) AS DateTime)), 1, 2),
    (3, 6, 3, DATEADD(SECOND, 45000, CAST(DATEADD(DAY, -6, @hoy) AS DateTime)), 4, 2),
    (4, 2, 5, DATEADD(SECOND, 35100, CAST(DATEADD(DAY, -4, @hoy) AS DateTime)), 5, 2);
SET IDENTITY_INSERT [dbo].[CanjeIncentivos] OFF;

-- ------------------------------------------------------------
-- Resenias (7 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Resenias] ON;
INSERT INTO [dbo].[Resenias] ([Id], [IdEstado], [IdCliente], [Nombre], [Calificacion], [Comentario], [Visible], [IdUsuario], [Usuario], [Fecha])
VALUES
    (1, 2, 2, N'Carlos Pérez', 5, N'Excelente servicio, la alineación quedó perfecta y el carro ya no vibra.', 1, 7, N'carlos_35_7', DATEADD(SECOND, 47400, CAST(DATEADD(DAY, -25, @hoy) AS DateTime))),
    (2, 2, 3, N'Laura Gómez', 4, N'Buena atención y precios justos. Solo tuve que esperar un poco por el aceite.', 1, 8, N'laura_62_8', DATEADD(SECOND, 66600, CAST(DATEADD(DAY, -23, @hoy) AS DateTime))),
    (3, 2, 4, N'Andrés Villazón', 5, N'Me montaron las cuatro llantas en un par de horas y quedó todo balanceado.', 1, 9, N'andres_19_9', DATEADD(SECOND, 32400, CAST(DATEADD(DAY, -19, @hoy) AS DateTime))),
    (4, 2, 5, N'María Fernández', 5, N'Muy buena asesoría para escoger las llantas de mi camioneta.', 1, 10, N'maria_47_10', DATEADD(SECOND, 63900, CAST(DATEADD(DAY, -15, @hoy) AS DateTime))),
    (5, 2, 6, N'Jorge Arzuaga', 4, N'Atienden bien a las empresas y entregaron la camioneta el mismo día.', 1, 11, N'jorge_84_11', DATEADD(SECOND, 51900, CAST(DATEADD(DAY, -10, @hoy) AS DateTime))),
    (6, 2, 1, N'Ana Usuaria', 5, N'Cambio de aceite rápido y con buen trato. Volveré.', 1, 3, N'ana_12_3', DATEADD(SECOND, 59400, CAST(DATEADD(DAY, -8, @hoy) AS DateTime))),
    (7, 2, 7, N'Sofía Ospino', 3, N'El servicio fue bueno, pero esperaría más información sobre los tiempos de entrega.', 1, 12, N'sofia_26_12', DATEADD(SECOND, 36900, CAST(DATEADD(DAY, -4, @hoy) AS DateTime)));
SET IDENTITY_INSERT [dbo].[Resenias] OFF;

-- ------------------------------------------------------------
-- Sugerencias (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Sugerencias] ON;
INSERT INTO [dbo].[Sugerencias] ([Id], [IdEstado], [Area], [Calificacion], [Mensaje], [Nombre], [Correo], [IdUsuario], [Estado], [Fecha])
VALUES
    (1, 2, N'Página web', 4, N'Sería útil poder ver el historial de mis servicios y de mis puntos en la cuenta.', N'Carlos Pérez', N'carlos.perez@example.com', 7, N'Nueva', DATEADD(SECOND, 72900, CAST(DATEADD(DAY, -12, @hoy) AS DateTime2(7)))),
    (2, 2, N'Atención', 5, N'La atención de la sede Centro es muy buena, sigan así.', N'Laura Gómez', N'laura.gomez@example.com', 8, N'Revisada', DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -10, @hoy) AS DateTime2(7)))),
    (3, 2, N'Servicios', 3, N'Me gustaría que se pudiera agendar el servicio a domicilio los sábados en la tarde.', NULL, NULL, NULL, N'Nueva', DATEADD(SECOND, 31200, CAST(DATEADD(DAY, -7, @hoy) AS DateTime2(7)))),
    (4, 2, N'Precios', 4, N'Podrían publicar el precio de los servicios de taller junto al de los productos.', N'Jorge Arzuaga', N'jorge.arzuaga@example.com', 11, N'Implementada', DATEADD(SECOND, 55800, CAST(DATEADD(DAY, -29, @hoy) AS DateTime2(7)))),
    (5, 2, N'Instalaciones', 4, N'La sala de espera es cómoda, pero faltan más puestos de sombra en el parqueadero.', N'Sofía Ospino', N'sofia.ospino@example.com', 12, N'Nueva', DATEADD(SECOND, 58200, CAST(DATEADD(DAY, -1, @hoy) AS DateTime2(7))));
SET IDENTITY_INSERT [dbo].[Sugerencias] OFF;

-- ------------------------------------------------------------
-- Pqrs (5 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Pqrs] ON;
INSERT INTO [dbo].[Pqrs] ([Id], [IdEstado], [Radicado], [Tipo], [Nombre], [NumeroDocumento], [Correo], [Telefono], [Asunto], [Mensaje], [AutorizaDatos], [IdCliente], [Estado], [Respuesta], [FechaCreacion], [FechaRespuesta])
VALUES
    (1, 2, N'PQRS-2026-0001', N'Petición', N'Andrés Villazón Castro', N'77012404', N'andres.villazon@example.com', N'+57 3001234504', N'Certificado de garantía', N'Solicito copia del certificado de garantía de las llantas que compré el 10 de octubre.', 1, 4, N'Respondida', N'Le enviamos el certificado de garantía a su correo registrado. Cualquier duda, escríbanos al WhatsApp de la sede Centro.', DATEADD(SECOND, 33300, CAST(DATEADD(DAY, -18, @hoy) AS DateTime)), DATEADD(SECOND, 39600, CAST(DATEADD(DAY, -17, @hoy) AS DateTime))),
    (2, 2, N'PQRS-2026-0002', N'Queja', N'Laura Gómez Rincón', N'1065987403', N'laura.gomez@example.com', N'+57 3001234503', N'Demora en el servicio', N'El cambio de aceite tardó más de lo que me habían informado al agendar la cita.', 1, 3, N'En trámite', NULL, DATEADD(SECOND, 64800, CAST(DATEADD(DAY, -22, @hoy) AS DateTime)), NULL),
    (3, 2, N'PQRS-2026-0003', N'Reclamo', N'María Fernández Daza', N'49765405', N'maria.fernandez@example.com', N'+57 3001234505', N'Cobro de montaje', N'En mi factura aparece el montaje de cuatro llantas y la promoción indicaba que era sin costo en la compra de un juego completo.', 1, 5, N'Respondida', N'Revisamos su caso: la promoción aplica solo en campañas especiales. Le ofrecemos un descuento en su próxima alineación.', DATEADD(SECOND, 37800, CAST(DATEADD(DAY, -15, @hoy) AS DateTime)), DATEADD(SECOND, 55200, CAST(DATEADD(DAY, -14, @hoy) AS DateTime))),
    (4, 2, N'PQRS-2026-0004', N'Sugerencia', N'Luisa Cantillo', NULL, N'luisa.cantillo@example.com', NULL, N'Más horarios los sábados', N'Sería bueno tener atención hasta las 5 p. m. los sábados.', 1, NULL, N'Recibida', NULL, DATEADD(SECOND, 76200, CAST(DATEADD(DAY, -4, @hoy) AS DateTime)), NULL),
    (5, 2, N'PQRS-2026-0005', N'Petición', N'Sofía Ospino Brito', N'1065456407', N'sofia.ospino@example.com', N'+57 3001234507', N'Factura electrónica', N'Necesito que me reenvíen la factura de la batería a mi correo.', 1, 7, N'Recibida', NULL, DATEADD(SECOND, 60300, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)), NULL);
SET IDENTITY_INSERT [dbo].[Pqrs] OFF;

-- ------------------------------------------------------------
-- SolicitudesEmpresa (3 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[SolicitudesEmpresa] ON;
INSERT INTO [dbo].[SolicitudesEmpresa] ([Id], [IdEstado], [Empresa], [Nit], [Contacto], [Telefono], [Correo], [Ciudad], [TipoFlota], [NumeroVehiculos], [Mensaje], [Estado], [FechaCreacion])
VALUES
    (1, 2, N'Transportes del Cesar S.A.S.', N'900456789-3', N'Jorge Arzuaga Maestre', N'+57 3001234506', N'flotas@example.com', N'Valledupar', N'Transporte de carga', 12, N'Buscamos un plan de mantenimiento mensual para nuestras camionetas.', N'Aprobada', DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -28, @hoy) AS DateTime))),
    (2, 2, N'Taxis Sol y Sabana Ltda.', N'900765432-1', N'Wilson Redondo', N'+57 3115556677', N'taxis.sol@example.com', N'Valledupar', N'Taxis', 25, N'Queremos cotizar cambio de aceite y alineación para 25 taxis.', N'Contactada', DATEADD(SECOND, 52200, CAST(DATEADD(DAY, -11, @hoy) AS DateTime))),
    (3, 2, N'Distribuciones La Esperanza', NULL, N'Yolanda Brito', N'+57 3126667788', N'la.esperanza@example.com', N'Aguachica', N'Distribución de alimentos', 6, NULL, N'Pendiente', DATEADD(SECOND, 61500, CAST(DATEADD(DAY, -1, @hoy) AS DateTime)));
SET IDENTITY_INSERT [dbo].[SolicitudesEmpresa] OFF;

-- ------------------------------------------------------------
-- Gastos (20 filas)
-- ------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Gastos] ON;
INSERT INTO [dbo].[Gastos] ([Id], [IdEstado], [IdLlanteria], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport])
VALUES
    (1, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -182, @hoy) AS DateTime)), DATEADD(DAY, -178, @hoy), DATEADD(DAY, -180, @hoy), N'Pagado', NULL),
    (2, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -158, @hoy) AS DateTime)), DATEADD(DAY, -153, @hoy), DATEADD(DAY, -154, @hoy), N'Pagado', NULL),
    (3, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -151, @hoy) AS DateTime)), DATEADD(DAY, -147, @hoy), DATEADD(DAY, -149, @hoy), N'Pagado', NULL),
    (4, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -127, @hoy) AS DateTime)), DATEADD(DAY, -122, @hoy), DATEADD(DAY, -123, @hoy), N'Pagado', NULL),
    (5, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -121, @hoy) AS DateTime)), DATEADD(DAY, -117, @hoy), DATEADD(DAY, -119, @hoy), N'Pagado', NULL),
    (6, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -97, @hoy) AS DateTime)), DATEADD(DAY, -92, @hoy), DATEADD(DAY, -93, @hoy), N'Pagado', NULL),
    (7, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -90, @hoy) AS DateTime)), DATEADD(DAY, -86, @hoy), DATEADD(DAY, -88, @hoy), N'Pagado', NULL),
    (8, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -66, @hoy) AS DateTime)), DATEADD(DAY, -61, @hoy), DATEADD(DAY, -62, @hoy), N'Pagado', NULL),
    (9, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -59, @hoy) AS DateTime)), DATEADD(DAY, -55, @hoy), DATEADD(DAY, -57, @hoy), N'Pagado', NULL),
    (10, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -35, @hoy) AS DateTime)), DATEADD(DAY, -30, @hoy), DATEADD(DAY, -31, @hoy), N'Pagado', NULL),
    (11, 3, 1, 1, N'Arriendo sede Centro', 3500000.00, DATEADD(SECOND, 28800, CAST(DATEADD(DAY, -29, @hoy) AS DateTime)), DATEADD(DAY, -25, @hoy), DATEADD(DAY, -27, @hoy), N'Pagado', NULL),
    (12, 3, 2, 1, N'Arriendo sede Norte', 2800000.00, DATEADD(SECOND, 29100, CAST(DATEADD(DAY, -29, @hoy) AS DateTime)), DATEADD(DAY, -25, @hoy), DATEADD(DAY, -27, @hoy), N'Pagado', NULL),
    (13, 3, 1, 2, N'Energía y acueducto sede Centro', 780000.00, DATEADD(SECOND, 32400, CAST(DATEADD(DAY, -22, @hoy) AS DateTime)), DATEADD(DAY, -15, @hoy), DATEADD(DAY, -18, @hoy), N'Pagado', NULL),
    (14, 3, 2, 2, N'Energía y acueducto sede Norte', 540000.00, DATEADD(SECOND, 33000, CAST(DATEADD(DAY, -22, @hoy) AS DateTime)), DATEADD(DAY, -15, @hoy), DATEADD(DAY, -17, @hoy), N'Pagado', NULL),
    (15, 3, 1, 3, N'Nómina sede Centro', 3300000.00, DATEADD(SECOND, 36000, CAST(DATEADD(DAY, -5, @hoy) AS DateTime)), @hoy, DATEADD(DAY, -1, @hoy), N'Pagado', NULL),
    (16, 3, 2, 3, N'Nómina sede Norte', 1500000.00, DATEADD(SECOND, 36300, CAST(DATEADD(DAY, -5, @hoy) AS DateTime)), @hoy, DATEADD(DAY, -1, @hoy), N'Pagado', NULL),
    (17, 2, 1, 4, N'Compra de llantas a Distribuidora Andina', 12500000.00, DATEADD(SECOND, 39600, CAST(DATEADD(DAY, -10, @hoy) AS DateTime)), DATEADD(DAY, 11, @hoy), NULL, N'Pendiente', NULL),
    (18, 2, NULL, 5, N'IVA bimestral', 2100000.00, DATEADD(SECOND, 30600, CAST(DATEADD(DAY, -2, @hoy) AS DateTime)), DATEADD(DAY, 5, @hoy), NULL, N'Pendiente', NULL),
    (19, 3, 1, 6, N'Mantenimiento del compresor', 450000.00, DATEADD(SECOND, 54000, CAST(DATEADD(DAY, -13, @hoy) AS DateTime)), DATEADD(DAY, -10, @hoy), DATEADD(DAY, -12, @hoy), N'Pagado', NULL),
    (20, 3, 2, 7, N'Papelería y elementos de aseo', 120000.00, DATEADD(SECOND, 43200, CAST(DATEADD(DAY, -9, @hoy) AS DateTime)), DATEADD(DAY, -5, @hoy), DATEADD(DAY, -9, @hoy), N'Pagado', NULL);
SET IDENTITY_INSERT [dbo].[Gastos] OFF;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
