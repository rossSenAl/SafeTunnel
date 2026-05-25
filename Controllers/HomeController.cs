using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using SafeTunnel.Data;
using SafeTunnel.Models;
using SafeTunnel.Services;
using SafeTunnel.Hubs;

namespace SafeTunnel.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SeguridadVpnService _seguridad;
        private readonly CifradoRsaService _rsa;
        private readonly RedSimuladaService _red;
        private readonly IHubContext<SimulacionHub> _hubContext;
        private readonly AtaqueService _ataque;

        public HomeController(AppDbContext context,
                              SeguridadVpnService seguridad,
                              CifradoRsaService rsa,
                              RedSimuladaService red,
                              IHubContext<SimulacionHub> hubContext,
                              AtaqueService ataque)
        {
            _context = context;
            _seguridad = seguridad;
            _rsa = rsa;
            _red = red;
            _hubContext = hubContext;
            _ataque = ataque;
        }

        [Authorize]
        public IActionResult Index() => View();

        [Authorize]
        [HttpGet]
        public IActionResult Simulador()
        {
            var salaId = Guid.NewGuid().ToString();
            ViewBag.SalaId = salaId;
            var model = new SimulacionViewModel { SalaId = salaId };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulador(SimulacionViewModel modelo, string modo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            Console.WriteLine($"Ataque seleccionado: {modelo.AtaqueSeleccionado}");
            // Asegurar SalaId
            string salaId = modelo.SalaId ?? Guid.NewGuid().ToString();
            modelo.SalaId = salaId;

            // Notificar inicio
            await _hubContext.Clients.Group(salaId).SendAsync("RecibirProgreso", (string)"Inicio", (int)0, (string)"Iniciando simulación...");
            await Task.Delay(500);

            // Simular red
            bool usarVpn = modo == "con_vpn";
            var resultadoRed = await _red.EnviarPaqueteAsync(usarVpn, modelo.Mensaje?.Length ?? 1);

            if (!resultadoRed.Exitoso)
            {
                ModelState.AddModelError("", resultadoRed.MensajeError ?? "Error en la transmisión");
                return View(modelo);
            }

            ViewBag.Latencia = resultadoRed.LatenciaMs;
            ViewBag.CalidadConexion = _red.ObtenerCalidadConexion(usarVpn, resultadoRed.LatenciaMs);
            ViewBag.IconoLatencia = _red.ObtenerIconoLatencia(resultadoRed.LatenciaMs);
            ViewBag.PaquetesPerdidos = resultadoRed.PaquetesPerdidos;
            ViewBag.Retransmisiones = resultadoRed.Retransmisiones;
            ViewBag.MensajeRed = resultadoRed.MensajeError;

            // Notificar latencia
            await _hubContext.Clients.Group(salaId).SendAsync("LatenciaActualizada",
                (int)resultadoRed.LatenciaMs,
                (string)ViewBag.CalidadConexion);

            // Hashes
            string hashOriginal = _seguridad.GenerarHash(modelo.Mensaje!);
            string mensajeInterceptado = modelo.Mensaje!;
            string hashRecibido = hashOriginal;
            bool integridadValida = true;

            string alertaAtaque = string.Empty;
            bool integridadComprometida = false;

            // ========== ATAQUE DESDE DROPDOWN ==========
            if (modelo.AtaqueSeleccionado != TipoAtaque.Ninguno)
            {
                var resultadoAtaque = _ataque.EjecutarAtaque(modelo.AtaqueSeleccionado, modelo.Mensaje!, resultadoRed.LatenciaMs);
                if (!resultadoAtaque.exitoso)
                {
                    ModelState.AddModelError("", resultadoAtaque.alerta);
                    return View(modelo);
                }
                alertaAtaque = resultadoAtaque.alerta;
                if (resultadoAtaque.mensajeInterceptado != null && resultadoAtaque.mensajeInterceptado != modelo.Mensaje)
                {
                    mensajeInterceptado = resultadoAtaque.mensajeInterceptado;
                    hashRecibido = _seguridad.GenerarHash(mensajeInterceptado);
                    integridadValida = hashOriginal == hashRecibido;
                    integridadComprometida = !integridadValida;
                }
                if (resultadoAtaque.latenciaAdicional > 0)
                    resultadoRed.LatenciaMs += resultadoAtaque.latenciaAdicional;

                await _hubContext.Clients.Group(salaId).SendAsync("AtaqueDetectado",
                    (string)modelo.AtaqueSeleccionado.ToString(),
                    (string)alertaAtaque,
                    (bool)integridadComprometida);
            }

            // ========== ATAQUE DESDE CHECKBOX (MITM dinámico) ==========
            if (modelo.SimularAtaque)
            {
                if (modo == "con_vpn")
                {
                    mensajeInterceptado = "Ataque detectado: el atacante no puede leer fácilmente el contenido cifrado.";
                    hashRecibido = hashOriginal;
                    integridadValida = true;
                }
                else
                {
                    // Usamos el mismo servicio para generar un MITM dinámico
                    var resultadoAtaque = _ataque.EjecutarAtaque(TipoAtaque.MITM, modelo.Mensaje!, resultadoRed.LatenciaMs);
                    if (!resultadoAtaque.exitoso)
                    {
                        ModelState.AddModelError("", resultadoAtaque.alerta);
                        return View(modelo);
                    }
                    alertaAtaque = resultadoAtaque.alerta;
                    mensajeInterceptado = resultadoAtaque.mensajeInterceptado ?? modelo.Mensaje!;
                    hashRecibido = _seguridad.GenerarHash(mensajeInterceptado);
                    integridadValida = hashOriginal == hashRecibido;
                    integridadComprometida = !integridadValida;
                    if (resultadoAtaque.latenciaAdicional > 0)
                        resultadoRed.LatenciaMs += resultadoAtaque.latenciaAdicional;

                    await _hubContext.Clients.Group(salaId).SendAsync("AtaqueDetectado",
                        "MITM (checkbox)",
                        (string)alertaAtaque,
                        (bool)integridadComprometida);
                }
            }

            // Progreso nodos
            await _hubContext.Clients.Group(salaId).SendAsync("RecibirProgreso", (string)"Cliente", (int)20, (string)"Enviando paquete...");
            await Task.Delay(600);
            await _hubContext.Clients.Group(salaId).SendAsync("RecibirProgreso", (string)"Router", (int)50, (string)"Reenviando paquete...");
            await Task.Delay(600);
            await _hubContext.Clients.Group(salaId).SendAsync("RecibirProgreso", (string)(usarVpn ? "VPN" : "Internet"), (int)80, (string)(usarVpn ? "Cifrando en túnel VPN" : "Transmitiendo por red pública"));
            await Task.Delay(600);
            await _hubContext.Clients.Group(salaId).SendAsync("RecibirProgreso", (string)"Servidor", (int)100, (string)"Mensaje entregado");
            await Task.Delay(500);

            // Guardar en BD
            Simulacion simulacion;
            if (modo == "con_vpn")
            {
                string mensajeCifrado = _seguridad.Cifrar(modelo.Mensaje!);
                string mensajeDescifrado = _seguridad.Descifrar(mensajeCifrado);
                string cifradoRSA = _rsa.CifrarConLlavePublica(modelo.Mensaje!);
                string firmaDigital = _rsa.FirmarMensaje(modelo.Mensaje!);
                string huellaRSA = _rsa.ObtenerHuellaDigital();

                string riesgo = resultadoRed.LatenciaMs > 200 ? "Medio - Conexión lenta" : (modelo.SimularAtaque ? "Bajo - ataque mitigado" : "Bajo");

                simulacion = new Simulacion
                {
                    MensajeOriginal = modelo.Mensaje!,
                    MensajeProcesado = mensajeCifrado,
                    Modo = "Con VPN",
                    IpOrigen = modelo.IpOrigen,
                    IpDestino = modelo.IpDestino,
                    IpVPN = GenerarIpVpn(),
                    Riesgo = riesgo,
                    Recomendacion = modelo.SimularAtaque
                        ? "Se simuló un ataque, pero el túnel VPN mantiene la información protegida mediante cifrado."
                        : "La información viaja cifrada mediante un túnel VPN simulado.",
                    Ruta = $"Usuario → Router → Túnel VPN cifrado → Servidor VPN → Servidor final [{resultadoRed.LatenciaMs}ms]",
                    Fecha = DateTime.Now,
                    HashOriginal = hashOriginal,
                    HashRecibido = hashRecibido,
                    IntegridadValida = integridadValida,
                    AtaqueSimulado = modelo.SimularAtaque,
                    MensajeInterceptado = mensajeInterceptado,
                    CifradoRSA = cifradoRSA,
                    FirmaDigital = firmaDigital,
                    HuellaRSA = huellaRSA,
                    LatenciaMs = resultadoRed.LatenciaMs,
                    PaquetesPerdidos = resultadoRed.PaquetesPerdidos,
                    Retransmisiones = resultadoRed.Retransmisiones,
                    TipoAtaqueEjecutado = modelo.AtaqueSeleccionado.ToString(),
                    AlertaAtaque = alertaAtaque
                };
                ViewBag.MensajeDescifrado = mensajeDescifrado;
                ViewBag.CifradoRSA = cifradoRSA;
                ViewBag.FirmaDigital = firmaDigital;
                ViewBag.HuellaRSA = huellaRSA;
            }
            else
            {
                string riesgo = resultadoRed.PaquetesPerdidos > 0 ? "Crítico - Paquetes perdidos" :
                                (resultadoRed.LatenciaMs > 100 ? "Alto - Red congestionada" :
                                (modelo.SimularAtaque ? "Crítico" : "Alto"));

                simulacion = new Simulacion
                {
                    MensajeOriginal = modelo.Mensaje!,
                    MensajeProcesado = modelo.Mensaje!,
                    Modo = "Sin VPN",
                    IpOrigen = modelo.IpOrigen,
                    IpDestino = modelo.IpDestino,
                    IpVPN = "No aplica",
                    Riesgo = riesgo,
                    Recomendacion = modelo.SimularAtaque
                        ? "El mensaje fue interceptado y modificado porque no viajaba protegido por un túnel VPN."
                        : "No se recomienda enviar información sensible sin protección.",
                    Ruta = $"Usuario → Router → Internet público → Servidor final [{resultadoRed.LatenciaMs}ms]",
                    Fecha = DateTime.Now,
                    HashOriginal = hashOriginal,
                    HashRecibido = hashRecibido,
                    IntegridadValida = integridadValida,
                    AtaqueSimulado = modelo.SimularAtaque,
                    MensajeInterceptado = mensajeInterceptado,
                    LatenciaMs = resultadoRed.LatenciaMs,
                    PaquetesPerdidos = resultadoRed.PaquetesPerdidos,
                    Retransmisiones = resultadoRed.Retransmisiones,
                    TipoAtaqueEjecutado = modelo.AtaqueSeleccionado.ToString(),
                    AlertaAtaque = alertaAtaque
                };
                ViewBag.MensajeDescifrado = mensajeInterceptado;
                ViewBag.CifradoRSA = null;
                ViewBag.FirmaDigital = null;
                ViewBag.HuellaRSA = null;
            }

            _context.Simulaciones.Add(simulacion);
            await _context.SaveChangesAsync();

            modelo.Modo = simulacion.Modo;
            modelo.MensajeProcesado = simulacion.MensajeProcesado;
            modelo.IpVPN = simulacion.IpVPN;
            modelo.Riesgo = simulacion.Riesgo;
            modelo.Recomendacion = simulacion.Recomendacion;
            modelo.Ruta = simulacion.Ruta;
            modelo.HashOriginal = simulacion.HashOriginal;
            modelo.HashRecibido = simulacion.HashRecibido;
            modelo.IntegridadValida = simulacion.IntegridadValida;
            modelo.MensajeInterceptado = simulacion.MensajeInterceptado;
            modelo.SimularAtaque = simulacion.AtaqueSimulado;
            modelo.CifradoRSA = simulacion.CifradoRSA;
            modelo.FirmaDigital = simulacion.FirmaDigital;
            modelo.HuellaRSA = simulacion.HuellaRSA;

            return View(modelo);
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var simulaciones = await _context.Simulaciones.ToListAsync();
            ViewBag.Total = simulaciones.Count;
            ViewBag.ConVPN = simulaciones.Count(s => s.Modo == "Con VPN");
            ViewBag.SinVPN = simulaciones.Count(s => s.Modo == "Sin VPN");
            ViewBag.RiesgoBajo = simulaciones.Count(s => s.Riesgo != null && (s.Riesgo.Contains("Bajo") || s.Riesgo == "Bajo"));
            ViewBag.RiesgoMedio = simulaciones.Count(s => s.Riesgo != null && s.Riesgo.Contains("Medio"));
            ViewBag.RiesgoAlto = simulaciones.Count(s => s.Riesgo != null && (s.Riesgo.Contains("Alto") && !s.Riesgo.Contains("Crítico")));
            ViewBag.RiesgoCritico = simulaciones.Count(s => s.Riesgo != null && s.Riesgo.Contains("Crítico"));
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Historial()
        {
            var historial = await _context.Simulaciones.OrderByDescending(s => s.Fecha).ToListAsync();
            return View(historial);
        }

        [Authorize]
        public IActionResult Recomendaciones() => View();

        [Authorize]
        public IActionResult Transferencia()
        {
            return View();
        }

        private string GenerarIpVpn()
        {
            Random random = new Random();
            return $"10.8.0.{random.Next(2, 254)}";
        }
    }
}