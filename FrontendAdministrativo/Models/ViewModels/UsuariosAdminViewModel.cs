using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class UsuarioAdminViewModel
    {
        public int Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "USUARIO";

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }

    public class UsuariosIndexViewModel
    {
        public bool ApiDisponible { get; set; }

        public List<UsuarioAdminViewModel> Usuarios { get; set; }
            = new();

        public string? Buscar { get; set; }

        public string? RolSeleccionado { get; set; }

        public string? EstadoSeleccionado { get; set; }

        public int TotalUsuarios { get; set; }

        public int UsuariosActivos { get; set; }

        public int Administradores { get; set; }
    }

    public class EditarUsuarioViewModel
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; }
            = string.Empty;

        public string Correo { get; set; }
            = string.Empty;

        [Required(ErrorMessage = "Seleccione un rol.")]
        public string Rol { get; set; } = "USUARIO";

        public bool Activo { get; set; }
    }
}