/* ==============================================
   PARKit — Recibo de Compra
   ============================================== */

document.addEventListener('DOMContentLoaded', () => {
    cargarDatosRecibo();
    configurarBotonesRecibo();
});

function cargarDatosRecibo() {
    const raw = localStorage.getItem('confirmedReservation');
    if (!raw) { mostrarErrorRecibo(); return; }
    rellenarRecibo(JSON.parse(raw));
}

function rellenarRecibo(d) {
    const idPad = String(d.reservationId || 0).padStart(5, '0');
    const anio  = new Date().getFullYear();

    // Cabecera
    setText('receipt-invoice-id', `FAC-${anio}-${idPad}`);
    setText('receipt-booking-id', `#PK-${idPad}`);

    // Fecha de emisión
    const fechaEmision = d.timestamp ? new Date(d.timestamp) : new Date();
    setText('receipt-date', fechaEmision.toLocaleDateString('es-ES', {
        day: '2-digit', month: 'short', year: 'numeric'
    }));

    // Método de pago
    setText('receipt-method', `Tarjeta (Ref. ${d.paymentMethodId || '—'})`);

    // Titular
    const usuario = (window.AUTH && AUTH.obtenerUsuario) ? AUTH.obtenerUsuario() : null;
    setText('receipt-client-name', usuario?.name || usuario?.email || 'Cliente PARKit');

    // Descripción del artículo (con fecha y hora correctas)
    setText('receipt-item-title', 'Reserva de Plaza de Aparcamiento');

    const hIn  = d.startTime ? formatFechaHora(d.startTime) : '—';
    const hOut = d.endTime   ? formatFechaHora(d.endTime)   : (d.modo === 1 ? 'Tiempo real' : '—');
    const dur  = (d.startTime && d.endTime) ? ` · ${calcularDuracion(d.startTime, d.endTime)}` : '';

    setHTML('receipt-item-desc',
        `${d.parkingName || 'Parking'} · ${d.parkingAddress || 'Zaragoza'}<br>` +
        `Entrada: ${hIn} — Salida: ${hOut}${dur}`
    );

    // Importes
    const base       = parseFloat(d.baseFee          || 0);
    const gestion    = parseFloat(d.serviceFee        || 1.50);
    const suplemento = parseFloat(d.reservationSurcharge || 0);
    const total      = parseFloat(d.totalAmount       || (base + gestion + suplemento));

    setText('receipt-item-price', `${(base + suplemento).toFixed(2)} €`);
    setText('receipt-fee-price',  `${gestion.toFixed(2)} €`);

    // Fiscalidad (IVA 21% incluido)
    const baseImponible = total / 1.21;
    const iva           = total - baseImponible;
    setText('receipt-subtotal', `${baseImponible.toFixed(2)} €`);
    setText('receipt-tax',      `${iva.toFixed(2)} €`);
    setText('receipt-total',    total.toFixed(2));

    // Código verificación
    const verif = document.querySelector('.font-mono.text-\\[10px\\]');
    if (verif) verif.textContent = `Verificación e-ID *${anio}FAC${idPad}PARK*`;
}

function mostrarErrorRecibo() {
    const main = document.querySelector('main');
    if (!main) return;
    main.innerHTML = `
        <div class="flex flex-col items-center justify-center py-24 gap-4 text-center">
            <span class="material-symbols-outlined text-5xl text-[var(--texto-suave)]">receipt_long</span>
            <p class="font-bold text-lg">No se encontraron datos del recibo</p>
            <a href="map.html" class="btn btn-primario mt-2">Buscar parking</a>
        </div>`;
}

function configurarBotonesRecibo() {
    document.getElementById('btn-print')?.addEventListener('click', () => window.print());
    document.getElementById('btn-download')?.addEventListener('click', () => setTimeout(() => window.print(), 100));
}

// ── Utilidades ──
function setText(id, v)  { const e = document.getElementById(id); if (e) e.textContent = v; }
function setHTML(id, v)  { const e = document.getElementById(id); if (e) e.innerHTML   = v; }

function formatFechaHora(iso) {
    try {
        const d = new Date(iso);
        const dia  = d.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' });
        const hora = `${d.getHours().toString().padStart(2,'0')}:${d.getMinutes().toString().padStart(2,'0')}`;
        return `${dia} ${hora}`;
    } catch { return '—'; }
}

function calcularDuracion(inicio, fin) {
    try {
        const mins = Math.round((new Date(fin) - new Date(inicio)) / 60000);
        const h = Math.floor(mins / 60), m = mins % 60;
        if (h === 0) return `${m} min`;
        if (m === 0) return `${h}h`;
        return `${h}h ${m}min`;
    } catch { return ''; }
}
