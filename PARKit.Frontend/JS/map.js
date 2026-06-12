/* ==============================================
   PARKit — Controlador Principal del Mapa
   Geolocalización, Routing y Conexión a BD
   ============================================== */

mapboxgl.accessToken = 'pk.eyJ1IjoianVhbnBpbmEiLCJhIjoiY21sNzA3Mm4zMDJqeTNjc2k3MjBneHlpZiJ9.GXx1qQF4RW_EinsiHzTAIA';

class ParkingMap {
    constructor() {
        this.API_BASE     = window.AUTH ? AUTH.API_BASE : 'https://localhost:7033';
        this.map          = null;
        this.geocoder     = null;
        this.userLocation = null;
        this.parkings     = [];
        this.markers      = [];
        
        this.selectedParking = null;
        this.modoNavegacion = false;
        this.esperandoRuta = false;

        this.init();
    }

    init() {
        this.initializeMap();
        this.setupGeocoder();
        this.setupUIEventListeners();
        this.updateStatus('Cargando mapa...');
        this.comprobarRutaPendiente();
         this.setupSignalR();

    }

    initializeMap() {
        this.map = new mapboxgl.Map({
            container: 'map',
            style: 'mapbox://styles/mapbox/streets-v12', 
            center: [-0.880, 41.650], 
            zoom: 13,
            attributionControl: false
        });

        this.map.addControl(new mapboxgl.AttributionControl({ compact: true }), 'bottom-right');
        
        this.map.on('load', () => {
            this.updateCoordinates();
            this.cargarParkings(); 
        });
        
        this.map.on('move', () => this.updateCoordinates());
        
        // El click original del mapa que nos daba el problema
        this.map.on('click', (e) => {
            // Si estamos conduciendo, ignoramos los clics
            if (this.modoNavegacion) return; 

            // Si pinchamos el mapa vacío (sin parkings), lo limpiamos todo
            const features = this.map.queryRenderedFeatures(e.point);
            if (!features.length) {
                this.cerrarPanel();
            }
        });
    }

    setupGeocoder() {
        this.geocoder = new MapboxGeocoder({
            accessToken: mapboxgl.accessToken,
            mapboxgl: mapboxgl,
            placeholder: 'Buscar dirección...',
            countries: 'es',
            bbox: [-1.0, 41.5, -0.7, 41.8], 
            marker: true
        });

        const container = document.getElementById('geocoder-container');
        if (container) container.appendChild(this.geocoder.onAdd(this.map));
    }

    setupUIEventListeners() {
        this.geolocator = new mapboxgl.GeolocateControl({
            positionOptions: { enableHighAccuracy: true },
            trackUserLocation: true,
            showUserHeading: true
        });
        this.map.addControl(this.geolocator, 'bottom-right');

        this.geolocator.on('geolocate', (e) => {
            this.userLocation = [e.coords.longitude, e.coords.latitude];
            
            // Si estamos en modo coche, forzamos cámara a su estela
            if (this.modoNavegacion) {
                 const velocidad = e.coords.speed ? (e.coords.speed * 3.6).toFixed(0) : 0;
                 this.updateStatus(`Navegando... ${velocidad} km/h`);
                 
                 this.map.easeTo({
                     center: this.userLocation,
                     pitch: 65, 
                     bearing: e.coords.heading || 0,
                     zoom: 18.5
                 });
            } else {
                 this.updateStatus('Ubicación fijada');
            }
            
            // Si le había dado "Cómo llegar" y no tenía GPS cargado aún
            if (this.esperandoRuta) {
                this.esperandoRuta = false;
                this.trazarRuta();
            }
        });

        // Eventos visuales de HUD — ¡AQUÍ ESTÁ LA SOLUCIÓN DEL BUBBLING!
        document.getElementById('btn-cerrar-detalles')?.addEventListener('click', (e) => {
            e.stopPropagation();
            this.cerrarPanel();
        });

        document.getElementById('btn-como-llegar')?.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!this.userLocation) {
                this.esperandoRuta = true;
                this.updateStatus('Activando GPS...');
                this.geolocator.trigger(); 
            } else {
                this.trazarRuta();
            }
        });

        document.getElementById('btn-iniciar-navegacion')?.addEventListener('click', (e) => {
            e.stopPropagation(); // Evita que traspase al mapa y borre la ruta accidentalmente
            this.iniciarHUDConduccion();
        });

        document.getElementById('btn-exportar-gmaps')?.addEventListener('click', (e) => {
            e.stopPropagation();
            if (this.userLocation && this.selectedParking) {
                // Mapbox usa [longitud, latitud], pero Google Maps requiere "latitud,longitud"
                const origin = `${this.userLocation[1]},${this.userLocation[0]}`;
                const dest = `${this.selectedParking.latitude},${this.selectedParking.longitude}`;
                // Creamos la URL mágica de Google Maps
                const url = `https://www.google.com/maps/dir/?api=1&origin=${origin}&destination=${dest}&travelmode=driving`;
                window.open(url, '_blank'); // Abre en nueva pestaña
            }
        });

        document.getElementById('btn-salir-navegacion')?.addEventListener('click', (e) => {
            e.stopPropagation(); 
            this.salirHUDConduccion();
        });

        document.getElementById('btn-reservar')?.addEventListener('click', (e) => {
            e.stopPropagation();
            this.ejecutarReserva();
        });
    }

    // ── GESTIÓN DE PARKINGS Y RUTAS ──

    async cargarParkings() {
        try {
            const resp = await fetch(`${this.API_BASE}/api/Parking`);
            if (resp.ok) {
                this.parkings = await resp.json();
                this.dibujarPinchosEnMapa();
            }
        } catch (error) {
            console.error('Error cargando parkings de .NET', error);
        }
    }

    dibujarPinchosEnMapa() {
        this.parkings.forEach(p => {
            if (!p.latitude || !p.longitude) return;

            const el = document.createElement('div');
            el.className = 'w-10 h-10 bg-[var(--azul)] text-white rounded-full flex items-center justify-center border-2 border-white shadow-lg cursor-pointer hover:scale-110 transition-transform';
            el.innerHTML = '<span class="material-symbols-outlined text-xl">local_parking</span>';

            const marker = new mapboxgl.Marker({ element: el })
                .setLngLat([p.longitude, p.latitude])
                .addTo(this.map);

            el.addEventListener('click', (e) => {
                e.stopPropagation();
                this.abrirPanelDetalles(p);
            });

            this.markers.push(marker);
        });
    }

    async abrirPanelDetalles(parking) {
        this.selectedParking = parking;

        this.map.flyTo({ 
            center: [parking.longitude, parking.latitude], 
            zoom: 16, 
            offset: [0, 80], 
            essential: true 
        });

        // Nombres y Ubicación
        document.getElementById('detalle-nombre').textContent    = parking.name || 'Parking Municipal';
        document.getElementById('detalle-direccion').textContent = parking.address || 'Zaragoza';
        
        // ── AQUÍ LA MAGIA: Lectura de Plazas de Base de Datos ──
        // Si hay 'availableSpots' validos, los pone, sino avisa con '--'
        document.getElementById('detalle-plazas').textContent    = parking.availableSpots != null ? parking.availableSpots : '--'; 
        
        // La capacidad total será el número de items dentro de la lista de Seats o Spots
        const totalSpots = parking.spots && parking.spots.length > 0 ? parking.spots.length : 'Depende de vía';
        document.getElementById('detalle-capacidad').textContent = totalSpots;

        // ── LECTURA DE TARIFA OFICIAL ──
        let precioPantalla = 'Sin tarifa';
        if (parking.tarifs && parking.tarifs.length > 0) {
            precioPantalla = `${Number(parking.tarifs[0].pricePerHour).toFixed(2)} € / h`;
        }
        document.getElementById('detalle-precio').textContent = precioPantalla;

        document.getElementById('panel-detalles')?.classList.add('abierto');
        
        // Quitar vieja ruta si había una
        this.limpiarRuta();
    }


    cerrarPanel() {
        document.getElementById('panel-detalles')?.classList.remove('abierto');
        this.limpiarRuta();
        this.selectedParking = null;
    }

    async trazarRuta() {
        if (!this.userLocation || !this.selectedParking) return;

        const start = this.userLocation;
        const end = [this.selectedParking.longitude, this.selectedParking.latitude];
        const token = mapboxgl.accessToken;
        const url = `https://api.mapbox.com/directions/v5/mapbox/driving-traffic/${start[0]},${start[1]};${end[0]},${end[1]}?geometries=geojson&access_token=${token}`;

        try {
            this.updateStatus('Calculando...');
            const response = await fetch(url);
            const data = await response.json();
            
            if (data.routes && data.routes.length > 0) {
                const route = data.routes[0].geometry;
                this.dibujarPolyline(route);
                
                const bounds = new mapboxgl.LngLatBounds(start, end);
                this.map.fitBounds(bounds, { padding: 80 });

                this.updateStatus(`Ruta: ${(data.routes[0].duration / 60).toFixed(0)} min`);

                // ¡Plop! Aparece el botón de iniciar HUD.
                document.getElementById('contenedor-rutas')?.classList.remove('hidden');
            }
        } catch (e) {
            console.error(e);
        }
    }

    dibujarPolyline(geojson) {
        if (this.map.getSource('route-src')) {
            this.map.getSource('route-src').setData(geojson);
        } else {
            this.map.addSource('route-src', { type: 'geojson', data: { type: 'Feature', geometry: geojson } });
            this.map.addLayer({
                id: 'route-line',
                type: 'line',
                source: 'route-src',
                layout: { 'line-join': 'round', 'line-cap': 'round' },
                paint: { 'line-color': '#3b82f6', 'line-width': 6, 'line-opacity': 0.8 }
            }, 'waterway-label');
        }
    }

    limpiarRuta() {
        if (this.map.getSource('route-src')) {
            this.map.removeLayer('route-line');
            this.map.removeSource('route-src');
        }
        document.getElementById('contenedor-rutas')?.classList.add('hidden');
    }

    // ── MOTOR DE CONDUCCIÓN HUD AUTOMÁTICA ──

    iniciarHUDConduccion() {
        if (!this.userLocation) return;
        this.modoNavegacion = true;

        // Limpiar pantalla al estilo Android Auto
        document.getElementById('panel-detalles')?.classList.remove('abierto');
        document.getElementById('contenedor-rutas')?.classList.add('hidden');
        document.getElementById('geocoder-container').style.display = 'none';

        // Botón rojo de salida
        document.getElementById('btn-salir-navegacion').classList.remove('hidden');

        // Cámara de conductor de rally (Pitch 65º)
        this.map.flyTo({
            center: this.userLocation,
            zoom: 18,
            pitch: 65,
            bearing: this.map.getBearing(),
            essential: true
        });

        this.geolocator.trigger(); 
    }

    salirHUDConduccion() {
        this.modoNavegacion = false;
        
        document.getElementById('btn-salir-navegacion').classList.add('hidden');
        document.getElementById('geocoder-container').style.display = 'block';

        // Volver a cámara de pájaro plana 0 Pitch
        this.map.easeTo({
            pitch: 0,
            bearing: 0,
            zoom: 14,
            essential: true
        });

        // Limpiamos la ruta dibujada en pantalla al salir
        this.limpiarRuta();
        this.selectedParking = null;
        
        this.updateStatus('Explorando');
    }

    // ── PASARELA DE CHECKOUT ──

    ejecutarReserva() {
        const p = this.selectedParking;
        if(!p) return;

        if (window.AUTH && !AUTH.estaAutenticado()) {
            alert("🔒 Inicia sesión para guardar la plaza.");
            window.location.href = 'login.html';
            return;
        }

        const btn = document.getElementById('btn-reservar');
        btn.innerHTML = `<span class="w-4 h-4 rounded-full border-2 border-[var(--azul)] border-t-white animate-spin"></span> Cargando...`;
        
        setTimeout(() => {
            btn.innerHTML = `<span class="material-symbols-outlined text-sm">front_loader</span> Transfiriendo...`;
            btn.classList.replace('btn-ghost', 'btn-primario');
            btn.classList.replace('text-[var(--texto)]', 'text-white');
            
            setTimeout(() => { 
                window.location.href = `pagos.html?parkingId=${p.id}`; 
            }, 1000); 

        }, 1000);
    }

    updateCoordinates() {
        const el = document.getElementById('coordinates');
        if (el) {
            const c = this.map.getCenter();
            el.textContent = `${c.lat.toFixed(4)}, ${c.lng.toFixed(4)}`;
        }
    }

    updateStatus(message) {
        const el = document.getElementById('status-text');
        if (el) el.textContent = message;
    }
    // ── RUTA PENDIENTE DESDE CONFIRMACIÓN DE PAGO ──
    comprobarRutaPendiente() {
        const raw = localStorage.getItem('pendingRoute');
        if (!raw) return;

        try {
            const ruta = JSON.parse(raw);
            if (!ruta.trazar || !ruta.destLat || !ruta.destLng) return;

            localStorage.removeItem('pendingRoute'); // limpiar para no re-trazar en siguiente visita

            // Esperamos a que el mapa cargue del todo
            this.map.once('load', () => {
                // Marcador del destino
                const elDest = document.createElement('div');
                elDest.className = 'w-10 h-10 bg-[var(--azul)] text-white rounded-full flex items-center justify-center border-2 border-white shadow-lg';
                elDest.innerHTML = '<span class="material-symbols-outlined text-xl">local_parking</span>';
                new mapboxgl.Marker({ element: elDest })
                    .setLngLat([ruta.destLng, ruta.destLat])
                    .addTo(this.map);

                this.updateStatus('Activando GPS para trazar ruta...');

                // Activamos el geolocalizador; cuando responda, trazamos la ruta
                this.geolocator.once('geolocate', (e) => {
                    this.userLocation = [e.coords.longitude, e.coords.latitude];

                    // Simulamos tener un parking seleccionado para aprovechar trazarRuta()
                    this.selectedParking = { latitude: ruta.destLat, longitude: ruta.destLng, name: ruta.destName };
                    this.trazarRuta();

                    // Abrimos el panel de detalles con el nombre
                    document.getElementById('detalle-nombre').textContent    = ruta.destName;
                    document.getElementById('detalle-direccion').textContent = 'Parking reservado';
                    document.getElementById('panel-detalles')?.classList.add('abierto');
                });

                this.geolocator.trigger();
            });

        } catch (e) {
            console.error('[Map] Error al leer pendingRoute:', e);
        }
    }

    setupSignalR() {
        // Validación de seguridad para que no casque si olvidaste el script en el HTML
        if (typeof signalR === 'undefined') {
            console.warn('[SignalR] No detectado. Añade la librería CDN en tu HTML.');
            return;
        }

        const url = `${this.API_BASE}/hubs/parking`;
        
        const connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .build();

        // Escuchamos el evento exacto que mandamos desde .NET
        connection.on("UpdateSpots", (parkingId, newCount) => {
            console.log(`[SignalR] Actualización recibida para Parking ${parkingId}. Nuevas plazas: ${newCount}`);
            
            // 1. Actualizamos la memoria caché del frontend para próximos clics
            const parking = this.parkings.find(p => p.id === parkingId);
            if (parking) {
                parking.availableSpots = newCount;
            }

            // 2. Si resulta que el usuario está viendo ahora mismo ESE parking, actualizamos el número mágico
            if (this.selectedParking && this.selectedParking.id === parkingId) {
                const plazasEl = document.getElementById('detalle-plazas');
                if (plazasEl) {
                    plazasEl.textContent = newCount;
                    
                    // Efecto visual (Destello verde) para que se note en directo
                    plazasEl.style.transition = 'all 0.3s ease';
                    plazasEl.style.color = '#10b981'; // Verde llamativo
                    plazasEl.style.transform = 'scale(1.3)';
                    
                    setTimeout(() => {
                        plazasEl.style.color = '';
                        plazasEl.style.transform = 'scale(1)';
                    }, 500);
                }
            }
        });

        connection.start()
        .then(() => console.log('[SignalR] Conectado al Hub exitosamente para tiempo real 🟢'))
        .catch(err => console.error('[SignalR] Error de conexión:', err));
    }



}

document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => { window.parkingMap = new ParkingMap(); }, 150);
});
