using FrontendAdministrativo.Models.Api;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontendAdministrativo.Services
{
    public class UTNGolCoinApiService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<UTNGolCoinApiService>
            _logger;

        public UTNGolCoinApiService(
            HttpClient httpClient,
            ILogger<UTNGolCoinApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ==========================================
        // RANKING
        // ==========================================

        public async Task<List<RankingUTNGolCoinApiDto>?>
            ObtenerRankingAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("ranking");

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "UTNGolCoin respondió con código {Codigo}.",
                        respuesta.StatusCode);

                    return null;
                }

                return await respuesta.Content
                    .ReadFromJsonAsync<
                        List<RankingUTNGolCoinApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible consultar el ranking.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta del ranking tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "El ranking no tiene el formato esperado.");

                return null;
            }
        }

        // ==========================================
        // OBTENER CONFIGURACIÓN
        // ==========================================

        public async Task<ConfiguracionUTNGolCoinApiDto?>
            ObtenerConfiguracionAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("configuracion");

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "No se pudo obtener la configuración. " +
                        "Código: {Codigo}.",
                        respuesta.StatusCode);

                    return null;
                }

                return await respuesta.Content
                    .ReadFromJsonAsync<
                        ConfiguracionUTNGolCoinApiDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible consultar la configuración.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de configuración tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La configuración no tiene el formato esperado.");

                return null;
            }
        }

        // ==========================================
        // ACTUALIZAR CONFIGURACIÓN
        // ==========================================

        public async Task<bool> ActualizarConfiguracionAsync(
            ConfiguracionUTNGolCoinApiDto configuracion)
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.PutAsJsonAsync(
                        "configuracion",
                        configuracion);

                if (respuesta.IsSuccessStatusCode)
                {
                    return true;
                }

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "No se pudo actualizar la configuración. " +
                    "Código: {Codigo}. Respuesta: {Respuesta}",
                    respuesta.StatusCode,
                    contenido);

                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible actualizar la configuración.");

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La actualización tardó demasiado.");

                return false;
            }
        }
        // ==========================================
        // SIMULAR AVANCE DE UN DÍA
        // ==========================================

        public async Task<SimulacionDiaApiDto?> AvanzarDiaAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.PostAsync(
                        "simulacion/avanzar-dia",
                        content: null);

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "No se pudo avanzar el día. " +
                        "Código: {Codigo}. Respuesta: {Respuesta}",
                        respuesta.StatusCode,
                        contenido);

                    return null;
                }

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    _logger.LogWarning(
                        "La simulación respondió sin información.");

                    return null;
                }

                return JsonSerializer.Deserialize<SimulacionDiaApiDto>(
                    contenido,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible ejecutar la simulación.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La simulación tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de simulación no tiene " +
                    "el formato esperado.");

                return null;
            }
        }
        public async Task<bool> EstaDisponibleAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync(
                        "configuracion",
                        HttpCompletionOption.ResponseHeadersRead);

                return respuesta.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "El servicio UTNGolCoin no está disponible.");

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(
                    ex,
                    "El servicio UTNGolCoin tardó demasiado en responder.");

                return false;
            }
        }
    }
}