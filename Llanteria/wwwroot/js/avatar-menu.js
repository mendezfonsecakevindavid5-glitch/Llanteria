// Abre/cierra el menú del avatar: clic en el avatar, clic fuera o Escape.
(function () {
    'use strict';
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-avatar-menu]').forEach(function (menu) {
            var boton = menu.querySelector('.avatar-btn');
            var panel = menu.querySelector('.avatar-panel');
            if (!boton || !panel) return;

            // Red de seguridad: si el menú se sale por un borde de la pantalla, se desplaza lo justo para verse completo.
            function ajustar() {
                panel.style.transform = '';
                var r = panel.getBoundingClientRect(), margen = 8, dx = 0;
                if (r.right > window.innerWidth - margen) dx = (window.innerWidth - margen) - r.right;
                else if (r.left < margen) dx = margen - r.left;
                if (dx) panel.style.transform = 'translateX(' + Math.round(dx) + 'px)';
            }

            function abrir(v) {
                panel.hidden = !v;
                boton.setAttribute('aria-expanded', v ? 'true' : 'false');
                if (v) ajustar();
            }
            window.addEventListener('resize', function () { if (!panel.hidden) ajustar(); });

            boton.addEventListener('click', function (e) { e.stopPropagation(); abrir(panel.hidden); });
            document.addEventListener('click', function (e) { if (!menu.contains(e.target)) abrir(false); });
            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape' && !panel.hidden) { abrir(false); boton.focus(); }
            });
        });
    });
})();