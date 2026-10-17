// Panel: contraer/expandir el menú lateral (se recuerda) y menú móvil.
(function () {
    'use strict';
    var body = document.body;
    var KEY = 'llanteria.sidebar';

    try { if (localStorage.getItem(KEY) === 'colapsado') body.classList.add('sidebar-collapsed'); } catch (e) { }

    var toggle = document.getElementById('sidebarToggle');
    if (toggle) toggle.addEventListener('click', function () {
        // En pantallas pequeñas el mismo botón cierra el menú.
        if (window.matchMedia('(max-width: 991.98px)').matches) { body.classList.remove('sidebar-open'); return; }
        var c = body.classList.toggle('sidebar-collapsed');
        try { localStorage.setItem(KEY, c ? 'colapsado' : 'expandido'); } catch (e) { }
    });

    var abrir = document.getElementById('adminMenuBtn');
    var fondo = document.getElementById('adminBackdrop');
    if (abrir) abrir.addEventListener('click', function () { body.classList.add('sidebar-open'); });
    if (fondo) fondo.addEventListener('click', function () { body.classList.remove('sidebar-open'); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') body.classList.remove('sidebar-open'); });
})();

// El modal de perfil se mueve al <body> para que no quede atrapado en el contenedor fijo del avatar.
document.addEventListener('DOMContentLoaded', function () {
    var m = document.getElementById('perfilModal');
    if (m && m.parentElement !== document.body) document.body.appendChild(m);
});
