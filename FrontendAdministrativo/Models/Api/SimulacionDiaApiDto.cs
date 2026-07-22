using System.Text.Json.Serialization;

namespace FrontendAdministrativo.Models.Api
{
    public class SimulacionDiaApiDto
    {
        [JsonPropertyName("fechaSimulada")]
        public DateTime FechaSimulada { get; set; }

        [JsonPropertyName("usuariosBeneficiados")]
        public int UsuariosBeneficiados { get; set; }

        [JsonPropertyName("monedasEntregadas")]
        public decimal MonedasEntregadas { get; set; }
    }
}