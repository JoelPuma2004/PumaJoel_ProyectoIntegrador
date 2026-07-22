namespace FrontendAdministrativo.Models.Api
{
    public class CrearPartidoApiDto
    {
        public int NumeroPartidoFifa { get; set; }

        public int SeleccionLocalId { get; set; }

        public int SeleccionVisitanteId { get; set; }

        public int SedeId { get; set; }

        public int? GrupoId { get; set; }

        public DateTime FechaHora { get; set; }

        public string Fase { get; set; } = "GRUPOS";

        public decimal CuotaLocal { get; set; }

        public decimal CuotaEmpate { get; set; }

        public decimal CuotaVisitante { get; set; }
    }
}