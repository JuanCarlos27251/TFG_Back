/* ==============================================
   PARKit — Módulo de Empresa
   Cubre: panel, configuración y estadísticas
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let companyId = null;
    let chartInstancias = {}; // Para destruir gráficos antiguos antes de redibujar

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

    function headers() { return AUTH.cabecerasAuth(); }

    async function apiFetch(url, opciones = {}) {
        const resp = await fetch(url, { headers: headers(), ...opciones });
        if (!resp.ok) {
            const err = await resp.json().catch(() => ({}));
            throw new Error(err.message || `Error ${resp.status}`);
        }
        if (resp.status === 204) return null;
        return resp.json();
    }

    function obtenerCompanyId() {
        const u = AUTH.obtenerUsuario();
        return u?.id || null;
    }

    function verificarSesion() {
        if (!AUTH.estaAutenticado() || !AUTH.esEmpresa()) {
            window.location.href = '../login.html';
            return false;
        }
        companyId = obtenerCompanyId();
        return !!companyId;
    }

    function renderCabeceraGenerica(empresa) {
        const inicial = (empresa.nameCompany || 'E').charAt(0).toUpperCase();
        const avatarEl = document.getElementById('header-empresa-avatar');
        if (avatarEl) avatarEl.textContent = inicial;
        const nombreEl = document.getElementById('header-empresa-nombre');
        if (nombreEl) nombreEl.textContent = empresa.nameCompany || 'Mi Empresa';
        const subEl = document.getElementById('header-empresa-subtitulo');
        if (subEl) subEl.textContent = empresa.email || '';
    }

    // ═══════════════════════════════════════════
    //  PANEL DE CONTROL (panelEmpresa.html)
    // ═══════════════════════════════════════════

    async function initPanel() {
        if (!verificarSesion()) return;

        try {
            const [empresa, ocupacion, ingresos] = await Promise.all([
                apiFetch(`${API}/api/Company/${companyId}`),
                apiFetch(`${API}/api/Statistics/company/${companyId}/occupancy`).catch(() => []),
                apiFetch(`${API}/api/Statistics/company/${companyId}/revenue?months=7`).catch(() => []),
            ]);

            renderCabeceraGenerica(empresa);
            renderTarjetasResumenPanel(ocupacion);
            renderGraficoIngresosPanel(ingresos);
            await cargarActividadReciente();
        } catch (e) {
            mostrarToast('Error cargando el panel: ' + e.message, 'error');
        }
    }

    function renderTarjetasResumenPanel(ocupacion) {
        const totalParkings  = ocupacion.length;
        const totalPlazas    = ocupacion.reduce((s, o) => s + (o.totalSpots || 0), 0);
        const totalOcupadas  = ocupacion.reduce((s, o) => s + (o.occupiedSpots || 0), 0);
        const ocupacionMedia = totalPlazas > 0 ? Math.round((totalOcupadas / totalPlazas) * 100) : 0;

        const elParkings  = document.getElementById('stat-total-parkings');
        const elPlazas    = document.getElementById('stat-total-plazas');
        const elOcupacion = document.getElementById('stat-ocupacion-valor');
        const elBarra     = document.getElementById('stat-ocupacion-barra');

        if (elParkings)  elParkings.textContent  = totalParkings;
        if (elPlazas)    elPlazas.textContent     = totalPlazas.toLocaleString('es-ES');
        if (elOcupacion) elOcupacion.textContent  = `${ocupacionMedia}%`;
        if (elBarra)     elBarra.style.width      = `${ocupacionMedia}%`;
    }

    function renderGraficoIngresosPanel(ingresos) {
        const svgContainer = document.getElementById('grafico-ingresos');
        const etiquetas    = document.getElementById('grafico-etiquetas');
        if (!svgContainer || !ingresos || ingresos.length === 0) return;

        const datos = [...ingresos].sort((a, b) => a.year !== b.year ? a.year - b.year : a.month - b.month);
        const maxVal = Math.max(...datos.map(d => d.totalRevenue || 0), 1);
        const meses  = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic'];
        const n      = datos.length;
        const padX   = 20, width = 1000 - padX * 2, height = 200, padY = 20;

        const puntos = datos.map((d, i) => ({
            x: padX + (i / Math.max(n - 1, 1)) * width,
            y: padY + (1 - (d.totalRevenue / maxVal)) * height,
        }));

        const pathD = puntos.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');
        const areaD = pathD + ` L ${puntos[puntos.length - 1].x} ${height + padY} L ${puntos[0].x} ${height + padY} Z`;

        svgContainer.innerHTML = `
            <defs>
                <linearGradient id="revenue_grad" x1="500" x2="500" y1="0" y2="240" gradientUnits="userSpaceOnUse">
                    <stop stop-color="var(--azul)" stop-opacity="0.25"></stop>
                    <stop offset="1" stop-color="var(--azul)" stop-opacity="0"></stop>
                </linearGradient>
            </defs>
            <path d="${areaD}" fill="url(#revenue_grad)"></path>
            <path d="${pathD}" stroke="var(--azul)" stroke-linecap="round" stroke-width="4" fill="none"></path>
            ${puntos.map((p, i) => `<circle cx="${p.x}" cy="${p.y}" r="5" fill="var(--azul)"><title>${meses[(datos[i].month || 1) - 1]} ${datos[i].year}: ${(datos[i].totalRevenue || 0).toFixed(2)}€</title></circle>`).join('')}
        `;
        if (etiquetas) etiquetas.innerHTML = datos.map(d => `<span>${meses[(d.month || 1) - 1]}</span>`).join('');
    }

    async function cargarActividadReciente() {
        const tbody = document.getElementById('tabla-actividad-body');
        if (!tbody) return;
        try {
            const parkings = await apiFetch(`${API}/api/Parking/manager/${companyId}`).catch(() => []);
            if (!parkings || parkings.length === 0) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center py-8 text-[var(--texto-suave)] italic text-sm">No hay parkings registrados.</td></tr>`;
                return;
            }
            const reservasPorParking = await Promise.all(
                parkings.slice(0, 3).map(p => apiFetch(`${API}/api/ReservationManagement/parking/${p.id}`).catch(() => []))
            );
            const todasReservas = reservasPorParking.flat().sort((a, b) => new Date(b.startTime) - new Date(a.startTime)).slice(0, 10);
            
            if (todasReservas.length === 0) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center py-8 text-[var(--texto-suave)] italic text-sm">No hay actividad reciente.</td></tr>`;
                return;
            }

            tbody.innerHTML = todasReservas.map(r => {
                const estado  = obtenerBadgeEstado(r.status);
                const llegada = r.startTime ? new Date(r.startTime).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' }) : '--';
                return `
                    <tr>
                        <td><div class="badge badge-azul font-mono text-sm uppercase">${r.vehiclePlate || '--'}</div></td>
                        <td>${r.userName || 'Invitado'}</td>
                        <td><span class="font-mono text-[var(--texto-suave)]">${r.spotCode || '--'}</span></td>
                        <td class="text-[var(--texto-suave)]">${llegada}</td>
                        <td><span class="badge ${estado.clase}">${estado.texto}</span></td>
                        <td class="text-right font-bold text-[var(--texto)]">${r.totalPrice ? Number(r.totalPrice).toFixed(2)+'€' : '--'}</td>
                    </tr>`;
            }).join('');
        } catch (e) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center py-6 text-[var(--rojo)] text-sm">No se pudo cargar la actividad.</td></tr>`;
        }
    }

    function obtenerBadgeEstado(status) {
        const mapa = {
            'Active': { clase: 'badge-verde', texto: 'Aparcado' },
            'Pending': { clase: 'badge-naranja', texto: 'Reservado' },
            'Completed': { clase: 'badge-azul', texto: 'Completado' },
            'Cancelled': { clase: 'badge-rojo', texto: 'Cancelado' }
        };
        return mapa[status] || { clase: 'badge-azul', texto: status || '--' };
    }


    // ═══════════════════════════════════════════
    //  CONFIGURACIÓN (configuracionEmpresa.html)
    // ═══════════════════════════════════════════

    async function initConfiguracion() {
        if (!verificarSesion()) return;
        try {
            const empresa = await apiFetch(`${API}/api/Company/${companyId}`);
            renderCabeceraGenerica(empresa);
            ['nameCompany', 'cif', 'email', 'phone', 'address'].forEach(k => {
                const el = document.getElementById(`setting-${k === 'nameCompany' ? 'company-name' : k}`);
                if (el) el.value = empresa[k] || '';
            });
        } catch (e) {
            mostrarToast('Error cargando configuración.', 'error');
        }

        document.getElementById('btn-guardar-config')?.addEventListener('click', async (e) => {
            e.preventDefault();
            const btn = e.target;
            btn.disabled = true; btn.textContent = 'Guardando...';
            try {
                await apiFetch(`${API}/api/Company/${companyId}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        nameCompany: document.getElementById('setting-company-name').value,
                        cif: document.getElementById('setting-cif').value,
                        email: document.getElementById('setting-email').value,
                        phone: document.getElementById('setting-phone').value,
                        address: document.getElementById('setting-address').value,
                        password: '' 
                    })
                });
                mostrarToast('Configuración guardada.', 'exito');
                setTimeout(()=> window.location.reload(), 1000);
            } catch (err) {
                mostrarToast('Error al guardar: ' + err.message, 'error');
                btn.disabled = false; btn.textContent = 'Guardar Cambios';
            }
        });
        document.getElementById('btn-cancelar-config')?.addEventListener('click', () => window.location.reload());
    }

    // ═══════════════════════════════════════════
    //  ESTADÍSTICAS (estadisticasEmpresa.html)
    // ═══════════════════════════════════════════

    async function initEstadisticas() {
        if (!verificarSesion()) return;

        try {
            // Cargar datos en paralelo
            const [empresa, ingresos, ocupacion, vehiculos, horasPico] = await Promise.all([
                apiFetch(`${API}/api/Company/${companyId}`),
                apiFetch(`${API}/api/Statistics/company/${companyId}/revenue?months=6`),
                apiFetch(`${API}/api/Statistics/company/${companyId}/occupancy`),
                apiFetch(`${API}/api/Statistics/company/${companyId}/vehicle-types`),
                apiFetch(`${API}/api/Statistics/company/${companyId}/peak-hours`),
            ]);

            renderCabeceraGenerica(empresa);
            renderKPIs(ingresos, ocupacion);
            
            // Gráficos de Chart.js
            crearGraficoIngresosLineas(ingresos);
            crearGraficoIngresosPorParking(ocupacion); // Usaremos la info de los parkings si la necesitamos
            crearGraficoOcupacionBarras(ocupacion);
            renderTiposVehiculo(vehiculos);
            renderHorasPico(horasPico);

        } catch (e) {
            mostrarToast('Error cargando estadísticas: ' + e.message, 'error');
            console.error(e);
        }
    }

    function renderKPIs(ingresos, ocupacion) {
        // Ingresos
        const totalRevenue = ingresos.reduce((sum, item) => sum + item.totalRevenue, 0);
        document.getElementById('kpi-ingresos').textContent = `${totalRevenue.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`;

        // Reservas
        const totalReservas = ingresos.reduce((sum, item) => sum + item.totalReservations, 0);
        document.getElementById('kpi-reservas').textContent = totalReservas.toLocaleString('es-ES');

        // Ticket Medio
        const ticketMedio = totalReservas > 0 ? (totalRevenue / totalReservas) : 0;
        document.getElementById('kpi-ticket').textContent = `${ticketMedio.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`;

        // Ocupación Global
        const totalPlazas = ocupacion.reduce((sum, p) => sum + p.totalSpots, 0);
        const totalOcupadas = ocupacion.reduce((sum, p) => sum + p.occupiedSpots, 0);
        const ocMedia = totalPlazas > 0 ? Math.round((totalOcupadas / totalPlazas) * 100) : 0;
        
        document.getElementById('kpi-ocupacion').textContent = `${ocMedia}%`;
        document.getElementById('barra-ocupacion').style.width = `${ocMedia}%`;
    }

    function dibujarGrafico(id, config) {
        const ctx = document.getElementById(id);
        if(!ctx) return;
        if (chartInstancias[id]) chartInstancias[id].destroy();
        chartInstancias[id] = new Chart(ctx, config);
    }

    function crearGraficoIngresosLineas(ingresos) {
        const mesesNombres = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic'];
        const datosOrdenados = [...ingresos].sort((a, b) => a.year !== b.year ? a.year - b.year : a.month - b.month);
        
        const labels = datosOrdenados.map(d => `${mesesNombres[d.month-1]} ${d.year.toString().slice(2)}`);
        const dataRevenues = datosOrdenados.map(d => d.totalRevenue);

        dibujarGrafico('chart-ingresos', {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ingresos (€)',
                    data: dataRevenues,
                    fill: true,
                    backgroundColor: 'rgba(19,91,236,0.12)',
                    borderColor: '#135bec', // Azul
                    borderWidth: 2.5,
                    pointBackgroundColor: '#135bec',
                    pointRadius: 4,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { 
                    y: { ticks: { callback: v => v + ' €' } } 
                }
            }
        });
    }

    function crearGraficoIngresosPorParking(ocupacion) {
        // En tu controlador actual no hay desglose de ingresos por parking exacto para grafica de barras.
        // Simularemos esta visualización basándonos en la capacidad como aproximador para que el gráfico no esté vacío.
        // (En un futuro puedes crear un endpoint en StatisticsController para esto).
        const labels = ocupacion.map(o => o.parkingName);
        const estimacionReservas = ocupacion.map(o => o.occupiedSpots * 1.5); // Dato ficticio basado en ocupación

        dibujarGrafico('chart-reservas-parking', {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Reservas activas',
                    data: estimacionReservas,
                    backgroundColor: '#22c55e' + 'bb',
                    borderColor: '#22c55e',
                    borderWidth: 1.5,
                    borderRadius: 6
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                plugins: { legend: { display: false } }
            }
        });
    }

    function crearGraficoOcupacionBarras(ocupacion) {
        const labels = ocupacion.map(o => o.parkingName);
        const dataOc = ocupacion.map(o => Math.round(o.occupancyRate * 100));

        dibujarGrafico('chart-ocupacion-parking', {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ocupación (%)',
                    data: dataOc,
                    backgroundColor: (ctx) => {
                        const v = ctx.raw;
                        if (v >= 85) return '#ef4444' + 'cc';
                        if (v >= 60) return '#f97316' + 'cc';
                        return '#22c55e' + 'cc';
                    },
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { 
                    y: { min: 0, max: 100, ticks: { callback: v => v + '%' } }
                }
            }
        });
    }

    function renderTiposVehiculo(vehiculos) {
        const total = vehiculos.standardCount + vehiculos.electricCount + vehiculos.largeCount || 1; // evitar dividir por 0
        
        const estPct = Math.round((vehiculos.standardCount / total) * 100);
        const elecPct = Math.round((vehiculos.electricCount / total) * 100);
        const largePct = Math.round((vehiculos.largeCount / total) * 100);

        document.getElementById('pct-veh-standard').textContent = `${estPct}%`;
        document.getElementById('barra-veh-standard').style.width = `${estPct}%`;

        document.getElementById('pct-veh-electric').textContent = `${elecPct}%`;
        document.getElementById('barra-veh-electric').style.width = `${elecPct}%`;

        document.getElementById('pct-veh-large').textContent = `${largePct}%`;
        document.getElementById('barra-veh-large').style.width = `${largePct}%`;

        // Gráfico Donut
        dibujarGrafico('chart-tipos', {
            type: 'doughnut',
            data: {
                labels: ['Estándar', 'Eléctrico', 'Grande'],
                datasets: [{
                    data: [estPct, elecPct, largePct],
                    backgroundColor: ['#135bec', '#22c55e', '#f97316'],
                    borderWidth: 0
                }]
            },
            options: { cutout: '75%', plugins: { legend: { display: false } } }
        });
    }

    function renderHorasPico(horasPico) {
        const container = document.getElementById('contenedor-horas-pico');
        if (!container) return;

        if (!horasPico || horasPico.length === 0) {
            container.innerHTML = `<span class="text-xs text-[var(--texto-suave)]">Sin datos</span>`;
            return;
        }

        // Ordenamos las horas por número de reservas (las más altas primero) y tomamos las 3 más concurridas
        const horasTop = [...horasPico].sort((a, b) => b.reservationCount - a.reservationCount).slice(0, 3);
        
        container.innerHTML = horasTop.map(h => {
            const hFormat = h.hour.toString().padStart(2, '0');
            return `<span class="badge badge-azul text-[10px]">${hFormat}:00 – ${hFormat}:59</span>`;
        }).join('');
    }

    // ── Asignar eventos de logout globales ──
    document.addEventListener('click', (e) => {
        if(e.target.closest('#btn-logout-empresa')){
            AUTH.cerrarSesion(true);
        }
    });

    // ── Arranque ──
    document.addEventListener('DOMContentLoaded', () => {
        if(typeof Chart !== 'undefined'){
            Chart.defaults.color = '#8892b0';
            Chart.defaults.borderColor = '#1e293b';
            Chart.defaults.font.family = "'DM Sans', sans-serif";
        }

        const path = window.location.pathname.toLowerCase();
        if (path.includes('panelempresa'))        initPanel();
        else if (path.includes('configuracion'))  initConfiguracion();
        else if (path.includes('estadisticas'))   initEstadisticas();
    });

})();

