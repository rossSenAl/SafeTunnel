using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Models;
using SafeTunnel.Services;
using System.Security.Claims;

namespace SafeTunnel.Controllers
{
    public class CuentaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly SeguridadVpnService _seguridad;
        private readonly ILogger<CuentaController> _logger;

        public CuentaController(AppDbContext context, EmailService emailService, SeguridadVpnService seguridad, ILogger<CuentaController> logger)
        {
            _context = context;
            _emailService = emailService;
            _seguridad = seguridad;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Opcional: crear usuario de prueba si no existe (solo para desarrollo)
            await CrearUsuarioPruebaSiNoExiste();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == model.Correo);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de login con correo no registrado: {Correo}", model.Correo);
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(model);
            }

            // Verificar contraseña usando el método seguro
            if (!_seguridad.VerificarHash(model.Password, usuario.PasswordHash))
            {
                _logger.LogWarning("Contraseña incorrecta para usuario: {Correo}", model.Correo);
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(model);
            }

            // ========== INICIO 2FA ==========
            // Generar código de 6 dígitos
            var codigo = new Random().Next(100000, 999999).ToString();
            usuario.Codigo2FA = codigo;
            usuario.Codigo2FAExpiracion = DateTime.Now.AddMinutes(5);
            await _context.SaveChangesAsync();

            // Enviar correo
            var enviado = await _emailService.EnviarCodigoAsync(usuario.Correo, codigo);
            if (!enviado)
            {
                _logger.LogError("No se pudo enviar el código 2FA a {Correo}", usuario.Correo);
                ModelState.AddModelError("", "Error al enviar el código de verificación. Intente más tarde.");
                return View(model);
            }

            TempData["Correo2FA"] = usuario.Correo;
            return RedirectToAction("Verificar2FA");
        }

        [HttpGet]
        public IActionResult Verificar2FA()
        {
            // Usamos Peek para no consumir el valor
            var correo = TempData.Peek("Correo2FA") as string;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("Login");

            // Pasamos el correo a la vista mediante ViewBag (para usarlo en un campo oculto)
            ViewBag.Correo = correo;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verificar2FA(string codigo, string correo)
        {
            // Si no recibimos correo del formulario, intentamos desde TempData (Peek)
            if (string.IsNullOrEmpty(correo))
                correo = TempData.Peek("Correo2FA") as string;

            Console.WriteLine("=== VERIFICACIÓN 2FA ===");
            Console.WriteLine($"Correo: '{correo}'");
            Console.WriteLine($"Código ingresado: '{codigo}'");

            if (string.IsNullOrEmpty(correo))
            {
                Console.WriteLine("Correo no disponible, redirigiendo a Login");
                return RedirectToAction("Login");
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
            {
                Console.WriteLine($"Usuario no encontrado: {correo}");
                TempData.Remove("Correo2FA");
                return RedirectToAction("Login");
            }

            Console.WriteLine($"Código guardado: {usuario.Codigo2FA}");
            Console.WriteLine($"Expiracion: {usuario.Codigo2FAExpiracion}");
            Console.WriteLine($"¿Expirado? {usuario.Codigo2FAExpiracion < DateTime.Now}");

            if (usuario.Codigo2FA != codigo || usuario.Codigo2FAExpiracion < DateTime.Now)
            {
                Console.WriteLine("Validación fallida");
                ModelState.AddModelError("", "Código inválido o expirado");
                ViewBag.Correo = correo; // Para volver a mostrar el campo oculto
                return View();
            }

            // Limpiar código
            usuario.Codigo2FA = null;
            usuario.Codigo2FAExpiracion = null;
            await _context.SaveChangesAsync();

            // Iniciar sesión
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nombre),
        new Claim(ClaimTypes.Email, usuario.Correo),
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);

            TempData.Remove("Correo2FA");
            Console.WriteLine("Verificación exitosa, redirigiendo a Home");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Deshabilitar registro público (opcional, puedes habilitarlo si quieres)
        [HttpGet]
        public IActionResult Registro() => RedirectToAction("Login");

        [HttpPost]
        public IActionResult Registro(RegistroViewModel model) => RedirectToAction("Login");

        // Método auxiliar para crear un usuario de prueba si la tabla está vacía o no existe el admin
        private async Task CrearUsuarioPruebaSiNoExiste()
        {
            // Verifica si ya existe el usuario admin con el correo correcto
            var admin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == "rossanasendoya@gmail.com");
            if (admin == null)
            {
                admin = new Usuario
                {
                    Nombre = "Administrador",
                    Correo = "rossanasendoya@gmail.com",
                    PasswordHash = _seguridad.GenerarHash("123456"), // Usa el método que ya tienes
                    Rol = "Admin",
                    FechaRegistro = DateTime.Now
                };
                _context.Usuarios.Add(admin);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario de prueba creado: rossanasendoya@gmail.com / 123456");
            }
            else
            {
                // Opcional: si existe pero el hash no es el correcto, lo actualiza
                string hashCorrecto = _seguridad.GenerarHash("123456");
                if (admin.PasswordHash != hashCorrecto)
                {
                    admin.PasswordHash = hashCorrecto;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Hash actualizado para el usuario admin");
                }
            }
        }
    }
}