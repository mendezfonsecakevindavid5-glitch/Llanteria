// Ayudas para conexiones lentas o inestables (se carga en el sitio público y en el panel).
(function () {
    "use strict";

    // 1) Si la librería de alertas (SweetAlert, desde un CDN) no llegó, se usan las ventanas del navegador:
    //    así un mensaje o una confirmación nunca se pierden ni rompen la página.
    if (!window.Swal) {
        window.Swal = {
            fire: function (o) {
                o = o || {};
                if (typeof o === "string") o = { title: o };
                var texto = [o.title, o.text].filter(Boolean).join("\n");
                var ok = true;
                if (o.showCancelButton) ok = window.confirm(texto); else window.alert(texto);
                return Promise.resolve({ isConfirmed: ok, isDismissed: !ok });
            }
        };
    }

    // 2) Aviso discreto cuando se cae el internet.
    var barra;
    function mostrar(texto, clase) {
        if (!barra) {
            barra = document.createElement("div");
            barra.setAttribute("role", "status");
            barra.style.cssText = "position:fixed;left:0;right:0;bottom:0;z-index:2000;padding:.55rem 1rem;text-align:center;font:600 14px system-ui,sans-serif;color:#fff;";
            document.body.appendChild(barra);
        }
        barra.textContent = texto;
        barra.style.background = clase === "ok" ? "#1e6b34" : "#16181c";
        barra.style.display = "block";
    }
    window.addEventListener("offline", function () { mostrar("Sin conexión: verás lo que ya se guardó en tu dispositivo.", "off"); });
    window.addEventListener("online", function () { mostrar("Conexión restablecida.", "ok"); setTimeout(function () { if (barra) barra.style.display = "none"; }, 2500); });

    // 3) Service worker (solo con HTTPS o en localhost).
    if ("serviceWorker" in navigator && (location.protocol === "https:" || location.hostname === "localhost")) {
        window.addEventListener("load", function () { navigator.serviceWorker.register("/sw.js").catch(function () { /* sin SW el sitio funciona igual */ }); });
    }
})();
