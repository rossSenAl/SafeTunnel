using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class Simulacion
    {
        public int Id { get; set; }

        [Required]
        public string MensajeOriginal { get; set; } = string.Empty;

        public string MensajeProcesado { get; set; } = string.Empty;

        public string Modo { get; set; } = string.Empty;

        public string IpOrigen { get; set; } = string.Empty;

        public string IpDestino { get; set; } = string.Empty;

        public string IpVPN { get; set; } = string.Empty;

        public string Riesgo { get; set; } = string.Empty;

        public string Recomendacion { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}