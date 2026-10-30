// Búsqueda en vivo por marca y chips de filtro rápido en Llantas, Lubricantes, Baterías y Accesorios.
// Solo recarga el bloque de resultados (grid + paginación), no toda la página.
// Pensado para conexiones lentas o inestables: cancela la petición anterior, tiene tiempo máximo,
// guarda los últimos resultados para mostrarlos sin conexión y avisa con un botón "Reintentar".
(function () {
    "use strict";

    var cont = document.getElementById("resultadosCatalogo");
    var filtrosForm = document.getElementById("filtros");
    if (!cont || !filtrosForm) return;

    var TIEMPO_MAXIMO_MS = 12000;
    var debounceTimer;
    var controlador = null;
    var ultimaPeticion = null;

    function campoOculto(nombre) {
        var el = filtrosForm.querySelector('[name="' + nombre + '"]');
        if (!el) {
            el = document.createElement("input");
            el.type = "hidden";
            el.name = nombre;
            filtrosForm.appendChild(el);
        }
        return el;
    }

    // Guarda el valor en el formulario de filtros para que "Aplicar filtros" no pierda lo elegido por chips/búsqueda.
    function recordarEnFormulario(extra) {
        Object.keys(extra).forEach(function (clave) {
            if (clave === "pagina") return;
            var el = campoOculto(clave);
            if (extra[clave]) el.value = extra[clave];
            else el.remove();
        });
    }

    function parametros(extra) {
        var datos = new FormData(filtrosForm);
        var params = new URLSearchParams();
        datos.forEach(function (valor, clave) {
            if (valor) params.append(clave, valor);
        });
        Object.keys(extra || {}).forEach(function (clave) {
            params.delete(clave);
            if (extra[clave]) params.set(clave, extra[clave]);
        });
        return params;
    }

    // ---- Copia local de los últimos resultados (sessionStorage), para verlos si se cae la conexión ----
    function guardar(url, html) {
        try { sessionStorage.setItem("cat:" + url, html); } catch (e) { /* cuota llena o modo privado: se ignora */ }
    }
    function leer(url) {
        try { return sessionStorage.getItem("cat:" + url); } catch (e) { return null; }
    }

    function aviso(texto, conReintento) {
        var div = document.createElement("div");
        div.className = "alert alert-warning d-flex align-items-center justify-content-between gap-2 mt-2";
        div.setAttribute("role", "status");
        var span = document.createElement("span");
        span.textContent = texto;
        div.appendChild(span);
        if (conReintento) {
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-sm btn-dark";
            btn.textContent = "Reintentar";
            btn.addEventListener("click", function () { if (ultimaPeticion) cargar(ultimaPeticion.extra, ultimaPeticion.foco); });
            div.appendChild(btn);
        }
        cont.insertBefore(div, cont.firstChild);
    }

    function pintar(html, params, mantenerFoco) {
        cont.innerHTML = html;
        cont.removeAttribute("aria-busy");
        cont.style.opacity = "";
        vincular();
        if (mantenerFoco) {
            var input = document.getElementById("buscarMarca");
            if (input) {
                input.focus();
                var v = input.value;
                input.value = "";
                input.value = v; // mueve el cursor al final sin perder lo escrito
            }
        }
        history.replaceState(null, "", "?" + params.toString());
    }

    function cargar(extra, mantenerFoco) {
        ultimaPeticion = { extra: extra, foco: mantenerFoco };
        recordarEnFormulario(extra);
        var params = parametros(extra);
        var url = cont.getAttribute("data-url") + "?" + params.toString();

        // Una búsqueda nueva cancela la anterior: con conexión lenta no se acumulan peticiones ni llegan respuestas desordenadas.
        if (controlador) controlador.abort();
        controlador = new AbortController();
        var miControlador = controlador;
        var vencio = false;
        var reloj = setTimeout(function () { vencio = true; miControlador.abort(); }, TIEMPO_MAXIMO_MS);

        cont.setAttribute("aria-busy", "true");
        cont.style.opacity = ".55"; // indica que se está cargando sin vaciar lo que ya se ve

        fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" }, signal: miControlador.signal })
            .then(function (resp) {
                if (!resp.ok) throw new Error("http " + resp.status);
                return resp.text();
            })
            .then(function (html) {
                clearTimeout(reloj);
                guardar(url, html);
                pintar(html, params, mantenerFoco);
            })
            .catch(function (err) {
                clearTimeout(reloj);
                if (err && err.name === "AbortError" && !vencio) return; // la reemplazó otra petición más nueva
                cont.removeAttribute("aria-busy");
                cont.style.opacity = "";
                var copia = leer(url);
                if (copia) {
                    pintar(copia, params, mantenerFoco);
                    aviso(navigator.onLine === false
                        ? "Sin conexión: se muestran los últimos resultados guardados."
                        : "La conexión está lenta: se muestran los últimos resultados guardados.", true);
                } else {
                    aviso(navigator.onLine === false
                        ? "Sin conexión. Revisa tu internet e inténtalo de nuevo."
                        : "No pudimos cargar los resultados. Puedes reintentar o usar \"Aplicar filtros\".", true);
                }
            });
    }

    function vincular() {
        var input = document.getElementById("buscarMarca");
        if (input) {
            input.addEventListener("input", function () {
                clearTimeout(debounceTimer);
                var texto = input.value;
                debounceTimer = setTimeout(function () {
                    cargar({ qmarca: texto, pagina: "" }, true);
                }, 450); // un poco más de espera: menos peticiones con conexión lenta
            });
        }

        document.querySelectorAll(".cat-chip").forEach(function (chip) {
            chip.addEventListener("click", function () {
                var filtro = chip.getAttribute("data-filtro");
                var valor = chip.getAttribute("data-valor");
                var activo = chip.classList.contains("is-active");
                var extra = { pagina: "" };
                if (filtro === "tipo") extra.tipo = activo ? "" : valor;
                if (filtro === "orden") extra.orden = activo ? "" : valor; // sin chip marcado = orden alfabético
                if (filtro === "promociones") { extra.promociones = activo ? "" : "true"; extra.promocion = ""; } // el chip reemplaza al desplegable de promociones
                cargar(extra, false);
            });
        });
    }

    window.addEventListener("online", function () { if (ultimaPeticion) cargar(ultimaPeticion.extra, ultimaPeticion.foco); });

    vincular();
})();
