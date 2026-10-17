/*
 * Protección de formularios (misma conducta que las vistas de AccountController, aplicada a todo el sitio):
 *  - desactiva el autocompletado del navegador y de los gestores de contraseñas,
 *  - agrega campos señuelo para que el navegador no rellene los reales,
 *  - bloquea copiar, cortar, pegar y arrastrar dentro de los campos.
 * Para excluir un campo a propósito: data-allow-paste="true" (pegar/copiar) o data-allow-autofill="true".
 * Nota: esto es una barrera de uso, no de seguridad; la seguridad real se valida siempre en el servidor.
 */
(function () {
    'use strict';

    var EXCLUIDOS = ['hidden', 'file', 'checkbox', 'radio', 'submit', 'button', 'reset', 'image', 'range', 'color'];
    var EVENTOS = ['paste', 'copy', 'cut', 'drop'];

    function bloquear(e) { e.preventDefault(); }

    function protegerCampo(el) {
        if (el.dataset.protegido === '1') return;
        var tipo = (el.getAttribute('type') || 'text').toLowerCase();
        if (el.tagName === 'INPUT' && EXCLUIDOS.indexOf(tipo) !== -1) return;

        if (el.dataset.allowAutofill !== 'true') {
            // "new-password" es el único valor que los gestores de contraseñas respetan en campos de clave.
            el.setAttribute('autocomplete', tipo === 'password' ? 'new-password' : 'off');
            el.setAttribute('data-lpignore', 'true');   // LastPass
            el.setAttribute('data-1p-ignore', 'true');  // 1Password
            el.setAttribute('data-bwignore', 'true');   // Bitwarden
            el.setAttribute('data-form-type', 'other'); // Dashlane
            el.setAttribute('spellcheck', 'false');
            el.setAttribute('autocapitalize', 'off');
        }
        if (el.dataset.allowPaste !== 'true') {
            EVENTOS.forEach(function (ev) { el.addEventListener(ev, bloquear); });
        }
        el.dataset.protegido = '1';
    }

    function agregarSenuelos(form) {
        if (form.dataset.senuelos === '1') return;
        form.setAttribute('autocomplete', 'off');
        var caja = document.createElement('div');
        caja.setAttribute('aria-hidden', 'true');
        caja.style.cssText = 'position:absolute;left:-9999px;width:0;height:0;overflow:hidden;';
        caja.innerHTML =
            // Sin atributo name: así no viajan en el envío ni ensucian la URL de los formularios GET.
            '<input type="text" autocomplete="username" tabindex="-1">' +
            '<input type="password" autocomplete="current-password" tabindex="-1">';
        form.insertBefore(caja, form.firstChild);
        form.dataset.senuelos = '1';
    }

    function proteger(raiz) {
        (raiz || document).querySelectorAll('form').forEach(function (f) {
            agregarSenuelos(f);
            f.querySelectorAll('input, textarea, select').forEach(protegerCampo);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Las vistas de AccountController ya traen esta protección propia (con sus señuelos y mensajes).
        if (/^\/account(\/|$)/i.test(location.pathname)) return;
        proteger(document);
        // Campos que otros scripts crean después (modales, pasos del checkout, etc.).
        new MutationObserver(function (muts) {
            muts.forEach(function (m) {
                m.addedNodes.forEach(function (n) {
                    if (n.nodeType !== 1) return;
                    if (n.matches && n.matches('form')) { agregarSenuelos(n); }
                    if (n.matches && n.matches('input, textarea, select')) protegerCampo(n);
                    if (n.querySelectorAll) n.querySelectorAll('input, textarea, select').forEach(protegerCampo);
                });
            });
        }).observe(document.body, { childList: true, subtree: true });
    });
})();
