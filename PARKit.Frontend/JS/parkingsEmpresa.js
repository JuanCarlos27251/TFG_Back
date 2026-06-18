/* ==============================================
   PARKit — Módulo de Gestión de Parkings (Empresa)
   Usado en: misparkingsEmpresa.html
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let companyId = null;
    let todosLosParkings = []; // Para guardarlos en local para filtros/búsqueda

    //  Utilidades Básicas 

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
        companyId = AUTH.obtenerUsuario()?.id;
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

    //  Lógica Principal "Mis Parkings" 

    async function initMisParkings() {
        if (!verificarSesion()) return;

        try {
            // Paralelizar llamadas al cargarlo todo de golpe
            const [empresa, parkings] = await Promise.all([
                apiFetch(`${API}/api/Company/${companyId}`),
                apiFetch(`${API}/api/Parking/manager/${companyId}`)
            ]);

            renderCabeceraGenerica(empresa);

            todosLosParkings = parkings;
            document.getElementById('parking-count').textContent = `${todosLosParkings.length} parkings registrados en tu cuenta`;
            
            renderGridParkings(todosLosParkings);

        } catch (e) {
            mostrarToast('Error obteniendo listado de parkings: ' + e.message, 'error');
            document.getElementById('parking-grid').innerHTML = `<p class="col-span-3 text-center py-6 text-[var(--rojo)]">Hubo un problema de conexión con el servidor.</p>`;
        }

        // Eventos del buscador en tiempo real
        document.getElementById('buscador-parking')?.addEventListener('input', aplicarFiltrosYBusqueda);
        
        // Eventos de los botones de filtro
        document.querySelectorAll('.filtro-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                // Quitar color a los demás
                document.querySelectorAll('.filtro-btn').forEach(b => {
                    b.classList.remove('activo', 'bg-[var(--primario)]', 'text-white');
                    b.classList.add('bg-[var(--fondo-card)]', 'text-[var(--texto-suave)]');
                });
                // Darle color al clickado
                const clicked = e.currentTarget;
                clicked.classList.add('activo', 'bg-[var(--primario)]', 'text-white');
                clicked.classList.remove('bg-[var(--fondo-card)]', 'text-[var(--texto-suave)]');
                
                aplicarFiltrosYBusqueda();
            });
        });

         conectarSignalRParkings();
    }

    function aplicarFiltrosYBusqueda() {
        const textBusqueda = document.getElementById('buscador-parking')?.value.toLowerCase() || '';
        const btnActivo = document.querySelector('.filtro-btn.activo');
        const filtroTipo = btnActivo ? btnActivo.dataset.filter : 'all';

        const filtrados = todosLosParkings.filter(p => {
            const pasaTexto = p.name.toLowerCase().includes(textBusqueda) || p.address.toLowerCase().includes(textBusqueda);
            
            let pasaFiltro = true;
            if (filtroTipo === 'active') pasaFiltro = p.isActive === true;
            else if (filtroTipo === 'inactive') pasaFiltro = p.isActive === false;

            return pasaTexto && pasaFiltro;
        });

        renderGridParkings(filtrados);
    }

    function renderGridParkings(lista) {
        const grid = document.getElementById('parking-grid');
        if (!grid) return;

        grid.innerHTML = '';

        if (lista.length === 0) {
            grid.innerHTML = `<p class="col-span-3 text-center py-10 text-[var(--texto-suave)] italic">No se encontraron parkings.</p>`;
        }

        lista.forEach(p => {
            const tipoMap = {
                0: { nombre: 'Público', clase: 'badge-primario' },
                1: { nombre: 'Privado', clase: 'badge-naranja' },
                2: { nombre: 'Zona Regulada', clase: 'badge-primario-claro' },
                3: { nombre: 'Zona Naranja', clase: 'badge-naranja' } 
            };
            
            const tipoProp = tipoMap[p.type] || { nombre: 'Desconocido', clase: 'badge-primario' };
            const img = p.imageUrl ? p.imageUrl : 'https://images.unsplash.com/photo-1506521781263-d8422e82f27a?auto=format&fit=crop&w=600&q=70';

            let total = 0, ocupadas = 0, libres = 0;
            if (p.spots && p.spots.length > 0) {
                total = p.spots.length;
                // Usamos el dato maestro de plazas libres que viene del servidor
                libres = p.availableSpots !== undefined ? p.availableSpots : total;
                ocupadas = total - libres; 
            }
            
            const pct = total > 0 ? Math.round((ocupadas / total) * 100) : 0;

            const estadoBadge = p.isActive 
                ? '<span class="absolute top-3 right-3 badge badge-verde text-[10px] font-extrabold uppercase tracking-widest flex items-center gap-1"><span class="w-1.5 h-1.5 rounded-full bg-[var(--verde)] animate-pulse"></span>Activo</span>'
                : '<span class="absolute top-3 right-3 badge badge-rojo text-[10px] font-extrabold uppercase tracking-widest flex items-center gap-1"><span class="w-1.5 h-1.5 rounded-full bg-[var(--rojo)]"></span>Inactivo</span>';
                
            const opacidadBase = p.isActive ? '' : 'opacity-70 hover:opacity-90 grayscale-[0.8]';

            const tarjeta = `
                 <div class="card flex flex-col overflow-hidden group hover:shadow-xl hover:shadow-black/20 hover:-translate-y-0.5 transition-all duration-200 ${opacidadBase}">
                    <div class="relative h-44 bg-[var(--fondo)] overflow-hidden flex-shrink-0">
                        <img src="${img}" alt="${p.name}" class="w-full h-full object-cover opacity-80 group-hover:scale-105 transition-transform duration-300" />
                        <div class="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent"></div>
                        <span class="absolute top-3 left-3 badge ${tipoProp.clase} text-[10px] font-extrabold uppercase tracking-widest">${tipoProp.nombre}</span>
                        ${estadoBadge}
                        <div class="absolute bottom-3 left-4 right-4">
                            <p class="text-white font-black text-base leading-tight drop-shadow-sm">${p.name}</p>
                            <p class="text-white/70 text-xs mt-0.5 flex items-center gap-1 truncate w-full" title="${p.address}">
                                <span class="material-symbols-outlined text-[13px]">location_on</span>
                                ${p.address}
                            </p>
                        </div>
                    </div>

                    <div class="p-5 flex flex-col gap-4 flex-1">
                        <div class="grid grid-cols-3 gap-2 text-center">
                            <div class="bg-[var(--fondo)] rounded-lg p-2.5">
                                <p class="text-lg font-black ${p.isActive && libres > 0 ? 'text-[var(--verde)]' : 'text-[var(--texto-suave)]'}">${total > 0 ? libres : '--'}</p>
                                <p class="text-[10px] text-[var(--texto-suave)] font-bold uppercase tracking-wider mt-0.5">Libres</p>
                            </div>
                            <div class="bg-[var(--fondo)] rounded-lg p-2.5">
                                <p class="text-lg font-black text-[var(--texto)]">${total > 0 ? total : '--'}</p>
                                <p class="text-[10px] text-[var(--texto-suave)] font-bold uppercase tracking-wider mt-0.5">Total</p>
                            </div>
                            <div class="bg-[var(--fondo)] rounded-lg p-2.5">
                                <p class="text-lg font-black text-[var(--primario)]">${total > 0 ? pct + '%' : '--'}</p>
                                <p class="text-[10px] text-[var(--texto-suave)] font-bold uppercase tracking-wider mt-0.5">Ocup.</p>
                            </div>
                        </div>

                        <div>
                            <div class="w-full h-1.5 bg-[var(--borde)] rounded-full overflow-hidden">
                                <div class="h-full bg-[var(--primario)] rounded-full" style="width: ${pct}%"></div>
                            </div>
                        </div>

                        <div class="flex gap-2 mt-auto pt-1">
                            <a href="gestionplazasEmpresa.html?id=${p.id}" class="flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-lg border border-[var(--borde)] text-xs font-bold text-[var(--texto-suave)] hover:border-[var(--primario)] hover:text-[var(--primario)] transition-all">
                                <span class="material-symbols-outlined text-[16px]">garage</span> Plazas
                            </a>
                            <a href="editarparkingEmpresa.html?id=${p.id}" class="flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-lg bg-[var(--primario)] text-white text-xs font-bold hover:bg-[var(--primario-oscuro)] transition-all shadow-md shadow-[var(--primario)]/20">
                                <span class="material-symbols-outlined text-[16px]">edit</span> Editar
                            </a>
                        </div>
                    </div>
                </div>
            `;
            grid.insertAdjacentHTML('beforeend', tarjeta);
        });

        // Botón grande final "Añadir Parking"
        grid.insertAdjacentHTML('beforeend', `
             <a href="addparkingEmpresa.html" class="card flex flex-col items-center justify-center gap-4 p-8 border-dashed border-2 border-[var(--borde)] bg-transparent hover:border-[var(--primario)] hover:bg-[var(--primario)]/5 cursor-pointer group transition-all duration-200 min-h-[320px]">
                <div class="w-14 h-14 rounded-full border-2 border-dashed border-[var(--borde)] group-hover:border-[var(--primario)] flex items-center justify-center text-[var(--texto-suave)] group-hover:text-[var(--primario)] transition-all">
                    <span class="material-symbols-outlined text-2xl">add</span>
                </div>
                <div class="text-center">
                    <p class="font-bold text-sm text-[var(--texto-suave)] group-hover:text-[var(--primario)] transition-colors">Añadir nuevo parking</p>
                    <p class="text-xs text-[var(--texto-suave)] mt-1 opacity-70">Registra una nueva instalación</p>
                </div>
            </a>
        `);
    }

    // Hace una petición limpia al servidor y repinta la pantalla respetando lo que el usuario esté buscando
    async function refrescarDatosSilencioso() {
        try {
            const parkingsActualizados = await apiFetch(`${API}/api/Parking/manager/${companyId}`);
            todosLosParkings = parkingsActualizados; 
            
            // Repintamos las tarjetas aplicando el texto o filtro que el usuario tenga marcado
            aplicarFiltrosYBusqueda();   
        } catch(e) {
            console.error("[SignalR] Error refrescando parkings en vivo", e);
        }
    }

    function conectarSignalRParkings() {
        // Por si acaso te olvidas el script en el HTML
        if (typeof signalR === 'undefined') {
            console.warn('[SignalR] No cargado. Faltan las actualizaciones en tiempo real.');
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API}/hubs/parking`)
        .withAutomaticReconnect()
        .build();

        // 1. Si el estado de una sola plaza cambia (sensor o panel interno)
        connection.on("SpotStatusChanged", (payload) => {
            console.log("[SignalR] Movimiento de plaza detectado. Actualizando tarjetas...");
            refrescarDatosSilencioso();
        });

        // 2. Si entra un nuevo pago online de un cliente
        connection.on("UpdateSpots", (hubParkingId, newCount) => {
            // Comprobamos que el pago es de uno de nuestros parkings
            if (todosLosParkings.some(p => p.id == hubParkingId)) {
                console.log("[SignalR] Reserva completada. Actualizando tarjetas...");
                refrescarDatosSilencioso();
            }
        });

        connection.start()
            .then(() => {
                console.log('[SignalR] Conectado. Suscribiéndose a los parkings de la empresa...');
                // Magia: Nos metemos en la sala de chat de TODOS los parkings que nos pertenecen
                todosLosParkings.forEach(p => {
                    connection.invoke("JoinParking", parseInt(p.id)).catch(e => console.error(e));
                });
            })
            .catch(err => console.error('[SignalR] Error de conexión:', err));
    }


    //  Log Out de esta pantalla 
    document.addEventListener('click', (e) => {
        if(e.target.closest('#btn-logout-empresa')){
            AUTH.cerrarSesion(true);
        }
    });

    //  Arranque Automático 
    document.addEventListener('DOMContentLoaded', initMisParkings);

})();
