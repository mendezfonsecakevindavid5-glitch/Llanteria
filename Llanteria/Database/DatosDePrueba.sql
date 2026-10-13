/* ============================================================================
   Llantería - DATOS DE PRUEBA  (base LlanteriaDB)
   - Mínimo 10 registros por tabla. Sin imágenes (las columnas de imagen quedan en NULL).
   - Las tablas Sexo, TipoDocumento, EstadoInventario y CategoriaGasto ya traen sus datos del script
     de estructura; aquí solo se completa Roles (agrega "Cliente").
   - Pensado para ejecutarse UNA vez sobre tablas vacías, en este orden.
   - Contraseñas ya cifradas (formato de la aplicación):
        admin (Administrador)        ->  Admin123*
        jperez ... hvega (Empleados) ->  Empleado123*
        Clientes (correo/documento)  ->  Cliente123*
     Cámbialas apenas entres.
   ============================================================================ */
USE [LlanteriaDB]
GO
SET NOCOUNT ON
GO
-- Roles: Administrador (todo el sistema), Empleado, Cliente (quien se registra en el club de puntos)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [NombreRol] = N'Cliente')
    INSERT [dbo].[Roles] ([NombreRol]) VALUES (N'Cliente');
GO
-- Bodegas: 10 registros
SET IDENTITY_INSERT [dbo].[Bodegas] ON;
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (1, N'Bodega Principal', N'Calle 15 # 12-40, Valledupar', 400);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (2, N'Patio de Llantas', N'Calle 15 # 12-40, patio trasero', 600);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (3, N'Bodega de Lubricantes', N'Carrera 9 # 14-22, Valledupar', 250);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (4, N'Sala de Exhibición', N'Calle 15 # 12-40, local 1', 80);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (5, N'Bodega Norte', N'Av. Simón Bolívar # 20-10', 300);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (6, N'Bodega Sur', N'Carrera 19 # 5-30', 300);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (7, N'Taller Móvil', N'Vehículo serviteca móvil', 40);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (8, N'Almacén de Baterías', N'Carrera 9 # 14-22, local 2', 120);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (9, N'Bodega de Devoluciones', N'Calle 15 # 12-40, bodega 3', 100);
INSERT [dbo].[Bodegas] ([Id], [NombreBodega], [Ubicacion], [CapacidadMaxima]) VALUES (10, N'Bodega de Accesorios', N'Calle 16 # 11-05', 150);
SET IDENTITY_INSERT [dbo].[Bodegas] OFF;
GO

-- Marcas: 10 registros
SET IDENTITY_INSERT [dbo].[Marcas] ON;
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (1, N'Michelin');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (2, N'Bridgestone');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (3, N'Goodyear');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (4, N'Pirelli');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (5, N'Continental');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (6, N'Hankook');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (7, N'Mobil');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (8, N'Castrol');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (9, N'Terpel');
INSERT [dbo].[Marcas] ([Id], [Nombre]) VALUES (10, N'Shell');
SET IDENTITY_INSERT [dbo].[Marcas] OFF;
GO

-- Proveedores: 10 registros
SET IDENTITY_INSERT [dbo].[Proveedores] ON;
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (1, N'Distribuidora Llantas del Caribe S.A.S.', N'Jorge Pérez', N'3001234501');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (2, N'Importadora Cesar Neumáticos', N'Martha Ríos', N'3001234502');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (3, N'Lubricantes y Aceites del Norte', N'Carlos Mendoza', N'3001234503');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (4, N'Baterías Energía Total', N'Luisa Fernández', N'3001234504');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (5, N'Neumáticos Andinos Ltda.', N'Andrés Gómez', N'3001234505');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (6, N'Comercializadora Ruta 80', N'Paola Castro', N'3001234506');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (7, N'Repuestos y Llantas La Nevada', N'Iván Torres', N'3001234507');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (8, N'Distribuciones Sol del Valle', N'Diana Ospino', N'3001234508');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (9, N'Aceites Premium Colombia', N'Hernán Vega', N'3001234509');
INSERT [dbo].[Proveedores] ([Id], [NombreEmpresa], [Contacto], [Telefono]) VALUES (10, N'Accesorios Automotrices Dorada', N'Sandra Peña', N'3001234510');
SET IDENTITY_INSERT [dbo].[Proveedores] OFF;
GO

-- Productos: 12 registros
SET IDENTITY_INSERT [dbo].[Productos] ON;
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (1, N'Michelin Primacy 4 205/55 R16', N'Llanta turismo, excelente agarre en mojado.', 380000, 495000, 1, N'Llantas', NULL, 2);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (2, N'Bridgestone Turanza T005 195/65 R15', N'Llanta confort para sedán.', 310000, 405000, 2, N'Llantas', NULL, 2);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (3, N'Goodyear Assurance 185/65 R14', N'Llanta económica para carro pequeño.', 245000, 320000, 5, N'Llantas', NULL, 2);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (4, N'Pirelli Cinturato P1 175/70 R13', N'Llanta urbana de bajo consumo.', 215000, 285000, 1, N'Llantas', NULL, 2);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (5, N'Continental ContiPowerContact 225/45 R17', N'Llanta deportiva de alto desempeño.', 440000, 575000, 2, N'Llantas', NULL, 1);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (6, N'Hankook Kinergy Eco 205/60 R16', N'Llanta de larga duración.', 330000, 430000, 7, N'Llantas', NULL, 2);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (7, N'Michelin Agilis 215/70 R15C', N'Llanta reforzada para camioneta y furgón.', 395000, 520000, 5, N'Llantas', NULL, 1);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (8, N'Mobil Super 10W-40 (galón)', N'Aceite semisintético para motor a gasolina.', 78000, 105000, 3, N'Aceites', NULL, 3);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (9, N'Castrol GTX 20W-50 (galón)', N'Aceite mineral para motores con kilometraje alto.', 64000, 89000, 9, N'Aceites', NULL, 3);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (10, N'Terpel Celerity 15W-40 diésel (galón)', N'Aceite para motores diésel.', 72000, 98000, 3, N'Aceites', NULL, 3);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (11, N'Shell Helix HX7 5W-30 (cuarto)', N'Aceite sintético, presentación de 1 litro.', 32000, 45000, 9, N'Aceites', NULL, 3);
INSERT [dbo].[Productos] ([Id], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [IdProveedor], [Categoria], [RutaImagen], [IdBodega]) VALUES (12, N'Batería 12V 650A caja 34', N'Batería libre de mantenimiento, 18 meses de garantía.', 290000, 375000, 4, N'Baterías', NULL, 8);
SET IDENTITY_INSERT [dbo].[Productos] OFF;
GO

-- DetalleProductos: 11 registros
SET IDENTITY_INSERT [dbo].[DetalleProductos] ON;
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (1, 1, 1, N'205', N'55', N'R16', N'91', N'V', NULL, NULL, 24);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (2, 2, 2, N'195', N'65', N'R15', N'91', N'H', NULL, NULL, 24);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (3, 3, 3, N'185', N'65', N'R14', N'86', N'H', NULL, NULL, 18);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (4, 4, 4, N'175', N'70', N'R13', N'82', N'T', NULL, NULL, 18);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (5, 5, 5, N'225', N'45', N'R17', N'94', N'W', NULL, NULL, 36);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (6, 6, 6, N'205', N'60', N'R16', N'92', N'H', NULL, NULL, 24);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (7, 7, 1, N'215', N'70', N'R15C', N'109/107', N'R', NULL, NULL, 24);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (8, 8, 7, NULL, NULL, NULL, NULL, NULL, N'10W-40', N'Semisintético', NULL);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (9, 9, 8, NULL, NULL, NULL, NULL, NULL, N'20W-50', N'Mineral', NULL);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (10, 10, 9, NULL, NULL, NULL, NULL, NULL, N'15W-40', N'Mineral diésel', NULL);
INSERT [dbo].[DetalleProductos] ([Id], [IdProducto], [IdMarca], [Ancho], [Perfil], [Diametro], [IndiceCarga], [IndiceVelocidad], [Viscosidad], [TipoAceite], [GarantiaMeses]) VALUES (11, 11, 10, NULL, NULL, NULL, NULL, NULL, N'5W-30', N'Sintético', NULL);
SET IDENTITY_INSERT [dbo].[DetalleProductos] OFF;
GO

-- TipoServicio: 10 registros
SET IDENTITY_INSERT [dbo].[TipoServicio] ON;
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (1, N'Montaje y balanceo (por llanta)', 25000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (2, N'Alineación 3D', 60000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (3, N'Cambio de aceite y filtro', 35000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (4, N'Rotación de llantas', 20000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (5, N'Revisión de suspensión', 40000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (6, N'Instalación de batería', 15000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (7, N'Reparación de pinchazo', 12000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (8, N'Serviteca móvil (visita a domicilio)', 50000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (9, N'Revisión de frenos', 45000, NULL);
INSERT [dbo].[TipoServicio] ([Id], [NombreServicio], [ValorServicio], [IconoPath]) VALUES (10, N'Cambio de amortiguadores (mano de obra)', 80000, NULL);
SET IDENTITY_INSERT [dbo].[TipoServicio] OFF;
GO

-- Empleados: 10 registros
SET IDENTITY_INSERT [dbo].[Empleados] ON;
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (1, NULL, N'Administrador', N'General', 1, N'1065001001', N'admin@example.com', N'3101000001', 4200000, N'Lun-Vie 7:30-17:30', 1, '1996-03-14', 1);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (2, NULL, N'Jorge Luis', N'Pérez Molina', 1, N'1065001002', N'jorge.perez@example.com', N'3101000002', 2100000, N'Lun-Vie 7:30-17:30', 1, '1990-07-02', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (3, NULL, N'Marta Cecilia', N'Ríos Daza', 1, N'1065001003', N'marta.rios@example.com', N'3101000003', 2300000, N'Lun-Sáb 8:00-16:00', 2, '1988-11-23', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (4, NULL, N'Carlos Andrés', N'Mendoza Ortiz', 1, N'1065001004', N'carlos.mendoza@example.com', N'3101000004', 1900000, N'Lun-Vie 7:30-17:30', 1, '1994-01-30', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (5, NULL, N'Luisa Fernanda', N'Fernández Cuello', 1, N'1065001005', N'luisa.fernandez@example.com', N'3101000005', 2000000, N'Lun-Sáb 8:00-16:00', 2, '1995-05-09', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (6, NULL, N'Andrés Felipe', N'Gómez Araújo', 1, N'1065001006', N'andres.gomez@example.com', N'3101000006', 1850000, N'Lun-Vie 7:30-17:30', 1, '1992-09-17', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (7, NULL, N'Paola Andrea', N'Castro Maestre', 1, N'1065001007', N'paola.castro@example.com', N'3101000007', 1950000, N'Lun-Sáb 8:00-16:00', 2, '1991-12-04', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (8, NULL, N'Iván Darío', N'Torres Pumarejo', 1, N'1065001008', N'ivan.torres@example.com', N'3101000008', 1800000, N'Lun-Vie 7:30-17:30', 1, '1998-04-21', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (9, NULL, N'Diana Carolina', N'Ospino Lacouture', 1, N'1065001009', N'diana.ospino@example.com', N'3101000009', 2200000, N'Lun-Sáb 8:00-16:00', 2, '1989-08-12', 2);
INSERT [dbo].[Empleados] ([Id], [Foto], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Correo], [Telefono], [Salario], [Horario], [IdSexo], [FechaNacimiento], [IdRol]) VALUES (10, NULL, N'Hernán Alberto', N'Vega Zuleta', 1, N'1065001010', N'hernan.vega@example.com', N'3101000010', 1850000, N'Lun-Vie 7:30-17:30', 1, '1993-02-06', 2);
SET IDENTITY_INSERT [dbo].[Empleados] OFF;
GO

-- Usuarios: 10 registros
SET IDENTITY_INSERT [dbo].[Usuarios] ON;
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (1, N'admin', N'admin@example.com', N'AQAAAAIAAYagAAAAEHXJ0fX5cE4gKbVvGClKapg1dHmN9/fUHsmBvTWVZxdYdFIaq+z/iZoydQlbcFoysw==', 1, 1, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (2, N'jperez', N'jorge.perez@example.com', N'AQAAAAIAAYagAAAAEEn6E1rcfEM5hdHdMdDyl2A8r99qzWjyW1g7tmFIKJV7kQaYeHk+BpH59UNzYrqgSQ==', 2, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (3, N'mrios', N'marta.rios@example.com', N'AQAAAAIAAYagAAAAEDBZvsE+6d7YJEJR71sd7NnmX2jA8mk6+pXrmtIMN1GDrTWtW9hRX9EMHnHoDq5pjA==', 3, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (4, N'cmendoza', N'carlos.mendoza@example.com', N'AQAAAAIAAYagAAAAECjDiHhvQMe6pEZzOd6PCH7gNFevqS1lpcxPfO/hOK7Kl5g0DHzxtuRwkkvptooCDg==', 4, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (5, N'lfernandez', N'luisa.fernandez@example.com', N'AQAAAAIAAYagAAAAEM9M+zXfnzwDtLYHuHvMk93v+qXKzrF2kaYfpUxCYxv8eF3sNmqhxpg/gVm6XSUf2A==', 5, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (6, N'agomez', N'andres.gomez@example.com', N'AQAAAAIAAYagAAAAEGzZ3qz7tlNjT+14eVSjmtuIHyAz3MDtBvf+irIAfyq7Up79zUOvU5Nn3SvIbQQdsg==', 6, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (7, N'pcastro', N'paola.castro@example.com', N'AQAAAAIAAYagAAAAEAiM8z1o8W6EG+tZyfKBBeaFTmcCWx5Jhkw2alUGf28Lj+c9Oglot5oMQl+DJaCkHA==', 7, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (8, N'itorres', N'ivan.torres@example.com', N'AQAAAAIAAYagAAAAEAbTo8+kdRUSM+5jeHsgGVm27FWFAjOr4V7e4r5ymUZBbddgQl2DDynUO57JrJZKuA==', 8, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (9, N'dospino', N'diana.ospino@example.com', N'AQAAAAIAAYagAAAAEKSj5vrhzKQ1l1wk2mv8m31pBJ1d4Ky7FJnSG5pfBuQilFBZymMi4SsVFDZurvT/MQ==', 9, 2, N'Activo', NULL, NULL, '2026-09-01 08:00:00', '2026-10-11 17:30:00', 1);
INSERT [dbo].[Usuarios] ([Id], [Username], [Correo], [PasswordHash], [IdEmpleado], [IdRol], [Estado], [MotivoSancion], [FechaFinBaneo], [FechaCreacion], [UltimaConexion], [IdTipoDocumento]) VALUES (10, N'hvega', N'hernan.vega@example.com', N'AQAAAAIAAYagAAAAEPreZB3ZbkVWgEjFDT1MLIcYNY1SMkqHCJbWlvDd+lwxYGyKBQJU0/0oljS3mRMy9g==', 10, 2, N'Inactivo', NULL, NULL, '2026-09-01 08:00:00', NULL, 1);
SET IDENTITY_INSERT [dbo].[Usuarios] OFF;
GO

-- PerfilUsuario: 10 registros
SET IDENTITY_INSERT [dbo].[PerfilUsuario] ON;
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (1, 1, N'Administrador general del sistema.', N'Dark', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (2, 2, N'Atención al cliente y facturación.', N'Light', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (3, 3, N'Encargada de inventario.', N'Light', NULL, 0);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (4, 4, N'Montaje y balanceo.', N'Light', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (5, 5, N'Caja y facturación.', N'Dark', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (6, 6, N'Alineación y suspensión.', N'Light', NULL, 0);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (7, 7, N'Atención al cliente.', N'Light', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (8, 8, N'Serviteca móvil.', N'Light', NULL, 1);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (9, 9, N'Compras y proveedores.', N'Dark', NULL, 0);
INSERT [dbo].[PerfilUsuario] ([Id], [IdUsuario], [Bio], [TemaPreferencia], [FotoCircular], [NotificacionesActivas]) VALUES (10, 10, N'Apoyo de taller.', N'Light', NULL, 1);
SET IDENTITY_INSERT [dbo].[PerfilUsuario] OFF;
GO

-- Clientes: 12 registros
SET IDENTITY_INSERT [dbo].[Clientes] ON;
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (1, N'Valentina', N'Maestre Daza', 1, N'1065100001', N'3201000001', N'valentina.maestre@example.com', 2, NULL, 2626, N'AQAAAAIAAYagAAAAEJGLpwlJk1GNV9g9oh6/mtQEbWSaLTxp3jUqKS01m14oDGrHp0uP+f902Vdqsa4VKA==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (2, N'Santiago', N'Araújo Pinto', 1, N'1065100002', N'3201000002', N'santiago.araujo@example.com', 1, NULL, 216, N'AQAAAAIAAYagAAAAEKcn9Jsxx86tkfSt3rGgejmKH+RaZTR5wtjjU6pZubPGCql8RDmWOAbPtkVu39Oa1w==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (3, N'Camila', N'Zuleta Lacouture', 1, N'1065100003', N'3201000003', N'camila.zuleta@example.com', 2, NULL, 871, N'AQAAAAIAAYagAAAAEAn0LnDAGzWz1YBJWYp5fWrXbt5rnxE3d0IxoNmpFIyD0om+FPl42Ml9dhSOKhs8ZQ==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (4, N'Mateo', N'Cuello Ariza', 1, N'1065100004', N'3201000004', N'mateo.cuello@example.com', 1, NULL, 514, N'AQAAAAIAAYagAAAAEJtGaUaunOWsr9J/+D2t/UfF2mP8aRg6CyceAk4HzCZMG88mAwkwKlKxTVoFfp23dA==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (5, N'Isabella', N'Pumarejo Rosado', 1, N'1065100005', N'3201000005', N'isabella.pumarejo@example.com', 2, NULL, 2639, N'AQAAAAIAAYagAAAAEKj2VW7fAS+6I7VGA7fr391CQIDV3pDk/SV+itgUwneH2nU6iVrDL9quQ1gGHQTmfQ==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (6, N'Sebastián', N'Ortiz Barros', 1, N'1065100006', N'3201000006', N'sebastian.ortiz@example.com', 1, NULL, 305, N'AQAAAAIAAYagAAAAEGAVKxc0thbTEzFuq3JmGIaccHuG1MYZGeD0O/Mi3zcNhMET1n7LhWb16TeXV7UTFQ==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (7, N'Daniela', N'Molina Cabello', 1, N'1065100007', N'3201000007', N'daniela.molina@example.com', 2, NULL, 923, N'AQAAAAIAAYagAAAAEFCO+7oj3p4PVcdNq1JTZfSfylc8jJMyDfSCALDBoz+Xn6EOA1rzWju3I0oVKscJBg==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (8, N'Nicolás', N'Ospino Mejía', 1, N'1065100008', N'3201000008', N'nicolas.ospino@example.com', 1, NULL, 310, N'AQAAAAIAAYagAAAAECZS9UXvRdTgzwLv/XTFHx0HdUXnfWPXe6dLIsGZ6NSMxJiPmzIEt+l4uQ8PwvIxkA==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (9, N'Mariana', N'Peñaloza Gutiérrez', 1, N'1065100009', N'3201000009', N'mariana.penaloza@example.com', 2, NULL, 251, N'AQAAAAIAAYagAAAAEGpt5cjsT0BZiPjOITBJGG8CkFz7qzjCfKwOUDAF9gLCWDJYWl++zwABptrR91alVQ==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (10, N'Julián', N'Redondo Fuentes', 1, N'1065100010', N'3201000010', N'julian.redondo@example.com', 1, NULL, 2087, N'AQAAAAIAAYagAAAAEDJA6YyHLV8qoDB7LUxYU2Xupj4drG97JFm6Zx5nOQoHO9uis/GNWANjKaqu0UgwzA==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (11, N'Laura', N'Quintero Mora', 1, N'1065100011', N'3201000011', N'laura.quintero@example.com', 2, NULL, 756, N'AQAAAAIAAYagAAAAEK3yfIhtEVRFM692PCrXw23xZlzw4pT0goYhdxuQ5HRl0rxJ9fLygbaF4H4VaEqJdA==');
INSERT [dbo].[Clientes] ([Id], [Nombres], [Apellidos], [IdDocumento], [NumeroDocumento], [Telefono], [Correo], [IdSexo], [Foto], [PuntosAcumulados], [PasswordHash]) VALUES (12, N'David', N'Escobar Rangel', 1, N'1065100012', N'3201000012', N'david.escobar@example.com', 1, NULL, 1417, N'AQAAAAIAAYagAAAAEKC/l6eOA4dZPAFFZ7StBXMSciyToXpi7OrSmonlJTdG510mIXl5YzeggGBRLIZSVw==');
SET IDENTITY_INSERT [dbo].[Clientes] OFF;
GO

-- Vehiculos: 12 registros
SET IDENTITY_INSERT [dbo].[Vehiculos] ON;
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (1, N'KLM123', N'Chevrolet', N'Spark GT 2018', 1);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (2, N'NPR482', N'Renault', N'Logan 2020', 2);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (3, N'UVX307', N'Mazda', N'Mazda 3 2019', 3);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (4, N'FTR915', N'Kia', N'Picanto 2021', 4);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (5, N'GHB664', N'Toyota', N'Hilux 2017', 5);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (6, N'JKL208', N'Nissan', N'Versa 2022', 6);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (7, N'PQS731', N'Chevrolet', N'Onix 2023', 7);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (8, N'MNB459', N'Hyundai', N'Tucson 2020', 8);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (9, N'RTY125', N'Renault', N'Duster 2019', 9);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (10, N'CDE876', N'Suzuki', N'Swift 2018', 10);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (11, N'WSX340', N'Mazda', N'CX-5 2021', 11);
INSERT [dbo].[Vehiculos] ([Id], [Placa], [Marca], [Modelo], [IdCliente]) VALUES (12, N'ZAQ592', N'Ford', N'Ranger 2016', 12);
SET IDENTITY_INSERT [dbo].[Vehiculos] OFF;
GO

-- Facturas: 12 registros
SET IDENTITY_INSERT [dbo].[Facturas] ON;
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (1, '2026-09-03', 1, 2140000, 406600.0, 0, 2546600.0, NULL);
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (2, '2026-09-05', 2, 140000, 26600.0, 0, 166600.0, N'Pago en efectivo.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (3, '2026-09-09', 3, 690000, 131100.0, 0, 821100.0, N'Pago con tarjeta.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (4, '2026-09-12', 4, 390000, 74100.0, 0, 464100.0, NULL);
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (5, '2026-09-16', 5, 2260000, 429400.0, 0, 2689400.0, N'Cliente frecuente.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (6, '2026-09-19', 6, 215000, 40850.0, 0, 255850.0, N'Pago por transferencia.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (7, '2026-09-24', 7, 860000, 163400.0, 0, 1023400.0, NULL);
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (8, '2026-09-29', 8, 236000, 44840.0, 0, 280840.0, N'Entrega en bodega.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (9, '2026-10-02', 9, 169000, 32110.0, 0, 201110.0, NULL);
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (10, '2026-10-05', 10, 1880000, 357200.0, 0, 2237200.0, N'Cliente frecuente.');
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (11, '2026-10-08', 11, 594000, 112860.0, 0, 706860.0, NULL);
INSERT [dbo].[Facturas] ([Id], [Fecha], [IdCliente], [TotalBruto], [IVA], [RteFte], [TotalPagar], [Observaciones]) VALUES (12, '2026-10-10', 12, 1250000, 237500.0, 0, 1487500.0, N'Servicio a domicilio incluido.');
SET IDENTITY_INSERT [dbo].[Facturas] OFF;
GO

-- DetalleFactura: 30 registros
SET IDENTITY_INSERT [dbo].[DetalleFactura] ON;
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (1, 1, 1, NULL, N'P-1', N'Michelin Primacy 4 205/55 R16', 4, 495000, 1980000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (2, 1, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 4, 25000, 100000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (3, 1, NULL, 2, N'S-2', N'Alineación 3D', 1, 60000, 60000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (4, 2, 8, NULL, N'P-8', N'Mobil Super 10W-40 (galón)', 1, 105000, 105000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (5, 2, NULL, 3, N'S-3', N'Cambio de aceite y filtro', 1, 35000, 35000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (6, 3, 3, NULL, N'P-3', N'Goodyear Assurance 185/65 R14', 2, 320000, 640000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (7, 3, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 2, 25000, 50000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (8, 4, 12, NULL, N'P-12', N'Batería 12V 650A caja 34', 1, 375000, 375000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (9, 4, NULL, 6, N'S-6', N'Instalación de batería', 1, 15000, 15000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (10, 5, 7, NULL, N'P-7', N'Michelin Agilis 215/70 R15C', 4, 520000, 2080000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (11, 5, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 4, 25000, 100000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (12, 5, NULL, 2, N'S-2', N'Alineación 3D', 1, 60000, 60000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (13, 5, NULL, 4, N'S-4', N'Rotación de llantas', 1, 20000, 20000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (14, 6, 11, NULL, N'P-11', N'Shell Helix HX7 5W-30 (cuarto)', 4, 45000, 180000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (15, 6, NULL, 3, N'S-3', N'Cambio de aceite y filtro', 1, 35000, 35000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (16, 7, 2, NULL, N'P-2', N'Bridgestone Turanza T005 195/65 R15', 2, 405000, 810000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (17, 7, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 2, 25000, 50000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (18, 8, 10, NULL, N'P-10', N'Terpel Celerity 15W-40 diésel (galón)', 2, 98000, 196000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (19, 8, NULL, 5, N'S-5', N'Revisión de suspensión', 1, 40000, 40000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (20, 9, 9, NULL, N'P-9', N'Castrol GTX 20W-50 (galón)', 1, 89000, 89000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (21, 9, NULL, 3, N'S-3', N'Cambio de aceite y filtro', 1, 35000, 35000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (22, 9, NULL, 9, N'S-9', N'Revisión de frenos', 1, 45000, 45000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (23, 10, 6, NULL, N'P-6', N'Hankook Kinergy Eco 205/60 R16', 4, 430000, 1720000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (24, 10, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 4, 25000, 100000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (25, 10, NULL, 2, N'S-2', N'Alineación 3D', 1, 60000, 60000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (26, 11, 4, NULL, N'P-4', N'Pirelli Cinturato P1 175/70 R13', 2, 285000, 570000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (27, 11, NULL, 7, N'S-7', N'Reparación de pinchazo', 2, 12000, 24000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (28, 12, 5, NULL, N'P-5', N'Continental ContiPowerContact 225/45 R17', 2, 575000, 1150000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (29, 12, NULL, 1, N'S-1', N'Montaje y balanceo (por llanta)', 2, 25000, 50000);
INSERT [dbo].[DetalleFactura] ([Id], [IdFactura], [IdProducto], [IdServicio], [CodigoItem], [Descripción], [Cantidad], [ValorUnitario], [Subtotal]) VALUES (30, 12, NULL, 8, N'S-8', N'Serviteca móvil (visita a domicilio)', 1, 50000, 50000);
SET IDENTITY_INSERT [dbo].[DetalleFactura] OFF;
GO

-- Inventario: 15 registros
SET IDENTITY_INSERT [dbo].[Inventario] ON;
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (1, 1, N'LL-MIC-2055516', 20, 4, 1, '2026-08-20', 24, 2);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (2, 2, N'LL-BRI-1956515', 18, 4, 1, '2026-08-20', 20, 2);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (3, 3, N'LL-GOO-1856514', 14, 4, 1, '2026-08-20', 16, 2);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (4, 4, N'LL-PIR-1757013', 0, 4, 2, '2026-08-20', 2, 2);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (5, 5, N'LL-CON-2254517', 8, 3, 1, '2026-08-20', 10, 1);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (6, 6, N'LL-HAN-2056016', 14, 4, 1, '2026-08-20', 18, 2);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (7, 7, N'LL-MIC-2157015C', 3, 3, 1, '2026-08-20', 7, 1);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (8, 8, N'AC-MOB-10W40', 35, 6, 1, '2026-08-20', 36, 3);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (9, 9, N'AC-CAS-20W50', 29, 6, 1, '2026-08-20', 30, 3);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (10, 10, N'AC-TER-15W40', 22, 6, 1, '2026-08-20', 24, 3);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (11, 11, N'AC-SHE-5W30', 36, 8, 1, '2026-08-20', 40, 3);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (12, 12, N'BA-12V-650A', 11, 3, 1, '2026-08-20', 12, 8);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (13, 1, N'LL-MIC-2055516-B', 6, 4, 1, '2026-09-25', 6, 1);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (14, 8, N'AC-MOB-10W40-B', 12, 6, 1, '2026-09-25', 12, 10);
INSERT [dbo].[Inventario] ([Id], [IdProducto], [Codigo], [StockActual], [StockMinimo], [IdEstado], [FechaIngreso], [CantidadIngresada], [IdBodega]) VALUES (15, 3, N'LL-GOO-1856514-D', 2, 4, 3, '2026-09-01', 2, 9);
SET IDENTITY_INSERT [dbo].[Inventario] OFF;
GO

-- Gastos: 10 registros
SET IDENTITY_INSERT [dbo].[Gastos] ON;
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (1, 1, N'Arriendo del local - octubre', 3800000, '2026-10-01 09:00:00', '2026-10-05', '2026-10-03', N'Pagado', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (2, 2, N'Energía eléctrica - septiembre', 1250000, '2026-10-01 09:05:00', '2026-10-10', '2026-10-08', N'Pagado', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (3, 2, N'Agua y alcantarillado - septiembre', 420000, '2026-10-01 09:06:00', '2026-10-12', NULL, N'Pendiente', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (4, 3, N'Nómina quincena 1 - octubre', 9800000, '2026-10-01 09:10:00', '2026-10-15', NULL, N'Pendiente', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (5, 4, N'Pedido de llantas - Distribuidora del Caribe', 6200000, '2026-09-20 10:00:00', '2026-10-20', NULL, N'Pendiente', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (6, 4, N'Pedido de aceites - Lubricantes del Norte', 2100000, '2026-09-22 10:00:00', '2026-10-07', '2026-10-06', N'Pagado', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (7, 5, N'Impuesto de industria y comercio', 1480000, '2026-09-15 11:00:00', '2026-10-30', NULL, N'Pendiente', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (8, 6, N'Mantenimiento de elevador hidráulico', 650000, '2026-09-28 14:00:00', '2026-10-09', '2026-10-09', N'Pagado', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (9, 7, N'Publicidad y volantes', 300000, '2026-10-03 15:00:00', '2026-10-18', NULL, N'Pendiente', NULL);
INSERT [dbo].[Gastos] ([Id], [IdCategoria], [Descripcion], [Monto], [FechaRegistro], [FechaVencimiento], [FechaPago], [Estado], [ArchivoSupport]) VALUES (10, 2, N'Internet y telefonía - octubre', 180000, '2026-10-02 08:30:00', '2026-10-14', NULL, N'Pendiente', NULL);
SET IDENTITY_INSERT [dbo].[Gastos] OFF;
GO

-- CatalogoIncentivos: 10 registros
SET IDENTITY_INSERT [dbo].[CatalogoIncentivos] ON;
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (1, N'Lavado de carro gratis', N'Lavado exterior e interior.', 300, 24);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (2, N'Descuento 10% en montaje', N'Válido en el próximo montaje y balanceo.', 200, 38);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (3, N'Cambio de aceite con 20% off', N'Mano de obra y filtro.', 400, 20);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (4, N'Gorra Llantería Valledupar', N'Gorra bordada.', 150, 27);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (5, N'Kit de limpieza para auto', N'Ambientador, paño y brillo.', 350, 15);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (6, N'Alineación 3D gratis', N'Una alineación en cualquier vehículo.', 900, 10);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (7, N'Bono de $50.000', N'Descontable en compras de aceites.', 1200, 8);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (8, N'Rotación de llantas gratis', N'Rotación de las 4 llantas.', 250, 29);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (9, N'Inflado con nitrógeno gratis', N'Las 4 llantas.', 120, 47);
INSERT [dbo].[CatalogoIncentivos] ([Id], [NombrePremio], [Descripcion], [PuntosRequeridos], [StockDisponible]) VALUES (10, N'Bono de $100.000', N'Descontable en llantas.', 2400, 5);
SET IDENTITY_INSERT [dbo].[CatalogoIncentivos] OFF;
GO

-- CanjeIncentivos: 10 registros
SET IDENTITY_INSERT [dbo].[CanjeIncentivos] ON;
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (1, 1, 4, '2026-10-02 11:15:00', 2);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (2, 1, 9, '2026-10-03 12:15:00', 3);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (3, 3, 4, '2026-10-04 13:15:00', 1);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (4, 5, 1, '2026-10-05 14:15:00', 4);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (5, 5, 2, '2026-10-06 10:15:00', 2);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (6, 8, 9, '2026-10-07 11:15:00', 5);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (7, 10, 8, '2026-10-08 12:15:00', 1);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (8, 10, 2, '2026-10-09 13:15:00', 3);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (9, 12, 9, '2026-10-10 14:15:00', 2);
INSERT [dbo].[CanjeIncentivos] ([Id], [IdCliente], [IdIncentivo], [FechaCanje], [EntregadoPor]) VALUES (10, 7, 4, '2026-10-11 10:15:00', 4);
SET IDENTITY_INSERT [dbo].[CanjeIncentivos] OFF;
GO

-- Ofertas: 10 registros
SET IDENTITY_INSERT [dbo].[Ofertas] ON;
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (1, N'Montaje y balanceo gratis', N'Llevando 4 llantas nuevas, el montaje y balanceo va por la casa.', NULL, 100000, '2026-10-01 00:00:00', '2026-10-31 23:59:59', 1, N'MONTAJE4');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (2, N'20% en aceites Mobil', N'Descuento en toda la línea Mobil.', 20, NULL, '2026-10-01 00:00:00', '2026-10-15 23:59:59', 1, N'MOBIL20');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (3, N'Alineación 3D a mitad de precio', N'Con cualquier cambio de llantas.', 50, NULL, '2026-10-05 00:00:00', '2026-11-05 23:59:59', 1, N'ALINEA50');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (4, N'Semana de la batería', N'15% en baterías con instalación incluida.', 15, NULL, '2026-10-12 00:00:00', '2026-10-19 23:59:59', 1, N'BATERIA15');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (5, N'Combo cambio de aceite', N'Aceite + filtro + revisión de 10 puntos.', NULL, 15000, '2026-10-01 00:00:00', '2026-12-31 23:59:59', 1, N'COMBOACE');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (6, N'Descuento por pago en efectivo', N'5% sobre el total de la factura.', 5, NULL, '2026-10-01 00:00:00', '2026-12-31 23:59:59', 1, N'EFECTIVO5');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (7, N'Llantas Michelin: 10% off', N'Toda la línea Michelin turismo.', 10, NULL, '2026-11-01 00:00:00', '2026-11-30 23:59:59', 1, N'MICHELIN10');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (8, N'Black Friday Llantería', N'Hasta 25% en llantas seleccionadas.', 25, NULL, '2026-11-27 00:00:00', '2026-11-29 23:59:59', 1, N'BLACK25');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (9, N'Promo de septiembre', N'Rotación de llantas gratis.', NULL, 20000, '2026-09-01 00:00:00', '2026-09-30 23:59:59', 0, N'SEPT2026');
INSERT [dbo].[Ofertas] ([Id], [Titulo], [Descripcion], [PorcentajeDescuento], [MontoDescuentoFijo], [FechaInicio], [FechaFin], [Estado], [CodigoPromocional]) VALUES (10, N'Amigo del club de puntos', N'Doble de puntos en compras de más de $500.000.', NULL, NULL, '2026-10-01 00:00:00', '2026-10-31 23:59:59', 1, N'PUNTOS2X');
SET IDENTITY_INSERT [dbo].[Ofertas] OFF;
GO

-- Citas: 10 registros
SET IDENTITY_INSERT [dbo].[Citas] ON;
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (1, N'montaje-balanceo', N'Montaje y balanceo', '2026-10-13', N'09:30', N'KLM123 Chevrolet Spark', NULL, N'Efectivo', N'Cuatro llantas nuevas.', N'Valentina Maestre Daza', N'3201000001', N'valentina.maestre@example.com', 1, N'Confirmada', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (2, N'alineacion-3d', N'Alineación 3D', '2026-10-13', N'14:00', N'NPR482 Renault Logan', NULL, N'Tarjeta', NULL, N'Santiago Araújo Pinto', N'3201000002', N'santiago.araujo@example.com', 2, N'Pendiente', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (3, N'mantenimiento-express', N'Mantenimiento express', '2026-10-14', N'08:00', N'UVX307 Mazda 3', NULL, N'Efectivo', N'Cambio de aceite y revisión.', N'Camila Zuleta Lacouture', N'3201000003', N'camila.zuleta@example.com', 3, N'Pendiente', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (4, N'baterias', N'Venta e instalación de baterías', '2026-10-14', N'11:00', N'FTR915 Kia Picanto', NULL, N'Transferencia', N'No enciende en las mañanas.', N'Mateo Cuello Ariza', N'3201000004', N'mateo.cuello@example.com', 4, N'Confirmada', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (5, N'suspension-amortiguadores', N'Suspensión y amortiguadores', '2026-10-15', N'15:30', N'GHB664 Toyota Hilux', NULL, N'Efectivo', N'Ruido al pasar huecos.', N'Isabella Pumarejo Rosado', N'3201000005', N'isabella.pumarejo@example.com', 5, N'Pendiente', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (6, N'serviteca-movil', N'Serviteca móvil', '2026-10-16', N'09:30', N'JKL208 Nissan Versa', N'Calle 12 # 8-45, barrio Los Músicos', N'Efectivo', N'Pinchazo en vía.', N'Sebastián Ortiz Barros', N'3201000006', N'sebastian.ortiz@example.com', 6, N'Pendiente', '2026-10-11 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (7, N'montaje-balanceo', N'Montaje y balanceo', '2026-10-08', N'11:00', N'PQS731 Chevrolet Onix', NULL, N'Tarjeta', NULL, N'Daniela Molina Cabello', N'3201000007', N'daniela.molina@example.com', 7, N'Completada', '2026-10-08 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (8, N'alineacion-3d', N'Alineación 3D', '2026-10-09', N'08:00', N'MNB459 Hyundai Tucson', NULL, N'Efectivo', NULL, N'Nicolás Ospino Mejía', N'3201000008', N'nicolas.ospino@example.com', 8, N'Completada', '2026-10-09 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (9, N'mantenimiento-express', N'Mantenimiento express', '2026-10-07', N'14:00', N'RTY125 Renault Duster', NULL, N'Efectivo', N'El cliente canceló por viaje.', N'Mariana Peñaloza Gutiérrez', N'3201000009', N'mariana.penaloza@example.com', 9, N'Cancelada', '2026-10-07 10:00:00');
INSERT [dbo].[Citas] ([Id], [Slug], [Servicio], [Fecha], [Hora], [Vehiculo], [Direccion], [FormaPago], [Notas], [Solicitante], [Telefono], [Correo], [IdCliente], [Estado], [FechaCreacion]) VALUES (10, N'serviteca-movil', N'Serviteca móvil', '2026-10-17', N'11:00', N'CDE876 Suzuki Swift', N'Carrera 7 # 20-15, barrio Novalito', N'Transferencia', N'Cambio de llanta de repuesto.', N'Julián Redondo Fuentes', N'3201000010', N'julian.redondo@example.com', 10, N'Pendiente', '2026-10-11 10:00:00');
SET IDENTITY_INSERT [dbo].[Citas] OFF;
GO

-- LogActividad: 10 registros
SET IDENTITY_INSERT [dbo].[LogActividad] ON;
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (1, 1, N'Inició sesión', N'Usuarios', '2026-10-01 08:10:00', N'192.168.1.10');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (2, 1, N'Creó nuevo producto', N'Productos', '2026-10-02 09:10:00', N'192.168.1.11');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (3, 2, N'Creó nueva factura', N'Facturas', '2026-10-03 10:10:00', N'192.168.1.12');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (4, 3, N'Registró inventario', N'Inventario', '2026-10-04 11:10:00', N'192.168.1.13');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (5, 2, N'Creó nueva factura', N'Facturas', '2026-10-05 12:10:00', N'192.168.1.14');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (6, 1, N'Creó nuevo empleado', N'Empleados', '2026-10-06 13:10:00', N'192.168.1.15');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (7, 4, N'Creó nueva factura', N'Facturas', '2026-10-07 14:10:00', N'192.168.1.16');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (8, 1, N'Registró un gasto', N'Gastos', '2026-10-08 15:10:00', N'192.168.1.17');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (9, 5, N'Registró entrada de mercancía', N'Inventario', '2026-10-09 16:10:00', N'192.168.1.18');
INSERT [dbo].[LogActividad] ([Id], [IdUsuario], [Accion], [TablaAfectada], [FechaRegistro], [DireccionIP]) VALUES (10, 3, N'Actualizó un cliente', N'Clientes', '2026-10-10 17:10:00', N'192.168.1.19');
SET IDENTITY_INSERT [dbo].[LogActividad] OFF;
GO

-- RecuperacionCuentas: 10 registros
SET IDENTITY_INSERT [dbo].[RecuperacionCuentas] ON;
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (1, 1, N'tok-b6589fc6ab0dc82cf12099d1', '2026-09-10 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (2, 2, N'tok-356a192b7913b04c54574d18', '2026-09-11 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (3, 3, N'tok-da4b9237bacccdf19c0760ca', '2026-09-12 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (4, 4, N'tok-77de68daecd823babbb58edb', '2026-09-13 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (5, 5, N'tok-1b6453892473a467d07372d4', '2026-09-14 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (6, 6, N'tok-ac3478d69a3c81fa62e60f5c', '2026-09-15 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (7, 7, N'tok-c1dfd96eea8cc2b62785275b', '2026-09-16 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (8, 8, N'tok-902ba3cda1883801594b6e1b', '2026-09-17 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (9, 9, N'tok-fe5dbbcea5ce7e2988b8c69b', '2026-09-18 12:00:00', 1);
INSERT [dbo].[RecuperacionCuentas] ([Id], [IdUsuario], [TokenRecuperacion], [FechaExpiracion], [Usado]) VALUES (10, 10, N'tok-0ade7c2cf97f75d009975f4d', '2026-09-19 12:00:00', 1);
SET IDENTITY_INSERT [dbo].[RecuperacionCuentas] OFF;
GO

-- Verificación rápida
SELECT 'Roles' AS Tabla, COUNT(*) AS Registros FROM Roles UNION ALL SELECT 'Bodegas',COUNT(*) FROM Bodegas UNION ALL
SELECT 'Marcas',COUNT(*) FROM Marcas UNION ALL SELECT 'Proveedores',COUNT(*) FROM Proveedores UNION ALL SELECT 'Productos',COUNT(*) FROM Productos UNION ALL
SELECT 'DetalleProductos',COUNT(*) FROM DetalleProductos UNION ALL SELECT 'Inventario',COUNT(*) FROM Inventario UNION ALL SELECT 'TipoServicio',COUNT(*) FROM TipoServicio UNION ALL
SELECT 'Empleados',COUNT(*) FROM Empleados UNION ALL SELECT 'Usuarios',COUNT(*) FROM Usuarios UNION ALL SELECT 'PerfilUsuario',COUNT(*) FROM PerfilUsuario UNION ALL
SELECT 'Clientes',COUNT(*) FROM Clientes UNION ALL SELECT 'Vehiculos',COUNT(*) FROM Vehiculos UNION ALL SELECT 'Facturas',COUNT(*) FROM Facturas UNION ALL
SELECT 'DetalleFactura',COUNT(*) FROM DetalleFactura UNION ALL SELECT 'Gastos',COUNT(*) FROM Gastos UNION ALL SELECT 'CatalogoIncentivos',COUNT(*) FROM CatalogoIncentivos UNION ALL
SELECT 'CanjeIncentivos',COUNT(*) FROM CanjeIncentivos UNION ALL SELECT 'Ofertas',COUNT(*) FROM Ofertas UNION ALL SELECT 'Citas',COUNT(*) FROM Citas UNION ALL
SELECT 'LogActividad',COUNT(*) FROM LogActividad UNION ALL SELECT 'RecuperacionCuentas',COUNT(*) FROM RecuperacionCuentas;
GO
