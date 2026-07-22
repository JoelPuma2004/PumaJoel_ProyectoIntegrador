namespace FrontendAdministrativo.Models.Api
{
    public class PartidoApiDto
    {
        public int Id { get; set; }

        public int NumeroPartidoFifa { get; set; }

        public SeleccionApiDto? SeleccionLocal { get; set; }

        public SeleccionApiDto? SeleccionVisitante { get; set; }

        public DateTime FechaHora { get; set; }

        public string Sede { get; set; } = string.Empty;

        public string Fase { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public int? GolesLocal { get; set; }

        public int? GolesVisitante { get; set; }

        public CuotasPartidoApiDto? Cuotas { get; set; }
    }
}