using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(
            ErrorMessage = "Ingrese el correo del administrador."
        )]
        [EmailAddress(
            ErrorMessage = "Ingrese un correo válido."
        )]
        [Display(Name = "Correo electrónico")]
        public string Usuario { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Ingrese la contraseña."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Display(Name = "Mantener la sesión iniciada")]
        public bool Recordarme { get; set; }

        public string? ReturnUrl { get; set; }
    }
}