using System;
using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class TransmisionVpn
    {
        public int Id { get; set; }

        [Required]
        public string MensajeOriginal { get; set; } = string.Empty;

        public string MensajeCifrado { get; set; } = string.Empty;

        public string MensajeDescifrado { get; set; } = string.Empty;

        public string HashIntegridad { get; set; } = string.Empty;

        public string IpRealSimulada { get; set; } = string.Empty;

        public string IpProtegidaSimulada { get; set; } = string.Empty;

        public string TipoConexion { get; set; } = string.Empty;

        public string NivelSeguridad { get; set; } = string.Empty;

        public string Protocolo { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Recomendacion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}