using System.ComponentModel.DataAnnotations;

namespace SafeTunnel.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su correo.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su contraseña.")]
        public string Password { get; set; } = string.Empty;
    }
}