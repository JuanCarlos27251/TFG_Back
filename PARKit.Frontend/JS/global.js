const avatarHeader = document.getElementById('header-user-avatar');
if (avatarHeader && usuarioLogueado) {
    // Reemplaza el icono de la silueta por la primera letra de su nombre
    avatarHeader.innerHTML = usuarioLogueado.nombre.charAt(0).toUpperCase();
}


window.tailwind.config = {
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                azul: "#135bec",
                "azul-oscuro": "#0a3bbf",
                "fondo-oscuro": "#0e1120",
            },
            fontFamily: { 
                sans: ["DM Sans", "sans-serif"] 
            },
        }
    }
};