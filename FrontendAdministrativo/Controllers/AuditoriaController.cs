using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class AuditoriaController : Controller
    {
        private readonly EstadisticasApiService
            _estadisticasApiService;

        public AuditoriaController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService =
                estadisticasApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            string? modulo)
        {
            List<AuditoriaApiDto>? respuestaApi =
                await _estadisticasApiService
                    .ObtenerAuditoriaAsync();

            if (respuestaApi is null)
            {
                TempData["MensajeError"] =
                    "No fue posible consultar la auditoría. " +
                    "Revise que el servicio de Andrea esté disponible.";

                respuestaApi =
                    new List<AuditoriaApiDto>();
            }

            List<AuditoriaViewModel> todosLosRegistros =
                respuestaApi
                    .Select(ConvertirAuditoria)
                    .ToList();

            IEnumerable<AuditoriaViewModel> registros =
                todosLosRegistros;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                registros = registros.Where(registro =>
                    registro.Usuario.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    registro.Accion.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    registro.Descripcion.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                registros = registros.Where(registro =>
                    registro.Modulo.Equals(
                        modulo,
                        StringComparison.OrdinalIgnoreCase));
            }

            List<AuditoriaViewModel> resultado =
                registros
                    .OrderByDescending(registro =>
                        registro.FechaHora)
                    .ToList();

            var modelo = new AuditoriaIndexViewModel
            {
                Registros =
                    resultado,

                Buscar =
                    buscar,

                ModuloSeleccionado =
                    modulo,

                TotalRegistros =
                    resultado.Count,

                UltimaActividad =
                    resultado
                        .FirstOrDefault()?
                        .FechaHora
            };

            return View(modelo);
        }

        private static AuditoriaViewModel ConvertirAuditoria(
            AuditoriaApiDto auditoriaApi)
        {
            return new AuditoriaViewModel
            {
                Id =
                    auditoriaApi.Id,

                Usuario =
                    ObtenerUsuarioVisible(
                        auditoriaApi.Usuario),

                Accion =
                    auditoriaApi.Accion?
                        .Trim()
                    ?? "Sin acción",

                Modulo =
                    ConvertirModulo(
                        auditoriaApi.Entidad),

                Descripcion =
                    auditoriaApi.Detalle?
                        .Trim()
                    ?? "Sin detalle",

                FechaHora =
                    ConvertirFecha(
                        auditoriaApi.FechaEvento),

                // Andrea no devuelve la dirección IP.
                DireccionIp =
                    "No disponible"
            };
        }

        private static string ObtenerUsuarioVisible(
            UsuarioApiDto? usuario)
        {
            if (usuario is null)
            {
                return "No identificado";
            }

            if (!string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                return usuario.Nombre.Trim();
            }

            if (!string.IsNullOrWhiteSpace(usuario.Username))
            {
                return usuario.Username.Trim();
            }

            return $"Usuario {usuario.Id}";
        }

        private static string ConvertirModulo(
            string? entidad)
        {
            string valor =
                entidad?
                    .Trim()
                    .ToUpperInvariant()
                ?? string.Empty;

            return valor switch
            {
                "USUARIO" =>
                    "Usuarios",

                "SESION" =>
                    "Seguridad",

                "PARTIDO" =>
                    "Partidos",

                _ when !string.IsNullOrWhiteSpace(entidad) =>
                    entidad.Trim(),

                _ =>
                    "General"
            };
        }

        private static DateTime ConvertirFecha(
            List<int>? valores)
        {
            if (valores is null ||
                valores.Count < 3)
            {
                return DateTime.MinValue;
            }

            try
            {
                int anio = valores[0];
                int mes = valores[1];
                int dia = valores[2];

                int hora =
                    valores.Count > 3
                        ? valores[3]
                        : 0;

                int minuto =
                    valores.Count > 4
                        ? valores[4]
                        : 0;

                int segundo =
                    valores.Count > 5
                        ? valores[5]
                        : 0;

                int milisegundo =
                    valores.Count > 6
                        ? valores[6] / 1_000_000
                        : 0;

                return new DateTime(
                    anio,
                    mes,
                    dia,
                    hora,
                    minuto,
                    segundo,
                    milisegundo,
                    DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }
    }
}