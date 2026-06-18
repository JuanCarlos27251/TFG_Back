/* ==============================================
   PARKit — Formulario Parking (Gestión Pro)
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let parkingId = null;
    let modoEdicion = false;
    let companyId = null;

    //  Utilidades 
    async function apiFetch(url, opciones = {}) {
        const config = { 
            headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
            ...opciones 
        };
        const resp = await fetch(url, config);
        if (!resp.ok) {
            const err = await resp.json().catch(() => ({}));
            throw new Error(err.message || `Error ${resp.status}`);
        }
        return resp.status === 204 ? null : resp.json();
    }

    //  Arranque 
    async function initFormulario() {
        if (!AUTH.estaAutenticado() || !AUTH.esEmpresa()) {
            window.location.href = '../login.html';
            return;
        }

        companyId = parseInt(AUTH.obtenerUsuario()?.id);
        const urlParams = new URLSearchParams(window.location.search);
        parkingId = urlParams.get('id');
        modoEdicion = !!parkingId;

        // Cargar datos de empresa para el header
        try {
            const empresa = await apiFetch(`${API}/api/Company/${companyId}`);
            if (document.getElementById('header-empresa-avatar'))
                document.getElementById('header-empresa-avatar').textContent = (empresa.nameCompany || 'E').charAt(0).toUpperCase();
        } catch (e) {}

        if (modoEdicion) {
            await cargarDatosEdicion();
            await cargarTarifas();
        }

        // Eventos
        document.getElementById('parking-form')?.addEventListener('submit', guardarParking);
        document.getElementById('btn-add-tarif')?.addEventListener('click', () => {
            if (!parkingId) return mostrarToast('Guarda el parking primero', 'aviso');
            insertarFilaTarifa(document.getElementById('tarifs-body'), null);
        });
        document.getElementById('btn-generar-plazas')?.addEventListener('click', generarPlazas);
        document.getElementById('btn-desactivar')?.addEventListener('click', desactivarParking);
        document.getElementById('btn-eliminar')?.addEventListener('click', eliminarParking);

        mapboxgl.accessToken = 'pk.eyJ1IjoianVhbnBpbmEiLCJhIjoiY21sNzA3Mm4zMDJqeTNjc2k3MjBneHlpZiJ9.GXx1qQF4RW_EinsiHzTAIA';
        
        const geocoderContainer = document.getElementById('geocoder-container');
        if (geocoderContainer && typeof MapboxGeocoder !== 'undefined') {
            const geocoder = new MapboxGeocoder({
                accessToken: mapboxgl.accessToken,
                types: 'address,poi',
                placeholder: '    Ej: Calle Mayor 12, Zaragoza...',
                language: 'es',
                country: 'ES',
                mapboxgl: mapboxgl
            });
            // Inyectarlo en la vista
            geocoderContainer.appendChild(geocoder.onAdd());
            
            // Escuchar cuando el usuario elige una dirección del desplegable
            geocoder.on('result', (e) => {
                const coordenadas = e.result.center; // [longitud, latitud]
                const direccionCompleta = e.result.place_name;
                
                // Rellenar nuestros campos ocultos y bloqueados mágicamente
                document.getElementById('p-address').value = direccionCompleta;
                document.getElementById('p-lng').value = coordenadas[0];
                document.getElementById('p-lat').value = coordenadas[1];
                
                mostrarToast('Coordenadas capturadas correctamente');
            });
            // Si estamos editando, rellenar el buscador con la dirección guardada
            if (modoEdicion) {
                setTimeout(() => {
                    const addrGuardada = document.getElementById('p-address').value;
                    if (addrGuardada) geocoder.setInput(addrGuardada);
                }, 800); 
            }
        }
        
        document.addEventListener('click', (e) => {
            if (e.target.closest('#btn-logout-empresa')) AUTH.cerrarSesion(true);
        });
    }

    async function cargarDatosEdicion() {
        try {
            const p = await apiFetch(`${API}/api/Parking/${parkingId}`);
            document.querySelectorAll('[data-parking-nombre]').forEach(el => el.textContent = p.name);
            document.querySelectorAll('[data-parking-direccion]').forEach(el => el.textContent = p.address);
            
            if (document.getElementById('p-name')) document.getElementById('p-name').value = p.name || '';
            if (document.getElementById('p-address')) document.getElementById('p-address').value = p.address || '';
            if (document.getElementById('p-desc')) document.getElementById('p-desc').value = p.description || '';
            if (document.getElementById('p-lat')) document.getElementById('p-lat').value = p.latitude || '';
            if (document.getElementById('p-lng')) document.getElementById('p-lng').value = p.longitude || '';
            if (document.getElementById('facility-type')) document.getElementById('facility-type').value = p.type ?? 0;
            if (document.getElementById('is-active')) document.getElementById('is-active').value = p.isActive ? 'true' : 'false';

            const total = p.spots?.length || 0;
            const libres = p.spots?.filter(s => s.status === 0).length || 0;
            if (document.getElementById('resumen-libres')) document.getElementById('resumen-libres').textContent = libres;
            if (document.getElementById('resumen-total')) document.getElementById('resumen-total').textContent = total;
            const pct = total > 0 ? Math.round(((total - libres) / total) * 100) : 0;
            if (document.getElementById('resumen-pct')) document.getElementById('resumen-pct').textContent = `${pct}%`;
            if (document.getElementById('resumen-barra')) document.getElementById('resumen-barra').style.width = `${pct}%`;
        } catch (e) { mostrarToast(e.message, 'error'); }
    }

    async function guardarParking(e) {
        e.preventDefault();
        const payload = {
            companyId,
            name: document.getElementById('p-name').value,
            description: document.getElementById('p-desc').value,
            address: document.getElementById('p-address').value,
            latitude: parseFloat(document.getElementById('p-lat').value),
            longitude: parseFloat(document.getElementById('p-lng').value),
            type: parseInt(document.getElementById('facility-type').value),
            isActive: document.getElementById('is-active').value === 'true'
        };

        try {
            if (modoEdicion) {
                await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, { method: 'PUT', body: JSON.stringify(payload) });
                mostrarToast('Parking actualizado');
            } else {
                const res = await apiFetch(`${API}/api/ParkingManagement`, { method: 'POST', body: JSON.stringify(payload) });
                mostrarToast('Parking creado. Ahora añade tarifas.');
                setTimeout(() => window.location.replace(`editarparkingEmpresa.html?id=${res.id}`), 1000);
            }
        } catch (e) { mostrarToast(e.message, 'error'); }
    }

    //  Tarifas 
    async function cargarTarifas() {
        const tarifs = await apiFetch(`${API}/api/ParkingManagement/parking/${parkingId}/tarifs`);
        const cont = document.getElementById('tarifs-body');
        cont.innerHTML = '';
        if (!tarifs?.length) {
            cont.innerHTML = `<p class="text-center py-6 text-xs text-[var(--texto-suave)] italic">Sin tarifas registradas.</p>`;
            return;
        }
        tarifs.forEach(t => insertarFilaTarifa(cont, t));
    }

    function insertarFilaTarifa(contenedor, tarifa = null) {
        if (!contenedor) return;
        const vacio = contenedor.querySelector('p'); if (vacio) vacio.remove();

        const fila = document.createElement('div');
        // Rejilla de 10 columnas: Nombre (fr), Precio(55), Res(55), Can(55), Grn(55), Ele(55), Ini(80), Fin(80), Fest(40), Acciones(80)
        fila.className = 'tarif-row grid grid-cols-1 md:grid-cols-[1fr_55px_55px_55px_55px_55px_80px_80px_40px_80px] gap-2 px-4 py-3 border-b border-[var(--borde)] items-center hover:bg-white/5 transition-colors';
        fila.dataset.tarifId = tarifa?.id || '';

        const sTime = tarifa?.starTime?.substring(0, 5) || '';
        const eTime = tarifa?.endTime?.substring(0, 5) || '';

        // Estilo común para inputs: Oscuro, texto pequeño y sin bordes llamativos
        const inputClase = "input-base bg-[#0f172a] border-slate-700 text-white text-[11px] py-1 px-1 text-center font-medium focus:ring-1 focus:ring-blue-500";
        const nombreClase = "input-base bg-[#0f172a] border-slate-700 text-white text-[12px] py-1 px-3 font-bold focus:ring-1 focus:ring-blue-500";

        fila.innerHTML = `
            <!-- 1. Nombre -->
            <input type="text" class="t-nombre ${nombreClase} text-left" placeholder="Nombre" value="${tarifa?.nameTarif || ''}" />
            
            <!-- 2. Precio Base -->
            <input type="number" step="0.01" class="t-precio ${inputClase} font-bold text-blue-400" value="${tarifa?.pricePerHour ?? ''}" placeholder="0.0"/>
            
            <!-- 3-6. Suplementos -->
            <input type="number" step="0.01" class="t-reserva ${inputClase}" value="${tarifa?.reservationSurcharge ?? 0}" title="Reserva" />
            <input type="number" step="0.01" class="t-cancelacion ${inputClase}" value="${tarifa?.cancellationFee ?? 0}" title="Cancelación" />
            <input type="number" step="0.01" class="t-grande ${inputClase}" value="${tarifa?.largeVehicleSurcharge ?? 0}" title="Grande" />
            <input type="number" step="0.01" class="t-electrico ${inputClase}" value="${tarifa?.electricVehicleSurcharge ?? 0}" title="Eléctrico" />
            
            <!-- 7-8. Horarios -->
            <input type="time" class="t-inicio ${inputClase}" value="${sTime}" />
            <input type="time" class="t-fin ${inputClase}" value="${eTime}" />
            
            <!-- 9. Festivo -->
            <div class="flex justify-center">
                <label class="relative inline-flex items-center cursor-pointer scale-90">
                    <input type="checkbox" class="t-festivo sr-only peer" ${tarifa?.isHoliday ? 'checked' : ''} />
                    <div class="w-7 h-4 bg-slate-700 rounded-full peer peer-checked:bg-blue-600 peer-checked:after:translate-x-3 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-3 after:w-3 after:transition-all"></div>
                </label>
            </div>

            <!-- 10. Acciones -->
            <div class="flex items-center gap-1 justify-center">
                <button type="button" class="btn-guardar-tarif w-8 h-8 flex items-center justify-center rounded-lg bg-blue-600/10 text-blue-500 hover:bg-blue-600 hover:text-white transition-all shadow-sm" title="Guardar">
                    <span class="material-symbols-outlined text-[16px]">save</span>
                </button>
                <button type="button" class="btn-borrar-tarif w-8 h-8 flex items-center justify-center rounded-lg bg-red-600/10 text-red-500 hover:bg-red-600 hover:text-white transition-all shadow-sm" title="Borrar">
                    <span class="material-symbols-outlined text-[16px]">delete</span>
                </button>
            </div>
        `;

        fila.querySelector('.btn-guardar-tarif').addEventListener('click', () => guardarTarifa(fila));
        fila.querySelector('.btn-borrar-tarif').addEventListener('click', () => borrarTarifa(fila));
        contenedor.appendChild(fila);
    }


    async function guardarTarifa(fila) {
        const id = fila.dataset.tarifId;
        const payload = {
            parkingId: parseInt(parkingId),
            nameTarif: fila.querySelector('.t-nombre').value.trim(),
            pricePerHour: parseFloat(fila.querySelector('.t-precio').value),
            reservationSurcharge: parseFloat(fila.querySelector('.t-reserva').value || 0),
            cancellationFee: parseFloat(fila.querySelector('.t-cancelacion').value || 0),
            largeVehicleSurcharge: parseFloat(fila.querySelector('.t-grande').value || 0),
            electricVehicleSurcharge: parseFloat(fila.querySelector('.t-electrico').value || 0),
            isHoliday: fila.querySelector('.t-festivo').checked,
            starTime: fila.querySelector('.t-inicio').value ? fila.querySelector('.t-inicio').value + ":00" : null,
            endTime: fila.querySelector('.t-fin').value ? fila.querySelector('.t-fin').value + ":00" : null
        };

        try {
            if (id) await apiFetch(`${API}/api/ParkingManagement/tarifs/${id}`, { method: 'DELETE' });
            const res = await apiFetch(`${API}/api/ParkingManagement/tarifs`, { method: 'POST', body: JSON.stringify(payload) });
            fila.dataset.tarifId = res.id;
            mostrarToast('Tarifa guardada');
        } catch (e) { mostrarToast(e.message, 'error'); }
    }

    async function borrarTarifa(f) {
        if (f.dataset.tarifId) await apiFetch(`${API}/api/ParkingManagement/tarifs/${f.dataset.tarifId}`, { method: 'DELETE' });
        f.remove();
        mostrarToast('Tarifa eliminada');
    }

    //  Otras Funciones 
    async function generarPlazas() {
        const cant = parseInt(document.getElementById('p-total').value);
        const tipo = document.getElementById('p-spot-type').value;
        if (!cant || cant < 1) return mostrarToast('Indica cantidad', 'aviso');
        
        mostrarToast('Generando plazas...');
        for (let i = 0; i < cant; i++) {
            const sN = `${'ABCDEF'[Math.floor(i/10)]}-${(i%10)+1}`;
            await apiFetch(`${API}/api/ParkingManagement/spots`, { 
                method: 'POST', 
                body: JSON.stringify({ parkingId: parseInt(parkingId), spotNumber: sN, status: 0, type: tipo }) 
            }).catch(() => {});
        }
        mostrarToast('Plazas generadas');
        cargarDatosEdicion();
    }

    async function desactivarParking() {
        if (!confirm('¿Desactivar?')) return;
        const p = await apiFetch(`${API}/api/Parking/${parkingId}`);
        await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, { method: 'PUT', body: JSON.stringify({ ...p, isActive: false, companyId }) });
        window.location.reload();
    }

    async function eliminarParking() {
        if (!confirm('⚠️ ¿Eliminar definitivamente?')) return;
        await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, { method: 'DELETE' });
        window.location.href = 'misparkingsEmpresa.html';
    }

    document.addEventListener('DOMContentLoaded', initFormulario);
})();
