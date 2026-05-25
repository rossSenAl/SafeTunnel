using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class SimulacionViewModel
    {
        [Required(ErrorMessage = "Debe ingresar un mensaje.")]
        public string Mensaje { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar una IP de origen.")]
        public string IpOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar una IP de destino.")]
        public string IpDestino { get; set; } = string.Empty;

        public string Modo { get; set; } = string.Empty;

        public string MensajeProcesado { get; set; } = string.Empty;

        public string IpVPN { get; set; } = string.Empty;

        public string Riesgo { get; set; } = string.Empty;

        public string Recomendacion { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public bool SimularAtaque { get; set; }

        public string HashOriginal { get; set; } = string.Empty;

        public string HashRecibido { get; set; } = string.Empty;

        public bool IntegridadValida { get; set; }

        public string MensajeInterceptado { get; set; } = string.Empty;

        // En Models/SimulacionViewModel.cs
        public string? CifradoRSA { get; set; }
        public string? FirmaDigital { get; set; }
        public string? HuellaRSA { get; set; }
        public int? LatenciaMs { get; set; }
        public int? PaquetesPerdidos { get; set; }
        public int? Retransmisiones { get; set; }
        public string? CalidadConexion { get; set; }
        public TipoAtaque AtaqueSeleccionado { get; set; } = TipoAtaque.Ninguno;
        public string? AlertaAtaque { get; set; }
        public string? SalaId { get; set; }   // Para SignalR
    }
}