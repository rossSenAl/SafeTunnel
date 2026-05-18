using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class SimulacionViewModel
    {
        [Required(ErrorMessage = "Debe ingresar un mensaje.")]
        public string Mensaje { get; set; } = string.Empty;

        public string IpOrigen { get; set; } = "192.168.1.25";

        public string IpDestino { get; set; } = "200.58.120.10";

        public string? Modo { get; set; }

        public string? MensajeProcesado { get; set; }

        public string? IpVPN { get; set; }

        public string? Riesgo { get; set; }

        public string? Recomendacion { get; set; }

        public string? Ruta { get; set; }
    }
}