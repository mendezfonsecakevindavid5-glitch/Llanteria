/* Protección de formularios: misma que Login y Registro, aplicada a todo el sitio.
   - Sin autocompletado del navegador ni de gestores de contraseñas.
   - Sin copiar, cortar, pegar ni arrastrar en los campos.
   Excluir un campo: data-guard="off". Los campos creados dinámicamente también se protegen. */
(function () {
    'use strict';

    var SIN_PROTECCION = ['hidden', 'checkbox', 'radio', 'file', 'submit', 'button', 'reset', 'image', 'range', 'color'];
    var EVENTOS = ['paste', 'copy', 'cut', 'drop'];

    function bloquear(e) { e.preventDefault(); }

    function protegerCampo(el) {
        if (el.dataset.guard === 'off' || el.dataset.guardOk === '1') return;
        var tipo = (el.getAttribute('type') || 'text').toLowerCase();
        if (el.tagName === 'INPUT' && SIN_PROTECCION.indexOf(tipo) !== -1) return;

        el.setAttribute('autocomplete', tipo === 'password' ? 'new-password' : 'off');
        el.setAttribute('data-lpignore', 'true');
        el.setAttribute('data-1p-ignore', 'true');
        el.setAttribute('data-form-type', 'other');
        el.setAttribute('spellcheck', 'false');
        if (el.tagName !== 'SELECT') EVENTOS.forEach(function (ev) { el.addEventListener(ev, bloquear); });
        el.dataset.guardOk = '1';
    }

    function protegerFormulario(f) {
        if (f.dataset.guard === 'off') return;
        f.setAttribute('autocomplete', 'off');
        // Campos señuelo: los navegadores rellenan estos en lugar de los reales (igual que en Login y Registro).
        if ((f.getAttribute('method') || 'get').toLowerCase() === 'post' && !f.querySelector('input[name^="prevent_autofill"]')) {
            var u = document.createElement('input');
            u.type = 'text'; u.name = 'prevent_autofill_user'; u.autocomplete = 'username';
            u.style.display = 'none'; u.tabIndex = -1; u.dataset.guard = 'off';
            var p = document.createElement('input');
            p.type = 'password'; p.name = 'prevent_autofill_pass'; p.autocomplete = 'current-password';
            p.style.display = 'none'; p.tabIndex = -1; p.dataset.guard = 'off';
            f.insertBefore(p, f.firstChild);
            f.insertBefore(u, f.firstChild);
        }
    }

    function barrer(raiz) {
        if (raiz.matches && raiz.matches('form')) protegerFormulario(raiz);
        if (raiz.matches && raiz.matches('input,textarea,select')) protegerCampo(raiz);
        if (!raiz.querySelectorAll) return;
        raiz.querySelectorAll('form').forEach(protegerFormulario);
        raiz.querySelectorAll('input,textarea,select').forEach(protegerCampo);
    }

    function iniciar() {
        barrer(document);
        new MutationObserver(function (cambios) {
            cambios.forEach(function (c) {
                c.addedNodes.forEach(function (n) { if (n.nodeType === 1) barrer(n); });
            });
        }).observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar);
    else iniciar();
})();
