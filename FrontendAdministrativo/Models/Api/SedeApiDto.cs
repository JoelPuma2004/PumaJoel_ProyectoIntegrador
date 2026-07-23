namespace FrontendAdministrativo.Models.Api
{
    public class SedeApiDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Ciudad { get; set; } = string.Empty;

        public string Pais { get; set; } = string.Empty;

        public int Capacidad { get; set; }
    }
}