/* ==============================================
   PARKit — Perfil de Usuario
   ============================================== */

(() => {
    const API = AUTH.API_BASE;
    let userId = null;

    // ── Utilidades ──────────────────────────────

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

    function abrirModal(id) {
        const m = document.getElementById(id);
        m.classList.remove('hidden');
        m.classList.add('flex');
    }
    function cerrarModal(id) {
        const m = document.getElementById(id);
        m.classList.add('hidden');
        m.classList.remove('flex');
    }

    // ── Inicialización ──────────────────────────

    async function init() {
        if (!AUTH.estaAutenticado()) {
            window.location.href = 'login.html';
            return;
        }
        actualizarHeaderAvatar();
        try {
            const user = await apiFetch(`${API}/api/User/me`);
            userId = user.id;
            renderDatosUsuario(user);
            await Promise.all([cargarVehiculos(), cargarMetodosPago()]);
        } catch (e) {
            mostrarToast('No se pudo cargar el perfil. ' + e.message, 'error');
        }
    }

    function actualizarHeaderAvatar() {
        const u = AUTH.obtenerUsuario();
        const btn = document.getElementById('header-user-avatar');
        if (btn && u) btn.innerHTML = `<span style="font-size:1rem;font-weight:700">${u.inicial || 'U'}</span>`;
    }

    // ── Datos personales ────────────────────────

    function renderDatosUsuario(user) {
        const inicial = (user.name || 'U').charAt(0).toUpperCase();
        document.getElementById('user-avatar').textContent = inicial;
        document.getElementById('profile-avatar-large').textContent = inicial;
        document.getElementById('sidebar-name').textContent = user.name || '';
        document.getElementById('client-greeting').textContent = `¡Hola, ${user.name?.split(' ')[0] || 'usuario'}!`;
        document.getElementById('input-name').value  = user.name  || '';
        document.getElementById('input-email').value = user.email || '';
        document.getElementById('input-phone').value = user.phone || '';
    }

    document.getElementById('btn-save-profile')?.addEventListener('click', async () => {
        if (!userId) return;
        const name  = document.getElementById('input-name').value.trim();
        const phone = document.getElementById('input-phone').value.trim();
        const email = document.getElementById('input-email').value.trim();
        if (!name) { mostrarToast('El nombre no puede estar vacío.', 'aviso'); return; }
        try {
            await apiFetch(`${API}/api/User/${userId}`, {
                method: 'PUT',
                body: JSON.stringify({ name, email, phone, password: null }),
            });
            // Actualizar localStorage
            const u = AUTH.obtenerUsuario();
            if (u) { u.nombre = name; u.inicial = name.charAt(0).toUpperCase(); localStorage.setItem('parkit_usuario', JSON.stringify(u)); }
            document.getElementById('sidebar-name').textContent = name;
            document.getElementById('user-avatar').textContent = name.charAt(0).toUpperCase();
            document.getElementById('profile-avatar-large').textContent = name.charAt(0).toUpperCase();
            document.getElementById('client-greeting').textContent = `¡Hola, ${name.split(' ')[0]}!`;
            mostrarToast('Perfil actualizado correctamente.', 'exito');
        } catch (e) {
            mostrarToast('Error al guardar: ' + e.message, 'error');
        }
    });

    // ── Modal cambio de contraseña ──────────────

    document.getElementById('btn-change-password')?.addEventListener('click', () => abrirModal('modal-password'));
    document.getElementById('btn-cancel-password')?.addEventListener('click', cerrarModalPassword);

    document.getElementById('btn-submit-password')?.addEventListener('click', async () => {
        if (!userId) return;
        const newPass     = document.getElementById('password-new').value;
        const confirmPass = document.getElementById('password-confirm').value;
        if (newPass.length < 8) { mostrarToast('Mínimo 8 caracteres.', 'aviso'); return; }
        if (newPass !== confirmPass) { mostrarToast('Las contraseñas no coinciden.', 'aviso'); return; }
        const name  = document.getElementById('input-name').value.trim();
        const email = document.getElementById('input-email').value.trim();
        const phone = document.getElementById('input-phone').value.trim();
        try {
            await apiFetch(`${API}/api/User/${userId}`, {
                method: 'PUT',
                body: JSON.stringify({ name, email, phone, password: newPass }),
            });
            mostrarToast('Contraseña actualizada.', 'exito');
            cerrarModalPassword();
        } catch (e) {
            mostrarToast('Error: ' + e.message, 'error');
        }
    });

    function cerrarModalPassword() {
        cerrarModal('modal-password');
        document.getElementById('password-new').value = '';
        document.getElementById('password-confirm').value = '';
    }

    ['password-new', 'password-confirm'].forEach(id => {
        const btn = document.getElementById(`btn-ver-${id}`);
        if (!btn) return;
        btn.addEventListener('click', () => {
            const input = document.getElementById(id);
            input.type = input.type === 'text' ? 'password' : 'text';
            btn.querySelector('.material-symbols-outlined').textContent =
                input.type === 'text' ? 'visibility_off' : 'visibility';
        });
    });

    // ── Vehículos ───────────────────────────────

    async function cargarVehiculos() {
        const cont = document.getElementById('vehicles-list');
        cont.innerHTML = `<div class="col-span-2 flex justify-center py-4"><span class="w-6 h-6 border-2 border-primario border-t-transparent rounded-full animate-spin"></span></div>`;
        try {
            const cars = await apiFetch(`${API}/api/Car/MyCars`);
            renderVehiculos(cars);
        } catch (e) {
            cont.innerHTML = `<p class="text-sm text-[var(--texto-suave)] col-span-2">No se pudieron cargar los vehículos.</p>`;
        }
    }

    function renderVehiculos(cars) {
        const cont = document.getElementById('vehicles-list');
        if (!cars || cars.length === 0) {
            cont.innerHTML = `<p class="text-sm text-[var(--texto-suave)] col-span-2 italic">Sin vehículos registrados. Añade uno pulsando el botón.</p>`;
            return;
        }
        cont.innerHTML = cars.map(c => `
            <div class="flex items-center justify-between gap-3 p-4 rounded-xl border border-[var(--borde)] bg-gray-50/50 dark:bg-[#161b2e]/40 group">
                <div class="flex items-center gap-3 min-w-0">
                    <div class="w-10 h-10 rounded-lg bg-primario/10 text-primario flex items-center justify-center flex-shrink-0">
                        <span class="material-symbols-outlined text-lg">${c.electricVehicle ? 'electric_car' : c.largeVehicle ? 'local_shipping' : 'directions_car'}</span>
                    </div>
                    <div class="min-w-0">
                        <p class="font-bold text-sm truncate">${c.name}</p>
                        <div class="flex items-center gap-1.5 mt-0.5 flex-wrap">
                            <span class="font-mono text-xs text-[var(--texto-suave)] uppercase">${c.matricule}</span>
                            ${c.electricVehicle ? `<span class="badge badge-verde text-[10px]">Eléctrico</span>` : ''}
                            ${c.largeVehicle    ? `<span class="badge badge-naranja text-[10px]">Grande</span>`  : ''}
                        </div>
                    </div>
                </div>
                <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0">
                    <button onclick="editarVehiculo(${c.id})" title="Editar"
                        class="w-8 h-8 rounded-lg flex items-center justify-center text-[var(--texto-suave)] hover:bg-primario/10 hover:text-primario transition-colors">
                        <span class="material-symbols-outlined text-base">edit</span>
                    </button>
                    <button onclick="eliminarVehiculo(${c.id})" title="Eliminar"
                        class="w-8 h-8 rounded-lg flex items-center justify-center text-[var(--texto-suave)] hover:bg-red-50 dark:hover:bg-red-900/20 hover:text-[var(--rojo)] transition-colors">
                        <span class="material-symbols-outlined text-base">delete</span>
                    </button>
                </div>
            </div>`).join('');
    }

    document.getElementById('btn-add-vehicle')?.addEventListener('click', () => {
        limpiarModalVehiculo();
        document.getElementById('modal-vehicle-title').textContent = 'Añadir Vehículo';
        delete document.getElementById('modal-vehicle').dataset.editId;
        abrirModal('modal-vehicle');
    });

    document.getElementById('btn-submit-vehicle')?.addEventListener('click', async () => {
        const modal       = document.getElementById('modal-vehicle');
        const editId      = modal.dataset.editId;
        const brand       = document.getElementById('vehicle-brand').value.trim();
        const model       = document.getElementById('vehicle-model').value.trim();
        const name        = [brand, model].filter(Boolean).join(' ');
        const matricule   = document.getElementById('vehicle-plate').value.trim().toUpperCase();
        const largeVehicle    = document.getElementById('vehicle-is-large').checked;
        const electricVehicle = document.getElementById('vehicle-is-electric').checked;
        if (!name || !matricule) { mostrarToast('Marca, modelo y matrícula son obligatorios.', 'aviso'); return; }
        const body = JSON.stringify({ name, matricule, largeVehicle, electricVehicle });
        try {
            if (editId) {
                await apiFetch(`${API}/api/Car/${editId}`, { method: 'PUT', body });
                mostrarToast('Vehículo actualizado.', 'exito');
            } else {
                await apiFetch(`${API}/api/Car`, { method: 'POST', body });
                mostrarToast('Vehículo añadido.', 'exito');
            }
            cerrarModal('modal-vehicle');
            await cargarVehiculos();
        } catch (e) {
            mostrarToast('Error: ' + e.message, 'error');
        }
    });

    document.getElementById('btn-cancel-vehicle')?.addEventListener('click', () => cerrarModal('modal-vehicle'));

    function limpiarModalVehiculo() {
        ['vehicle-plate', 'vehicle-brand', 'vehicle-model'].forEach(id => document.getElementById(id).value = '');
        document.getElementById('vehicle-is-large').checked    = false;
        document.getElementById('vehicle-is-electric').checked = false;
    }

    window.editarVehiculo = async function(id) {
        try {
            const car = await apiFetch(`${API}/api/Car/${id}`);
            const partes = (car.name || '').split(' ');
            document.getElementById('vehicle-brand').value  = partes[0] || '';
            document.getElementById('vehicle-model').value  = partes.slice(1).join(' ') || '';
            document.getElementById('vehicle-plate').value  = car.matricule || '';
            document.getElementById('vehicle-is-large').checked    = car.largeVehicle    || false;
            document.getElementById('vehicle-is-electric').checked = car.electricVehicle || false;
            document.getElementById('modal-vehicle-title').textContent = 'Editar Vehículo';
            document.getElementById('modal-vehicle').dataset.editId = id;
            abrirModal('modal-vehicle');
        } catch (e) {
            mostrarToast('No se pudo cargar el vehículo.', 'error');
        }
    };

    window.eliminarVehiculo = async function(id) {
        if (!confirm('¿Eliminar este vehículo?')) return;
        try {
            await apiFetch(`${API}/api/Car/${id}`, { method: 'DELETE' });
            mostrarToast('Vehículo eliminado.', 'exito');
            await cargarVehiculos();
        } catch (e) {
            mostrarToast('Error al eliminar: ' + e.message, 'error');
        }
    };

    // ── Métodos de pago ─────────────────────────

    async function cargarMetodosPago() {
        if (!userId) return;
        const cont = document.getElementById('payment-methods-list');
        cont.innerHTML = `<div class="col-span-2 flex justify-center py-4"><span class="w-6 h-6 border-2 border-primario border-t-transparent rounded-full animate-spin"></span></div>`;
        try {
            const methods = await apiFetch(`${API}/api/PaymentMethod/user/${userId}`);
            renderMetodosPago(methods);
        } catch (e) {
            cont.innerHTML = `<p class="text-sm text-[var(--texto-suave)] col-span-2">No se pudieron cargar los métodos de pago.</p>`;
        }
    }

    function renderMetodosPago(methods) {
        const cont = document.getElementById('payment-methods-list');
        if (!methods || methods.length === 0) {
            cont.innerHTML = `<p class="text-sm text-[var(--texto-suave)] col-span-2 italic">Sin tarjetas guardadas. Añade una pulsando el botón.</p>`;
            return;
        }
        cont.innerHTML = methods.map(m => `
            <div class="flex items-center justify-between gap-3 p-4 rounded-xl border border-[var(--borde)] bg-gray-50/50 dark:bg-[#161b2e]/40 group">
                <div class="flex items-center gap-3 min-w-0">
                    <div class="w-10 h-10 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400 flex items-center justify-center flex-shrink-0">
                        <span class="material-symbols-outlined">credit_card</span>
                    </div>
                    <div class="min-w-0">
                        <p class="font-bold text-sm">${m.cadType} •••• ${m.lastFourDigits}</p>
                        <p class="text-xs text-[var(--texto-suave)] mt-0.5">${m.holderName} · Exp. ${m.expiryDate}</p>
                    </div>
                </div>
                <button onclick="eliminarMetodoPago(${m.id})" title="Eliminar"
                    class="w-8 h-8 rounded-lg flex items-center justify-center text-[var(--texto-suave)] hover:bg-red-50 dark:hover:bg-red-900/20 hover:text-[var(--rojo)] transition-colors opacity-0 group-hover:opacity-100 flex-shrink-0">
                    <span class="material-symbols-outlined text-base">delete</span>
                </button>
            </div>`).join('');
    }

    document.getElementById('btn-add-payment')?.addEventListener('click', () => {
        limpiarModalPago();
        abrirModal('modal-payment');
    });

    document.getElementById('btn-submit-payment')?.addEventListener('click', async () => {
        if (!userId) return;
        const holderName     = document.getElementById('card-holder').value.trim().toUpperCase();
        const cardNumber     = document.getElementById('card-number').value.replace(/\s/g, '');
        const expiryDate     = document.getElementById('card-expiry').value.trim();
        const cadType        = detectarTipoTarjeta(cardNumber);
        const lastFourDigits = cardNumber.slice(-4);
        if (!holderName || cardNumber.length < 13 || !expiryDate) {
            mostrarToast('Completa todos los campos de la tarjeta.', 'aviso'); return;
        }
        if (!/^(0[1-9]|1[0-2])\/\d{2}$/.test(expiryDate)) {
            mostrarToast('Formato de fecha inválido. Usa MM/YY.', 'aviso'); return;
        }
        try {
            await apiFetch(`${API}/api/PaymentMethod`, {
                method: 'POST',
                body: JSON.stringify({ userId, cadType, lastFourDigits, holderName, expiryDate }),
            });
            mostrarToast('Tarjeta añadida correctamente.', 'exito');
            cerrarModal('modal-payment');
            await cargarMetodosPago();
        } catch (e) {
            mostrarToast('Error: ' + e.message, 'error');
        }
    });

    document.getElementById('btn-cancel-payment')?.addEventListener('click', () => cerrarModal('modal-payment'));

    function limpiarModalPago() {
        ['card-holder', 'card-number', 'card-expiry', 'card-cvv'].forEach(id => document.getElementById(id).value = '');
    }

    function detectarTipoTarjeta(numero) {
        if (/^4/.test(numero))      return 'Visa';
        if (/^5[1-5]/.test(numero)) return 'Mastercard';
        if (/^3[47]/.test(numero))  return 'Amex';
        return 'Tarjeta';
    }

    document.getElementById('card-number')?.addEventListener('input', e => {
        let v = e.target.value.replace(/\D/g, '').substring(0, 16);
        e.target.value = v.match(/.{1,4}/g)?.join(' ') || v;
    });

    document.getElementById('card-expiry')?.addEventListener('input', e => {
        let v = e.target.value.replace(/\D/g, '').substring(0, 4);
        if (v.length >= 3) v = v.substring(0, 2) + '/' + v.substring(2);
        e.target.value = v;
    });

    window.eliminarMetodoPago = async function(id) {
        if (!confirm('¿Eliminar esta tarjeta?')) return;
        try {
            await apiFetch(`${API}/api/PaymentMethod/${id}`, { method: 'DELETE' });
            mostrarToast('Tarjeta eliminada.', 'exito');
            await cargarMetodosPago();
        } catch (e) {
            mostrarToast('Error al eliminar: ' + e.message, 'error');
        }
    };

    // ── Cerrar sesión ───────────────────────────
    document.getElementById('btn-logout')?.addEventListener('click', () => AUTH.cerrarSesion(true));

    document.addEventListener('DOMContentLoaded', init);
})();