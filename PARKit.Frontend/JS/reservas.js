/* ==============================================
   PARKit — Mis Reservas (Usuario)
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let reservasGlobal = []; // Para almacenar las reservas y poder filtrar

    // Mapeo según el enum ReservationStatus del Backend
    const STATUS_MAP = {
        0: { text: "Activa", class: "badge-verde" },
        1: { text: "Cancelada", class: "badge-rojo" },
        2: { text: "Finalizada", class: "badge-primario" },
        3: { text: "Pendiente", class: "badge-naranja" },
        4: { text: "Confirmada", class: "badge-verde" }
    };
    
    async function apiFetch(url, opciones = {}) {
        const resp = await fetch(url, { headers: AUTH.cabecerasAuth(), ...opciones });
        if (!resp.ok) {
            const err = await resp.json().catch(() => ({}));
            throw new Error(err.message || `Error ${resp.status}`);
        }
        if (resp.status === 204) return null;
        return resp.json();
    }

    function verificarSesion() {
        if (!AUTH.estaAutenticado() || AUTH.esEmpresa()) {
            window.location.href = 'login.html';
            return false;
        }
        return true;
    }

    async function initReservas() {
        if (!verificarSesion()) return;

        // Mostrar inicial de usuario en header
        const usuario = AUTH.obtenerUsuario();
        const headerAvatar = document.getElementById('header-user-avatar');
        if (headerAvatar) {
            headerAvatar.innerHTML = `<span class="font-bold text-lg flex items-center justify-center w-full h-full">${usuario?.inicial || 'U'}</span>`;
        }

        // Evento para el buscador
        document.getElementById('activity-search-input')?.addEventListener('input', filtrarReservas);

        await cargarReservas();
    }

    async function cargarReservas() {
        try {
            reservasGlobal = await apiFetch(`${API}/api/Reservation/my`);
            
            // Ordenamos por fecha más reciente
            reservasGlobal.sort((a, b) => new Date(b.startTime) - new Date(a.startTime));
            
            actualizarEstadisticas();
            renderizarReservas(reservasGlobal);

        } catch (error) {
            mostrarToast('Error al cargar las reservas: ' + error.message, 'error');
            document.getElementById('activity-tbody').innerHTML = `
                <tr>
                    <td colspan="7" class="px-6 py-10 text-center text-[var(--rojo)] font-bold">No se pudieron cargar las reservas o la sesión ha expirado.</td>
                </tr>
            `;
        }
    }

    function actualizarEstadisticas() {
        const totalReservas = reservasGlobal.length;
        
        let gastoMensual = 0;
        const currentMonth = new Date().getMonth();
        const currentYear = new Date().getFullYear();

        let ultimoParking = "...";

        for (const res of reservasGlobal) {
            const fecha = new Date(res.startTime);
            // Gasto de reservas de ESTE MES que NO estén canceladas
            if (res.status !== 1 && fecha.getMonth() === currentMonth && fecha.getFullYear() === currentYear) {
                gastoMensual += res.totalAmount || 0;
            }
        }

        // Último parking usado
        if (reservasGlobal.length > 0) {
            ultimoParking = reservasGlobal[0].parkingName || "Desconocido";
        }

        if (document.getElementById('stat-monthly-spending')) 
            document.getElementById('stat-monthly-spending').textContent = `${gastoMensual.toFixed(2)} €`;
            
        if (document.getElementById('stat-total-bookings')) 
            document.getElementById('stat-total-bookings').textContent = totalReservas;
            
        if (document.getElementById('stat-last-parking')) 
            document.getElementById('stat-last-parking').textContent = ultimoParking;

        if (document.getElementById('table-stats'))
            document.getElementById('table-stats').textContent = `Mostrando ${totalReservas} registros de actividad`;
    }

    function filtrarReservas(e) {
        const texto = e.target.value.toLowerCase();
        const filtradas = reservasGlobal.filter(r => 
            (r.parkingName || '').toLowerCase().includes(texto) ||
            (r.spotNumber || '').toLowerCase().includes(texto)
        );
        renderizarReservas(filtradas);
    }

    // Funciones auxiliares de formateo de fechas
    function formatoFecha(fechaIso) {
        const d = new Date(fechaIso);
        return d.toLocaleDateString('es-ES', { day: '2-digit', month: 'short', year: 'numeric' });
    }

    function formatoHora(fechaIso) {
        const d = new Date(fechaIso);
        return d.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
    }

    function calcularDuracionFormateada(inicioIso, finIso) {
        const inicio = new Date(inicioIso);
        const fin = new Date(finIso);
        const diffMinutos = Math.round((fin - inicio) / 60000);
        
        if (diffMinutos < 60) return `${diffMinutos} min`;
        
        const horas = Math.floor(diffMinutos / 60);
        const minsRestantes = diffMinutos % 60;
        return `${horas}h ${minsRestantes > 0 ? minsRestantes + 'm' : ''}`;
    }

    function renderizarReservas(lista) {
        const tbody = document.getElementById('activity-tbody');
        if (!tbody) return;
        tbody.innerHTML = '';

        if (lista.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="px-6 py-16 text-center text-[var(--texto-suave)] italic">
                        No hay reservas que coincidan.
                    </td>
                </tr>
            `;
            return;
        }

        lista.forEach(res => {
            // Evaluamos estado
            const objEstado = STATUS_MAP[res.status] || { text: "Desconocido", class: "badge-naranja" };
            const importe = res.totalAmount != null ? `${res.totalAmount.toFixed(2)} €` : '-- €';
            const fechaStr = formatoFecha(res.startTime);
            const horaInicio = formatoHora(res.startTime);
            const horaFin = formatoHora(res.endTime);
            const duracion = calcularDuracionFormateada(res.startTime, res.endTime);

            // Cancelar solo si está: 0 (Activa), 3 (Pendiente) o 4 (Confirmada)
            const puedeCancelar = (res.status === 0 || res.status === 3 || res.status === 4);
            const btnCancelar = puedeCancelar 
                ? `<button class="btn-cancelar text-[var(--rojo)] hover:underline text-xs font-bold" data-id="${res.id}">Cancelar</button>`
                : `<span class="text-[var(--texto-suave)] text-xs">--</span>`;

            const tr = document.createElement('tr');
            tr.className = "hover:bg-gray-50/30 dark:hover:bg-[#1b2138]/20 transition-colors";
            
            tr.innerHTML = `
                <td class="px-6 py-4 whitespace-nowrap">
                    <p class="font-bold text-[#111318] dark:text-white">${fechaStr}</p>
                    <p class="text-[11px] text-[var(--texto-suave)] mt-0.5">${horaInicio} - ${horaFin}</p>
                </td>
                <td class="px-6 py-4">
                    <p class="font-bold text-[var(--primario)] dark:text-[#a0c4ff]">${res.parkingName || 'Desconocido'}</p>
                    <p class="text-[11px] text-[var(--texto-suave)] mt-0.5">ID: #${res.id}</p>
                </td>
                <td class="px-6 py-4 text-center">
                    <span class="inline-flex items-center justify-center min-w-[2.5rem] px-2 py-1 rounded bg-[#f5f6fa] dark:bg-[#111524] border border-[var(--borde)] font-mono text-sm font-bold text-[#111318] dark:text-white">
                        ${res.spotNumber || '--'}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center gap-1.5 text-sm text-[#111318] dark:text-gray-300">
                        <span class="material-symbols-outlined text-[16px] text-[var(--texto-suave)]">schedule</span>
                        ${duracion}
                    </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="badge ${objEstado.class} text-[10px] font-extrabold uppercase tracking-widest px-2.5 py-1">
                        ${objEstado.text}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right">
                    <p class="font-bold text-base text-[#111318] dark:text-white">${importe}</p>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right">
                    ${btnCancelar}
                </td>
            `;

            if (puedeCancelar) {
                tr.querySelector('.btn-cancelar').addEventListener('click', () => cancelarReserva(res.id));
            }

            tbody.appendChild(tr);
        });
    }

    async function cancelarReserva(id) {
        if (!confirm('¿Estás seguro de que deseas cancelar esta reserva?')) return;

        try {
            await apiFetch(`${API}/api/Reservation/${id}/cancel`, { method: 'PUT' });
            mostrarToast('Reserva cancelada correctamente', 'exito');
            // Recargar datos puros de DB
            await cargarReservas();
        } catch (error) {
            mostrarToast('Error al cancelar: ' + error.message, 'error');
        }
    }

    // Arrancado inicial
    document.addEventListener('DOMContentLoaded', initReservas);

})();
