/* ==============================================
   PARKit — Checkout de Pagos (Modificado: Timer antes de pagar)
   ============================================== */

(() => {
    const API = AUTH.API_BASE;

    let parkingActivo = null;
    let vehiculosGlobal = [];
    let metodosPagoGlobal = [];
    
    let cocheSeleccionado = null;
    let metodoPagoSeleccionadoId = null;

    // Timer logic
    let timerInterval = null;
    let timerStartMs  = null;
    let estanciaMinutosFinal = 0;

    const reserva = {
        modo: 0, // 0 = Fijo, 1 = Tiempo Real
        horasReserva: 1,
        gastosGestion: 1.50
    };

    async function apiFetch(url, opciones = {}) {
        const resp = await fetch(url, { headers: AUTH.cabecerasAuth(), ...opciones });
        if (!resp.ok) throw new Error(`Error HTTP: ${resp.status}`);
        return resp.status === 244 ? null : resp.json();
    }

    async function initPagos() {
        if (!AUTH.estaAutenticado() || AUTH.esEmpresa()) {
            window.location.href = 'login.html';
            return;
        }

        const params = new URLSearchParams(window.location.search);
        const pid = params.get('parkingId');

        if (!pid) {
            window.location.href = 'map.html';
            return;
        }

        await cargarDatosParking(pid);
        await Promise.all([cargarCochesUsuario(), cargarMetodosPagoUsuario(AUTH.obtenerUsuario()?.id)]);

        setupEventosInteractivos();
        actualizarResumenMatematico();
    }

    async function cargarDatosParking(id) {
        try {
            parkingActivo = await apiFetch(`${API}/api/Parking/${id}`);
            document.getElementById('summary-parking-name').textContent = parkingActivo.name;
            document.getElementById('summary-parking-address').innerHTML = `<span class="material-symbols-outlined text-base text-primario">location_on</span> ${parkingActivo.address}`;
            if (parkingActivo.imageUrl) document.getElementById('parking-preview-img').style.backgroundImage = `url('${parkingActivo.imageUrl}')`;
        } catch (e) { mostrarToast("Error cargando parking.", "error"); }
    }

    async function cargarCochesUsuario() {
        try {
            vehiculosGlobal = await apiFetch(`${API}/api/Car/MyCars`);
            const cont = document.getElementById('vehicles-list');
            cont.innerHTML = '';
            if (!vehiculosGlobal || vehiculosGlobal.length === 0) {
                cont.innerHTML = `<p class="text-sm text-red-500 font-bold p-3">No tienes vehículos registrados.</p>`;
                document.getElementById('btn-complete-booking').disabled = true;
                return;
            }
            cocheSeleccionado = vehiculosGlobal[0];
            vehiculosGlobal.forEach((car, i) => {
                const label = document.createElement('label');
                label.className = `flex items-center justify-between p-4 border rounded-xl cursor-pointer transition-all ${i===0?'border-primario bg-primario/5':'border-[var(--borde)]'}`;
                label.innerHTML = `<div><p class="text-sm font-bold">${car.matricule}</p><p class="text-xs text-gray-500">${car.name}</p></div><input type="radio" name="car" ${i===0?'checked':''}>`;
                label.onclick = () => { cocheSeleccionado = car; actualizarResumenMatematico(); };
                cont.appendChild(label);
            });
        } catch (e) { console.error(e); }
    }

    async function cargarMetodosPagoUsuario(uid) {
        try {
            metodosPagoGlobal = await apiFetch(`${API}/api/PaymentMethod/user/${uid}`);
            const cont = document.getElementById('payment-methods-list');
            cont.innerHTML = '';
            if (!metodosPagoGlobal?.length) return;
            metodoPagoSeleccionadoId = metodosPagoGlobal[0].id;
            metodosPagoGlobal.forEach((p, i) => {
                const label = document.createElement('label');
                label.className = `flex items-center justify-between p-4 border rounded-xl cursor-pointer ${i===0?'border-indigo-500 bg-indigo-50':''}`;
                label.innerHTML = `<div><p class="text-sm font-bold">•••• ${p.lastFourDigits}</p></div><input type="radio" name="pay" ${i===0?'checked':''}>`;
                label.onclick = () => metodoPagoSeleccionadoId = p.id;
                cont.appendChild(label);
            });
        } catch (e) { console.error(e); }
    }

    function setupEventosInteractivos() {
        // Fechas
        const inF = document.getElementById('input-arrival-date');
        const inH = document.getElementById('input-arrival-time');
        const hoy = new Date();
        inF.value = hoy.toISOString().split('T')[0];
        inH.value = `${String(hoy.getHours()).padStart(2,'0')}:${String(hoy.getMinutes()).padStart(2,'0')}`;
        [inF, inH].forEach(el => el?.addEventListener('change', actualizarResumenMatematico));

        // Radios de modo (Fijo vs Tiempo Real)
        document.querySelectorAll('input[name="reservation-type"]').forEach(r => {
            r.addEventListener('change', (e) => {
                reserva.modo = parseInt(e.target.value);
                const isFijo = reserva.modo === 0;
                document.getElementById('fixed-time-selector').classList.toggle('hidden', !isFijo);
                document.getElementById('dynamic-time-controls').classList.toggle('hidden', isFijo);
                document.getElementById('btn-complete-booking').disabled = !isFijo;
                actualizarResumenMatematico();
            });
        });

        // Duración (Botones)
        document.querySelectorAll('.duration-btn').forEach(btn => {
            btn.onclick = () => {
                document.querySelectorAll('.duration-btn').forEach(b => b.classList.remove('active','bg-primario/10','border-primario','text-primario'));
                btn.classList.add('active','bg-primario/10','border-primario','text-primario');
                reserva.horasReserva = parseInt(btn.dataset.hours);
                actualizarResumenMatematico();
            };
        });

        // Cronómetro
        document.getElementById('btn-start-timer')?.addEventListener('click', () => {
            timerStartMs = Date.now();
            timerInterval = setInterval(() => {
                const s = Math.floor((Date.now() - timerStartMs)/1000);
                document.getElementById('timer-display').textContent = `⏱ ${Math.floor(s/3600).toString().padStart(2,'0')}:${Math.floor((s%3600)/60).toString().padStart(2,'0')}:${(s%60).toString().padStart(2,'0')}`;
            }, 1000);
            document.getElementById('btn-start-timer').classList.add('hidden');
            document.getElementById('btn-stop-timer').classList.remove('hidden');
        });

        document.getElementById('btn-stop-timer')?.addEventListener('click', () => {
            clearInterval(timerInterval);
            estanciaMinutosFinal = Math.ceil((Date.now() - timerStartMs) / 60000);
            document.getElementById('btn-stop-timer').classList.add('hidden');
            document.getElementById('btn-complete-booking').disabled = false;
            actualizarResumenMatematico();
        });

        document.getElementById('btn-complete-booking').onclick = procesarPagoFinal;
    }

    function actualizarResumenMatematico() {
        if (!parkingActivo) return;
        const tarifa = parkingActivo.tarifs?.[0];
        if (!tarifa) return;

        let extra = 0;
        if (cocheSeleccionado?.largeVehicle) extra += Number(tarifa.largeVehicleSurcharge);
        if (cocheSeleccionado?.electricVehicle) extra += Number(tarifa.electricVehicleSurcharge);
        const precioH = Number(tarifa.pricePerHour) + extra;

        let estancia = 0;
        let suplReserva = 0;

        if (reserva.modo === 0) {
            estancia = reserva.horasReserva * precioH;
            suplReserva = Number(tarifa.reservationSurcharge || 0);
            const inF = document.getElementById('input-arrival-date').value;
            const inH = document.getElementById('input-arrival-time').value;
            document.getElementById('summary-checkin').textContent = `${inF} ${inH}`;
            const outD = new Date(new Date(`${inF}T${inH}`).getTime() + (reserva.horasReserva * 3600000));
            document.getElementById('summary-checkout').textContent = `${outD.toLocaleDateString()} ${outD.getHours()}:${String(outD.getMinutes()).padStart(2,'0')}`;
        } else {
            estancia = (estanciaMinutosFinal / 60) * precioH;
            document.getElementById('summary-checkin').textContent = "Inicio Tiempo Real";
            document.getElementById('summary-checkout').textContent = estanciaMinutosFinal > 0 ? "Fin Tiempo Real" : "En curso...";
        }

        const total = estancia + reserva.gastosGestion + suplReserva;
        document.getElementById('summary-base-price').textContent = `${estancia.toFixed(2)} €`;
        document.getElementById('summary-total').textContent = `${total.toFixed(2)} €`;
        document.getElementById('summary-total-btn').textContent = `${total.toFixed(2)} €`;
    }

    async function procesarPagoFinal() {
        const btn = document.getElementById('btn-complete-booking');
        btn.disabled = true;
        btn.textContent = "Procesando...";

        try {
            const tarifa = parkingActivo.tarifs?.[0];
            const extra = (cocheSeleccionado?.largeVehicle ? Number(tarifa.largeVehicleSurcharge) : 0) + (cocheSeleccionado?.electricVehicle ? Number(tarifa.electricVehicleSurcharge) : 0);
            const precioH = Number(tarifa.pricePerHour) + extra;
            const suplReserva = reserva.modo === 0 ? Number(tarifa.reservationSurcharge || 0) : 0;
            
            const totalEstancia = reserva.modo === 0 ? (reserva.horasReserva * precioH) : ((estanciaMinutosFinal / 60) * precioH);
            const totalPagar = totalEstancia + reserva.gastosGestion + suplReserva;

            // 1. Crear Reserva en Backend
            const start = reserva.modo === 0 ? new Date(`${document.getElementById('input-arrival-date').value}T${document.getElementById('input-arrival-time').value}`) : new Date(timerStartMs);
            const end = reserva.modo === 0 ? new Date(start.getTime() + (reserva.horasReserva * 3600000)) : new Date();
            const timezoneOffsetStart = start.getTimezoneOffset() * 60000;
            const timezoneOffsetEnd = end.getTimezoneOffset() * 60000;
            
            const startLocal = new Date(start.getTime() - timezoneOffsetStart).toISOString().slice(0, -1);
            const endLocal = new Date(end.getTime() - timezoneOffsetEnd).toISOString().slice(0, -1);
            const resCreada = await apiFetch(`${API}/api/Reservation`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
                body: JSON.stringify({
                    userId: AUTH.obtenerUsuario().id,
                    parkingSpotId: parkingActivo.spots?.[0]?.id,
                    startTime: startLocal, // Usamos la hora de España
                    endTime: endLocal,     // Usamos la hora de España
                    carId: cocheSeleccionado.id,
                    status: 0 // 0 = Pendiente
                })
            });
            // 2. Crear Pago
            const pagoCreado = await apiFetch(`${API}/api/Payments`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
                body: JSON.stringify({
                    reservationId: resCreada.id,
                    amount: totalPagar,
                    status: 1,
                    currency: "EUR",
                    externalTransactionId: `CARD_${metodoPagoSeleccionadoId}`
                })
            });

            await apiFetch(`${API}/api/Payments/${pagoCreado.id}/confirm?transactionId=${pagoCreado.externalTransactionId}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() }
            });

            // 3. Guardar para confirmación
            localStorage.setItem('confirmedReservation', JSON.stringify({
                reservationId: resCreada.id,
                parkingName: parkingActivo.name,
                parkingAddress: parkingActivo.address,
                spotNumber: resCreada.spotNumber || 'Asignada',
                startTime: start.toISOString(),
                endTime: end.toISOString(),
                baseFee: totalEstancia,
                serviceFee: reserva.gastosGestion,
                reservationSurcharge: suplReserva,
                cancellationFee: Number(tarifa.cancellationFee || 0),
                totalAmount: totalPagar,
                paymentMethodId: metodoPagoSeleccionadoId,
                parkingLat: parkingActivo.latitude,
                parkingLng: parkingActivo.longitude,
                timestamp: new Date().toISOString()
            }));

            mostrarToast("Pago realizado con éxito");
            setTimeout(() => window.location.href = "confirmacionpago.html", 1500);
        } catch (e) {
            mostrarToast("Error en el pago: " + e.message, "error");
            btn.disabled = false;
        }
    }

    document.addEventListener('DOMContentLoaded', initPagos);
})();
