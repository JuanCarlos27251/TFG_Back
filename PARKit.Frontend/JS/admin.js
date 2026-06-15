document.addEventListener('DOMContentLoaded', () => {

    if (!AUTH.estaAutenticado() || AUTH.obtenerRol() !== 'Admin') {
        window.location.href = 'index.html';
        return;
    }

    let mapaUsuarios = new Map(); 

    const navButtons = document.querySelectorAll('.nav-admin');
    const tabContents = document.querySelectorAll('.tab-content');

    navButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            navButtons.forEach(b => b.className = 'nav-admin text-left px-4 py-3 rounded-xl font-bold flex items-center gap-3 text-black/70 hover:text-black hover:bg-black/5 transition-all');
            tabContents.forEach(tc => tc.classList.add('hidden', 'aparecer'));
            
            btn.className = 'nav-admin text-left px-4 py-3 rounded-xl font-bold flex items-center gap-3 transition-all bg-[#e05d47] text-white shadow-lg shadow-[#e05d47]/30';
            document.getElementById(btn.dataset.tab).classList.remove('hidden');

            if(btn.dataset.tab === 'tab-empresas') cargarEmpresas();
            if(btn.dataset.tab === 'tab-usuarios') cargarUsuarios();
            if(btn.dataset.tab === 'tab-res-usuarios' || btn.dataset.tab === 'tab-res-empresas') cargarReservas();
        });
    });

        // ── EMPRESAS ──
    async function cargarEmpresas() {
        const tbody = document.getElementById('lista-empresas');
        tbody.innerHTML = '<tr><td colspan="5" class="px-6 py-8 text-center text-gray-400">Cargando empresas...</td></tr>';
        
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/Company`, { headers: AUTH.cabecerasAuth() });
            const data = await res.json();
            
            tbody.innerHTML = data.map(emp => `
                <tr class="hover:bg-[#fce8e6]/30 transition-colors border-b border-gray-50">
                    <td class="px-6 py-4 font-black text-black">${emp.nameCompany}</td>
                    <td class="px-6 py-4 font-mono text-gray-500">${emp.cif}</td>
                    <td class="px-6 py-4 text-gray-600">
                        <div class="flex flex-col">
                            <span>${emp.email}</span>
                            <span class="text-xs text-gray-400">${emp.phone || '-'}</span>
                        </div>
                    </td>
                    <td class="px-6 py-4">
                        <span class="badge ${emp.isActive ? 'badge-verde' : 'badge-rojo'}">${emp.isActive ? 'Activa' : 'Inactiva'}</span>
                    </td>
                    <td class="px-6 py-4 text-right">
                        <!-- BOTÓN DESPLEGAR -->
                        <button onclick="toggleEstadisticas(${emp.id})" class="text-black hover:text-[#e05d47] p-2 rounded-lg transition-colors mr-2" title="Desplegar Estadísticas">
                            <span class="material-symbols-outlined text-[24px] mb-0" id="icon-stats-${emp.id}">expand_more</span>
                        </button>
                        <!-- BOTÓN BORRAR -->
                        <button onclick="borrarEmpresa(${emp.id})" class="text-red-500 hover:bg-red-50 p-2 rounded-lg transition-colors" title="Eliminar Definitivamente">
                            <span class="material-symbols-outlined text-[20px] mb-0">delete_forever</span>
                        </button>
                    </td>
                </tr>
                <!-- FILA DESPLEGABLE OCULTA -->
                <tr id="stats-row-${emp.id}" class="hidden bg-gray-50/50 shadow-inner">
                    <td colspan="5" class="p-0 border-b-2 border-gray-100">
                        <div id="stats-content-${emp.id}" class="p-6"></div>
                    </td>
                </tr>
            `).join('');
        } catch (e) { window.mostrarToast('Error al cargar empresas', 'error'); }
    }


    // ── USUARIOS ──
    async function cargarUsuarios() {
        const tbody = document.getElementById('lista-usuarios');
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/User`, { headers: AUTH.cabecerasAuth() });
            const data = await res.json();
            
            mapaUsuarios.clear();
            data.forEach(u => mapaUsuarios.set(u.id, u.name));

            tbody.innerHTML = data.map(u => `
                <tr class="hover:bg-[#fce8e6]/30 transition-colors ${!u.isActive ? 'opacity-50' : ''}">
                    <td class="px-6 py-4 font-bold text-black">${u.name}</td>
                    <td class="px-6 py-4 text-gray-600">${u.email}</td>
                    <td class="px-6 py-4 text-gray-600">${u.phone || '-'}</td>
                    <td class="px-6 py-4">
                        <span class="badge ${u.isActive ? 'badge-verde' : 'badge-rojo'}">${u.isActive ? 'Activo' : 'Inactivo'}</span>
                    </td>
                    <td class="px-6 py-4 text-right">
                        ${u.isActive ? `
                        <button onclick="desactivarUsuario(${u.id})" class="text-orange-500 hover:bg-orange-50 p-2 rounded-lg transition-colors" title="Desactivar (Soft Delete)">
                            <span class="material-symbols-outlined text-[20px] mb-0">person_off</span>
                        </button>
                        ` : '<span class="text-xs text-gray-400 italic">Desactivado</span>'}
                    </td>
                </tr>
            `).join('');
        } catch (e) { window.mostrarToast('Error al cargar usuarios', 'error'); }
    }

    // ── RESERVAS (AGRUPACIÓN Y COLUMNAS ESPECÍFICAS) ──
    async function cargarReservas() {
        const contUser = document.getElementById('lista-reservas-usuarios');
        const contEmpresa = document.getElementById('lista-reservas-empresas');
        contUser.innerHTML = '<p class="text-center py-4 text-gray-400">Procesando transacciones...</p>';
        contEmpresa.innerHTML = '<p class="text-center py-4 text-gray-400">Procesando transacciones...</p>';

        try {
            if(mapaUsuarios.size === 0) await cargarUsuarios();

            const res = await fetch(`${AUTH.API_BASE}/api/ReservationManagement/all`, { headers: AUTH.cabecerasAuth() });
            const reservas = await res.json();

            // 1. Agrupar por Usuario
            const porUsuario = reservas.reduce((acc, r) => {
                const nombre = mapaUsuarios.get(r.userId) || `Usuario #${r.userId}`;
                if(!acc[nombre]) acc[nombre] = [];
                acc[nombre].push(r);
                return acc;
            }, {});

            // 2. Agrupar por Empresa (ParkingName ya incluye el nombre de la empresa gracias al Join de Backend)
            const porEmpresa = reservas.reduce((acc, r) => {
                const nombre = r.parkingName || 'Parking Desconocido';
                if(!acc[nombre]) acc[nombre] = [];
                acc[nombre].push(r);
                return acc;
            }, {});

            // Pintar Por Usuario (Fecha, Importe, Plaza, Parking)
            contUser.innerHTML = Object.keys(porUsuario).map(nombreUser => `
                <div class="border border-gray-100 rounded-xl overflow-hidden mb-6 shadow-sm">
                    <div class="bg-gray-50 px-4 py-3 border-b border-gray-100 font-black text-lg text-black flex items-center gap-2">
                        <span class="material-symbols-outlined text-[#e05d47]">person</span> ${nombreUser}
                    </div>
                    <table class="w-full text-left text-sm whitespace-nowrap">
                        <thead>
                            <tr class="text-gray-400 text-xs uppercase tracking-wider">
                                <th class="px-4 py-2">Fecha</th>
                                <th class="px-4 py-2">Parking / Empresa</th>
                                <th class="px-4 py-2">Plaza</th>
                                <th class="px-4 py-2">Estado</th>
                                <th class="px-4 py-2 font-black">Importe</th>
                                <th class="px-4 py-2 text-right">Acción</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            ${porUsuario[nombreUser].map(r => renderFilaReservaUser(r)).join('')}
                        </tbody>
                    </table>
                </div>
            `).join('');

            // Pintar Por Empresa (Fecha, Importe, Plaza, Usuario)
            contEmpresa.innerHTML = Object.keys(porEmpresa).map(nombreParking => `
                <div class="border border-gray-100 rounded-xl overflow-hidden mb-6 shadow-sm">
                    <div class="bg-[#fce8e6] px-4 py-3 border-b border-[#e05d47]/20 font-black text-lg text-[#b14532] flex items-center gap-2">
                        <span class="material-symbols-outlined">domain</span> ${nombreParking}
                    </div>
                    <table class="w-full text-left text-sm whitespace-nowrap">
                        <thead>
                            <tr class="text-gray-400 text-xs uppercase tracking-wider">
                                <th class="px-4 py-2">Fecha</th>
                                <th class="px-4 py-2">Usuario Cliente</th>
                                <th class="px-4 py-2">Plaza</th>
                                <th class="px-4 py-2">Estado</th>
                                <th class="px-4 py-2 font-black">Importe</th>
                                <th class="px-4 py-2 text-right">Acción</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            ${porEmpresa[nombreParking].map(r => renderFilaReservaEmpresa(r)).join('')}
                        </tbody>
                    </table>
                </div>
            `).join('');

        } catch (e) { window.mostrarToast('Error al procesar reservas', 'error'); }
    }

    function renderFilaReservaUser(r) {
        const fecha = new Date(r.startTime).toLocaleString('es-ES', { dateStyle: 'short', timeStyle: 'short' });
        const cancelable = r.status !== 4 && r.status !== 3; 

        return `
            <tr class="hover:bg-gray-50/50">
                <td class="px-4 py-3">${fecha}</td>
                <td class="px-4 py-3 font-medium text-black">${r.parkingName}</td>
                <td class="px-4 py-3 text-gray-500">${r.spotNumber || r.parkingSpotId}</td>
                <td class="px-4 py-3"><span class="text-xs px-2 py-1 rounded-full border ${r.status === 4 ? 'border-red-200 text-red-600 bg-red-50' : 'border-blue-200 text-blue-600 bg-blue-50'}">${r.status === 4 ? 'Cancelada' : r.status === 2 ? 'Activa' : 'Confirmada'}</span></td>
                <td class="px-4 py-3 font-bold text-black">${r.totalAmount.toFixed(2)} €</td>
                <td class="px-4 py-3 text-right">
                    ${cancelable ? `<button onclick="cancelarReserva(${r.id})" class="text-red-500 text-xs font-bold hover:underline">Cancelar</button>` : '-'}
                </td>
            </tr>
        `;
    }

    function renderFilaReservaEmpresa(r) {
        const fecha = new Date(r.startTime).toLocaleString('es-ES', { dateStyle: 'short', timeStyle: 'short' });
        const cancelable = r.status !== 4 && r.status !== 3; 
        const nombreCliente = mapaUsuarios.get(r.userId) || `ID #${r.userId}`;

        return `
            <tr class="hover:bg-gray-50/50">
                <td class="px-4 py-3">${fecha}</td>
                <td class="px-4 py-3 font-medium text-black">${nombreCliente}</td>
                <td class="px-4 py-3 text-gray-500">${r.spotNumber || r.parkingSpotId}</td>
                <td class="px-4 py-3"><span class="text-xs px-2 py-1 rounded-full border ${r.status === 4 ? 'border-red-200 text-red-600 bg-red-50' : 'border-blue-200 text-blue-600 bg-blue-50'}">${r.status === 4 ? 'Cancelada' : r.status === 2 ? 'Activa' : 'Confirmada'}</span></td>
                <td class="px-4 py-3 font-bold text-black">${r.totalAmount.toFixed(2)} €</td>
                <td class="px-4 py-3 text-right">
                    ${cancelable ? `<button onclick="cancelarReserva(${r.id})" class="text-red-500 text-xs font-bold hover:underline">Cancelar</button>` : '-'}
                </td>
            </tr>
        `;
    }


    // ── ACCIONES GLOBALES ──
    window.desactivarUsuario = async function(id) {
        if(!confirm('¿Seguro que quieres desactivar a este usuario?')) return;
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/User/${id}`, { method: 'DELETE', headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('Error al desactivar usuario');
            window.mostrarToast('Usuario desactivado correctamente', 'exito');
            cargarUsuarios();
        } catch(e) { window.mostrarToast(e.message, 'error'); }
    }

    window.cancelarReserva = async function(id) {
        if(!confirm('¿Confirmas la cancelación forzosa de esta reserva?')) return;
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/ReservationManagement/${id}/cancel`, { method: 'PUT', headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('No se pudo cancelar la reserva.');
            window.mostrarToast('Reserva Cancelada con éxito', 'exito');
            cargarReservas();
        } catch(e) { window.mostrarToast(e.message, 'error'); }
    }

    window.borrarEmpresa = async function(id) {
        if(!confirm('⚠️ ADVERTENCIA:\\n\\n¿Seguro que quieres borrar a esta empresa? Es irreversible.')) return;
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/Company/${id}`, { method: 'DELETE', headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('Error al eliminar empresa');
            window.mostrarToast('Empresa eliminada', 'exito');
            cargarEmpresas();
        } catch(e) { window.mostrarToast(e.message, 'error'); }
    }

       // ── ESTADÍSTICAS REALES DESDE EL BACKEND ──
    window.toggleEstadisticas = async function(managerId) {
        const row = document.getElementById(`stats-row-${managerId}`);
        const icon = document.getElementById(`icon-stats-${managerId}`);
        const content = document.getElementById(`stats-content-${managerId}`);
        // Si la fila está abierta, la cerramos
        if (!row.classList.contains('hidden')) {
            row.classList.add('hidden');
            icon.textContent = 'expand_more';
            return;
        }
        // Si está cerrada, la abrimos
        row.classList.remove('hidden');
        icon.textContent = 'expand_less';
        // Si ya descargamos los datos antes, no hacemos la petición HTTP de nuevo
        if (content.innerHTML !== '') return;
        content.innerHTML = '<p class="text-center text-gray-400 my-4 text-sm font-bold animate-pulse">Recopilando datos de analítica...</p>';
        try {
            const resIngresos = await fetch(`${AUTH.API_BASE}/api/Statistics/company/${managerId}/revenue`, { headers: AUTH.cabecerasAuth() });
            const resOcupacion = await fetch(`${AUTH.API_BASE}/api/Statistics/company/${managerId}/occupancy`, { headers: AUTH.cabecerasAuth() });
            
            if(!resIngresos.ok || !resOcupacion.ok) throw new Error('No se pudieron obtener los datos.');
            
            const ingresos = await resIngresos.json(); 
            const ocupacion = await resOcupacion.json(); 
            const ingresosTotales = ingresos.reduce((sum, item) => sum + item.totalRevenue, 0);
            let ocupacionMediaGlobal = 0;
            if (ocupacion.length > 0) {
                const sumaOcupacion = ocupacion.reduce((sum, item) => sum + item.occupancyRate, 0);
                ocupacionMediaGlobal = (sumaOcupacion / ocupacion.length) * 100;
            }
            // Pintamos el diseño del desplegable a dos columnas
            content.innerHTML = `
                <div class="flex flex-col md:flex-row gap-6 max-w-5xl mx-auto">
                    <!-- Columna Izquierda: Tarjetas Resumen -->
                    <div class="flex-shrink-0 flex flex-col gap-4 md:w-72">
                        <div class="bg-white p-5 rounded-xl shadow-sm border border-gray-100">
                            <p class="text-[10px] text-gray-400 uppercase font-black tracking-wider mb-1">Ingresos (Últ. 6 meses)</p>
                            <p class="text-3xl font-black text-black">${ingresosTotales.toFixed(2)} <span class="text-xl text-gray-300 font-medium ml-1">€</span></p>
                        </div>
                        <div class="bg-[#fce8e6]/40 p-5 rounded-xl shadow-sm border border-[#e05d47]/20">
                            <p class="text-[10px] text-[#b14532] uppercase font-black tracking-wider mb-1">Ocupación Media Global</p>
                            <p class="text-3xl font-black text-[#e05d47]">${ocupacionMediaGlobal.toFixed(1)} <span class="text-xl opacity-50 font-medium ml-1">%</span></p>
                        </div>
                    </div>
                    <!-- Columna Derecha: Lista Detallada -->
                    <div class="flex-1 bg-white p-5 rounded-xl shadow-sm border border-gray-100">
                        <p class="text-xs text-gray-400 uppercase font-bold tracking-wider mb-4">Desglose de Parkings Activos</p>
                        <ul class="divide-y divide-gray-50 max-h-48 overflow-y-auto pr-3">
                            ${ocupacion.length === 0 ? '<li class="text-gray-400 italic text-sm py-2">Esta empresa no tiene parkings activos.</li>' : ''}
                            ${ocupacion.map(p => `
                                <li class="py-3 flex justify-between items-center group hover:bg-gray-50 px-2 rounded-lg transition-colors">
                                    <span class="font-medium text-black text-sm flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-gray-300">local_parking</span> 
                                        ${p.parkingName}
                                    </span>
                                    <span class="text-[11px] font-black tracking-wide px-3 py-1.5 rounded-md ${p.occupancyRate > 0.8 ? 'bg-red-50 text-red-600 border border-red-100' : 'bg-green-50 text-green-700 border border-green-100'}">
                                        ${(p.occupancyRate * 100).toFixed(1)}% Ocupado
                                    </span>
                                </li>
                            `).join('')}
                        </ul>
                    </div>
                </div>
            `;
        } catch(e) {
            content.innerHTML = `<p class="text-red-500 text-center py-4 text-sm font-bold"><span class="material-symbols-outlined align-middle mr-1">error</span> ${e.message}</p>`;
        }
    }


    cargarEmpresas();
    cargarUsuarios(); 
});
