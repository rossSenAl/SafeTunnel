using Microsoft.AspNetCore.SignalR;

namespace SafeTunnel.Hubs
{
    public class SimulacionHub : Hub
    {
        // Método existente (para la simulación VPN)
        public async Task UnirseASala(string salaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, salaId);
        }

        // ========== NUEVOS MÉTODOS PARA TRANSFERENCIA DE ARCHIVOS ==========

        /// <summary>
        /// Unirse a una sala de transferencia (código generado por el emisor)
        /// </summary>
        public async Task UnirseATransferencia(string codigoSala)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, codigoSala);
            // Opcional: notificar al grupo que alguien se unió
            await Clients.Group(codigoSala).SendAsync("UsuarioUnido", Context.ConnectionId);
        }

        /// <summary>
        /// Emisor envía metadatos del archivo (nombre, tamaño, total fragmentos)
        /// </summary>
        public async Task EnviarMetadatos(string codigoSala, string nombreArchivo, long tamaño, int totalFragmentos)
        {
            await Clients.Group(codigoSala).SendAsync("RecibirMetadatos", nombreArchivo, tamaño, totalFragmentos);
        }

        /// <summary>
        /// Emisor envía un fragmento (paquete) al grupo
        /// </summary>
        public async Task EnviarFragmento(string codigoSala, int secuencia, string base64Datos, bool esUltimo)
        {
            Console.WriteLine($"Fragmento {secuencia} recibido (Base64, longitud {base64Datos?.Length ?? 0})");
            await Clients.Group(codigoSala).SendAsync("RecibirFragmento", secuencia, base64Datos, esUltimo);
        }

        // ========== MÉTODOS PARA LA SIMULACIÓN VPN (ya existentes, corregidos) ==========
        public Task RecibirProgreso(string etapa, int progreso, string mensaje) => Task.CompletedTask;
        public Task LatenciaActualizada(int latencia, string calidad) => Task.CompletedTask;
        public Task AtaqueDetectado(string tipo, string alerta, bool integridadComprometida) => Task.CompletedTask;
    }
}