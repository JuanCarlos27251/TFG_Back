/* ==============================================
   PARKit — Confirmación y Resumen de Pago
   ============================================== */

const API_BASE = window.CONFIG?.API_BASE || 'https://localhost:7033';

document.addEventListener('DOMContentLoaded', () => {
    const raw = localStorage.getItem('confirmedReservation');
    if (!raw) return;
    const d = JSON.parse(raw);

    // 1. Mostrar Datos
    setText('conf-booking-id', `#PK-${String(d.reservationId).padStart(6, '0')}`);
    setText('conf-parking-name', d.parkingName);
    setText('conf-spot-detail', d.spotNumber ? `Plaza nº ${d.spotNumber}` : 'Plaza asignada');
    
    const start = new Date(d.startTime);
    const end = new Date(d.endTime);
    setText('conf-date', start.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' }));
    setText('conf-time-range', `${start.getHours()}:${String(start.getMinutes()).padStart(2,'0')} → ${end.getHours()}:${String(end.getMinutes()).padStart(2,'0')} (Estancia finalizada)`);

    setText('conf-base-price', `${parseFloat(d.baseFee).toFixed(2)} €`);
    setText('conf-service-fee', `${parseFloat(d.serviceFee).toFixed(2)} €`);
    setText('conf-total-price', `${parseFloat(d.totalAmount).toFixed(2)} €`);
    setText('conf-payment-info', `Pagado con Ref. ${d.paymentMethodId}`);

    // 2. Configurar Botones Navegación
    document.getElementById('conf-receipt-link')?.addEventListener('click', (e) => {
        e.preventDefault();
        window.open('recibo.html', '_blank');
    });

    document.getElementById('conf-map-link')?.addEventListener('click', (e) => {
        e.preventDefault();
        localStorage.setItem('pendingRoute', JSON.stringify({
            destLat: d.parkingLat, destLng: d.parkingLng, destName: d.parkingName, trazar: true
        }));
        window.location.href = 'map.html';
    });

    // 3. Cancelación
    document.getElementById('btn-cancel-reservation')?.addEventListener('click', async () => {
        const fee = parseFloat(d.cancellationFee || 0);
        if (!confirm(`¿Deseas cancelar esta reserva?\nPenalización: ${fee.toFixed(2)} €`)) return;

        try {
            //  Llamada al endpoint correcto de cancelación
            const resp = await fetch(`${API_BASE}/api/Reservation/${d.reservationId}/cancel`, {
                method: 'PUT',
                headers: AUTH.cabecerasAuth()
            });

            if (!resp.ok) throw new Error("No se pudo cancelar en el servidor.");

            if (fee > 0) {
                await fetch(`${API_BASE}/api/Payments`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', ...AUTH.cabecerasAuth() },
                    body: JSON.stringify({
                        reservationId: d.reservationId,
                        amount: fee,
                        status: 1,
                        currency: "EUR",
                        externalTransactionId: `CANCEL_FEE_${d.reservationId}`
                    })
                });
            }

            alert("Reserva cancelada correctamente.");
            window.location.href = 'reservas.html';
        } catch (e) {
            alert("Error: " + e.message);
        }
    });

    // Aseguramos que el panel de timer de esta vista esté oculto
    document.getElementById('usage-controls')?.classList.add('hidden');
});

function setText(id, v) { const e = document.getElementById(id); if (e) e.textContent = v; }
