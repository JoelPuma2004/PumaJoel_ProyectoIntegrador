using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class PartidoFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el número del partido.")]
        [Range(1, 104, ErrorMessage = "El número debe estar entre 1 y 104.")]
        [Display(Name = "Número del partido")]
        public int NumeroPartidoFifa { get; set; }

        [Required(ErrorMessage = "Seleccione la fase.")]
        [Display(Name = "Fase")]
        public string Fase { get; set; } = "Fase de grupos";

        [Display(Name = "Grupo")]
        public string? Grupo { get; set; }

        [Required(ErrorMessage = "Ingrese la selección local.")]
        [Display(Name = "Selección local")]
        public string SeleccionLocal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la selección visitante.")]
        [Display(Name = "Selección visitante")]
        public string SeleccionVisitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la fecha y hora.")]
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "Ingrese la sede.")]
        [Display(Name = "Sede")]
        public string Sede { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "PROGRAMADO";

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(SeleccionLocal) &&
                !string.IsNullOrWhiteSpace(SeleccionVisitante) &&
                SeleccionLocal.Trim().Equals(
                    SeleccionVisitante.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Una selección no puede jugar contra sí misma.",
                    new[]
                    {
                        nameof(SeleccionLocal),
                        nameof(SeleccionVisitante)
                    });
            }

            if (Fase == "Fase de grupos" &&
                string.IsNullOrWhiteSpace(Grupo))
            {
                yield return new ValidationResult(
                    "Debe seleccionar un grupo para la fase de grupos.",
                    new[] { nameof(Grupo) });
            }
        }
    }
}