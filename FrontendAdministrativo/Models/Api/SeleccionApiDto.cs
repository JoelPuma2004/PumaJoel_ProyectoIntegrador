namespace FrontendAdministrativo.Models.Api
{
    public class SeleccionApiDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string CodigoPais { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;

        public string CodigoBandera { get; set; } = string.Empty;
    }
}