document.addEventListener("DOMContentLoaded", () => {

    // Busca los nodos del simulador: Cliente, Red/VPN y Servidor
    const nodes = document.querySelectorAll(".node");

    // Si la página no tiene nodos, no hace nada
    if (nodes.length === 0) {
        return;
    }

    // Animación suave de aparición de los nodos
    nodes.forEach((node, index) => {
        node.style.opacity = "0";
        node.style.transform = "translateY(10px)";

        setTimeout(() => {
            node.style.transition = "all 0.5s ease";
            node.style.opacity = "1";
            node.style.transform = "translateY(0)";
        }, index * 180);
    });

    // Si ya existe un resultado, baja suavemente hacia la simulación
    const resultado = document.querySelector(".network-map.active");

    if (resultado) {
        setTimeout(() => {
            resultado.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });
        }, 500);
    }
});