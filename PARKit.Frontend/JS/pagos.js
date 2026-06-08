/* ==============================================
   PARKit — Checkout de Pagos (Validación Server-Side)
   ============================================== */

(() => {
    const API = AUTH.API_BASE;

    let parkingActivo = null;
    let vehiculosGlobal = [];
    let metodosPagoGlobal = [];
    
    let cocheSeleccionado = null;
    let metodoPagoSeleccionadoId = null;

    const reserva = {
        modo: 0, 
        horasReserva: 1,
        gastosGestion: 1.50, // Gastos PARKit
        precioFinalHora: 0 
    };

    function mostrarToast(msg, tipo = 'exito') {
        const cont = document.getElementById('contenedor-toast');
        if (!cont) return;
        const toast = document.createElement('div');
        const colores = { exito: 'verde', error: 'rojo', aviso: 'naranja' };
        toast.className = `toast toast-${tipo}`;
        toast.innerHTML = `
            <span class="material-symbols-outlined icono-relleno mb-0.5" style="color:var(--${colores[tipo]||'azul'})">${tipo === 'exito' ? 'check_circle' : (tipo === 'rojo' ? 'error' : 'warning')}</span>
            <span class="text-sm font-medium pr-4">${msg}</span>`;
        cont.appendChild(toast);
        setTimeout(() => toast.remove(), 4000);
    }

    async function apiFetch(url, opciones = {}) {
        const resp = await fetch(url, { headers: AUTH.cabecerasAuth(), ...opciones });
        if (!resp.ok) throw new Error(`Error HTTP: ${resp.status}`);
        if (resp.status === 204) return null;
        return resp.json();
    }

    async function initPagos() {
        if (!AUTH.estaAutenticado() || AUTH.esEmpresa()) {
            window.location.href = 'login.html';
            return;
        }

        const params = new URLSearchParams(window.location.search);
        const pid = params.get('parkingId');

        if (!pid) {
            alert("No se seleccionó parking.");
            window.location.href = 'map.html';
            return;
        }

        const user = AUTH.obtenerUsuario();

        await Promise.all([
            cargarDatosParking(pid),
            cargarCochesUsuario(),
            cargarMetodosPagoUsuario(user?.id)
        ]);

        setupEventosInteractivos();
        actualizarResumenMatematico(); 
    }

    async function cargarDatosParking(id) {
        try {
            parkingActivo = await apiFetch(`${API}/api/Parking/${id}`);
            document.getElementById('summary-parking-name').textContent = parkingActivo.name;
            document.getElementById('summary-parking-address').innerHTML = `<span class="material-symbols-outlined text-base text-azul">location_on</span> ${parkingActivo.address}`;
            if (parkingActivo.imageUrl) {
                document.getElementById('parking-preview-img').style.backgroundImage = `url('${parkingActivo.imageUrl}')`;
            }
        } catch (error) {
            mostrarToast("Error cargando parking.", "error");
        }
    }

    async function cargarCochesUsuario() {
        const contenedor = document.getElementById('vehicles-list');
        try {
            vehiculosGlobal = await apiFetch(`${API}/api/Car/MyCars`);
            contenedor.innerHTML = '';
            
            if (!vehiculosGlobal || vehiculosGlobal.length === 0) {
                contenedor.innerHTML = `<p class="text-sm text-[var(--rojo)] bg-red-50 p-3 rounded font-bold border border-red-200">No tienes vehículos registrados. Añade uno en tu perfil.</p>`;
                document.getElementById('btn-complete-booking').disabled = true;
                return;
            }

            cocheSeleccionado = vehiculosGlobal[0];

            vehiculosGlobal.forEach((car, index) => {
                const isSelected = index === 0;
                const badges = [];
                if (car.electricVehicle) badges.push('<span class="bg-[var(--verde)]/10 text-[var(--verde)] px-2 py-0.5 rounded text-[10px] font-black uppercase">Eléctrico</span>');
                if (car.largeVehicle) badges.push('<span class="bg-[var(--naranja)]/10 text-[var(--naranja)] px-2 py-0.5 rounded text-[10px] font-black uppercase">Grande</span>');

                const label = document.createElement('label');
                label.className = `flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all ${isSelected ? 'border-azul bg-azul/5' : 'border-[var(--borde)] hover:bg-[#f5f6fa]'}`;
                label.innerHTML = `
                    <div class="flex items-center gap-3">
                        <div class="w-10 h-10 rounded bg-[#f5f6fa] dark:bg-[#161b2e] flex items-center justify-center font-bold text-xs uppercase border border-[var(--borde)]">
                            ${car.matricule.substring(0,3)}
                        </div>
                        <div>
                            <p class="text-sm font-bold uppercase tracking-wider text-[#111318] dark:text-white">${car.matricule} <span class="pl-2 flex gap-1 mt-1">${badges.join(' ')}</span></p>
                            <p class="text-xs text-[var(--texto-suave)]">${car.name}</p>
                        </div>
                    </div>
                    <input type="radio" name="car_selection" value="${car.id}" class="w-4 h-4 text-azul focus:ring-azul" ${isSelected ? 'checked' : ''}>
                `;

                label.querySelector('input').addEventListener('change', () => {
                    cocheSeleccionado = car; 
                    contenedor.querySelectorAll('label').forEach(l => l.className = 'flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all border-[var(--borde)] hover:bg-[#f5f6fa]');
                    label.className = 'flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all border-azul bg-azul/5';
                    actualizarResumenMatematico(); 
                });

                contenedor.appendChild(label);
            });
        } catch (error) {
            contenedor.innerHTML = `<p class="text-[var(--rojo)] font-bold text-sm">Error conectando con tus vehículos.</p>`;
        }
    }

    async function cargarMetodosPagoUsuario(userId) {
        const contenedor = document.getElementById('payment-methods-list');
        if(!userId) return;

        try {
            metodosPagoGlobal = await apiFetch(`${API}/api/PaymentMethod/user/${userId}`);
            contenedor.innerHTML = '';
            
            if (!metodosPagoGlobal || metodosPagoGlobal.length === 0) {
                contenedor.innerHTML = `<p class="text-sm text-[var(--rojo)] bg-red-50 p-3 rounded font-bold border border-red-200">No tienes tarjetas en tu perfil. Registra una para pagar.</p>`;
                document.getElementById('btn-complete-booking').disabled = true;
                return;
            }

            metodoPagoSeleccionadoId = metodosPagoGlobal[0].id;

            metodosPagoGlobal.forEach((pago, index) => {
                const isSelected = index === 0;
                let colorTarjeta = pago.cadType?.toLowerCase() === 'visa' ? 'bg-indigo-600' : 'bg-red-500';

                const label = document.createElement('label');
                label.className = `flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all ${isSelected ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20' : 'border-[var(--borde)] hover:bg-[#f5f6fa]'}`;
                label.innerHTML = `
                    <div class="flex items-center gap-3">
                        <div class="w-10 h-6 rounded ${colorTarjeta} text-white flex items-center justify-center font-bold text-[9px] italic border shadow-sm">
                            ${pago.cadType || 'CARD'}
                        </div>
                        <div>
                            <p class="text-sm font-bold uppercase text-[#111318] dark:text-white">•••• ${pago.lastFourDigits || '0000'}</p>
                            <p class="text-[10px] text-[var(--texto-suave)]">Expira: ${pago.expiryDate || '12/28'}</p>
                        </div>
                    </div>
                    <input type="radio" name="payment_selection" value="${pago.id}" class="w-4 h-4 text-indigo-500 focus:ring-indigo-500" ${isSelected ? 'checked' : ''}>
                `;

                label.querySelector('input').addEventListener('change', () => {
                    metodoPagoSeleccionadoId = pago.id;
                    contenedor.querySelectorAll('label').forEach(l => l.className = 'flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all border-[var(--borde)] hover:bg-[#f5f6fa]');
                    label.className = 'flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20';
                });

                contenedor.appendChild(label);
            });
        } catch (error) {
             contenedor.innerHTML = `<p class="text-[var(--rojo)] text-sm font-bold border border-red-200 bg-red-50 p-3 rounded">Falló la red al buscar tus tarjetas.</p>`;
        }
    }


    function setupEventosInteractivos() {
        const radiosModo = document.querySelectorAll('input[name="reservation-type"]');
        radiosModo.forEach(radio => {
            radio.addEventListener('change', (e) => {
                reserva.modo = parseInt(e.target.value);
                
                if (reserva.modo === 0) {
                    document.getElementById('label-type-fixed').classList.replace('border-[var(--borde)]', 'border-azul');
                    document.getElementById('label-type-fixed').classList.replace('hover:bg-[#f5f6fa]', 'bg-azul/5');
                    document.getElementById('icon-type-fixed').textContent = 'check_circle';
                    
                    document.getElementById('label-type-dynamic').classList.replace('border-azul', 'border-[var(--borde)]');
                    document.getElementById('label-type-dynamic').classList.replace('bg-azul/5', 'hover:bg-[#f5f6fa]');
                    document.getElementById('icon-type-dynamic').textContent = 'circle';
                    
                    document.getElementById('duration-selector').style.opacity = '1';
                    document.getElementById('duration-selector').style.pointerEvents = 'auto';
                } else {
                    document.getElementById('label-type-dynamic').classList.replace('border-[var(--borde)]', 'border-azul');
                    document.getElementById('label-type-dynamic').classList.replace('hover:bg-[#f5f6fa]', 'bg-azul/5');
                    document.getElementById('icon-type-dynamic').textContent = 'check_circle';
                    
                    document.getElementById('label-type-fixed').classList.replace('border-azul', 'border-[var(--borde)]');
                    document.getElementById('label-type-fixed').classList.replace('bg-azul/5', 'hover:bg-[#f5f6fa]');
                    document.getElementById('icon-type-fixed').textContent = 'circle';
                    
                    document.getElementById('duration-selector').style.opacity = '0.3';
                    document.getElementById('duration-selector').style.pointerEvents = 'none';
                }
                actualizarResumenMatematico();
            });
        });

        const btnHoras = document.querySelectorAll('.duration-btn');
        btnHoras.forEach(btn => {
            btn.addEventListener('click', () => {
                btnHoras.forEach(b => b.classList.remove('active', 'bg-azul/10', 'border-azul', 'text-azul'));
                btn.classList.add('active', 'bg-azul/10', 'border-azul', 'text-azul');
                reserva.horasReserva = parseInt(btn.dataset.hours);
                actualizarResumenMatematico();
            });
        });

        const inputLlegada = document.getElementById('input-arrival-time');
        const ahora = new Date();
        inputLlegada.value = `${ahora.getHours().toString().padStart(2, '0')}:${ahora.getMinutes().toString().padStart(2, '0')}`;
        inputLlegada.addEventListener('change', actualizarResumenMatematico);

        document.getElementById('btn-complete-booking').addEventListener('click', procesarReservaYPagoEnBBDD);
    }

    function actualizarResumenMatematico() {
        if (!parkingActivo) return;

        // Comprobamos si el backend envió la tarifa explícitamente en el JSON
        const tarifaFija = (parkingActivo.tarifs && parkingActivo.tarifs.length > 0) ? parkingActivo.tarifs[0] : null;
        
        // Tiempos
        const horaVal = document.getElementById('input-arrival-time').value;
        const [h, m] = horaVal.split(':').map(Number);
        const dIn = new Date(); dIn.setHours(h, m, 0, 0);

        document.getElementById('summary-checkin').textContent = `${h.toString().padStart(2,'0')}:${m.toString().padStart(2,'0')}`;

        if (reserva.modo === 0) { 
            const dOut = new Date(dIn.getTime() + (reserva.horasReserva * 60*60*1000));
            document.getElementById('summary-checkout').textContent = `${dOut.getHours().toString().padStart(2,'0')}:${dOut.getMinutes().toString().padStart(2,'0')}`;
            
            // Si hay tarifa fija en el frontend, calculamos la previa. Si no, le decimos al usuario que el coste depende de la zona de Zaragoza.
            if (tarifaFija) {
                let precioBase = tarifaFija.pricePerHour;
                let sobreCostes = 0;
                if (cocheSeleccionado && cocheSeleccionado.largeVehicle) sobreCostes += tarifaFija.largeVehicleSurcharge || 0;
                if (cocheSeleccionado && cocheSeleccionado.electricVehicle) sobreCostes += tarifaFija.electricVehicleSurcharge || 0;
                
                const finalHora = precioBase + sobreCostes;
                const costeEstancia = reserva.horasReserva * finalHora;
                
                document.getElementById('summary-base-price').innerHTML = `${costeEstancia.toFixed(2)} € <span class="text-[10px] block opacity-60 font-bold text-azul uppercase tracking-wider">${sobreCostes > 0 ? '+ Extras incl.' : ''}</span>`;
                const total = costeEstancia + reserva.gastosGestion;
                document.getElementById('summary-total').textContent = `${total.toFixed(2)} €`;
                document.getElementById('summary-total-btn').textContent = `${total.toFixed(2)} €`;
            } else {
                // UI PARA APARCAMIENTOS IMPORTADOS / OPACOS (Ej. Zona Azul Zaragoza)
                document.getElementById('summary-base-price').innerHTML = `<span class="text-[12px] font-bold text-naranja">Regulación Municipal</span>`;
                document.getElementById('summary-total').innerHTML = `Sujeto a Zona`;
                document.getElementById('summary-total-btn').textContent = `Autorizar Reserva`;
            }

        } else {
            document.getElementById('summary-checkout').innerHTML = `<span class="text-[var(--naranja)] material-symbols-outlined mt-0.5 text-base animate-spin">refresh</span>`;
            document.getElementById('summary-base-price').innerHTML = `<code class="bg-[var(--borde)] px-2 py-0.5 rounded text-xs">${tarifaFija ? tarifaFija.pricePerHour.toFixed(2) + '€ / h' : 'Tarifa Activa'}</code>`;
            document.getElementById('summary-total').textContent = `En vivo`;
            document.getElementById('summary-total-btn').textContent = `Iniciar Modo En Vivo`;
        }
    }

    // ── INSERCIÓN MAGISTRAL EN BBDD QUE UNIFICA PRECIOS ──
    async function procesarReservaYPagoEnBBDD() {
        if (!parkingActivo || !cocheSeleccionado || !metodoPagoSeleccionadoId) {
            mostrarToast("Faltan datos de Vehículo o Tarjeta seleccionados", "aviso");
            return;
        }

        const btn = document.getElementById('btn-complete-booking');
        btn.disabled = true;
        btn.innerHTML = `<span class="material-symbols-outlined text-xl animate-spin">refresh</span> Reclamando Plaza en Sistema...`;

        try {
            const horaIn = new Date();
            const [h, m] = document.getElementById('input-arrival-time').value.split(':').map(Number);
            horaIn.setHours(h, m, 0, 0);
            
            // Si es Vivo, forzamos fecha final 30 días para evitar cruces
            const horaOut = reserva.modo === 0 ? new Date(horaIn.getTime() + (reserva.horasReserva * 60*60*1000)) : new Date(horaIn.getTime() + (30*24*60*60*1000));

            // Buscador de Plazas Flexibles (Para external data de Zaragoza sin mapping)
            let spotId = 0;
            if (parkingActivo.spots && parkingActivo.spots.length > 0) {
                const disponible = parkingActivo.spots.find(s => s.status === 0);
                spotId = disponible ? disponible.id : parkingActivo.spots[0].id;
            } else {
                throw new Error("El parking (o la zona importada) carece de plazas asignables cargadas localmente.");
            }

            // ================= 1. PEDIMOS LA RESERVA AL BACKEND =================
            const payloadReserva = {
                userId: AUTH.obtenerUsuario().id, 
                parkingSpotId: spotId,
                startTime: horaIn.toISOString(),
                endTime: horaOut.toISOString(),
                carId: cocheSeleccionado.id,
                status: 0 
            };

            const reservaAPI = await fetch(`${API}/api/Reservation`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
                body: JSON.stringify(payloadReserva)
            });

            if (!reservaAPI.ok) throw new Error("Backend rechazó la Reserva (La plaza tal vez esté ocupada).");
            
            // OJO AQUÍ: Extraemos lo que ".NET" determinó que cuesta la estancia (Los 1.8€)
            const reservaCreada = await reservaAPI.json();

            // ================= 2. PAGAMOS EXACTAMENTE LO QUE DICTÓ .NET =================
            btn.innerHTML = `<span class="material-symbols-outlined text-xl animate-spin">price_check</span> Pasando Tarjeta...`;
            
            // Hacemos que Payments = TotalAmount dictado por la Reserva (Así NUNCA descuadran tus tablas en BD)
            const cantidadPagar = reserva.modo === 0 ? reservaCreada.totalAmount : 0.00;
            
            const payloadPago = {
                reservationId: reservaCreada.id, 
                amount: cantidadPagar,
                status: 1, // Confirmado
                currency: "EUR",
                externalTransactionId: `CREDITCARD_${metodoPagoSeleccionadoId}`
            };

            const pagoAPI = await fetch(`${API}/api/Payments`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
                body: JSON.stringify(payloadPago)
            });

            if (!pagoAPI.ok) throw new Error("Reserva Creada PERO la tarjeta de crédito denegó el saldo.");

            mostrarToast("Transacción sellada por el Banco.", "exito");
            btn.classList.replace('bg-azul', 'bg-[var(--verde)]');
            btn.classList.replace('hover:bg-azul-oscuro', 'hover:bg-green-600');
            btn.innerHTML = `<span class="material-symbols-outlined text-xl">check_circle</span> ¡Plaza Pagada y Lista!`;
            
            setTimeout(() => { window.location.href = "reservas.html"; }, 1500);

        } catch (error) {
            console.error(error);
            mostrarToast(error.message, "error");
            btn.disabled = false;
            btn.innerHTML = `<span class="material-symbols-outlined text-xl">lock</span> Reintentar Checkout`;
        }
    }

    document.addEventListener('DOMContentLoaded', initPagos);
})();
