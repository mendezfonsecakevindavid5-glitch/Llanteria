// Abre/cierra el menú del avatar: clic en el avatar, clic fuera o Escape.
(function () {
    'use strict';
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-avatar-menu]').forEach(function (menu) {
            var boton = menu.querySelector('.avatar-btn');
            var panel = menu.querySelector('.avatar-panel');
            if (!boton || !panel) return;

            function abrir(v) {
                panel.hidden = !v;
                boton.setAttribute('aria-expanded', v ? 'true' : 'false');
            }

            boton.addEventListener('click', function (e) { e.stopPropagation(); abrir(panel.hidden); });
            document.addEventListener('click', function (e) { if (!menu.contains(e.target)) abrir(false); });
            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape' && !panel.hidden) { abrir(false); boton.focus(); }
            });
        });
    });
})();