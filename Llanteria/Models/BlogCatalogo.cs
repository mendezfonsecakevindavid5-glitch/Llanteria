using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Llanteria.Models;

public record BlogPost(string Slug, string Titulo, string Resumen, string Categoria, DateOnly Fecha,
    string Autor, string Imagen, string Contenido)
{
    /// <summary>Minutos de lectura estimados (200 palabras por minuto).</summary>
    public int MinutosLectura => Math.Max(1, (int)Math.Ceiling(Contenido.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length / 200.0));
}

/// <summary>
/// Artículos del blog. Cada uno es texto con marcas simples: "## " título de sección, "- " viñeta,
/// "> " recuadro destacado, **negrita**; una línea en blanco separa párrafos.
/// Para agregar uno nuevo basta con añadir otro BlogPost a la lista y su portada en wwwroot/images/blog/.
/// </summary>
public static class BlogCatalogo
{
    public static readonly string[] Categorias = { "Actualidad", "Rueda seguro", "Tips de compra" };
    private const string Autor = "Equipo técnico Llantería Valledupar";

    public static BlogPost? Buscar(string slug) => Todos.FirstOrDefault(p => p.Slug == slug);

    public static IEnumerable<BlogPost> Relacionados(BlogPost actual, int cantidad = 3)
        => Todos.Where(p => p.Slug != actual.Slug)
                .OrderByDescending(p => p.Categoria == actual.Categoria).ThenByDescending(p => p.Fecha).Take(cantidad);

    /// <summary>Convierte las marcas simples a HTML seguro (todo el texto se codifica antes).</summary>
    public static string ARenderizar(string contenido)
    {
        var sb = new StringBuilder();
        var parrafo = new List<string>();
        var lista = new List<string>();

        string Inline(string t) => Regex.Replace(WebUtility.HtmlEncode(t), @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        void CerrarParrafo() { if (parrafo.Count > 0) { sb.Append("<p>").Append(Inline(string.Join(' ', parrafo))).Append("</p>"); parrafo.Clear(); } }
        void CerrarLista() { if (lista.Count > 0) { sb.Append("<ul>"); foreach (var i in lista) sb.Append("<li>").Append(Inline(i)).Append("</li>"); sb.Append("</ul>"); lista.Clear(); } }

        foreach (var linea in contenido.Replace("\r", "").Split('\n'))
        {
            var l = linea.Trim();
            if (l.Length == 0) { CerrarParrafo(); CerrarLista(); }
            else if (l.StartsWith("## ")) { CerrarParrafo(); CerrarLista(); sb.Append("<h2>").Append(Inline(l[3..])).Append("</h2>"); }
            else if (l.StartsWith("> ")) { CerrarParrafo(); CerrarLista(); sb.Append("<aside class=\"blog-callout\">").Append(Inline(l[2..])).Append("</aside>"); }
            else if (l.StartsWith("- ")) { CerrarParrafo(); lista.Add(l[2..]); }
            else { CerrarLista(); parrafo.Add(l); }
        }
        CerrarParrafo(); CerrarLista();
        return sb.ToString();
    }

    public static readonly List<BlogPost> Todos = new()
    {
        new BlogPost("como-leer-la-medida-de-tu-llanta", "Cómo leer la medida de tu llanta: qué significa 205/55 R16 91V",
            "Los números del costado de la llanta no son un código secreto. Aprende a leerlos y compra la medida correcta sin equivocarte.",
            "Tips de compra", new DateOnly(2026, 10, 8), Autor, "como-leer-la-medida-de-tu-llanta.svg", """
Si alguna vez has ido a comprar llantas y te han preguntado «¿qué medida?», seguramente miraste el costado de la llanta y encontraste algo como **205/55 R16 91V**. Parece un código, pero es información muy concreta que te dice si esa llanta sirve para tu carro y para cómo lo usas.

## Los tres primeros números: la medida

El primer número, **205**, es el ancho de la llanta en milímetros, medido de pared a pared. A mayor ancho, mayor superficie de contacto con el piso.

El segundo, **55**, es el perfil: la altura de la pared lateral expresada como porcentaje del ancho. Aquí significa que la altura es el 55 % de 205 mm. Un perfil bajo da respuesta más precisa en curvas; un perfil alto absorbe mejor los huecos, algo muy útil en calles con tapas y huecos.

La letra **R** indica construcción radial, la más común hoy, y el número que le sigue, **16**, es el diámetro del rin en pulgadas. Es el único dato que no admite cambios: una llanta de rin 16 solo monta en un rin de 16 pulgadas.

## Los dos últimos datos: carga y velocidad

El **91** es el índice de carga: cada número corresponde a un peso máximo que soporta la llanta. No conviene bajar de lo que indica el fabricante de tu carro, que aparece en el manual o en la etiqueta del marco de la puerta del conductor.

La letra **V** es el índice de velocidad, es decir, la velocidad máxima sostenida para la que fue probada la llanta. Aunque en la ciudad nunca te acerques a ese límite, el índice habla también de la calidad de la construcción y del comportamiento al frenar.

> Regla práctica: usa la medida y los índices que recomienda el fabricante de tu vehículo. Puedes subir un poco el índice de velocidad, pero no bajarlo.

## Dónde encontrar la medida de tu carro

- En el costado de la llanta que tienes montada.
- En la etiqueta del marco de la puerta del conductor.
- En el manual del propietario, en la sección de llantas.
- En la tapa del tanque de combustible, en algunos modelos.

## Fecha de fabricación: el dato que casi nadie mira

Junto a la medida hay un código que empieza por **DOT** y termina con cuatro dígitos. Los dos primeros son la semana y los dos últimos el año de fabricación: 2425 significa semana 24 del 2025. Una llanta almacenada con buenas condiciones puede estar nueva, pero conviene evitar las que llevan varios años en bodega, porque el caucho envejece incluso sin rodar.

## Errores comunes al comprar

Comprar solo por precio sin revisar el índice de carga, mezclar llantas de marcas y desgaste distintos en el mismo eje, o cambiar a un ancho muy diferente sin consultar son los fallos más frecuentes. Una medida equivocada puede rozar con la carrocería, alterar el velocímetro y afectar el frenado.

Si tienes dudas, trae una foto del costado de tu llanta y nuestro equipo te confirma la medida exacta y las alternativas que sí son compatibles con tu vehículo.
"""),

        new BlogPost("presion-de-las-llantas", "Presión de las llantas: la revisión de dos minutos que te ahorra dinero",
            "Una llanta con poco aire gasta más gasolina, se desgasta antes y frena peor. Así se revisa y se corrige, paso a paso.",
            "Rueda seguro", new DateOnly(2026, 10, 3), Autor, "presion-de-las-llantas.svg", """
Es probablemente el mantenimiento más barato y el que más se olvida. Revisar la presión de aire de tus llantas toma dos minutos y tiene efecto directo sobre tu bolsillo y tu seguridad.

## Por qué importa tanto

Una llanta con menos aire del recomendado se deforma más al rodar. Eso genera calor, aumenta la resistencia y obliga al motor a gastar más combustible. Además, el desgaste se concentra en los hombros de la banda de rodadura y la llanta dura menos.

Con exceso de aire pasa lo contrario: el centro de la llanta se gasta primero, el agarre disminuye y el carro se siente más duro y saltón, sobre todo en vías con huecos.

## Cuál es la presión correcta

La presión adecuada **no está escrita en la llanta**. Lo que aparece en el costado es la presión máxima que soporta la estructura. La recomendada la define el fabricante del vehículo y la encuentras en:

- La etiqueta del marco de la puerta del conductor.
- El manual del propietario.
- La tapa del tanque de combustible, en algunos modelos.

Suele expresarse en PSI y puede variar entre el eje delantero y el trasero, o si viajas con el carro cargado.

## Cómo medirla bien

- Hazlo con las llantas **frías**: antes de rodar o después de estar estacionado un par de horas. El calor sube la presión y te dará una lectura falsa.
- Quita la tapa de la válvula, presiona el manómetro con firmeza y lee el valor.
- Ajusta hasta llegar al valor recomendado y vuelve a poner la tapa, que protege la válvula del polvo.
- No olvides la llanta de repuesto: es la que nadie revisa y la que necesitas el día del pinchazo.

> En Valledupar, con el pavimento caliente durante buena parte del día, la presión aumenta rápido. Revisa siempre en la mañana o con el carro estacionado a la sombra.

## Cada cuánto revisar

Una vez al mes y antes de cada viaje largo. Una llanta puede perder entre uno y dos PSI al mes de forma natural, y mucho más si tiene un pinchazo lento o una válvula defectuosa.

## Señales de que algo anda mal

Si el carro se «jala» hacia un lado, si el volante vibra o si notas que una llanta se ve más baja que las demás, detente a revisarla. Rodar con una llanta muy baja puede dañar la pared interna de forma irreparable, y entonces ya no basta con inflarla.

En nuestro taller revisamos la presión de tus cuatro llantas y la del repuesto sin costo cuando vienes por cualquier servicio.
"""),

        new BlogPost("alineacion-balanceo-y-rotacion", "Alineación, balanceo y rotación: cada cuánto y para qué sirve cada una",
            "Se confunden todo el tiempo y no son lo mismo. Te explicamos qué corrige cada servicio y cada cuánto conviene hacerlos.",
            "Rueda seguro", new DateOnly(2026, 9, 26), Autor, "alineacion-balanceo-y-rotacion.svg", """
«Llévalo a alinear y balancear» es una frase que se escucha mucho, pero casi siempre se mezclan los tres servicios básicos de las llantas. Cada uno resuelve un problema distinto, y saber cuál necesitas te evita gastar de más o, peor, rodar con un problema sin atender.

## Alineación: que las llantas miren hacia el mismo lado

La alineación ajusta los ángulos de la suspensión (convergencia, caída y avance) para que las llantas estén correctamente orientadas respecto a la carrocería y entre sí. Cuando están fuera de rango, el carro tiende a irse hacia un lado, el volante queda torcido y las llantas se desgastan de forma irregular.

**Señales de que la necesitas:**

- El carro se jala a la izquierda o a la derecha al soltar el volante.
- El volante queda descentrado en línea recta.
- Desgaste más fuerte en un solo borde de la llanta.
- Después de golpear un hueco fuerte, un andén o de cambiar piezas de suspensión.

Como referencia, conviene revisarla cada 10.000 km, al estrenar llantas y siempre después de un impacto fuerte. La alineación 3D mide con cámaras y sensores, y es más precisa que los métodos tradicionales.

## Balanceo: que la llanta gire sin vibrar

El balanceo corrige las diferencias de peso en el conjunto llanta-rin. Se le añaden pequeñas pesas hasta que gire de forma pareja. Si está desbalanceada, sentirás vibración en el volante a cierta velocidad (muchas veces entre 80 y 100 km/h) o en el asiento.

Se hace al montar llantas nuevas, después de reparar un pinchazo y cuando aparezca la vibración. Una llanta desbalanceada también acelera el desgaste de rodamientos y amortiguadores.

## Rotación: repartir el desgaste

Las llantas delanteras y traseras no se desgastan igual: en un carro de tracción delantera, por ejemplo, las delanteras trabajan más. Rotarlas, es decir, cambiarlas de posición siguiendo un patrón, hace que se gasten de manera pareja y que el juego completo dure más.

La recomendación habitual es rotarlas cada 8.000 a 10.000 km, algo que puedes aprovechar junto con el cambio de aceite.

> Cuadro rápido: **alineación** = ángulos de la suspensión, **balanceo** = distribución del peso, **rotación** = cambiar la posición de las llantas.

## Cómo no olvidarte de ninguno

Una buena costumbre es agendar los tres servicios en la misma visita, cada seis meses o cada 10.000 km, lo que ocurra primero. Así detectamos a tiempo desgastes raros, golpes ocultos o piezas de suspensión flojas.

Puedes agendar tu cita desde la sección de servicios de nuestra página y escoger el horario que mejor te quede.
"""),

        new BlogPost("cuando-cambiar-las-llantas", "¿Cuándo cambiar las llantas? Cinco señales que no debes ignorar",
            "El dibujo, la edad, las grietas y las vibraciones hablan. Aprende a leer las señales antes de que el problema sea una emergencia.",
            "Rueda seguro", new DateOnly(2026, 9, 18), Autor, "cuando-cambiar-las-llantas.svg", """
Las llantas son el único punto de contacto entre tu carro y la vía. Por eso conviene reemplazarlas a tiempo, ni antes, tirando dinero, ni después, jugándote la seguridad. Estas son las cinco señales más claras de que llegó el momento.

## 1. El dibujo está gastado

La banda de rodadura es la que evacúa el agua y da agarre. Cuando se gasta, el carro pierde capacidad de frenado, sobre todo en piso mojado. La profundidad mínima que exige la revisión técnico-mecánica es de **1,6 mm**, pero muchos técnicos recomiendan cambiarlas antes de llegar a ese punto, hacia los 3 mm, especialmente si manejas bajo lluvia.

Las llantas traen unos pequeños resaltes llamados indicadores de desgaste (TWI) en el fondo de los surcos. Cuando la banda queda a ras de ellos, la llanta ya cumplió su vida útil.

## 2. Se ven grietas o abultamientos

Las grietas finas en la pared lateral o entre los tacos indican que el caucho se resecó, algo común con el sol fuerte. Un abultamiento o «hernia» en el costado es todavía más serio: significa que la estructura interna se rompió y puede reventar. Si lo ves, no sigas rodando.

## 3. La llanta es vieja aunque se vea bien

El caucho envejece con el calor, la luz y el tiempo, incluso si el carro anda poco. Revisa el código DOT en el costado: los últimos cuatro dígitos son la semana y el año de fabricación. Como regla general, se recomienda revisar con detalle las llantas de más de cinco años y reemplazarlas hacia los diez, aunque tengan buen dibujo.

## 4. Vibraciones o desgaste dispar

Si el volante vibra, el carro se jala o ves desgaste más fuerte en un borde o en el centro, probablemente hay un problema de alineación, balanceo, presión o suspensión. Corregir la causa a tiempo te permite salvar el juego de llantas.

## 5. Pinchazos repetidos o reparaciones mal ubicadas

Una reparación bien hecha en la banda de rodadura es segura. Pero los daños en el costado no se reparan, y varias reparaciones en la misma llanta son motivo para cambiarla.

> Truco rápido: revisa tus llantas una vez al mes. Pasa la mano por la banda de rodadura y busca cortes, clavos incrustados, desgaste dispar o deformaciones.

## ¿Cambiar una o las cuatro?

Lo ideal es cambiar de a pares por eje, o el juego completo, para mantener un comportamiento estable. Si solo cambias una, que sea de la misma marca y modelo y con desgaste parecido a la otra del eje. Las llantas más nuevas deben ir atrás: así se conserva la estabilidad en piso mojado.

Trae tu carro y revisamos el estado de tus llantas sin compromiso; te mostramos las medidas de desgaste y te decimos con honestidad si todavía te sirven.
"""),

        new BlogPost("como-elegir-el-aceite-de-tu-motor", "Cómo elegir el aceite de tu motor: viscosidad, tipo y cada cuánto cambiarlo",
            "10W-40, 5W-30, mineral, semisintético, sintético: qué significa cada cosa y cuál le conviene a tu carro.",
            "Tips de compra", new DateOnly(2026, 9, 10), Autor, "como-elegir-el-aceite-de-tu-motor.svg", """
El aceite es la sangre del motor: lubrica, enfría, limpia y protege las piezas internas. Elegir el correcto no es cuestión de marca o de precio, sino de entender dos cosas: la **viscosidad** y el **tipo de base**.

## Qué significan los números: 10W-40, 5W-30

El primer número, seguido de la letra W (de *winter*), indica qué tan fluido es el aceite en frío. Entre más bajo, más fácil fluye al arrancar. El segundo número es la viscosidad a temperatura de trabajo: más alto significa un aceite más «espeso» cuando el motor está caliente.

Un 5W-30 fluye mejor en el arranque y suele usarse en motores modernos con tolerancias muy ajustadas. Un 20W-50 protege con más cuerpo y se usa en motores con mucho kilometraje. La viscosidad correcta **la define el fabricante** y está en el manual del propietario: usa esa.

## Los tres tipos de aceite

- **Mineral:** refinado del petróleo. Es el más económico y sirve para motores antiguos o de uso suave. Se cambia con más frecuencia.
- **Semisintético:** mezcla de base mineral y sintética. Ofrece mejor protección que el mineral sin el costo del sintético.
- **Sintético:** diseñado en laboratorio, es más estable con el calor y con el paso de los kilómetros. Es el que piden muchos motores modernos y turbo.

## ¿Cada cuánto se cambia?

Depende del tipo de aceite, del motor y de cómo lo uses. Como referencia general:

- Mineral: entre 3.000 y 5.000 km.
- Semisintético: entre 5.000 y 7.500 km.
- Sintético: entre 7.500 y 10.000 km, o lo que indique tu manual.

Si usas el carro en trayectos cortos, tráfico pesado, calor extremo o con carga, conviene acortar el intervalo. En una ciudad calurosa como Valledupar, el aceite trabaja más duro.

> No esperes a que se encienda la luz del tablero: cuando se prende, el problema ya es serio. Revisa el nivel con la varilla una vez al mes, con el motor frío y el carro en piso plano.

## No olvides el filtro

Cada cambio de aceite debe ir acompañado de filtro nuevo. Un filtro viejo deja pasar impurezas y puede arruinar el aceite recién puesto.

## Señales de que tu aceite ya cumplió

Color muy oscuro y espeso, ruido de motor más fuerte, olor a quemado o consumo más alto de combustible son señales de que el aceite perdió propiedades.

En nuestro taller te recomendamos el aceite correcto según tu motor, lo cambiamos en el momento y te entregamos el recibo con la fecha y el kilometraje para tu próximo cambio.
"""),

        new BlogPost("bateria-del-carro-en-clima-caliente", "La batería de tu carro en clima caliente: señales de falla y cómo alargar su vida",
            "El calor es el peor enemigo de las baterías. Conoce las señales de aviso y los hábitos que te evitan quedar varado.",
            "Rueda seguro", new DateOnly(2026, 8, 29), Autor, "bateria-del-carro-en-clima-caliente.svg", """
Mucha gente cree que las baterías fallan en el frío. En realidad, el calor es lo que más las desgasta: acelera las reacciones químicas internas y hace que el líquido se evapore con más rapidez. En ciudades calurosas, una batería dura en promedio entre dos y cuatro años, bastante menos que en climas templados.

## Señales de que la batería está fallando

- El motor arranca lento o con esfuerzo, sobre todo en la mañana.
- Las luces bajan de intensidad al arrancar o con el carro en ralentí.
- Se enciende la luz de batería en el tablero.
- Los bornes tienen sulfato: un polvo blanco o verdoso alrededor.
- La carcasa está hinchada o notas olor a azufre.
- La batería tiene más de tres años y empiezan los fallos esporádicos.

## Hábitos que alargan su vida

- **Estaciona a la sombra** siempre que puedas: reduce la temperatura bajo el capó.
- **Limpia los bornes** con un cepillo y una mezcla de agua con bicarbonato, y asegúrate de que queden bien apretados.
- **Evita los trayectos muy cortos**: el alternador necesita tiempo para recargarla.
- **Apaga luces, aire y radio** antes de apagar el motor y no uses accesorios con el carro apagado.
- **Revisa el sistema de carga**: un alternador débil acaba con cualquier batería nueva.

> Antes de cambiar la batería, pide una prueba de carga y del alternador. A veces el problema no es la batería, sino el sistema que la recarga.

## Cómo escoger la batería correcta

Fíjate en tres datos: el tamaño o grupo (que debe caber en la bandeja), la capacidad de arranque en frío (CCA) y la polaridad de los bornes. Escoge una con capacidad igual o mayor a la que pide tu vehículo. Las baterías libres de mantenimiento son las más cómodas, y los vehículos con sistema *start-stop* requieren un tipo específico (EFB o AGM).

## Qué hacer si te quedas sin batería

Si el carro no enciende, no insistas muchas veces seguidas: gastas lo poco que queda. Puedes pasar corriente con cables si tienes ayuda y sigues el orden correcto (positivo con positivo, negativo a masa en el vehículo con carga). Si el problema se repite, ya es hora de cambiarla.

Nuestra serviteca móvil te asiste en el sitio donde estés: prueba de batería, cambio e instalación con garantía.
"""),

        new BlogPost("temporada-de-lluvias-prepara-tu-carro", "Temporada de lluvias en el Cesar: prepara tu carro para manejar con seguridad",
            "Llantas, frenos, plumillas y luces: la revisión que conviene hacer antes de que lleguen los aguaceros.",
            "Actualidad", new DateOnly(2026, 10, 10), Autor, "temporada-de-lluvias-prepara-tu-carro.svg", """
Con la llegada de las lluvias cambian las condiciones de la vía: el pavimento se vuelve resbaloso, la visibilidad baja y los charcos esconden huecos. Un poco de preparación hace una gran diferencia en tu seguridad y en la de tu familia.

## 1. Revisa el dibujo de tus llantas

En piso mojado, el dibujo es lo único que evacúa el agua entre la llanta y el pavimento. Con la banda desgastada aumenta el riesgo de **hidroplaneo**: la llanta flota sobre el agua y pierdes dirección y frenado. Si el dibujo está cerca del límite, cámbialas antes de la temporada.

Revisa también la presión: una llanta mal inflada tiene menos agarre y se deforma más sobre el agua.

## 2. Frenos en buen estado

Con el piso mojado la distancia de frenado se alarga. Si tus frenos chillan, vibran o el pedal se siente esponjoso, es momento de revisarlos. Pastillas y discos gastados no responden igual cuando más los necesitas.

## 3. Limpiabrisas y luces

Unas plumillas resecas dejan rayas y baja visibilidad. Cámbialas si dejan marcas o hacen ruido. Revisa también que funcionen todas las luces: bajas, altas, de freno, direccionales y de parqueo. Con lluvia, ver y ser visto es fundamental.

## 4. Batería y sistema eléctrico

La humedad empeora los falsos contactos. Revisa los bornes de la batería y cualquier cable expuesto. Si la batería es antigua, es mejor reemplazarla antes de quedar varado bajo un aguacero.

## Consejos para manejar bajo la lluvia

- Reduce la velocidad y aumenta la distancia con el carro de adelante, al menos el doble de lo normal.
- Frena con suavidad y evita los giros bruscos.
- Enciende las luces bajas, aunque sea de día.
- No cruces charcos de profundidad desconocida: el agua puede dañar el motor o dejarte sin freno.
- Si se forma una lámina de agua y sientes que el carro «flota», suelta el acelerador sin frenar de golpe y mantén el volante firme.

> Pasar por un charco profundo y luego pisar el freno varias veces suavemente ayuda a secar las pastillas.

## Una revisión completa en una sola visita

En nuestro taller revisamos llantas, frenos, suspensión, luces y batería en una sola visita, para que enfrentes la temporada con tranquilidad. Agenda tu cita desde la página y escoge el horario que prefieras.
"""),
    };
}
