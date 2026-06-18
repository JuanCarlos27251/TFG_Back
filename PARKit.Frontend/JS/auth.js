/* ==============================================
   PARKit — Módulo de Autenticación
   Centraliza toda la lógica de sesión y llamadas
   a la API de auth. Importar antes de cualquier
   script de página que requiera sesión.
   ============================================== */

const AUTH = (() => {

    // ── Configuración ──────────────────────────────────────────────
    const API_BASE = window.CONFIG?.API_BASE || 'https://localhost:7033';

    const RUTAS = {
        loginUsuario:  `${API_BASE}/api/Auth/Login`,
        loginEmpresa:  `${API_BASE}/api/Auth/LoginCompany`,
        registroUsuario: `${API_BASE}/api/Auth/Register`,
        registroEmpresa: `${API_BASE}/api/Company/register`,
    };

    const CLAVE_TOKEN   = 'parkit_token';
    const CLAVE_ROL     = 'parkit_rol';     // 'usuario' | 'empresa'
    const CLAVE_USUARIO = 'parkit_usuario'; // objeto JSON básico del usuario

    // ── Helpers de localStorage ────────────────────────────────────

    function guardarSesion(token, rol, datosUsuario) {
        localStorage.setItem(CLAVE_TOKEN,   token);
        localStorage.setItem(CLAVE_ROL,     rol);
        localStorage.setItem(CLAVE_USUARIO, JSON.stringify(datosUsuario));
    }

    function obtenerToken() {
        return localStorage.getItem(CLAVE_TOKEN);
    }

    function obtenerRol() {
        return localStorage.getItem(CLAVE_ROL);
    }

    function obtenerUsuario() {
        const raw = localStorage.getItem(CLAVE_USUARIO);
        try { return raw ? JSON.parse(raw) : null; }
        catch { return null; }
    }

    function estaAutenticado() {
        const token = obtenerToken();
        if (!token) return false;
        // Comprobamos que el token no haya expirado (campo exp del payload JWT)
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.exp * 1000 > Date.now();
        } catch {
            return false;
        }
    }

    function esEmpresa() {
        return obtenerRol() === 'empresa';
    }

    function cerrarSesion(redirigir = true) {
        localStorage.removeItem(CLAVE_TOKEN);
        localStorage.removeItem(CLAVE_ROL);
        localStorage.removeItem(CLAVE_USUARIO);
        if (redirigir) window.location.href = '/PARKit.Frontend/HTMl/index.html';
    }

    // ── Decodificador de payload JWT ───────────────────────────────

    function decodificarToken(token) {
        try {
            return JSON.parse(atob(token.split('.')[1]));
        } catch {
            return null;
        }
    }

    // ── Extraer datos básicos del payload ──────────────────────────

    function extraerDatosDeToken(token, esEmpresaLogin) {
        const payload = decodificarToken(token);
        if (!payload) return { nombre: 'Usuario', inicial: 'U', rol: esEmpresaLogin ? 'empresa' : 'usuario' };

        const nombre =
            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
            payload['name'] ||
            payload['unique_name'] ||
            (esEmpresaLogin ? 'Empresa' : 'Usuario');

        const inicial = nombre.charAt(0).toUpperCase();

        const id =
            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
            payload['sub'] ||
            payload['nameid'] ||
            null;

        // --- NUEVO: Extraer rol real del JWT ---
        const rolJwt = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'];
        let rolFinal = esEmpresaLogin ? 'empresa' : 'usuario';
        
        if (rolJwt === 'Admin') {
            rolFinal = 'Admin'; // Reconocemos al superusuario
        }

        return { nombre, inicial, id: id ? parseInt(id) : null, rol: rolFinal };
    }


    // ── Llamada genérica a la API ──────────────────────────────────

    async function llamarAPI(url, cuerpo) {
        const respuesta = await fetch(url, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify(cuerpo),
        });

        const datos = await respuesta.json().catch(() => ({}));

        if (!respuesta.ok) {
            // La API devuelve el mensaje en distintos formatos
            const mensaje =
                datos?.message ||
                datos?.title   ||
                (typeof datos === 'string' ? datos : null) ||
                `Error ${respuesta.status}`;
            throw new Error(mensaje);
        }

        return datos;
    }

    // ── Login de usuario ───────────────────────────────────────────

    async function loginUsuario(email, password) {
        const datos = await llamarAPI(RUTAS.loginUsuario, { email, password });
        const token = datos.token || datos.Token;
        if (!token) throw new Error('Respuesta inesperada del servidor.');

        const infoUsuario = extraerDatosDeToken(token, false);
        // Ahora guardará 'usuario' o 'Admin' dependiendo del token
        guardarSesion(token, infoUsuario.rol, infoUsuario); 
        return infoUsuario;
    }


    // ── Login de empresa ───────────────────────────────────────────

    async function loginEmpresa(email, password) {
        const datos = await llamarAPI(RUTAS.loginEmpresa, { email, password });
        const token = datos.token || datos.Token;
        if (!token) throw new Error('Respuesta inesperada del servidor.');

        const infoUsuario = extraerDatosDeToken(token, true);
        guardarSesion(token, 'empresa', infoUsuario);
        return infoUsuario;
    }

    // ── Registro de usuario ────────────────────────────────────────

    async function registrarUsuario(nombre, email, password) {
        const datos = await llamarAPI(RUTAS.registroUsuario, {
            name: nombre, email, password,
        });
        const token = datos.token || datos.Token;
        if (!token) throw new Error('Respuesta inesperada del servidor.');

        const infoUsuario = extraerDatosDeToken(token, false);
        guardarSesion(token, 'usuario', infoUsuario);
        return infoUsuario;
    }

    // ── Registro de empresa ────────────────────────────────────────

    async function registrarEmpresa(nombre, cif, email, password) {
        // El registro de empresa no devuelve token, redirige al login
        await llamarAPI(RUTAS.registroEmpresa, {
            nameCompany: nombre,
            cif,
            email,
            password,
        });
        // Tras registro exitoso de empresa, se redirige al login para que inicien sesión
        return true;
    }

    // ── Cabecera con token para peticiones autenticadas ───────────

    function cabecerasAuth() {
        const token = obtenerToken();
        return {
            'Content-Type':  'application/json',
            'Authorization': `Bearer ${token}`,
        };
    }

    // ── API pública del módulo ─────────────────────────────────────

    return {
        loginUsuario,
        loginEmpresa,
        registrarUsuario,
        registrarEmpresa,
        cerrarSesion,
        estaAutenticado,
        esEmpresa,
        obtenerToken,
        obtenerUsuario,
        obtenerRol,
        cabecerasAuth,
        API_BASE,
    };

})();