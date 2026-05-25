using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Rol { get; set; } = "Usuario";

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Campos para 2FA
        public string? Codigo2FA { get; set; }
        public DateTime? Codigo2FAExpiracion { get; set; }
    }
}