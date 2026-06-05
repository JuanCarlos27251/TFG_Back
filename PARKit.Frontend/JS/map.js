/* ==============================================
   PARKit — Controlador del mapa (maqueta)
   Depende de: Mapbox GL JS + Mapbox Geocoder
   ============================================== */

mapboxgl.accessToken = 'pk.eyJ1IjoianVhbnBpbmEiLCJhIjoiY21sNzA3Mm4zMDJqeTNjc2k3MjBneHlpZiJ9.GXx1qQF4RW_EinsiHzTAIA';

class ParkingMap {
    constructor() {
        this.map          = null;
        this.geocoder     = null;
        this.searchMarker = null;
        this.userLocation = null;
        this.init();
    }

    init() {
        this.initializeMap();
        this.setupGeocoder();
        this.setupUIEventListeners();
        this.updateStatus('Mapa listo');
    }

    initializeMap() {
        this.map = new mapboxgl.Map({
            container:         'map',
            style:             'mapbox://styles/mapbox/streets-v12',
            center:            [-0.877, 41.6488],
            zoom:              14,
            attributionControl: false,
        });

        this.map.addControl(
            new mapboxgl.AttributionControl({ compact: true }),
            'bottom-right'
        );

        this.map.on('load', () => this.updateCoordinates());
        this.map.on('move', () => this.updateCoordinates());
    }

    setupGeocoder() {
        this.geocoder = new MapboxGeocoder({
            accessToken: mapboxgl.accessToken,
            mapboxgl:    mapboxgl,
            placeholder: 'Buscar dirección en Zaragoza...',
            countries:   'es',
            proximity:   { longitude: -0.877, latitude: 41.6488 },
            marker:      false,
        });

        const container = document.getElementById('geocoder-container');
        if (container) {
            container.appendChild(this.geocoder.onAdd(this.map));
        }

        this.geocoder.on('result', (e) => this.handleSearchResults(e.result));
    }

    setupUIEventListeners() {
        this.geolocator = new mapboxgl.GeolocateControl({
            positionOptions:  { enableHighAccuracy: true },
            trackUserLocation: true,
            showUserHeading:   true,
        });
        this.map.addControl(this.geolocator);

        this.geolocator.on('geolocate', (e) => {
            this.userLocation = [e.coords.longitude, e.coords.latitude];
            this.updateStatus('Ubicación fijada');
        });

        const btnCerrar = document.getElementById('btn-cerrar-detalles');
        if (btnCerrar) {
            btnCerrar.addEventListener('click', () => this.closeDetailsPanel());
        }
    }

    handleSearchResults(result) {
        if (this.searchMarker) this.searchMarker.remove();

        this.searchMarker = new mapboxgl.Marker({ color: '#135bec' })
            .setLngLat(result.center)
            .addTo(this.map);

        this.map.flyTo({ center: result.center, zoom: 16, essential: true });

        document.getElementById('detalle-nombre').textContent    = result.text      || 'Dirección encontrada';
        document.getElementById('detalle-tipo').textContent      = 'Punto de interés';
        document.getElementById('detalle-direccion').textContent = result.place_name || 'Zaragoza, España';
        document.getElementById('detalle-plazas').textContent    = '--';
        document.getElementById('detalle-capacidad').textContent = '--';
        document.getElementById('detalle-precio').textContent    = 'Selecciona un parking del mapa';

        document.getElementById('panel-detalles')?.classList.add('abierto');
        this.updateStatus(`Mostrando: ${result.text}`);
    }

    closeDetailsPanel() {
        document.getElementById('panel-detalles')?.classList.remove('abierto');

        if (this.searchMarker) {
            this.searchMarker.remove();
            this.searchMarker = null;
        }

        this.updateStatus('Listo');
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
}

document.addEventListener('DOMContentLoaded', () => {
    window.parkingMap = new ParkingMap();
});