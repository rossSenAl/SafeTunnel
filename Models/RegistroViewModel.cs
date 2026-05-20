using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su correo.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar una contraseña.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}