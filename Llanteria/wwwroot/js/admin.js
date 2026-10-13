// Confirmación de eliminación para cualquier botón con data-delete="<url>"
document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-delete]');
    if (!btn) return;

    const nombre = btn.dataset.nombre || 'este registro';
    const aviso = btn.dataset.aviso || 'Esta acción no se puede deshacer.';

    Swal.fire({
        title: '¿Eliminar registro?',
        html: '<strong></strong><p class="mt-2 mb-0 small text-muted"></p>',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#C92B0A',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true,
        focusCancel: true,
        didOpen: function (popup) {
            popup.querySelector('strong').textContent = nombre;
            popup.querySelector('p').textContent = aviso;
        }
    }).then(function (result) {
        if (!result.isConfirmed) return;
        // La eliminación se envía por POST con token antifalsificación.
        const form = document.getElementById('delete-form');
        form.action = btn.dataset.delete;
        form.submit();
    });
});

// Vista previa de imagen: <input type="file" data-preview="#idImg">
document.addEventListener('change', function (e) {
    const input = e.target.closest('input[type=file][data-preview]');
    if (!input || !input.files[0]) return;
    const img = document.querySelector(input.dataset.preview);
    if (!img) return;
    const reader = new FileReader();
    reader.onload = function (ev) { img.src = ev.target.result; };
    reader.readAsDataURL(input.files[0]);
});
