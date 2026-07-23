using FrontendAdministrativo.Models.Api;
using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class PartidoFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el número del partido.")]
        [Range(
            89,
            104,
            ErrorMessage = "Solo se permiten partidos desde octavos hasta la final (89 a 104).")]
        [Display(Name = "Número del partido")]
        public int NumeroPartidoFifa { get; set; }

        [Required]
        public string Fase { get; set; } = "OCTAVOS";

        // Propiedades antiguas conservadas para no afectar Editar.
        public string? Grupo { get; set; }
        public string SeleccionLocal { get; set; } = string.Empty;
        public string SeleccionVisitante { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string Estado { get; set; } = "PROGRAMADO";

        [Required(ErrorMessage = "Seleccione el Equipo 1.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Seleccione el Equipo 1.")]
        [Display(Name = "Equipo 1")]
        public int? SeleccionLocalId { get; set; }

        [Required(ErrorMessage = "Seleccione el Equipo 2.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Seleccione el Equipo 2.")]
        [Display(Name = "Equipo 2")]
        public int? SeleccionVisitanteId { get; set; }

        [Required(ErrorMessage = "Seleccione una sede.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Seleccione una sede.")]
        [Display(Name = "Sede")]
        public int? SedeId { get; set; }

        [Display(Name = "Grupo")]
        public int? GrupoId { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha y hora.")]
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; } =
            DateTime.Now.AddDays(1);

        [Display(Name = "Cuota Equipo 1")]
        [Range(
            typeof(decimal),
            "1",
            "1000",
            ErrorMessage = "Ingrese una cuota válida.")]
        public decimal CuotaLocal { get; set; } = 1.50m;

        [Display(Name = "Cuota empate")]
        [Range(
            typeof(decimal),
            "1",
            "1000",
            ErrorMessage = "Ingrese una cuota válida.")]
        public decimal CuotaEmpate { get; set; } = 2.50m;

        [Display(Name = "Cuota Equipo 2")]
        [Range(
            typeof(decimal),
            "1",
            "1000",
            ErrorMessage = "Ingrese una cuota válida.")]
        public decimal CuotaVisitante { get; set; } = 1.80m;

        public bool EquiposAsignadosAutomaticamente { get; set; }

        public bool PuedeCrearPartido { get; set; } = true;

        public bool CatalogosDisponibles { get; set; }

        public string? MensajeClasificacion { get; set; }

        public List<SeleccionApiDto> Selecciones { get; set; }
            = new();

        public List<SeleccionApiDto> SeleccionesLocalDisponibles { get; set; }
            = new();

        public List<SeleccionApiDto> SeleccionesVisitanteDisponibles { get; set; }
            = new();

        public List<SedeApiDto> Sedes { get; set; }
            = new();

        public List<GrupoApiDto> Grupos { get; set; }
            = new();
    }
}