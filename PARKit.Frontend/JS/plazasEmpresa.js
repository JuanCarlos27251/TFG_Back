/* ==============================================
   PARKit — Gestión de Plazas Individuales (Empresa)
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let parkingId = null;
    let tituloParking = "";
    
    let gridPlazasVivas = []; 
    let plazaSeleccionada = null;

    let filtroTipo = 'all';
    let filtroEstadoActivo = null; 

    //  Novedad: Fondos Sólidos Reales (Relleno completo) 
    const STATUS = {
        0: { name: 'Libre',         color: 'verde',   bgClass: 'bg-[#22c55e] text-white border-transparent shadow shadow-[#22c55e]/40' },
        1: { name: 'En Uso',        color: 'naranja', bgClass: 'bg-[#eab308] text-white border-transparent shadow shadow-[#eab308]/40' }, // Amarillo
        2: { name: 'Reservada',     color: 'primario',    bgClass: 'bg-[#3b82f6] text-white border-transparent shadow shadow-[#3b82f6]/40' }, // primario
        3: { name: 'Mantenimiento', color: 'rojo',    bgClass: 'bg-[#ef4444] text-white border-transparent shadow shadow-[#ef4444]/40' }  // Rojo
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
        if (!AUTH.estaAutenticado() || !AUTH.esEmpresa()) {
            window.location.href = '../login.html';
            return false;
        }
        return true;
    }

    async function initGestionPlazas() {
        if (!verificarSesion()) return;

        try {
            const companyId = AUTH.obtenerUsuario()?.id;
            const urlParams = new URLSearchParams(window.location.search);
            parkingId = urlParams.get('id');

            if (!parkingId) {
                const parkings = await apiFetch(`${API}/api/Parking/manager/${companyId}`);
                if (parkings && parkings.length > 0) {
                    parkingId = parkings[0].id;
                    window.history.replaceState({}, '', `?id=${parkingId}`);
                } else {
                    mostrarToast('Aún no tienes parkings. Redirigiendo...', 'aviso');
                    setTimeout(() => window.location.href = 'addparkingEmpresa.html', 1500);
                    return;
                }
            }

            const [empresa, infoParking] = await Promise.all([
                apiFetch(`${API}/api/Company/${companyId}`),
                apiFetch(`${API}/api/Parking/${parkingId}`)
            ]);

            const inicial = (empresa.nameCompany || 'E').charAt(0).toUpperCase();
            document.getElementById('header-empresa-avatar').textContent = inicial;
            document.getElementById('header-empresa-nombre').textContent = empresa.nameCompany || 'Mi Empresa';
            document.getElementById('header-empresa-subtitulo').textContent = empresa.email || '';

            tituloParking = infoParking.name || `Parking #${parkingId}`;
            document.getElementById('gp-parking-title').textContent = tituloParking;
            document.getElementById('gp-parking-subtitle').textContent = infoParking.address || 'Visualización en tiempo real';

            await recargarPlazas();

            conectarSignalR(parkingId);

        } catch (e) {
            mostrarToast('Error cargando el panel: ' + e.message, 'error');
        }

        // Acciones Laterales
        document.getElementById('btn-liberar')?.addEventListener('click', () => cambiarEstadoPlaza(0));
        document.getElementById('btn-mantenimiento')?.addEventListener('click', () => cambiarEstadoPlaza(3));
        
        // Filtros
        document.getElementById('spot-search-input')?.addEventListener('input', () => renderizarFiltros());
        document.getElementById('filter-type-select')?.addEventListener('change', (e) => {
            filtroTipo = e.target.value;
            renderizarFiltros();
        });

        [0, 1, 2, 3].forEach(idEstado => {
            const btn = document.getElementById(`filter-status-${idEstado}`);
            if(!btn) return;
            btn.addEventListener('click', () => {
                if (filtroEstadoActivo === idEstado) {
                    filtroEstadoActivo = null;
                    btn.classList.remove('ring-4', 'ring-white', 'ring-offset-2', 'ring-offset-[var(--fondo-card)]', 'scale-110');
                } else {
                    [0, 1, 2, 3].forEach(fid => {
                        const b = document.getElementById(`filter-status-${fid}`);
                        if(b) b.classList.remove('ring-4', 'ring-white', 'ring-offset-2', 'ring-offset-[var(--fondo-card)]', 'scale-110');
                    });
                    filtroEstadoActivo = idEstado;
                    btn.classList.add('ring-4', 'ring-white', 'ring-offset-2', 'ring-offset-[var(--fondo-card)]', 'scale-110');
                }
                renderizarFiltros();
            });
        });

        document.addEventListener('click', (e) => {
            if(e.target.closest('#btn-logout-empresa')) AUTH.cerrarSesion(true);
        });
    }

    async function recargarPlazas() {
        try {
            gridPlazasVivas = await apiFetch(`${API}/api/ParkingManagement/parking/${parkingId}/spots`);
            renderizarEstadisticasTotales();
            renderizarFiltros(); 
        } catch (error) {
            mostrarToast('No se pudieron cargar las plazas.', 'error');
        }
    }

    function renderizarEstadisticasTotales() {
        let libres = 0, ocupadas = 0, reservadas = 0, mantenimiento = 0;
        
        gridPlazasVivas.forEach(p => {
            if (p.status === 0) libres++;
            if (p.status === 1) ocupadas++;
            if (p.status === 2) reservadas++;
            if (p.status === 3) mantenimiento++;
        });

        if(document.getElementById('stat-available')) document.getElementById('stat-available').textContent = libres;
        if(document.getElementById('stat-occupied')) document.getElementById('stat-occupied').textContent = ocupadas;
        if(document.getElementById('stat-reserved')) document.getElementById('stat-reserved').textContent = reservadas;
        if(document.getElementById('stat-maintenance')) document.getElementById('stat-maintenance').textContent = mantenimiento;

        const total = libres + ocupadas + reservadas + mantenimiento;
        const ocupacionPct = total > 0 ? Math.round(((ocupadas + reservadas) / total) * 100) : 0;

        if(document.getElementById('gp-ocupacion-texto')) document.getElementById('gp-ocupacion-texto').textContent = `${ocupacionPct}%`;
        if(document.getElementById('gp-ocupacion-barra')) document.getElementById('gp-ocupacion-barra').style.width = `${ocupacionPct}%`;
    }

    // Buscador y filtrador de plazas
    function renderizarFiltros() {
        const textBusqueda = document.getElementById('spot-search-input')?.value.toLowerCase() || '';

        const filtradas = gridPlazasVivas.filter(plaza => {
            const coincideTexto = plaza.spotNumber.toLowerCase().includes(textBusqueda);
            if (!coincideTexto) return false;

            if (filtroEstadoActivo !== null && plaza.status !== filtroEstadoActivo) return false;

            if (filtroTipo !== 'all') {
                const typeDb = (plaza.type || '').toLowerCase();
                if (filtroTipo === 'electric' && !(typeDb.includes('electri') || typeDb.includes('ev'))) return false;
                if (filtroTipo === 'large' && !(typeDb.includes('large') || typeDb.includes('suv') || typeDb.includes('grande'))) return false;
            }
            return true;
        });

        renderGrid(filtradas);
        actualizarPanelLateral(); 
    }

    function renderGrid(plazasAMostrar) {
        const gridContenedor = document.getElementById('spot-grid');
        if(!gridContenedor) return;
        gridContenedor.innerHTML = '';

        if (plazasAMostrar.length === 0) {
            gridContenedor.innerHTML = `<p class="col-span-full text-center py-10 text-[var(--texto-suave)] italic">No hay plazas que coincidan con los filtros.</p>`;
            return;
        }

        plazasAMostrar.forEach(plaza => {
            const estadoObj = STATUS[plaza.status] || STATUS[0];
            
            // Iconos extra con fondo semitransparente para que destaquen en blanco
            let extraIcon = '';
            const t = (plaza.type || '').toLowerCase();
            if (t.includes('electri') || t.includes('ev')) {
                extraIcon = `<span class="material-symbols-outlined absolute -top-1.5 -right-1.5 text-[11px] bg-white/20 backdrop-blur-sm rounded-full text-white border border-white/50 p-0.5" title="Soporte Eléctrico">bolt</span>`;
            } else if (t.includes('large') || t.includes('suv') || t.includes('grande')) {
                extraIcon = `<span class="material-symbols-outlined absolute -top-1.5 -right-1.5 text-[10px] bg-white/20 backdrop-blur-sm rounded-full text-white border border-white/50 p-0.5" title="Vehículos Grandes">airport_shuttle</span>`;
            } else if (t.includes('pmr')) {
                 extraIcon = `<span class="material-symbols-outlined absolute -top-1.5 -right-1.5 text-[11px] bg-white/20 backdrop-blur-sm rounded-full text-white border border-white/50 p-0.5" title="PMR Movilidad Reducida">accessible</span>`;
            }

            const divPlaza = document.createElement('div');
            // Mantenemos la clase 'plaza' para el tamaño/padding y le sumamos tu selección de colores de fondo llenos
            divPlaza.className = `plaza ${estadoObj.bgClass} relative cursor-pointer hover:scale-105 transition-all duration-200 font-bold`;
            divPlaza.innerHTML = `<span>${plaza.spotNumber}</span> ${extraIcon}`;
            
            if (plazaSeleccionada && plazaSeleccionada.id === plaza.id) {
                divPlaza.classList.add('ring-2', 'ring-white', 'ring-offset-2', 'ring-offset-[var(--fondo-card)]', 'scale-110');
            }
            
            divPlaza.addEventListener('click', () => {
                plazaSeleccionada = plaza;
                renderizarFiltros(); 
            });

            gridContenedor.appendChild(divPlaza);
        });
    }

    function actualizarPanelLateral() {
        const panBadge = document.getElementById('panel-plaza-badge');
        const panId = document.getElementById('panel-plaza-id');
        const panEstado = document.getElementById('panel-plaza-estado');
        const panInfo = document.getElementById('panel-plaza-info');
        
        const btnL = document.getElementById('btn-liberar');
        const btnM = document.getElementById('btn-mantenimiento');

        if (!plazaSeleccionada) {
            if(panBadge) panBadge.className = 'w-12 h-12 rounded flex items-center justify-center bg-[var(--fondo)] border-2 border-dashed border-[var(--borde)] text-[var(--texto-suave)]';
            if(panId) panId.textContent = '--';
            if(panEstado) panEstado.textContent = 'Selecciona una plaza';
            if(panInfo) panInfo.textContent = 'Haz clic en el grid';
            if(btnL) btnL.style.display = 'none';
            if(btnM) btnM.style.display = 'none';
            return;
        }

        const st = STATUS[plazaSeleccionada.status] || STATUS[0];
        
        if(panBadge) panBadge.className = `w-12 h-12 rounded flex items-center justify-center ${st.bgClass} text-sm font-black`;
        if(panId) panId.textContent = plazaSeleccionada.spotNumber;
        if(panEstado) panEstado.textContent = `Estado: ${st.name}`;
        
        const tipoExtra = plazaSeleccionada.type ? ` (${plazaSeleccionada.type})` : '';
        if(panInfo) panInfo.textContent = `ID Int.: #${plazaSeleccionada.id} ${tipoExtra}`;

        if(btnL) {
            btnL.style.display = 'flex';
            btnL.disabled = (plazaSeleccionada.status === 0);
            btnL.style.opacity = btnL.disabled ? '0.3' : '1';
            btnL.style.cursor = btnL.disabled ? 'not-allowed' : 'pointer';
        }

        if(btnM) {
            btnM.style.display = 'flex';
            btnM.disabled = (plazaSeleccionada.status === 3);
            btnM.style.opacity = btnM.disabled ? '0.3' : '1';
            btnM.style.cursor = btnM.disabled ? 'not-allowed' : 'pointer';
        }
    }

    async function cambiarEstadoPlaza(nuevoEstado) {
        if (!plazaSeleccionada) return;
        try {
            await apiFetch(`${API}/api/ParkingManagement/spots/${plazaSeleccionada.id}/status?status=${nuevoEstado}`, {
                method: 'PATCH'
            });
            mostrarToast(`Plaza actualizada a ${STATUS[nuevoEstado].name}`, 'exito');
            
            await recargarPlazas();
            
            plazaSeleccionada = gridPlazasVivas.find(p => p.id === plazaSeleccionada.id);
            renderizarFiltros();

        } catch (e) {
            mostrarToast('Error cambiando estado: ' + e.message, 'error');
        }
    }

    function conectarSignalR(pId) {
        if (typeof signalR === 'undefined') return;

        const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API}/hubs/parking`)
        .withAutomaticReconnect()
        .build();

        // 1. Escuchar si un empleado/sensor cambia el estado directamente (SpotStatusChanged)
        connection.on("SpotStatusChanged", (payload) => {
            console.log("[SignalR] Estado de plaza modificado:", payload);
            recargarPlazas(); // Recargamos para refrescar grid y contadores
            mostrarToast(`Plaza ${payload.spotNumber} actualizada`, 'aviso');
        });

        // 2. Escuchar si un usuario hace un pago/reserva online (UpdateSpots)
        connection.on("UpdateSpots", (hubParkingId, newCount) => {
            if (hubParkingId == pId) {
                console.log("[SignalR] Nueva reserva online. Actualizando plazas...");
                recargarPlazas();
            }
        });

        connection.start()
            .then(() => {
                console.log('[SignalR] Conectado al grid de plazas');
                // Nos suscribimos al grupo exclusivo de ESTE parking usando el método de tu Hub
                connection.invoke("JoinParking", parseInt(pId)).catch(e => console.error(e));
            })
            .catch(err => console.error('[SignalR] Error:', err));
    }


    document.addEventListener('DOMContentLoaded', initGestionPlazas);

})();
