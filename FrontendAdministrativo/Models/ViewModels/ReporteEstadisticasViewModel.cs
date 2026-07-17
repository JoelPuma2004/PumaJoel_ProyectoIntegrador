namespace FrontendAdministrativo.Models.ViewModels
{
    public class ReporteEstadisticasViewModel
    {
        public bool ApiDisponible { get; set; }

        public int TotalPartidos { get; set; }

        public int Programados { get; set; }

        public int EnJuego { get; set; }

        public int Finalizados { get; set; }

        public int TotalGoles { get; set; }

        public double PromedioGoles { get; set; }

        public string PartidoMasGoleador { get; set; }
            = "Sin información";

        public int GolesPartidoMasGoleador { get; set; }

        public List<ReporteGrupoViewModel> ResumenPorGrupo { get; set; }
            = new();

        public List<ReportePartidoViewModel> UltimosResultados { get; set; }
            = new();
    }

    public class ReporteGrupoViewModel
    {
        public string Grupo { get; set; } = string.Empty;

        public int Partidos { get; set; }

        public int Finalizados { get; set; }

        public int Goles { get; set; }
    }

    public class ReportePartidoViewModel
    {
        public int PartidoId { get; set; }

        public string SeleccionLocal { get; set; } = string.Empty;

        public string SeleccionVisitante { get; set; } = string.Empty;

        public int GolesLocal { get; set; }

        public int GolesVisitante { get; set; }

        public DateTime FechaHora { get; set; }

        public string Fase { get; set; } = string.Empty;
    }
}