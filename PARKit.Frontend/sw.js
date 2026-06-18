const CACHE_NAME = 'parkit-pwa-v1';

self.addEventListener('install', (event) => {
    // Se instala inmediatamente
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    // Toma el control de los clientes abiertos
    event.waitUntil(clients.claim());
});

self.addEventListener('fetch', (event) => {
    // Si no es una petición GET (ej. un POST a tu API), no hacemos nada
    if (event.request.method !== 'GET') return;
    
    // Estrategia: Red Primero (Network First). 
    // Intenta descargar de internet, si falla, devuelve la versión guardada en caché.
    event.respondWith(
        fetch(event.request).then((response) => {
            return caches.open(CACHE_NAME).then((cache) => {
                // Guarda una copia en la caché
                cache.put(event.request, response.clone());
                return response;
            });
        }).catch(() => {
            // Si falla la red (offline), busca en la caché
            return caches.match(event.request);
        })
    );
});
