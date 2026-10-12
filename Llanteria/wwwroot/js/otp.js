// Flujo de código de verificación por correo (registro y recuperación de contraseña).
// Cada página define window.OTP = { enviar: url, verificar: url, onVerified: fn }.
(function () {
    const cfg = window.OTP;
    const form = document.querySelector('form[data-otp]');
    if (!cfg || !form) return;

    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const email = document.getElementById('emailInput');
    const btnEnviar = document.getElementById('btnEnviar');
    const bloqueCodigo = document.getElementById('bloqueCodigo');
    const codigo = document.getElementById('codigo');
    const btnVerificar = document.getElementById('btnVerificar');
    const estado = document.getElementById('otpEstado');

    function mensaje(texto, ok) {
        estado.textContent = texto;
        estado.className = 'small mt-2 ' + (ok ? 'text-success' : 'text-danger');
    }

    async function post(url, cuerpo) {
        const r = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify(cuerpo)
        });
        if (!r.ok) throw new Error(r.status);
        return r.json();
    }

    btnEnviar.addEventListener('click', async function () {
        if (!email.checkValidity() || !email.value) { email.reportValidity(); return; }
        btnEnviar.disabled = true;
        try {
            const res = await post(cfg.enviar, { correo: email.value, nombre: (document.getElementById('Nombres') || {}).value || '' });
            if (res.success) {
                bloqueCodigo.classList.remove('d-none');
                codigo.focus();
                mensaje('Te enviamos un código de 6 dígitos. Vence en 10 minutos.', true);
                setTimeout(() => { btnEnviar.disabled = false; btnEnviar.textContent = 'Reenviar código'; }, 30000);
            } else {
                mensaje(res.message, false);
                btnEnviar.disabled = false;
            }
        } catch (e) {
            mensaje('No se pudo enviar el código. Inténtalo de nuevo.', false);
            btnEnviar.disabled = false;
        }
    });

    btnVerificar.addEventListener('click', async function () {
        const valor = codigo.value.trim();
        if (valor.length !== 6) { mensaje('Escribe los 6 dígitos del código.', false); return; }
        try {
            const res = await post(cfg.verificar, valor);
            if (res.success) {
                email.readOnly = true;
                btnEnviar.classList.add('d-none');
                bloqueCodigo.classList.add('d-none');
                mensaje('Correo verificado.', true);
                cfg.onVerified();
            } else {
                mensaje(res.message, false);
            }
        } catch (e) {
            mensaje('No se pudo verificar el código.', false);
        }
    });
})();
