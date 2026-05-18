using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Models;
using SafeTunnel.Services;
using SafeTunnel.ViewModels;

namespace SafeTunnel.Controllers
{
    public class SimuladorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SeguridadVpnService _seguridad;

        public SimuladorController(AppDbContext context, SeguridadVpnService seguridad)
        {
            _context = context;
            _seguridad = seguridad;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new SimuladorVpnViewModel
            {
                Historial = await _context.TransmisionesVpn
                    .OrderByDescending(x => x.Fecha)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarSinVpn(SimuladorVpnViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.MensajeOriginal))
            {
                ModelState.AddModelError("", "Debe ingresar un mensaje para enviarlo.");
                vm.Historial = await ObtenerHistorial();
                return View("Index", vm);
            }

            var registro = new TransmisionVpn
            {
                MensajeOriginal = vm.MensajeOriginal,
                MensajeCifrado = vm.MensajeOriginal,
                MensajeDescifrado = vm.MensajeOriginal,
                HashIntegridad = "No aplicado",
                IpRealSimulada = GenerarIpPublica(),
                IpProtegidaSimulada = "No protegida",
                TipoConexion = "Sin VPN",
                NivelSeguridad = "Bajo",
                Protocolo = "HTTP simulado",
                Ruta = "Cliente → Internet público → Servidor",
                Estado = "Datos visibles en la red",
                Recomendacion = "Evite enviar información sensible sin una conexión segura.",
                Fecha = DateTime.Now
            };

            _context.TransmisionesVpn.Add(registro);
            await _context.SaveChangesAsync();

            vm.Resultado = registro;
            vm.Historial = await ObtenerHistorial();

            return View("Index", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarConVpn(SimuladorVpnViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.MensajeOriginal))
            {
                ModelState.AddModelError("", "Debe ingresar un mensaje para enviarlo.");
                vm.Historial = await ObtenerHistorial();
                return View("Index", vm);
            }

            string mensajeCifrado = _seguridad.Cifrar(vm.MensajeOriginal);
            string mensajeDescifrado = _seguridad.Descifrar(mensajeCifrado);

            var registro = new TransmisionVpn
            {
                MensajeOriginal = vm.MensajeOriginal,
                MensajeCifrado = mensajeCifrado,
                MensajeDescifrado = mensajeDescifrado,
                HashIntegridad = _seguridad.GenerarHash(vm.MensajeOriginal),
                IpRealSimulada = GenerarIpPublica(),
                IpProtegidaSimulada = GenerarIpVpn(),
                TipoConexion = "Con VPN",
                NivelSeguridad = "Alto",
                Protocolo = "Túnel VPN + AES + SHA-256",
                Ruta = "Cliente → Túnel VPN cifrado → Servidor",
                Estado = "Datos protegidos mediante cifrado",
                Recomendacion = "La conexión VPN protege la información y oculta la IP real simulada.",
                Fecha = DateTime.Now
            };

            _context.TransmisionesVpn.Add(registro);
            await _context.SaveChangesAsync();

            vm.Resultado = registro;
            vm.Historial = await ObtenerHistorial();

            return View("Index", vm);
        }

        private async Task<List<TransmisionVpn>> ObtenerHistorial()
        {
            return await _context.TransmisionesVpn
                .OrderByDescending(x => x.Fecha)
                .Take(10)
                .ToListAsync();
        }

        private string GenerarIpPublica()
        {
            Random random = new Random();
            return $"190.129.{random.Next(1, 255)}.{random.Next(1, 255)}";
        }

        private string GenerarIpVpn()
        {
            Random random = new Random();
            return $"10.8.0.{random.Next(2, 254)}";
        }
    }
}