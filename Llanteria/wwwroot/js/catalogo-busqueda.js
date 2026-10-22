// Búsqueda en vivo por marca y chips de filtro rápido en Llantas/Lubricantes/Baterías.
// Solo recarga el bloque de resultados (grid + paginación), no toda la página.
(function () {
    "use strict";

    var cont = document.getElementById("resultadosCatalogo");
    var filtrosForm = document.getElementById("filtros");
    if (!cont || !filtrosForm) return;

    var debounceTimer;

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

    function cargar(extra, mantenerFoco) {
        recordarEnFormulario(extra);
        var params = parametros(extra);
        var url = cont.getAttribute("data-url") + "?" + params.toString();

        fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (resp) { return resp.text(); })
            .then(function (html) {
                cont.innerHTML = html;
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
            })
            .catch(function () { /* si falla, el usuario siempre puede usar "Aplicar filtros" */ });
    }

    function vincular() {
        var input = document.getElementById("buscarMarca");
        if (input) {
            input.addEventListener("input", function () {
                clearTimeout(debounceTimer);
                var texto = input.value;
                debounceTimer = setTimeout(function () {
                    cargar({ qmarca: texto, pagina: "" }, true);
                }, 300);
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
                cargar(extra, false);
            });
        });
    }

    vincular();
})();
