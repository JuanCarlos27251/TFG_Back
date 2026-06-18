/* ==============================================
   PARKit — Configuración global de Tailwind
   + Lógica de header dinámico según sesión
   ============================================== */

//  Configuración de Tailwind 
window.tailwind.config = {
    darkMode: 'class',
    theme: {
        extend: {
            colors: {
                primario:           '#e05d47',
                'primario-oscuro':  '#bd4a36',
                'fondo-oscuro':     '#161413',
            },
            fontFamily: {
                sans: ['DM Sans', 'sans-serif'],
            },
        },
    },
};

//  SISTEMA GLOBAL DE NOTIFICACIONES (TOAST PREMIUM) 
window.mostrarToast = function(msg, tipo = 'exito') {
    // Buscar contenedor o crearlo automáticamente
    let cont = document.getElementById('contenedor-toast');
    if (!cont) {
        cont = document.createElement('div');
        cont.id = 'contenedor-toast';
        document.body.appendChild(cont);
    }
    const toast = document.createElement('div');
    toast.className = `toast toast-${tipo}`;
    
    // Iconografía material dinámica
    const iconos = {
        exito: 'check_circle',
        error: 'error',
        aviso: 'warning',
        info: 'info'
    };
    const icono = iconos[tipo] || 'info';
    toast.innerHTML = `
        <span class="material-symbols-outlined icono-toast text-[22px]">${icono}</span>
        <span class="pr-2 tracking-tight">${msg}</span>
    `;
    
    cont.appendChild(toast);
    // Salida sincronizada con la barra (2 segundos)
    setTimeout(() => {
        toast.classList.add('toast-saliendo');
        setTimeout(() => toast.remove(), 300); // 300ms de la animación CSS final
    }, 2000);
};

//  Lógica de header dinámico 
// Se ejecuta al cargar el DOM en todas las páginas que incluyan este script.
// Adapta los botones y la navegación según el estado de sesión del usuario.

document.addEventListener('DOMContentLoaded', () => {

    // ── Helpers de sesión (independientes de auth.js si no está cargado)
    // En páginas donde auth.js esté cargado, AUTH ya estará disponible.
    // En páginas donde no, usamos acceso directo a localStorage.

    const token      = localStorage.getItem('parkit_token');
    const rawUsuario = localStorage.getItem('parkit_usuario');
    let usuario      = null;

    try { usuario = rawUsuario ? JSON.parse(rawUsuario) : null; } catch {}

    // Verificamos que el token no haya expirado
    let sesionActiva = false;
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            sesionActiva  = payload.exp * 1000 > Date.now();
        } catch {}
    }

    //  Referencias a elementos del header 
    const enlacesAuth  = document.getElementById('enlaces-auth');   // div con botones login/registro
    const avatarBtn    = document.getElementById('header-user-avatar'); // avatar del perfil (páginas internas)
    const navEmpresa   = document.querySelector('nav a[href*="empresa"], nav a[href*="Empresa"], nav a[href*="panelEmpresa"], nav a[href*="EMPRESA"]');

    //  Estado: SIN sesión 
    if (!sesionActiva) {
        // Si el avatar está visible (páginas internas), lo ocultamos
        if (avatarBtn) avatarBtn.style.display = 'none';
        return; // el header por defecto ya muestra login/registro
    }

    //  Estado: CON sesión activa 
    const rol       = localStorage.getItem('parkit_rol');
    const esEmpresa = (rol === 'empresa');
    const esAdmin   = (rol === 'Admin'); // Nueva validación
    const inicial   = usuario?.inicial || (usuario?.nombre?.charAt(0).toUpperCase()) || 'U';
    const nombre    = usuario?.nombre  || 'Usuario';
    // 1. Ocultar enlace "Para empresas" en la navegación si es usuario o admin
    if ((!esEmpresa && !esAdmin) && navEmpresa) {
        navEmpresa.style.display = 'none';
    }
    // 2. Transformar el bloque de botones login/registro → avatar + menú
    if (enlacesAuth) {
        let destino = 'perfil.html';
        let textoDestino = 'Mi perfil';
        let iconoDestino = 'person';
        
        if (esEmpresa) {
            destino = 'EMPRESA/panelEmpresa.html';
            textoDestino = 'Mi Empresa';
            iconoDestino = 'store';
        } else if (esAdmin) {
            destino = 'panelAdmin.html';
            textoDestino = 'Panel Global (Admin)';
            iconoDestino = 'admin_panel_settings';
        }
        enlacesAuth.innerHTML = `
            <div class="flex items-center gap-2 relative" id="menu-usuario">
                <button id="btn-avatar-menu" class="avatar-btn hover:scale-105 transition-transform" title="${nombre}" aria-label="Menú de usuario">${inicial}</button>
                <!-- Menú desplegable -->
                <div id="dropdown-usuario" class="absolute right-0 top-full mt-2 w-56 bg-[var(--fondo-card)] border border-[var(--borde)] rounded-xl shadow-xl z-50 overflow-hidden hidden aparecer">
                    <div class="px-4 py-3 border-b border-[var(--borde)]">
                        <p class="text-xs font-bold text-[var(--texto-suave)] uppercase tracking-wider">Conectado como</p>
                        <p class="text-sm font-bold text-[var(--texto)] truncate mt-0.5">${nombre}</p>
                    </div>
                    <a href="${destino}" class="flex items-center gap-2 px-4 py-3 text-sm text-[var(--texto)] hover:bg-[var(--primario-claro)] hover:text-[var(--primario)] transition-colors">
                        <span class="material-symbols-outlined text-[18px]">${iconoDestino}</span>
                        ${textoDestino}
                    </a>
                    ${(!esEmpresa && !esAdmin) ? `
                    <a href="reservas.html" class="flex items-center gap-2 px-4 py-3 text-sm text-[var(--texto)] hover:bg-[var(--primario-claro)] hover:text-[var(--primario)] transition-colors">
                        <span class="material-symbols-outlined text-[18px]">history</span>
                        Mis reservas
                    </a>` : ''}
                    <div class="border-t border-[var(--borde)] mt-1">
                        <button id="btn-cerrar-sesion" class="w-full flex items-center gap-2 px-4 py-3 text-sm text-[var(--rojo)] hover:bg-red-50 dark:hover:bg-red-950/20 transition-colors">
                            <span class="material-symbols-outlined text-[18px]">logout</span>
                            Cerrar sesión
                        </button>
                    </div>
                </div>
            </div>
        `;
        const btnAvatar  = document.getElementById('btn-avatar-menu');
        const dropdown   = document.getElementById('dropdown-usuario');
        btnAvatar?.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown?.classList.toggle('hidden');
        });
        document.addEventListener('click', () => dropdown?.classList.add('hidden'));
        document.getElementById('btn-cerrar-sesion')?.addEventListener('click', () => {
            AUTH.cerrarSesion();
        });
    }
    // 3. Actualizar el avatar del header en páginas internas
    if (avatarBtn) {
        avatarBtn.textContent = inicial;
        avatarBtn.title       = nombre;
        if (esAdmin) avatarBtn.href = 'panelAdmin.html';
        else if (esEmpresa) avatarBtn.href = 'EMPRESA/panelEmpresa.html';
        else avatarBtn.href = 'perfil.html';
    }

    //  REGISTRO DE SERVICE WORKER (PWA) 
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', () => {
            // Registramos el sw.js que está en la raíz de PARKit.Frontend
            navigator.serviceWorker.register('../sw.js')
                .then(reg => console.log('PWA: Service Worker registrado.', reg.scope))
                .catch(err => console.error('PWA: Error al registrar Service Worker.', err));
        });
    }  

});


