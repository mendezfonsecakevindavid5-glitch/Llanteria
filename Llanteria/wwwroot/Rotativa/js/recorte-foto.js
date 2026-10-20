/* Recorte de foto de perfil (círculo): el usuario elige una imagen, la arrastra dentro del círculo y se devuelve un JPEG de 200x200.
   Lo usan el registro (etapa 3) y el perfil. Requiere el parcial _RecorteFoto. */
window.RecorteFoto = (function () {
    var RADIO = 90, SALIDA = 200, CALIDAD = 0.9;

    function iniciar(opciones) {
        var boton = document.querySelector(opciones.boton);
        var archivo = document.querySelector(opciones.archivo);
        var overlay = document.getElementById('cropModalOverlay');
        var canvas = document.getElementById('cropCanvas');
        var ctx = canvas.getContext('2d');
        var imagen = new Image();
        var escala = 1, x = 0, y = 0, arrastrando = false, inicioX = 0, inicioY = 0;

        function dibujar() {
            var cx = canvas.width / 2, cy = canvas.height / 2;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.save();
            ctx.drawImage(imagen, x, y, imagen.width * escala, imagen.height * escala);
            ctx.fillStyle = 'rgba(0, 0, 0, 0.55)';
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.beginPath();
            ctx.arc(cx, cy, RADIO, 0, Math.PI * 2);
            ctx.clip();
            ctx.drawImage(imagen, x, y, imagen.width * escala, imagen.height * escala);
            ctx.restore();
            ctx.beginPath();
            ctx.arc(cx, cy, RADIO, 0, Math.PI * 2);
            ctx.strokeStyle = '#ffffff';
            ctx.lineWidth = 2;
            ctx.stroke();
        }

        function cerrar() { overlay.style.display = 'none'; }

        function arrastrar(px, py) {
            if (!arrastrando) return;
            x = px - inicioX;
            y = py - inicioY;
            dibujar();
        }

        function recortar() {
            var salida = document.createElement('canvas');
            salida.width = SALIDA;
            salida.height = SALIDA;
            var c = salida.getContext('2d');
            c.beginPath();
            c.arc(SALIDA / 2, SALIDA / 2, SALIDA / 2, 0, Math.PI * 2);
            c.clip();
            var origenX = (canvas.width / 2 - RADIO - x) / escala;
            var origenY = (canvas.height / 2 - RADIO - y) / escala;
            var tamano = (RADIO * 2) / escala;
            c.drawImage(imagen, origenX, origenY, tamano, tamano, 0, 0, SALIDA, SALIDA);
            return salida.toDataURL('image/jpeg', CALIDAD);
        }

        boton.addEventListener('click', function () { archivo.click(); });

        archivo.addEventListener('change', function (e) {
            var f = e.target.files && e.target.files[0];
            if (!f) return;
            var lector = new FileReader();
            lector.onload = function (evt) {
                imagen = new Image();
                imagen.onload = function () {
                    escala = Math.max((RADIO * 2) / imagen.width, (RADIO * 2) / imagen.height);
                    x = (canvas.width - imagen.width * escala) / 2;
                    y = (canvas.height - imagen.height * escala) / 2;
                    dibujar();
                    overlay.style.display = 'flex';
                };
                imagen.src = evt.target.result;
            };
            lector.readAsDataURL(f);
        });

        canvas.addEventListener('mousedown', function (e) { arrastrando = true; inicioX = e.clientX - x; inicioY = e.clientY - y; });
        canvas.addEventListener('mousemove', function (e) { arrastrar(e.clientX, e.clientY); });
        window.addEventListener('mouseup', function () { arrastrando = false; });
        canvas.addEventListener('touchstart', function (e) {
            if (e.touches.length !== 1) return;
            arrastrando = true;
            inicioX = e.touches[0].clientX - x;
            inicioY = e.touches[0].clientY - y;
        });
        canvas.addEventListener('touchmove', function (e) { if (e.touches.length === 1) arrastrar(e.touches[0].clientX, e.touches[0].clientY); });
        canvas.addEventListener('touchend', function () { arrastrando = false; });

        document.getElementById('btnCloseCropModal').addEventListener('click', function () { cerrar(); archivo.value = ''; });
        document.getElementById('btnCambiarImagen').addEventListener('click', function () { cerrar(); archivo.click(); });
        document.getElementById('btnAceptarImagen').addEventListener('click', function () {
            opciones.alAceptar(recortar());
            cerrar();
        });
    }

    return { iniciar: iniciar };
})();
