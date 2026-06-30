namespace FrontendAdministrativo.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalSelecciones { get; set; }
        public int TotalPartidos { get; set; }
        public int PartidosFinalizados { get; set; }
        public int TotalUsuarios { get; set; }

        public bool ApiEstadisticasDisponible { get; set; }
        public bool ApiUtnGolCoinDisponible { get; set; }

        public List<ActividadRecienteViewModel> ActividadesRecientes { get; set; }
            = new List<ActividadRecienteViewModel>();
    }

    public class ActividadRecienteViewModel
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}