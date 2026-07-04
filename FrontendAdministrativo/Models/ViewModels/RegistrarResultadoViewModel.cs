using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class RegistrarResultadoViewModel
    {
        public int PartidoId { get; set; }

        public int NumeroPartidoFifa { get; set; }

        public string SeleccionLocal { get; set; } = string.Empty;

        public string SeleccionVisitante { get; set; } = string.Empty;

        [Display(Name = "Goles del equipo local")]
        [Required(ErrorMessage = "Ingrese los goles del equipo local.")]
        [Range(0, 99, ErrorMessage = "Los goles deben estar entre 0 y 99.")]
        public int GolesLocal { get; set; }

        [Display(Name = "Goles del equipo visitante")]
        [Required(ErrorMessage = "Ingrese los goles del equipo visitante.")]
        [Range(0, 99, ErrorMessage = "Los goles deben estar entre 0 y 99.")]
        public int GolesVisitante { get; set; }
    }
}