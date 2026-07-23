using System.Net;
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
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("partidos");

                respuesta.EnsureSuccessStatusCode();

                List<PartidoApiDto>? partidos =
                    await respuesta.Content
                        .ReadFromJsonAsync<List<PartidoApiDto>>();

                if (partidos is null)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(grupo))
                {
                    string grupoBuscado =
                        NormalizarGrupo(grupo);

                    partidos = partidos
                        .Where(partido =>
                            NormalizarGrupo(partido.Grupo) ==
                            grupoBuscado)
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    string estadoBuscado =
                        NormalizarEstado(estado);

                    partidos = partidos
                        .Where(partido =>
                            NormalizarEstado(partido.Estado) ==
                            estadoBuscado)
                        .ToList();
                }

                return partidos;
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

                if (respuesta.StatusCode == HttpStatusCode.NotFound)
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

                if (respuesta.IsSuccessStatusCode)
                {
                    return true;
                }

                string contenidoError =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "La API respondió con código {Codigo} al registrar " +
                    "el resultado del partido {PartidoId}. Respuesta: {Respuesta}",
                    respuesta.StatusCode,
                    partidoId,
                    contenidoError);

                // Aunque la API responda con error,
                // verificamos si el resultado sí quedó guardado.
                return await VerificarResultadoGuardadoAsync(
                    partidoId,
                    golesLocal,
                    golesVisitante);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible registrar el resultado del partido {PartidoId}.",
                    partidoId);

                return await VerificarResultadoGuardadoAsync(
                    partidoId,
                    golesLocal,
                    golesVisitante);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La API tardó demasiado al registrar el resultado " +
                    "del partido {PartidoId}.",
                    partidoId);

                return await VerificarResultadoGuardadoAsync(
                    partidoId,
                    golesLocal,
                    golesVisitante);
            }
        }

        public async Task<List<SeleccionApiDto>?>
            ObtenerSeleccionesAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("selecciones");

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<SeleccionApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible obtener las selecciones desde la API.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La API tardó demasiado al consultar las selecciones.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de selecciones no tiene el formato esperado.");

                return null;
            }
        }
        public async Task<List<SedeApiDto>?> ObtenerSedesAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("sedes");

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<SedeApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible obtener las sedes.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de sedes tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de sedes no tiene el formato esperado.");

                return null;
            }
        }

        public async Task<List<GrupoApiDto>?> ObtenerGruposAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("grupos");

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<GrupoApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible obtener los grupos.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de grupos tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de grupos no tiene el formato esperado.");

                return null;
            }
        }

        public async Task<bool> CrearPartidoAsync(
            CrearPartidoApiDto partido)
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.PostAsJsonAsync(
                        "partidos",
                        partido);

                if (respuesta.IsSuccessStatusCode)
                {
                    return true;
                }

                string contenidoError =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "La API no pudo crear el partido. " +
                    "Código: {Codigo}. Respuesta: {Respuesta}",
                    respuesta.StatusCode,
                    contenidoError);

                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible conectarse para crear el partido.");

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La creación del partido tardó demasiado.");

                return false;
            }
        }
        public async Task<List<UsuarioApiDto>?> ObtenerUsuariosAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("usuarios");

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<UsuarioApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible consultar los usuarios.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de usuarios tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de usuarios no tiene el formato esperado.");

                return null;
            }
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(
            int usuarioId,
            bool activo)
        {
            try
            {
                var solicitud = new
                {
                    activo
                };

                using HttpResponseMessage respuesta =
                    await _httpClient.PutAsJsonAsync(
                        $"usuarios/{usuarioId}/estado",
                        solicitud);

                if (respuesta.IsSuccessStatusCode)
                {
                    return true;
                }

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "No se pudo cambiar el estado del usuario {UsuarioId}. " +
                    "Código: {Codigo}. Respuesta: {Respuesta}",
                    usuarioId,
                    respuesta.StatusCode,
                    contenido);

                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible cambiar el estado del usuario {UsuarioId}.",
                    usuarioId);

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La actualización del usuario {UsuarioId} tardó demasiado.",
                    usuarioId);

                return false;
            }
        }

        public async Task<bool> CambiarRolUsuarioAsync(
            int usuarioId,
            int rolId)
        {
            try
            {
                var solicitud = new
                {
                    rolId
                };

                using HttpResponseMessage respuesta =
                    await _httpClient.PutAsJsonAsync(
                        $"usuarios/{usuarioId}/rol",
                        solicitud);

                if (respuesta.IsSuccessStatusCode)
                {
                    return true;
                }

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "No se pudo cambiar el rol del usuario {UsuarioId}. " +
                    "Código: {Codigo}. Respuesta: {Respuesta}",
                    usuarioId,
                    respuesta.StatusCode,
                    contenido);

                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible cambiar el rol del usuario {UsuarioId}.",
                    usuarioId);

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La actualización del rol del usuario {UsuarioId} " +
                    "tardó demasiado.",
                    usuarioId);

                return false;
            }
        }
        public async Task<List<AuditoriaApiDto>?> ObtenerAuditoriaAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync("auditoria");

                respuesta.EnsureSuccessStatusCode();

                return await respuesta.Content
                    .ReadFromJsonAsync<List<AuditoriaApiDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible consultar la auditoría.");

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de auditoría tardó demasiado.");

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "La respuesta de auditoría no tiene el formato esperado.");

                return null;
            }
        }
        public async Task<(bool Exito, string Mensaje)>
    EliminarPartidoAsync(int partidoId)
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.DeleteAsync(
                        $"partidos/{partidoId}");

                if (respuesta.IsSuccessStatusCode)
                {
                    return (
                        true,
                        "El partido fue eliminado correctamente."
                    );
                }

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "No se pudo eliminar el partido {PartidoId}. " +
                    "Código: {Codigo}. Respuesta: {Respuesta}",
                    partidoId,
                    respuesta.StatusCode,
                    contenido);

                string mensaje =
                    string.IsNullOrWhiteSpace(contenido)
                        ? "La API no permitió eliminar el partido."
                        : contenido;

                return (false, mensaje);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible conectarse para eliminar " +
                    "el partido {PartidoId}.",
                    partidoId);

                return (
                    false,
                    "No fue posible conectarse con el Servicio de Estadísticas."
                );
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La eliminación del partido {PartidoId} " +
                    "tardó demasiado.",
                    partidoId);

                return (
                    false,
                    "La API tardó demasiado en responder."
                );
            }
        }
        public async Task<bool> EstaDisponibleAsync()
        {
            try
            {
                using HttpResponseMessage respuesta =
                    await _httpClient.GetAsync(
                        "selecciones",
                        HttpCompletionOption.ResponseHeadersRead);

                return respuesta.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "El Servicio de Estadísticas no está disponible.");

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(
                    ex,
                    "El Servicio de Estadísticas tardó demasiado en responder.");

                return false;
            }
        }

        private static string NormalizarGrupo(
            string? grupo)
        {
            if (string.IsNullOrWhiteSpace(grupo))
            {
                return string.Empty;
            }

            string resultado =
                grupo.Trim().ToUpperInvariant();

            if (resultado.StartsWith("GRUPO "))
            {
                resultado =
                    resultado.Substring("GRUPO ".Length);
            }

            return resultado.Trim();
        }

        private static string NormalizarEstado(
            string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return string.Empty;
            }

            return estado
                .Trim()
                .Replace(" ", "_")
                .Replace("-", "_")
                .ToUpperInvariant();
        }
        private async Task<bool> VerificarResultadoGuardadoAsync(
    int partidoId,
    int golesLocal,
    int golesVisitante)
        {
            try
            {
                // Primero intenta consultar el partido individual.
                using HttpResponseMessage respuestaIndividual =
                    await _httpClient.GetAsync(
                        $"partidos/{partidoId}");

                if (respuestaIndividual.IsSuccessStatusCode)
                {
                    PartidoApiDto? partido =
                        await respuestaIndividual.Content
                            .ReadFromJsonAsync<PartidoApiDto>();

                    if (partido is not null &&
                        partido.GolesLocal == golesLocal &&
                        partido.GolesVisitante == golesVisitante)
                    {
                        return true;
                    }
                }

                // Si el endpoint individual falla,
                // busca el partido dentro de los 104 partidos.
                using HttpResponseMessage respuestaListado =
                    await _httpClient.GetAsync("partidos");

                if (!respuestaListado.IsSuccessStatusCode)
                {
                    return false;
                }

                List<PartidoApiDto>? partidos =
                    await respuestaListado.Content
                        .ReadFromJsonAsync<List<PartidoApiDto>>();

                if (partidos is null)
                {
                    return false;
                }

                PartidoApiDto? partidoEncontrado =
                    partidos.FirstOrDefault(
                        partido => partido.Id == partidoId);

                return partidoEncontrado is not null &&
                       partidoEncontrado.GolesLocal == golesLocal &&
                       partidoEncontrado.GolesVisitante == golesVisitante;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible comprobar el resultado del partido {PartidoId}.",
                    partidoId);

                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(
                    ex,
                    "La comprobación del partido {PartidoId} tardó demasiado.",
                    partidoId);

                return false;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "La respuesta del partido {PartidoId} no tiene el formato esperado.",
                    partidoId);

                return false;
            }
        }
    }
}