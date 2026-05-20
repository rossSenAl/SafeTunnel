using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Models;
using System.Security.Claims;

namespace SafeTunnel.Controllers
{
    public class CuentaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public CuentaController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == modelo.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError("", "Este correo ya está registrado.");
                return View(modelo);
            }

            var usuario = new Usuario
            {
                Nombre = modelo.Nombre,
                Correo = modelo.Correo,
                Rol = "Usuario",
                FechaRegistro = DateTime.Now
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, modelo.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Cuenta");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == modelo.Correo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(modelo);
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                modelo.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(modelo);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Simulador", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}