using SafeTunnel.Models;
using System.Collections.Generic;

namespace SafeTunnel.ViewModels
{
    public class SimuladorVpnViewModel
    {
        public string MensajeOriginal { get; set; } = string.Empty;

        public TransmisionVpn? Resultado { get; set; }

        public List<TransmisionVpn> Historial { get; set; } = new List<TransmisionVpn>();
    }
}