using System.ComponentModel.DataAnnotations;

namespace FrontendAdministrativo.Models.ViewModels
{
    public class UTNGolCoinViewModel
    {
        [Display(Name = "Bono inicial")]
        [Range(0, 100000,
            ErrorMessage = "El bono debe ser mayor o igual a cero.")]
        public int BonoInicial { get; set; }

        [Display(Name = "Monedas por acierto")]
        [Range(1, 100000,
            ErrorMessage = "Ingrese una cantidad válida.")]
        public int MonedasPorAcierto { get; set; }

        [Display(Name = "Límite máximo de apuesta")]
        [Range(1, 1000000,
            ErrorMessage = "Ingrese un límite válido.")]
        public int LimiteMaximoApuesta { get; set; }

        [Display(Name = "Apuestas habilitadas")]
        public bool ApuestasHabilitadas { get; set; }

        public int MonedasEnCirculacion { get; set; }

        public int TotalApuestas { get; set; }

        public string PartidoMasApostado { get; set; } = "";
    }
}