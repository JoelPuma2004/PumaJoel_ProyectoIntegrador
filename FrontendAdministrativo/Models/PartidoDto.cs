using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models
{
    public class PartidoDto
    {
        public int Id { get; set; }

        [Display(Name = "Número de partido")]
        public int NumeroPartidoFifa { get; set; }

        public string Fase { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;

        [Display(Name = "Selección local")]
        public string SeleccionLocal { get; set; } = string.Empty;

        [Display(Name = "Selección visitante")]
        public string SeleccionVisitante { get; set; } = string.Empty;

        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        public string Sede { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public int? GolesLocal { get; set; }

        public int? GolesVisitante { get; set; }
    }
}