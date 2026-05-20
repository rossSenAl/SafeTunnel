using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Models;
using SafeTunnel.Services;
using System.Security.Cryptography;

namespace SafeTunnel.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SeguridadVpnService _seguridad;
        

        public HomeController(AppDbContext context, SeguridadVpnService seguridad)
        {
            _context = context;
            _seguridad = seguridad;
            
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Simulador()
        {
            return View(new SimulacionViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulador(SimulacionViewModel modelo, string modo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Simulacion simulacion;

            string hashOriginal = _seguridad.GenerarHash(modelo.Mensaje);
            string mensajeInterceptado = modelo.Mensaje;
            string hashRecibido;
            bool integridadValida;

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
                    mensajeInterceptado = modelo.Mensaje + " [ALTERADO POR ATACANTE]";
                    hashRecibido = _seguridad.GenerarHash(mensajeInterceptado);
                    integridadValida = hashOriginal == hashRecibido;
                }
            }
            else
            {
                hashRecibido = hashOriginal;
                integridadValida = true;
            }

            if (modo == "con_vpn")
            {
                string mensajeCifrado = _seguridad.Cifrar(modelo.Mensaje);
                string mensajeDescifrado = _seguridad.Descifrar(mensajeCifrado);

                simulacion = new Simulacion
                {
                    MensajeOriginal = modelo.Mensaje,
                    MensajeProcesado = mensajeCifrado,
                    Modo = "Con VPN",
                    IpOrigen = modelo.IpOrigen,
                    IpDestino = modelo.IpDestino,
                    IpVPN = GenerarIpVpn(),
                    Riesgo = modelo.SimularAtaque ? "Bajo - ataque mitigado" : "Bajo",
                    Recomendacion = modelo.SimularAtaque
                        ? "Se simuló un ataque, pero el túnel VPN mantiene la información protegida mediante cifrado."
                        : "La información viaja cifrada mediante un túnel VPN simulado. La IP real queda protegida por una IP VPN simulada.",
                    Ruta = "Usuario → Router → Túnel VPN cifrado → Servidor VPN → Servidor final",
                    Fecha = DateTime.Now,
                    HashOriginal = hashOriginal,
                    HashRecibido = hashRecibido,
                    IntegridadValida = integridadValida,
                    AtaqueSimulado = modelo.SimularAtaque,
                    MensajeInterceptado = mensajeInterceptado
                };

                ViewBag.MensajeDescifrado = mensajeDescifrado;
            }
            else
            {
                simulacion = new Simulacion
                {
                    MensajeOriginal = modelo.Mensaje,
                    MensajeProcesado = modelo.Mensaje,
                    Modo = "Sin VPN",
                    IpOrigen = modelo.IpOrigen,
                    IpDestino = modelo.IpDestino,
                    IpVPN = "No aplica",
                    Riesgo = modelo.SimularAtaque ? "Crítico" : "Alto",
                    Recomendacion = modelo.SimularAtaque
                        ? "El mensaje fue interceptado y modificado porque no viajaba protegido por un túnel VPN."
                        : "No se recomienda enviar información sensible sin protección, porque los datos viajan visibles por la red.",
                    Ruta = "Usuario → Router → Internet público → Servidor final",
                    Fecha = DateTime.Now,
                    HashOriginal = hashOriginal,
                    HashRecibido = hashRecibido,
                    IntegridadValida = integridadValida,
                    AtaqueSimulado = modelo.SimularAtaque,
                    MensajeInterceptado = mensajeInterceptado
                };

                ViewBag.MensajeDescifrado = mensajeInterceptado;
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

            return View(modelo);
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Total = await _context.Simulaciones.CountAsync();
            ViewBag.ConVPN = await _context.Simulaciones.CountAsync(s => s.Modo == "Con VPN");
            ViewBag.SinVPN = await _context.Simulaciones.CountAsync(s => s.Modo == "Sin VPN");

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Historial()
        {
            var historial = await _context.Simulaciones
                .OrderByDescending(s => s.Fecha)
                .ToListAsync();

            return View(historial);
        }

        [Authorize]
        public IActionResult Recomendaciones()
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