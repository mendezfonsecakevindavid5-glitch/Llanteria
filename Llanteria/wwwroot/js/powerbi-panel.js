// Panel interactivo con Power BI.
//  - Modo "Iframe": el iframe ya está en la página; aquí solo se enlaza el botón de pantalla completa.
//  - Modo "Embebido": se pide al servidor un token de solo lectura (Dashboard/PowerBi), se dibuja el informe con powerbi-client
//    y se renueva el token antes de que venza. El secreto de Azure nunca llega al navegador.
(function () {
    "use strict";

    var marco = document.getElementById("pbiMarco");
    var cont = document.getElementById("pbiContenedor");
    var estado = document.getElementById("pbiEstado");
    var btnPantalla = document.getElementById("pbiPantallaCompleta");

    if (btnPantalla) {
        btnPantalla.addEventListener("click", function () {
            var el = marco || cont;
            if (el && el.requestFullscreen) el.requestFullscreen();
        });
    }
    if (!cont) return;

    function mostrarError(texto) {
        if (estado) { estado.textContent = texto; estado.style.display = "block"; }
    }

    function pedirToken() {
        return fetch(cont.getAttribute("data-url"), { headers: { "X-Requested-With": "XMLHttpRequest" }, credentials: "same-origin" })
            .then(function (r) { if (!r.ok) throw new Error("http " + r.status); return r.json(); });
    }

    if (!window["powerbi-client"]) { mostrarError("No se pudo cargar la librería de Power BI. Revisa tu conexión e intenta de nuevo."); return; }

    var models = window["powerbi-client"].models;

    pedirToken().then(function (d) {
        if (estado) estado.style.display = "none";
        var informe = window.powerbi.embed(cont, {
            type: "report",
            id: d.reportId,
            embedUrl: d.embedUrl,
            accessToken: d.token,
            tokenType: models.TokenType.Embed,
            permissions: models.Permissions.Read,
            settings: {
                panes: { filters: { visible: false }, pageNavigation: { visible: true } },
                background: models.BackgroundType.Transparent
            }
        });
        informe.on("error", function () { /* el detalle queda en la consola del navegador */ });

        // Renueva el token 10 minutos antes de que venza (la sesión del panel puede quedarse abierta horas).
        function programar(expira) {
            var ms = new Date(expira).getTime() - Date.now() - 10 * 60 * 1000;
            setTimeout(function () {
                pedirToken().then(function (n) { informe.setAccessToken(n.token); programar(n.expira); }).catch(function () { mostrarError("La sesión del informe venció. Recarga la página."); });
            }, Math.max(ms, 30 * 1000));
        }
        programar(d.expira);
    }).catch(function () {
        mostrarError("No se pudo cargar el informe de Power BI. Revisa la configuración o inténtalo más tarde.");
    });
})();
