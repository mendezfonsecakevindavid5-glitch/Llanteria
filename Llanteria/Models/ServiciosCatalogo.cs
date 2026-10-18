namespace Llanteria.Models;

/// <summary>
/// Servicios con los que arranca el sitio. Solo se usan para llenar la tabla ServiciosWeb cuando está vacía;
/// después el administrador los edita desde el panel (ServicioWebController).
/// </summary>
public static class ServiciosCatalogo
{
    private static readonly Dictionary<string, ServicioViewModel> Datos = new()
    {
        ["montaje-balanceo"] = new()
        {
            Slug = "montaje-balanceo", Titulo = "Montaje y balanceo", Icono = "fa-compact-disc",
            Subtitulo = "Precisión computarizada para una conducción suave",
            Descripcion = "El montaje profesional garantiza el acople perfecto entre la llanta y el rin sin dañar las pestañas ni los sensores de presión. Complementado con un balanceo computarizado de alta precisión, distribuimos el peso de forma equitativa en la rueda para asegurar una rodadura suave.",
            Ventajas = { new("fa-car-side", "Conducción suave", "Elimina vibraciones molestas en el volante y el pedal."), new("fa-arrows-spin", "Desgaste uniforme", "Evita deformaciones irregulares en la banda de rodamiento.") },
            InfoCita = { new("Tiempo estimado", "45 a 60 minutos."), new("Garantía", "30 días o 1.000 km de alineación/balanceo."), new("Recomendación", "Llega 10 minutos antes del horario reservado.") },
            TituloVisitante = "¿Por qué solicitar información hoy?",
            TextoVisitante = "Si notas vibraciones al conducir a más de 60 km/h o vas a realizar un viaje, una revisión preventiva evita daños costosos en la suspensión y llantas.",
            Precio = "$40.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Toyota Hilux", PlaceholderNotas = "Observaciones sobre la llanta o rin…",
            PlaceholderMensaje = "Ej. Toyota Hilux 2022 – balanceo para 4 ruedas"
        },
        ["alineacion-3d"] = new()
        {
            Slug = "alineacion-3d", Titulo = "Alineación 3D", Icono = "fa-arrows-to-dot",
            Subtitulo = "Ajuste técnico milimétrico con tecnología óptica",
            Descripcion = "Utilizamos sensores ópticos de alta definición para ajustar los ángulos de las ruedas (camber, caster y toe) exactamente según las especificaciones originales del fabricante de tu vehículo.",
            Ventajas = { new("fa-compass", "Trayectoria recta", "Evita que el vehículo se desvíe hacia los lados en recta."), new("fa-leaf", "Ahorro de combustible", "Reduce la fricción innecesaria del neumático con el asfalto.") },
            InfoCita = { new("Tiempo estimado", "30 a 45 minutos."), new("Garantía", "30 días o 1.000 km de alineación técnica."), new("Recomendación", "Llega 10 minutos antes para la lectura inicial de sensores.") },
            Precio = "$60.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Chevrolet Onix", PlaceholderNotas = "Observaciones sobre el volante o la alineación…",
            PlaceholderMensaje = "Ej. Renault Logan 2020 – volante desviado hacia la derecha"
        },
        ["mantenimiento-express"] = new()
        {
            Slug = "mantenimiento-express", Titulo = "Mantenimiento express", Icono = "fa-oil-can",
            Subtitulo = "Cambio de aceite, filtros y revisión de puntos de seguridad",
            Descripcion = "Servicio rápido diseñado para mantener la vida útil de tu motor. Incluye reemplazo de aceite lubricante homologado, filtro de aceite, filtro de aire y una inspección de seguridad preventiva en 15 puntos clave de tu vehículo.",
            Ventajas = { new("fa-shield-halved", "Insumos homologados", "Aceites sintéticos o semisintéticos y filtros de calidad garantizada."), new("fa-bolt", "Servicio rápido", "Atención express de menos de 45 minutos.") },
            InfoCita = { new("Tiempo estimado", "30 a 45 minutos."), new("Garantía", "Insumos originales homologados por marca."), new("Incluye", "Revisión de 15 puntos de seguridad (frenos, suspensión, fluidos y presión de llantas).") },
            Precio = "$120.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Renault Stepway", PlaceholderNotas = "Especificaciones de aceite o filtro de preferencia…",
            PlaceholderMensaje = "Ej. Chevrolet Spark GT 2018 – cambio de aceite 10W30 y filtro"
        },
        ["baterias"] = new()
        {
            Slug = "baterias", Titulo = "Venta e instalación de baterías", Icono = "fa-car-battery",
            Subtitulo = "Diagnóstico de carga e instalación profesional con garantía",
            Descripcion = "Realizamos la revisión digital del estado de la batería y del sistema de carga de tu vehículo (alternador y motor de arranque). Contamos con baterías selladas y de libre mantenimiento con respaldo de las marcas líderes del mercado.",
            Ventajas = { new("fa-award", "Garantía escrita", "Respaldo directo y soporte ante cualquier eventualidad."), new("fa-screwdriver-wrench", "Instalación gratis", "Montaje técnico y comprobación de voltaje sin costo al comprar tu batería.") },
            InfoCita = { new("Tiempo de instalación", "15 a 20 minutos."), new("Diagnóstico incluido", "Prueba electrónica de arranque y lectura del alternador."), new("Batería usada", "Recibimos la anterior y te damos un descuento de retoma.") },
            Precio = "$240.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Mazda 3", PlaceholderNotas = "Marca preferida de batería o fallas presentadas…",
            PlaceholderMensaje = "Ej. Toyota Hilux 2019 – batería de 800 amperios"
        },
        ["suspension-amortiguadores"] = new()
        {
            Slug = "suspension-amortiguadores", Titulo = "Suspensión y amortiguadores", Icono = "fa-truck-monster",
            Subtitulo = "Diagnóstico técnico para garantizar estabilidad y confort",
            Descripcion = "Inspeccionamos amortiguadores, espirales, bujes, terminales de dirección, tijeras y rótulas para asegurar el correcto agarre del vehículo, prevenir el desgaste irregular de los neumáticos y dar máximo confort en cabina.",
            Ventajas = { new("fa-shield-cat", "Seguridad en curvas", "Mantiene el vehículo firme, previene inclinaciones excesivas y mejora la adherencia."), new("fa-road", "Absorción de impactos", "Filtra las irregularidades del camino y prolonga la vida del auto.") },
            InfoCita = { new("Tiempo de diagnóstico", "45 a 60 minutos."), new("Diagnóstico incluido", "Prueba de rebote, revisión de fugas de aceite y estado de bujes y cazoletas."), new("Garantía", "Repuestos originales y homologados con respaldo en mano de obra.") },
            Precio = "$60.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Nissan Frontier", PlaceholderNotas = "Síntomas que presenta el auto (ruidos, golpe seco, inestabilidad)…",
            PlaceholderMensaje = "Ej. Ford Fiesta 2015 – ruido seco en la rueda delantera derecha al pasar baches"
        },
        ["serviteca-movil"] = new()
        {
            Slug = "serviteca-movil", Titulo = "Serviteca móvil", Icono = "fa-truck-pickup", EsDomicilio = true,
            Subtitulo = "Atención especializada a domicilio en Valledupar",
            Descripcion = "Llevamos nuestro taller móvil totalmente equipado hasta tu casa u oficina en Valledupar. Realizamos montaje, balanceo, despinche, recarga y cambio de baterías sin que tengas que desplazarte ni esperar en una sala.",
            Ventajas = { new("fa-house", "Comodidad total", "Atención personalizada en la puerta de tu hogar o lugar de trabajo."), new("fa-clock", "Ahorro de tiempo", "Sigues con tus actividades mientras atendemos tu vehículo.") },
            InfoCita = { new("Zona de cobertura", "Toda la zona urbana de Valledupar."), new("Requisito de espacio", "Terreno plano y acceso libre para la unidad móvil."), new("Equipamiento", "Generador propio, desmontadora y balanceadora de precisión.") },
            EtiquetaPrecio = "Desplazamiento desde:", Precio = "$35.000 COP",
            PlaceholderVehiculo = "Ej. ABC-123 / Renault Duster", PlaceholderNotas = "Indicaciones de llegada o requerimientos del servicio…",
            PlaceholderMensaje = "Ej. Calle 16 # 9-45, barrio Centro. Cambio de llantas o batería descargada"
        }
    };

    public static IEnumerable<ServicioViewModel> Semilla => Datos.Values;
}
