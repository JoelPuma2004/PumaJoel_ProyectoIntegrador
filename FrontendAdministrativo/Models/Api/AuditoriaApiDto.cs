namespace FrontendAdministrativo.Models.Api
{
    public class AuditoriaApiDto
    {
        public int Id { get; set; }

        public string? Accion { get; set; }

        public string? Detalle { get; set; }

        public string? Entidad { get; set; }

        public int? EntidadId { get; set; }

        public List<int>? FechaEvento { get; set; }

        public UsuarioApiDto? Usuario { get; set; }
    }
}