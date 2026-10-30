/* =====================================================================================================
   LLANDUPAR - INSERT DE EVENTOS (y una promoción por evento)
   SQL Server. Idempotente: un evento o promoción solo se agrega si no existe (por nombre / código promocional).

   CÓMO FUNCIONA UN EVENTO (regla de la aplicación): se muestra únicamente el día de su fecha (de 12:00 a. m. a 11:59 p. m.).
     - Recurrente = 1 -> se repite cada año el mismo día y mes (fechas fijas: Navidad, Año Nuevo...).
     - Recurrente = 0 -> ocurre una sola vez (fechas que cambian cada año: Día de la Madre, Black Friday...).
   Por eso el filtro "Evento" del catálogo solo ofrece opciones EL DÍA del evento y mientras haya productos con una promoción ligada a él.
   Los eventos que ya existen en la tabla (por ejemplo los de DatosDePrueba.sql) no se tocan ni se duplican.
   ===================================================================================================== */
-- USE [LlanteriaDB];
-- GO
SET NOCOUNT ON;
GO

/* 1) Eventos */
DECLARE @Lla int = (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);

INSERT INTO dbo.Eventos (Nombre, Descripcion, Fecha, FechaInicio, FechaFin, Recurrente, IdLlanteria)
SELECT v.N, v.D, v.F, v.F, v.F, v.R, @Lla
FROM (VALUES
    -- Fechas fijas: se repiten todos los años
    (N'Año Nuevo',                      N'Arranca el año con tu carro listo: descuentos en mantenimiento.',                 CONVERT(date, '2026-01-01'), 1),
    (N'Aniversario de Valledupar',      N'Celebramos la Ciudad de los Santos Reyes con ofertas en llantas y lubricantes.',  CONVERT(date, '2026-01-06'), 1),
    (N'Día de la Mujer',                N'Descuentos en revisión y mantenimiento para ellas.',                              CONVERT(date, '2026-03-08'), 1),
    (N'Día del Trabajo',                N'Ofertas especiales en lubricantes y servicios.',                                  CONVERT(date, '2026-05-01'), 1),
    (N'Día de la Independencia',        N'Ofertas patrias en llantas, baterías y accesorios.',                              CONVERT(date, '2026-07-20'), 1),
    (N'Batalla de Boyacá',              N'Promociones del puente festivo de agosto para tus viajes.',                       CONVERT(date, '2026-08-07'), 1),
    (N'Halloween',                      N'Una noche de descuentos en iluminación y accesorios.',                            CONVERT(date, '2026-10-31'), 1),
    (N'Noche de Velitas',               N'Arrancamos la Navidad con ofertas en llantas y baterías.',                        CONVERT(date, '2026-12-07'), 1),
    (N'Navidad',                        N'Promociones de temporada en llantas y baterías para viajar seguro.',              CONVERT(date, '2026-12-25'), 1),
    (N'Fin de Año',                     N'Deja el carro listo para el viaje de fin de año.',                                CONVERT(date, '2026-12-31'), 1),
    -- Fechas que cambian cada año: una sola vez
    (N'Black Friday 2026',              N'El viernes de las mejores ofertas del año en toda la tienda.',                    CONVERT(date, '2026-11-27'), 0),
    (N'Cyber Monday 2026',              N'Descuentos extra en accesorios, baterías y lubricantes.',                         CONVERT(date, '2026-11-30'), 0),
    (N'Día de la Madre 2027',           N'Un regalo útil para mamá: promociones en accesorios y mantenimiento.',            CONVERT(date, '2027-05-09'), 0),
    (N'Día del Padre 2027',             N'Papá merece lo mejor: ofertas en llantas, lubricantes y accesorios.',             CONVERT(date, '2027-06-20'), 0),
    (N'Amor y Amistad 2027',            N'Ofertas para compartir con tus amigos y tu familia.',                             CONVERT(date, '2027-09-18'), 0),
    (N'Black Friday 2027',              N'El viernes de las mejores ofertas del año en toda la tienda.',                    CONVERT(date, '2027-11-26'), 0),
    (N'Cyber Monday 2027',              N'Descuentos extra en accesorios, baterías y lubricantes.',                         CONVERT(date, '2027-11-29'), 0)
) v(N, D, F, R)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Eventos e WHERE e.Nombre = v.N);
GO

/* 2) Una promoción por evento (vigente un año; el porcentaje solo se ofrece el día del evento).
      Para que un producto participe, asígnale la promoción desde el panel (Producto > Promoción) o con el UPDATE del paso 3. */
INSERT INTO dbo.Ofertas (Titulo, Descripcion, PorcentajeDescuento, MontoDescuentoFijo, FechaInicio, FechaFin, Estado, CodigoPromocional, IdEvento, IdLlanteria)
SELECT v.T, v.D, v.P, 0, CAST(CAST(GETDATE() AS date) AS datetime), DATEADD(DAY, 730, GETDATE()), 1, v.C,
       (SELECT TOP 1 Id FROM dbo.Eventos WHERE Nombre = v.E ORDER BY Id), (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id)
FROM (VALUES
    (N'Halloween: luces y accesorios',  N'10 % de descuento solo el día de Halloween.',        10, N'HALLOWEEN10',  N'Halloween'),
    (N'Noche de Velitas',               N'10 % de descuento en llantas y baterías.',           10, N'VELITAS10',    N'Noche de Velitas'),
    (N'Black Friday 2026',              N'15 % de descuento en todo el catálogo participante.',15, N'BLACKFRIDAY26',N'Black Friday 2026'),
    (N'Cyber Monday 2026',              N'10 % extra en accesorios, baterías y lubricantes.',  10, N'CYBER26',      N'Cyber Monday 2026'),
    (N'Año Nuevo',                      N'10 % de descuento en mantenimiento.',                10, N'AÑONUEVO10',   N'Año Nuevo'),
    (N'Día del Trabajo',                N'10 % de descuento en lubricantes.',                  10, N'TRABAJO10',    N'Día del Trabajo'),
    (N'Día de la Madre 2027',           N'12 % de descuento en accesorios.',                   12, N'MADRE27',      N'Día de la Madre 2027'),
    (N'Día del Padre 2027',             N'12 % de descuento en llantas y lubricantes.',        12, N'PADRE27',      N'Día del Padre 2027'),
    (N'Amor y Amistad 2027',            N'10 % de descuento en accesorios.',                   10, N'AMISTAD27',    N'Amor y Amistad 2027')
) v(T, D, P, C, E)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Ofertas o WHERE o.CodigoPromocional = v.C)
  AND EXISTS (SELECT 1 FROM dbo.Eventos e WHERE e.Nombre = v.E);
GO

/* 3) OPCIONAL - prueba inmediata del filtro "Evento".
      Pon @ProbarHoy = 1 para crear un evento con fecha de HOY, una promoción ligada a él y asignarla a 8 accesorios.
      Verás el evento en el filtro "Evento" de Accesorios hoy mismo. Después puedes borrar el evento de prueba desde el panel. */
DECLARE @ProbarHoy bit = 0;
IF @ProbarHoy = 1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos WHERE Nombre = N'Evento de prueba (hoy)')
        INSERT INTO dbo.Eventos (Nombre, Descripcion, Fecha, FechaInicio, FechaFin, Recurrente, IdLlanteria)
        SELECT N'Evento de prueba (hoy)', N'Evento creado para probar el filtro.', CONVERT(date, GETDATE()), CONVERT(date, GETDATE()), CONVERT(date, GETDATE()), 0, (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);

    IF NOT EXISTS (SELECT 1 FROM dbo.Ofertas WHERE CodigoPromocional = N'PRUEBAHOY10')
        INSERT INTO dbo.Ofertas (Titulo, Descripcion, PorcentajeDescuento, MontoDescuentoFijo, FechaInicio, FechaFin, Estado, CodigoPromocional, IdEvento, IdLlanteria)
        SELECT N'Promoción de prueba (hoy)', N'10 % de descuento solo hoy.', 10, 0, CAST(CAST(GETDATE() AS date) AS datetime), DATEADD(DAY, 30, GETDATE()), 1, N'PRUEBAHOY10',
               (SELECT TOP 1 Id FROM dbo.Eventos WHERE Nombre = N'Evento de prueba (hoy)'), (SELECT TOP 1 Id FROM dbo.Llanterias ORDER BY Id);

    UPDATE TOP (8) dbo.Productos SET IdOferta = (SELECT TOP 1 Id FROM dbo.Ofertas WHERE CodigoPromocional = N'PRUEBAHOY10')
    WHERE Categoria = N'Accesorios' AND IdOferta IS NULL;
END;
GO

SELECT Nombre, Fecha, Recurrente FROM dbo.Eventos ORDER BY Fecha;
GO
