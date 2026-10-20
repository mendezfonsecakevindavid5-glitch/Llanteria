// Franja de marcas: avanza hasta el final, regresa al inicio y repite (ida y vuelta).
// Se pausa con el mouse encima o al tocarla, y no se mueve si el usuario prefiere menos animación.
(function () {
  const viewport = document.querySelector('[data-marquee]');
  if (!viewport) return;
  const track = viewport.querySelector('.tc-brands-track');
  if (!track) return;

  const VELOCIDAD = 45;    // píxeles por segundo
  const ESPERA = 900;      // ms de pausa en cada extremo
  const reducir = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  let pos = 0, dir = 1, max = 0, previo = null, pausado = false, esperaHasta = 0;

  function medir() {
    max = Math.max(0, track.scrollWidth - viewport.clientWidth);
    if (pos > max) pos = max;
  }

  function paso(t) {
    if (previo === null) previo = t;
    const dt = Math.min((t - previo) / 1000, 0.1);
    previo = t;

    if (!pausado && max > 0 && t >= esperaHasta) {
      pos += dir * VELOCIDAD * dt;
      if (pos >= max) { pos = max; dir = -1; esperaHasta = t + ESPERA; }
      else if (pos <= 0) { pos = 0; dir = 1; esperaHasta = t + ESPERA; }
      track.style.transform = 'translate3d(' + (-pos) + 'px,0,0)';
    }
    requestAnimationFrame(paso);
  }

  viewport.addEventListener('mouseenter', () => { pausado = true; });
  viewport.addEventListener('mouseleave', () => { pausado = false; });
  viewport.addEventListener('touchstart', () => { pausado = true; }, { passive: true });
  viewport.addEventListener('touchend', () => { setTimeout(() => { pausado = false; }, 1500); }, { passive: true });

  window.addEventListener('resize', medir);
  track.querySelectorAll('img').forEach(function (img) {
    if (!img.complete) img.addEventListener('load', medir, { once: true });
  });
  medir();

  if (!reducir) requestAnimationFrame(paso);
})();
