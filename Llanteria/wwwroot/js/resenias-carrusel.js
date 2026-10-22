// Carrusel de reseñas del inicio: flechas, deslizar con el dedo (nativo) y arrastrar con el mouse.
(function () {
    "use strict";
    var pista = document.getElementById("rvTrack");
    if (!pista) return;
    var cont = pista.parentElement;
    var prev = cont.querySelector(".rv-prev"), next = cont.querySelector(".rv-next");

    function paso() { var t = pista.querySelector(".rv-tarjeta"); return t ? t.getBoundingClientRect().width + 16 : 300; }
    prev.addEventListener("click", function () { pista.scrollBy({ left: -paso(), behavior: "smooth" }); });
    next.addEventListener("click", function () { pista.scrollBy({ left: paso(), behavior: "smooth" }); });

    var abajo = false, x0 = 0, s0 = 0;
    pista.addEventListener("pointerdown", function (e) {
        if (e.pointerType !== "mouse") return;
        abajo = true; x0 = e.clientX; s0 = pista.scrollLeft; pista.classList.add("arrastrando");
    });
    window.addEventListener("pointermove", function (e) { if (abajo) pista.scrollLeft = s0 - (e.clientX - x0); });
    window.addEventListener("pointerup", function () { abajo = false; pista.classList.remove("arrastrando"); });
})();
