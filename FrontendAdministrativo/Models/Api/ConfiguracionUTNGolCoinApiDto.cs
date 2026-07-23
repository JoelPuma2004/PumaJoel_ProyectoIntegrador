namespace FrontendAdministrativo.Models.Api
{
    public class ConfiguracionUTNGolCoinApiDto
    {
        public decimal BonoInicial { get; set; }

        public decimal MonedasPorAcierto { get; set; }

        public decimal LimiteMaximoApuesta { get; set; }

        public bool ApuestasHabilitadas { get; set; }
    }
}