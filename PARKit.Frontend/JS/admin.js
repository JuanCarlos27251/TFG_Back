document.addEventListener('DOMContentLoaded', () => {

    // 1. CORTAFUEGOS DE SEGURIDAD (Solo pasa el Admin)
    if (!AUTH.estaAutenticado() || AUTH.obtenerRol() !== 'Admin') {
        window.location.href = 'index.html';
        return;
    }

    // 2. Lógica de Pestañas (Tabs)
    const navButtons = document.querySelectorAll('.nav-admin');
    const tabContents = document.querySelectorAll('.tab-content');

    navButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            // Resetear estilos inactivos
            navButtons.forEach(b => {
                b.className = 'nav-admin text-left px-4 py-3 rounded-xl font-bold flex items-center gap-3 text-[var(--texto-suave)] hover:text-[var(--texto)] hover:bg-[var(--primario-claro)] transition-all';
            });
            tabContents.forEach(tc => tc.classList.add('hidden', 'aparecer'));
            
            // Activar botón clickeado
            btn.className = 'nav-admin text-left px-4 py-3 rounded-xl font-bold flex items-center gap-3 transition-all bg-[var(--primario)] text-white shadow-lg shadow-[var(--primario)]/30';
            document.getElementById(btn.dataset.tab).classList.remove('hidden');

            // Cargar datos correspondientes
            if(btn.dataset.tab === 'tab-empresas') cargarEmpresas();
            if(btn.dataset.tab === 'tab-usuarios') cargarUsuarios();
            if(btn.dataset.tab === 'tab-reservas') cargarReservas();
        });
    });

    // ── FUNCIONES DE CARGA Y FETCH ──

    async function cargarEmpresas() {
        const tbody = document.getElementById('lista-empresas');
        tbody.innerHTML = '<tr><td colspan="4" class="px-6 py-8 text-center text-[var(--texto-suave)]">Cargando empresas...</td></tr>';
        
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/Company`, { headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('Error al cargar la lista de empresas');
            const data = await res.json();
            
            if (data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" class="px-6 py-8 text-center text-[var(--texto-suave)]">No hay empresas registradas.</td></tr>';
                return;
            }

            tbody.innerHTML = data.map(emp => `
                <tr class="hover:bg-[var(--fondo)] transition-colors">
                    <td class="px-6 py-4 font-mono text-[var(--texto-suave)]">#${emp.id}</td>
                    <td class="px-6 py-4 font-bold text-[var(--texto)]">${emp.nameCompany}</td>
                    <td class="px-6 py-4 text-[var(--texto-suave)]">${emp.email}</td>
                    <td class="px-6 py-4 text-right">
                        <button onclick="borrarEmpresa(${emp.id})" class="text-[var(--rojo)] hover:bg-red-50 dark:hover:bg-red-900/20 p-2 rounded-lg transition-colors" title="Eliminar definitivamente">
                            <span class="material-symbols-outlined text-[20px] mb-0">delete</span>
                        </button>
                    </td>
                </tr>
            `).join('');
        } catch (e) {
            window.mostrarToast(e.message, 'error');
        }
    }

    async function cargarUsuarios() {
        const tbody = document.getElementById('lista-usuarios');
        tbody.innerHTML = '<tr><td colspan="4" class="px-6 py-8 text-center text-[var(--texto-suave)]">Cargando usuarios...</td></tr>';
        
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/User`, { headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('Error al cargar la lista de usuarios');
            const data = await res.json();
            
            tbody.innerHTML = data.map(u => `
                <tr class="hover:bg-[var(--fondo)] transition-colors">
                    <td class="px-6 py-4 font-mono text-[var(--texto-suave)]">#${u.id}</td>
                    <td class="px-6 py-4 font-bold text-[var(--texto)]">${u.name}</td>
                    <td class="px-6 py-4 text-[var(--texto-suave)]">${u.email}</td>
                    <td class="px-6 py-4 text-[var(--texto-suave)]">${u.phone || 'N/A'}</td>
                </tr>
            `).join('');
        } catch (e) {
            window.mostrarToast(e.message, 'error');
        }
    }

    async function cargarReservas() {
        const tbody = document.getElementById('lista-reservas');
        tbody.innerHTML = '<tr><td colspan="4" class="px-6 py-8 text-center text-[var(--texto-suave)]">Cargando reservas globales...</td></tr>';
        
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/ReservationManagement/all`, { headers: AUTH.cabecerasAuth() });
            if (!res.ok) throw new Error('Error al cargar reservas globales');
            const data = await res.json();
            
            tbody.innerHTML = data.map(r => {
                // Interpretar el status según el Enum del backend
                let textoEstado = 'Desconocido';
                let claseBadge = 'badge-naranja';
                
                if(r.status === 0) { textoEstado = 'Pendiente'; claseBadge = 'badge-naranja'; }
                if(r.status === 1) { textoEstado = 'Confirmada'; claseBadge = 'badge-azul'; }
                if(r.status === 2) { textoEstado = 'Activa'; claseBadge = 'badge-primario'; }
                if(r.status === 3) { textoEstado = 'Finalizada'; claseBadge = 'badge-verde'; }
                if(r.status === 4) { textoEstado = 'Cancelada'; claseBadge = 'badge-rojo'; }

                return `
                <tr class="hover:bg-[var(--fondo)] transition-colors">
                    <td class="px-6 py-4 font-mono text-[var(--texto-suave)]">#${r.id}</td>
                    <td class="px-6 py-4 font-medium text-[var(--texto)]">Plaza ${r.parkingSpotId || '?'}</td>
                    <td class="px-6 py-4">
                        <span class="badge ${claseBadge}">${textoEstado}</span>
                    </td>
                    <td class="px-6 py-4 font-bold text-[var(--texto)]">${r.totalAmount.toFixed(2)} €</td>
                </tr>
            `}).join('');
        } catch (e) {
            window.mostrarToast(e.message, 'error');
        }
    }

    // ── BORRADO CRÍTICO (ADMIN POWER) ──
    window.borrarEmpresa = async function(id) {
        if(!confirm('⚠️ ADVERTENCIA MÁXIMA:\\n\\n¿Seguro que quieres borrar a esta empresa?\\n¡Esto eliminará la empresa, todos sus parkings, plazas y tarifas asociadas!\\n\\nEsta acción es irreversible.')) return;
        
        try {
            const res = await fetch(`${AUTH.API_BASE}/api/Company/${id}`, { 
                method: 'DELETE',
                headers: AUTH.cabecerasAuth()
            });
            if (!res.ok) throw new Error('No se pudo borrar la empresa en la base de datos');
            
            window.mostrarToast('Empresa aniquilada del sistema', 'exito');
            cargarEmpresas(); // Refrescar la lista en vivo
        } catch(e) {
            window.mostrarToast(e.message, 'error');
        }
    }

    // Cargar la primera pestaña por defecto al entrar
    cargarEmpresas();
});
