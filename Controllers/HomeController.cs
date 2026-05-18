using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Models;
using System.Text;

namespace SafeTunnel.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Simulador()
        {
            return View(new SimulacionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Simulador(SimulacionViewModel modelo, string modo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Simulacion simulacion;

            if (modo == "con_vpn")
            {
                simulacion = new Simulacion
                {
                    MensajeOriginal = modelo.Mensaje,
                    MensajeProcesado = CifrarMensajeEducativo(modelo.Mensaje),
                    Modo = "Con VPN",
                    IpOrigen = modelo.IpOrigen,
                    IpDestino = modelo.IpDestino,
                    IpVPN = "181.45.22.90",
                    Riesgo = "Bajo",
                    Recomendacion = "La información viaja cifrada mediante un túnel VPN simulado.",
                    Ruta = "Usuario → Router → Túnel VPN → Servidor VPN → Servidor final",
                    Fecha = DateTime.Now
                };
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
                    Riesgo = "Alto",
                    Recomendacion = "No se recomienda enviar información sensible sin protección.",
                    Ruta = "Usuario → Router → Internet → Servidor final",
                    Fecha = DateTime.Now
                };
            }

            _context.Simulaciones.Add(simulacion);
            await _context.SaveChangesAsync();

            modelo.Modo = simulacion.Modo;
            modelo.MensajeProcesado = simulacion.MensajeProcesado;
            modelo.IpVPN = simulacion.IpVPN;
            modelo.Riesgo = simulacion.Riesgo;
            modelo.Recomendacion = simulacion.Recomendacion;
            modelo.Ruta = simulacion.Ruta;

            return View(modelo);
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Total = await _context.Simulaciones.CountAsync();
            ViewBag.ConVPN = await _context.Simulaciones.CountAsync(s => s.Modo == "Con VPN");
            ViewBag.SinVPN = await _context.Simulaciones.CountAsync(s => s.Modo == "Sin VPN");

            return View();
        }

        public async Task<IActionResult> Historial()
        {
            var historial = await _context.Simulaciones
                .OrderByDescending(s => s.Fecha)
                .ToListAsync();

            return View(historial);
        }

        public IActionResult Recomendaciones()
        {
            return View();
        }

        private string CifrarMensajeEducativo(string mensaje)
        {
            var desplazado = new StringBuilder();

            foreach (char caracter in mensaje)
            {
                desplazado.Append((char)(caracter + 3));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(desplazado.ToString());
            return Convert.ToBase64String(bytes);
        }
    }
}