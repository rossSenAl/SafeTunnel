using Microsoft.Extensions.Logging;

namespace SafeTunnel.Services
{
    public class RedSimuladaService
    {
        private readonly Random _random = new Random();
        private readonly ILogger<RedSimuladaService> _logger;

        public RedSimuladaService(ILogger<RedSimuladaService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simula el envío de un paquete por la red con latencia y posible pérdida
        /// </summary>
        /// <param name="usarVpn">Si se usa VPN o no</param>
        /// <param name="tamañoMensaje">Tamaño del mensaje para calcular latencia</param>
        /// <returns>Resultado de la transmisión</returns>
        public async Task<ResultadoTransmision> EnviarPaqueteAsync(bool usarVpn, int tamañoMensaje)
        {
            var resultado = new ResultadoTransmision();

            // 1. CALCULAR LATENCIA BASE
            int latenciaBase;
            if (usarVpn)
            {
                // VPN: más latencia pero más estable
                latenciaBase = _random.Next(80, 200);
            }
            else
            {
                // Sin VPN: menos latencia pero menos estable
                latenciaBase = _random.Next(15, 60);
            }

            // 2. AGREGAR CONGESTIÓN DE RED
            int congestion;
            if (usarVpn)
            {
                congestion = _random.Next(0, 40); // VPN: congestión moderada
            }
            else
            {
                congestion = _random.Next(0, 80); // Red pública: mucha congestión
            }

            // 3. AJUSTAR POR TAMAÑO DEL MENSAJE
            int tamañoExtra = (tamañoMensaje / 100) * 10; // +10ms por cada 100 caracteres
            int latenciaTotal = latenciaBase + congestion + tamañoExtra;

            resultado.LatenciaMs = latenciaTotal;

            // 4. SIMULAR PÉRDIDA DE PAQUETES
            if (!usarVpn)
            {
                // Sin VPN: 15% de probabilidad de pérdida
                int probabilidadPerdida = _random.Next(1, 100);
                if (probabilidadPerdida <= 15)
                {
                    resultado.Exitoso = false;
                    resultado.PaquetesPerdidos = 1;
                    resultado.MensajeError = "❌ El paquete se perdió durante la transmisión en red pública";
                    _logger.LogWarning("Paquete perdido en transmisión sin VPN");
                }
                // 5% de probabilidad de retransmisión
                else if (probabilidadPerdida <= 20)
                {
                    resultado.Retransmisiones = 1;
                    resultado.LatenciaMs += 50;
                    resultado.MensajeError = "⚠️ Se requirió retransmisión del paquete";
                }
            }
            else
            {
                // Con VPN: solo 2% de probabilidad de problemas menores
                int probabilidadError = _random.Next(1, 100);
                if (probabilidadError <= 2)
                {
                    resultado.LatenciaMs += 30;
                    resultado.MensajeError = "⚠️ Leve congestión en el túnel VPN";
                }
            }

            // 5. ESPERAR EL TIEMPO DE LATENCIA (simulación real)
            await Task.Delay(resultado.LatenciaMs);

            return resultado;
        }

        /// <summary>
        /// Obtiene una descripción de la calidad de la conexión
        /// </summary>
        public string ObtenerCalidadConexion(bool usarVpn, int latencia)
        {
            if (usarVpn)
            {
                if (latencia < 100) return "Excelente (VPN) 🔐";
                if (latencia < 150) return "Buena (VPN) 🔐";
                if (latencia < 220) return "Aceptable (VPN) ⚠️";
                return "Lenta (VPN) 🐌";
            }
            else
            {
                if (latencia < 30) return "Excelente 📶";
                if (latencia < 60) return "Buena 📱";
                if (latencia < 100) return "Regular ⚠️";
                return "Mala ❌";
            }
        }

        /// <summary>
        /// Obtiene un ícono según la latencia
        /// </summary>
        public string ObtenerIconoLatencia(int latencia)
        {
            if (latencia < 40) return "⚡";
            if (latencia < 80) return "📶";
            if (latencia < 120) return "⏱️";
            if (latencia < 180) return "🐌";
            return "🐢";
        }
    }

    /// <summary>
    /// Resultado de una transmisión simulada
    /// </summary>
    public class ResultadoTransmision
    {
        public bool Exitoso { get; set; } = true;
        public int LatenciaMs { get; set; }
        public int PaquetesPerdidos { get; set; }
        public int Retransmisiones { get; set; }
        public string? MensajeError { get; set; }
    }
}