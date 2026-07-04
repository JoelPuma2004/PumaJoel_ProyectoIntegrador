namespace FrontendAdministrativo.Models.ViewModels
{
    public class PartidosIndexViewModel
    {
        public List<PartidoDto> Partidos { get; set; } = new();

        public string GrupoSeleccionado { get; set; } = string.Empty;

        public string EstadoSeleccionado { get; set; } = string.Empty;

        public int TotalPartidos { get; set; }

        public int Programados { get; set; }

        public int EnJuego { get; set; }

        public int Finalizados { get; set; }
    }
}