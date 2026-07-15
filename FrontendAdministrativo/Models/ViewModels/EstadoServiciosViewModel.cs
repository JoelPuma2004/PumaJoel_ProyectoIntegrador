namespace FrontendAdministrativo.Models.ViewModels
{
    public class EstadoServicioViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public string Tecnologia { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool Configurado { get; set; }

        public bool Disponible { get; set; }

        public string Estado
        {
            get
            {
                if (!Configurado)
                {
                    return "NO CONFIGURADO";
                }

                return Disponible
                    ? "DISPONIBLE"
                    : "NO DISPONIBLE";
            }
        }
    }

    public class EstadoServiciosViewModel
    {
        public List<EstadoServicioViewModel> Servicios { get; set; }
            = new();

        public DateTime FechaConsulta { get; set; }
            = DateTime.Now;
    }
}