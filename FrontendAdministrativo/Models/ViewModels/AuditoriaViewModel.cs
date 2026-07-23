namespace FrontendAdministrativo.Models.ViewModels
{
    public class AuditoriaViewModel
    {
        public int Id { get; set; }

        public string Usuario { get; set; } = "";

        public string Accion { get; set; } = "";

        public string Modulo { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public DateTime FechaHora { get; set; }

        public string DireccionIp { get; set; } = "";
    }

    public class AuditoriaIndexViewModel
    {
        public bool ApiDisponible { get; set; }

        public List<AuditoriaViewModel> Registros { get; set; } = new();

        public string? Buscar { get; set; }

        public string? ModuloSeleccionado { get; set; }

        public int TotalRegistros { get; set; }

        public DateTime? UltimaActividad { get; set; }
    }
}