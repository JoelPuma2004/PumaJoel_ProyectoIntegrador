using System.Net.Http.Json;
using System.Text.Json;
using FrontendAdministrativo.Models.Api;

namespace FrontendAdministrativo.Services
{
    public class EstadisticasApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstadisticasApiService> _logger;

        public EstadisticasApiService(
            HttpClient httpClient,
            ILogger<EstadisticasApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<PartidoApiDto>?> ObtenerPartidosAsync(
            string? grupo = null,
            string? estado = null)
        {
            try
            {
                var parametros = new List<string>();

                if (!string.IsNullOrWhiteSpace(grupo))
                {
                    parametros.Add(
                        $"grupo={Uri.EscapeDataString(grupo)}");
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    parametros.Add(
                        $"estado={Uri.EscapeDataString(estado)}");
                }

                string ruta = "partidos";

                if (parametros.Count > 0)
                {
                    ruta += "?" + string.Join("&", parametros);
                }

                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync(ruta);

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<PartidoApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible conectarse con la API de Estadísticas.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La API de Estadísticas tardó demasiado en responder.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de partidos no tiene el formato esperado.");

                return null;
            }
        }

        public async Task<PartidoApiDto?> ObtenerPartidoPorIdAsync(
            int partidoId)
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync(
                        $"partidos/{partidoId}");

                if (respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<PartidoApiDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible obtener el partido {PartidoId}.",
                    partidoId);

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La API tardó demasiado al obtener el partido {PartidoId}.",
                    partidoId);

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "El partido {PartidoId} no tiene el formato esperado.",
                    partidoId);

                return null;
            }
        }

        public async Task<bool> RegistrarResultadoAsync(
            int partidoId,
            int golesLocal,
            int golesVisitante)
        {
            try
            {
                var resultado = new
                {
                    golesLocal,
                    golesVisitante
                };

                using HttpResponseMessage respuesta =
                    await _httpClient.PutAsJsonAsync(
                        $"partidos/{partidoId}/resultado",
                        resultado);

                return respuesta.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible registrar el resultado del partido {PartidoId}.",
                    partidoId);

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La API tardó demasiado al registrar el resultado del partido {PartidoId}.",
                    partidoId);

                return false;
            }
        }
    }
}