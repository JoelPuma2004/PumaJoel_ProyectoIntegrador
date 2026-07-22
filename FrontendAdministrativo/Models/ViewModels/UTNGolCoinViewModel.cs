using FrontendAdministrativo.Models.Api;
using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class UTNGolCoinViewModel
    {
        [Display(Name = "Bono inicial")]
        [Range(
            typeof(decimal),
            "0",
            "100000",
            ErrorMessage =
                "El bono debe ser mayor o igual a cero.")]
        public decimal BonoInicial { get; set; }

        [Display(Name = "Monedas por acierto")]
        [Range(
            typeof(decimal),
            "1",
            "100000",
            ErrorMessage =
                "Ingrese una cantidad válida.")]
        public decimal MonedasPorAcierto { get; set; }

        [Display(Name = "Límite máximo de apuesta")]
        [Range(
            typeof(decimal),
            "1",
            "1000000",
            ErrorMessage =
                "Ingrese un límite válido.")]
        public decimal LimiteMaximoApuesta { get; set; }

        [Display(Name = "Apuestas habilitadas")]
        public bool ApuestasHabilitadas { get; set; }

        public decimal MonedasEnCirculacion { get; set; }

        public int TotalBilleteras { get; set; }

        public bool ServicioDisponible { get; set; }

        public bool ConfiguracionDisponible { get; set; }

        public List<RankingUTNGolCoinApiDto> Ranking { get; set; }
            = new();
    }
}