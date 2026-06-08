/* ==============================================
   PARKit — Formulario Parking (Añadir / Editar)
   Usado en: addparkingEmpresa.html
             editarparkingEmpresa.html
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let parkingId = null;
    let modoEdicion = false;
    let companyId = null;

    // ── Utilidades ──────────────────────────────

    function mostrarToast(msg, tipo = 'exito') {
        const cont = document.getElementById('contenedor-toast');
        if (!cont) return;
        const toast = document.createElement('div');
        const iconos  = { exito: 'check_circle', error: 'error', aviso: 'warning' };
        const colores = { exito: 'verde', error: 'rojo', aviso: 'naranja' };
        toast.className = `toast toast-${tipo}`;
        toast.innerHTML = `
            <span class="material-symbols-outlined icono-relleno" style="color:var(--${colores[tipo] || 'azul'})">${iconos[tipo] || 'info'}</span>
            <span class="text-sm font-medium">${msg}</span>`;
        cont.appendChild(toast);
        setTimeout(() => toast.remove(), 3500);
    }

    async function apiFetch(url, opciones = {}) {
        const defaultHeaders = { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() };
        const resp = await fetch(url, { headers: defaultHeaders, ...opciones });
        if (!resp.ok) {
            const err = await resp.json().catch(() => ({}));
            throw new Error(err.message || `Error ${resp.status}`);
        }
        if (resp.status === 204) return null;
        return resp.json();
    }

    function verificarSesion() {
        if (!AUTH.estaAutenticado() || !AUTH.esEmpresa()) {
            window.location.href = '../login.html';
            return false;
        }
        return true;
    }

    // ── Arranque ────────────────────────────────

    async function initFormulario() {
        if (!verificarSesion()) return;

        companyId = parseInt(AUTH.obtenerUsuario()?.id);
        const urlParams = new URLSearchParams(window.location.search);
        parkingId = urlParams.get('id');
        modoEdicion = !!parkingId;

        try {
            const empresa = await apiFetch(`${API}/api/Company/${companyId}`);
            const inicial = (empresa.nameCompany || 'E').charAt(0).toUpperCase();
            if (document.getElementById('header-empresa-avatar'))
                document.getElementById('header-empresa-avatar').textContent = inicial;
        } catch (e) { /* silencioso */ }

        if (modoEdicion) {
            await cargarDatosEdicion();
            await cargarTarifas();
        }

        // Evento guardar parking
        document.getElementById('parking-form')?.addEventListener('submit', guardarParking);

        // Evento añadir tarifa
        document.getElementById('btn-add-tarif')?.addEventListener('click', agregarFilaTarifaNueva);

        // Evento generar plazas (capacidad total)
        document.getElementById('btn-generar-plazas')?.addEventListener('click', generarPlazas);

        // Zona de peligro (solo en editar)
        document.getElementById('btn-desactivar')?.addEventListener('click', desactivarParking);
        document.getElementById('btn-eliminar')?.addEventListener('click', eliminarParking);

        // Log out
        document.addEventListener('click', (e) => {
            if (e.target.closest('#btn-logout-empresa')) AUTH.cerrarSesion(true);
        });
    }

    // ── Modo Edición: Cargar datos del parking ──

    async function cargarDatosEdicion() {
        try {
            const p = await apiFetch(`${API}/api/Parking/${parkingId}`);

            // Breadcrumb / banner
            const nombre = p.name || `Parking #${parkingId}`;
            document.querySelectorAll('[data-parking-nombre]').forEach(el => el.textContent = nombre);
            document.querySelectorAll('[data-parking-direccion]').forEach(el => el.textContent = p.address || '');

            const badgeActivo = document.getElementById('badge-activo');
            if (badgeActivo) {
                badgeActivo.className = p.isActive 
                    ? 'badge badge-verde text-[9px] font-extrabold uppercase tracking-widest py-0.5 flex items-center gap-1'
                    : 'badge badge-rojo text-[9px] font-extrabold uppercase tracking-widest py-0.5 flex items-center gap-1';
                badgeActivo.innerHTML = p.isActive
                    ? `<span class="w-1.5 h-1.5 rounded-full bg-[var(--verde)] animate-pulse"></span>Activo`
                    : `<span class="w-1.5 h-1.5 rounded-full bg-[var(--rojo)]"></span>Inactivo`;
            }

            const bannerImg = document.getElementById('banner-img');
            if (bannerImg && p.imageUrl) bannerImg.src = p.imageUrl;

            // Campos del formulario
            if (document.getElementById('p-name')) document.getElementById('p-name').value = p.name || '';
            if (document.getElementById('p-address')) document.getElementById('p-address').value = p.address || '';
            if (document.getElementById('p-desc')) document.getElementById('p-desc').value = p.description || '';
            if (document.getElementById('p-lat')) document.getElementById('p-lat').value = p.latitude || '';
            if (document.getElementById('p-lng')) document.getElementById('p-lng').value = p.longitude || '';
            if (document.getElementById('is-active')) document.getElementById('is-active').value = p.isActive ? 'true' : 'false';
            if (document.getElementById('facility-type')) document.getElementById('facility-type').value = p.type ?? 0;

            // Resumen estado actual (solo en editarparkingEmpresa)
            actualizarResumenOcupacion(p.spots || []);

        } catch (e) {
            mostrarToast('Error cargando datos del parking: ' + e.message, 'error');
        }
    }

    function actualizarResumenOcupacion(spots) {
        if (!spots || spots.length === 0) return;
        const total = spots.length;
        const ocupadas = spots.filter(s => s.status === 1 || s.status === 2).length;
        const libres = total - ocupadas;
        const pct = total > 0 ? Math.round((ocupadas / total) * 100) : 0;

        if (document.getElementById('resumen-libres')) document.getElementById('resumen-libres').textContent = libres;
        if (document.getElementById('resumen-total')) document.getElementById('resumen-total').textContent = total;
        if (document.getElementById('resumen-pct')) document.getElementById('resumen-pct').textContent = `${pct}%`;
        if (document.getElementById('resumen-barra')) document.getElementById('resumen-barra').style.width = `${pct}%`;
    }

    // ── Guardar Parking (POST o PUT) ────────────

    async function guardarParking(e) {
        e.preventDefault();

        const payload = {
            companyId: companyId,
            name: document.getElementById('p-name')?.value.trim() || '',
            description: document.getElementById('p-desc')?.value.trim() || '',
            address: document.getElementById('p-address')?.value.trim() || '',
            latitude: parseFloat(document.getElementById('p-lat')?.value) || 0,
            longitude: parseFloat(document.getElementById('p-lng')?.value) || 0,
            type: parseInt(document.getElementById('facility-type')?.value ?? 0),
            isActive: document.getElementById('is-active')?.value === 'true',
            imageUrl: null
        };

        try {
            if (modoEdicion) {
                await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, {
                    method: 'PUT',
                    body: JSON.stringify(payload)
                });
                mostrarToast('Parking actualizado con éxito');
                // Recargamos el resumen de ocupación
                await cargarDatosEdicion();
            } else {
                const result = await apiFetch(`${API}/api/ParkingManagement`, {
                    method: 'POST',
                    body: JSON.stringify(payload)
                });
                mostrarToast('Parking creado con éxito');
                // Redirigir a edición para poder añadir tarifas y plazas
                setTimeout(() => {
                    window.location.replace(`editarparkingEmpresa.html?id=${result.id}`);
                }, 1500);
            }
        } catch (err) {
            mostrarToast('Error al guardar: ' + err.message, 'error');
        }
    }

    // ── Gestión de Tarifas ──────────────────────

    async function cargarTarifas() {
        if (!parkingId) return;
        try {
            const tarifs = await apiFetch(`${API}/api/ParkingManagement/parking/${parkingId}/tarifs`);
            const contenedor = document.getElementById('tarifs-body');
            if (!contenedor) return;
            contenedor.innerHTML = '';

            if (tarifs.length === 0) {
                contenedor.innerHTML = `<p class="text-center py-6 text-xs text-[var(--texto-suave)] italic col-span-full">Sin tarifas. Pulsa "Añadir Regla" para crear una.</p>`;
                return;
            }

            tarifs.forEach(t => insertarFilaTarifa(contenedor, t));

        } catch (e) {
            mostrarToast('Error cargando tarifas: ' + e.message, 'error');
        }
    }

    function insertarFilaTarifa(contenedor, tarifa = null) {
        const fila = document.createElement('div');
        fila.className = 'tarif-row grid grid-cols-1 md:grid-cols-[1fr_80px_80px_100px_100px_56px] gap-3 px-5 py-4 border-b border-[var(--borde)] items-center hover:bg-[var(--fondo)]/30 transition-colors';
        fila.dataset.tarifId = tarifa?.id || '';

        const starTime = tarifa?.starTime ? tarifa.starTime.substring(0, 5) : '';
        const endTime  = tarifa?.endTime  ? tarifa.endTime.substring(0, 5)  : '';

        fila.innerHTML = `
            <input type="text" class="t-nombre input-base bg-[var(--fondo)] text-sm font-bold" placeholder="Nombre tarifa" value="${tarifa?.nameTarif || ''}" />
            <div class="relative">
                <span class="absolute left-2 top-1/2 -translate-y-1/2 text-[var(--texto-suave)] text-xs font-bold">€</span>
                <input type="number" step="0.01" class="t-precio input-base bg-[var(--fondo)] pl-6 text-sm font-bold text-center" value="${tarifa?.pricePerHour ?? ''}" placeholder="0.00"/>
            </div>
            <div class="flex justify-center">
                <label class="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" class="t-festivo sr-only peer" ${tarifa?.isHoliday ? 'checked' : ''} />
                    <div class="w-9 h-5 bg-[var(--borde)] rounded-full peer peer-checked:after:translate-x-4 after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-[var(--azul)]"></div>
                </label>
            </div>
            <input type="time" class="t-inicio input-base bg-[var(--fondo)] text-sm text-center" value="${starTime}" />
            <input type="time" class="t-fin input-base bg-[var(--fondo)] text-sm text-center" value="${endTime}" />
            <div class="flex items-center gap-2 justify-center">
                <button type="button" class="btn-guardar-tarif w-9 h-9 flex items-center justify-center rounded-lg border border-[var(--borde)] text-[var(--texto-suave)] hover:border-[var(--azul)] hover:text-[var(--azul)] transition-all" title="Guardar">
                    <span class="material-symbols-outlined text-[16px]">save</span>
                </button>
                <button type="button" class="btn-borrar-tarif w-9 h-9 flex items-center justify-center rounded-lg border border-[var(--borde)] text-[var(--texto-suave)] hover:border-[var(--rojo)] hover:text-[var(--rojo)] transition-all" title="Eliminar">
                    <span class="material-symbols-outlined text-[16px]">delete</span>
                </button>
            </div>
        `;

        // Guardar tarifa
        fila.querySelector('.btn-guardar-tarif').addEventListener('click', () => guardarTarifa(fila));
        // Eliminar tarifa
        fila.querySelector('.btn-borrar-tarif').addEventListener('click', () => borrarTarifa(fila));

        contenedor.appendChild(fila);
    }

    function agregarFilaTarifaNueva() {
        // Si no tenemos parkingId todavía (modo creación), advertir
        if (!parkingId) {
            mostrarToast('Guarda primero el parking antes de añadir tarifas.', 'aviso');
            return;
        }
        const contenedor = document.getElementById('tarifs-body');
        if (!contenedor) return;
        // Limpiar mensaje vacío si existe
        const vacio = contenedor.querySelector('p');
        if (vacio) vacio.remove();
        insertarFilaTarifa(contenedor, null);
    }

    async function guardarTarifa(fila) {
        const id = fila.dataset.tarifId;
        const payload = {
            parkingId: parseInt(parkingId),
            nameTarif: fila.querySelector('.t-nombre').value.trim(),
            pricePerHour: parseFloat(fila.querySelector('.t-precio').value),
            isHoliday: fila.querySelector('.t-festivo').checked,
            starTime: fila.querySelector('.t-inicio').value || null,
            endTime: fila.querySelector('.t-fin').value || null
        };

        if (!payload.nameTarif || isNaN(payload.pricePerHour)) {
            mostrarToast('Rellena nombre y precio de la tarifa.', 'aviso');
            return;
        }

        try {
            if (id) {
                // Eliminar la antigua y crear nueva (el backend no expone PUT en tarifs)
                await apiFetch(`${API}/api/ParkingManagement/tarifs/${id}`, { method: 'DELETE' });
            }
            const nueva = await apiFetch(`${API}/api/ParkingManagement/tarifs`, {
                method: 'POST',
                body: JSON.stringify(payload)
            });
            fila.dataset.tarifId = nueva.id;
            mostrarToast('Tarifa guardada', 'exito');
        } catch (e) {
            mostrarToast('Error guardando tarifa: ' + e.message, 'error');
        }
    }

    async function borrarTarifa(fila) {
        const id = fila.dataset.tarifId;
        if (id) {
            try {
                await apiFetch(`${API}/api/ParkingManagement/tarifs/${id}`, { method: 'DELETE' });
                mostrarToast('Tarifa eliminada', 'exito');
            } catch (e) {
                mostrarToast('Error eliminando tarifa: ' + e.message, 'error');
                return;
            }
        }
        fila.remove();
    }

    // ── Generador de Plazas (Capacidad Total) ───

    async function generarPlazas() {
        if (!parkingId) {
            mostrarToast('Guarda el parking primero antes de generar plazas.', 'aviso');
            return;
        }
        const cantidad = parseInt(document.getElementById('p-total')?.value || 0);
        if (!cantidad || cantidad < 1 || cantidad > 200) {
            mostrarToast('Introduce un número de plazas entre 1 y 200.', 'aviso');
            return;
        }

        const tipoPlaza = document.getElementById('p-spot-type')?.value || 'Standard';

        mostrarToast(`Generando ${cantidad} plazas...`, 'aviso');
        let creadas = 0;

        // Nomenclatura: A-1, A-2 ... A-10, B-1, B-2 ... (10 por fila)
        const letras = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
        for (let i = 0; i < cantidad; i++) {
            const letra = letras[Math.floor(i / 10)] || 'Z';
            const num = (i % 10) + 1;
            const spotNumber = `${letra}-${num}`;

            try {
                await apiFetch(`${API}/api/ParkingManagement/spots`, {
                    method: 'POST',
                    body: JSON.stringify({
                        parkingId: parseInt(parkingId),
                        spotNumber: spotNumber,
                        status: 0, // Free
                        type: tipoPlaza
                    })
                });
                creadas++;
            } catch {
                // Si ya existe esa plaza la saltamos
            }
        }
        mostrarToast(`${creadas} plazas generadas con éxito`, 'exito');
    }

    // ── Zona de Peligro (solo editarparkingEmpresa) ──

    async function desactivarParking() {
        if (!confirm('¿Seguro que quieres desactivar este parking? Dejará de aparecer en el mapa.')) return;
        try {
            const p = await apiFetch(`${API}/api/Parking/${parkingId}`);
            const payload = { ...p, isActive: false, companyId };
            await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            mostrarToast('Parking desactivado', 'aviso');
            document.getElementById('is-active').value = 'false';
        } catch (e) {
            mostrarToast('Error desactivando: ' + e.message, 'error');
        }
    }

    async function eliminarParking() {
        if (!confirm('⚠️ Esta acción es IRREVERSIBLE. ¿Eliminar el parking y todos sus datos?')) return;
        try {
            await apiFetch(`${API}/api/ParkingManagement/${parkingId}`, { method: 'DELETE' });
            mostrarToast('Parking eliminado', 'exito');
            setTimeout(() => window.location.href = 'misparkingsEmpresa.html', 1500);
        } catch (e) {
            mostrarToast('Error eliminando: ' + e.message, 'error');
        }
    }

    // ── Arranque ──
    document.addEventListener('DOMContentLoaded', initFormulario);

})();
