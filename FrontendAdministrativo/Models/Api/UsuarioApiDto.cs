namespace FrontendAdministrativo.Models.Api
{
    public class UsuarioApiDto
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Nombre { get; set; }

        public string? Email { get; set; }

        public RolUsuarioApiDto? Rol { get; set; }

        
        public List<int>? FechaCreacion { get; set; }

        public bool Activo { get; set; }
    }

    public class RolUsuarioApiDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string? Descripcion { get; set; }
    }
}