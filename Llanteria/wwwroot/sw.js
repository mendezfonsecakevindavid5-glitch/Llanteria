/* Llandupar - service worker (funcionamiento con conexión lenta o sin conexión).
   - Archivos estáticos (css, js, imágenes, fuentes y las librerías de CDN): se sirven desde el caché y se actualizan en segundo plano.
   - Páginas públicas del catálogo y del sitio: primero la red (máx. 7 s); si falla o tarda, la última copia guardada; si no hay, /offline.html.
   - NUNCA se guarda: peticiones que no sean GET, cuenta, carrito, pedidos, panel ni ninguna respuesta con "no-store" (las de usuarios con sesión). */
const VERSION = 'v1';
const CACHE_ESTATICO = 'llandupar-estatico-' + VERSION;
const CACHE_PAGINAS = 'llandupar-paginas-' + VERSION;
const OFFLINE_URL = '/offline.html';
const MAX_PAGINAS = 40;
const ESPERA_RED_MS = 7000;
const PAGINAS_PUBLICAS = /^\/(?:$|llantas|baterias|lubricantes|accesorios|promociones|servicios|blog|contacto|empresas|legal)(?:\/|$)/i;
const HOSTS_CDN = /^(?:cdnjs\.cloudflare\.com|cdn\.jsdelivr\.net|fonts\.googleapis\.com|fonts\.gstatic\.com)$/;
const ESTATICO = /\.(?:css|js|png|jpe?g|webp|svg|gif|ico|woff2?|ttf)$/i;

self.addEventListener('install', function (e) {
    e.waitUntil(caches.open(CACHE_PAGINAS).then(function (c) { return c.add(OFFLINE_URL); }).then(function () { return self.skipWaiting(); }));
});

self.addEventListener('activate', function (e) {
    e.waitUntil(caches.keys().then(function (claves) {
        return Promise.all(claves.filter(function (k) { return k.indexOf(VERSION) === -1; }).map(function (k) { return caches.delete(k); }));
    }).then(function () { return self.clients.claim(); }));
});

function guardable(res) {
    if (!res) return false;
    if (res.type === 'opaque') return true; // recursos de CDN pedidos sin CORS
    if (!res.ok) return false;
    var cc = (res.headers.get('Cache-Control') || '').toLowerCase();
    return cc.indexOf('no-store') === -1 && cc.indexOf('private') === -1;
}

function recortar(nombre, max) {
    caches.open(nombre).then(function (c) {
        c.keys().then(function (claves) { if (claves.length > max) c.delete(claves[0]).then(function () { recortar(nombre, max); }); });
    });
}

function cacheYActualiza(req) {
    return caches.open(CACHE_ESTATICO).then(function (cache) {
        return cache.match(req).then(function (enCache) {
            var red = fetch(req).then(function (res) {
                if (guardable(res)) cache.put(req, res.clone());
                return res;
            }).catch(function () { return enCache; });
            return enCache || red;
        });
    });
}

function redPrimero(req, esNavegacion) {
    return new Promise(function (resolve) {
        var terminado = false;
        function conCopia() {
            caches.open(CACHE_PAGINAS).then(function (c) { return c.match(req); }).then(function (copia) {
                if (copia) return resolve(copia);
                if (esNavegacion) return caches.match(OFFLINE_URL).then(resolve);
                resolve(new Response('', { status: 503, statusText: 'Sin conexion' }));
            });
        }
        var reloj = setTimeout(function () { if (!terminado) { terminado = true; conCopia(); } }, ESPERA_RED_MS);
        fetch(req).then(function (res) {
            if (terminado) { // llegó tarde: igual se guarda para la próxima vez
                if (guardable(res)) caches.open(CACHE_PAGINAS).then(function (c) { c.put(req, res); recortar(CACHE_PAGINAS, MAX_PAGINAS); });
                return;
            }
            terminado = true; clearTimeout(reloj);
            if (guardable(res)) {
                var copia = res.clone();
                caches.open(CACHE_PAGINAS).then(function (c) { c.put(req, copia); recortar(CACHE_PAGINAS, MAX_PAGINAS); });
            }
            resolve(res);
        }).catch(function () { if (!terminado) { terminado = true; clearTimeout(reloj); conCopia(); } });
    });
}

self.addEventListener('fetch', function (e) {
    var req = e.request;
    if (req.method !== 'GET') return;
    var url = new URL(req.url);

    if (url.origin !== self.location.origin) {
        if (HOSTS_CDN.test(url.hostname)) e.respondWith(cacheYActualiza(req));
        return;
    }
    if (url.pathname === '/sw.js') return;
    if (ESTATICO.test(url.pathname)) { e.respondWith(cacheYActualiza(req)); return; }

    var esNavegacion = req.mode === 'navigate';
    var esAjaxCatalogo = req.headers.get('X-Requested-With') === 'XMLHttpRequest';
    if ((esNavegacion || esAjaxCatalogo) && PAGINAS_PUBLICAS.test(url.pathname)) e.respondWith(redPrimero(req, esNavegacion));
});
