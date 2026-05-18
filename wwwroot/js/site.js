document.addEventListener("DOMContentLoaded", () => {
    const nodes = document.querySelectorAll(".node");

    nodes.forEach((node, index) => {
        node.style.opacity = "0";
        node.style.transform = "translateY(10px)";

        setTimeout(() => {
            node.style.transition = "all 0.5s ease";
            node.style.opacity = "1";
            node.style.transform = "translateY(0)";
        }, index * 180);
    });
});