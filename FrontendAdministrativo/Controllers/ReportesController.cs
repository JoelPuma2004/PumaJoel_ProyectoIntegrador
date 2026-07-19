using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class ReportesController : Controller
    {
        private readonly EstadisticasApiService _estadisticasApiService;

        public ReportesController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService = estadisticasApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<PartidoApiDto>? partidosApi =
                await _estadisticasApiService
                    .ObtenerPartidosAsync();

            if (partidosApi is null)
            {
                var modeloSinConexion =
                    new ReporteEstadisticasViewModel
                    {
                        ApiDisponible = false
                    };

                return View(modeloSinConexion);
            }

            List<PartidoApiDto> partidosFinalizados =
                partidosApi
                    .Where(partido =>
                        NormalizarEstado(partido.Estado)
                            == "FINALIZADO")
                    .ToList();

            List<PartidoApiDto> partidosConMarcador =
                partidosFinalizados
                    .Where(partido =>
                        partido.GolesLocal.HasValue &&
                        partido.GolesVisitante.HasValue)
                    .ToList();

            int totalGoles =
                partidosConMarcador.Sum(partido =>
                    partido.GolesLocal!.Value +
                    partido.GolesVisitante!.Value);

            double promedioGoles =
                partidosConMarcador.Count == 0
                    ? 0
                    : (double)totalGoles /
                      partidosConMarcador.Count;

            PartidoApiDto? partidoMasGoleador =
                partidosConMarcador
                    .OrderByDescending(partido =>
                        partido.GolesLocal!.Value +
                        partido.GolesVisitante!.Value)
                    .FirstOrDefault();

            var resumenPorGrupo =
                partidosApi
                    .Where(partido =>
                        !string.IsNullOrWhiteSpace(
                            partido.Grupo))
                    .GroupBy(partido =>
                        NormalizarGrupo(partido.Grupo))
                    .Where(grupo =>
                        !string.IsNullOrWhiteSpace(
                            grupo.Key))
                    .Select(grupo =>
                        new ReporteGrupoViewModel
                        {
                            Grupo = grupo.Key,

                            Partidos =
                                grupo.Count(),

                            Finalizados =
                                grupo.Count(partido =>
                                    NormalizarEstado(
                                        partido.Estado)
                                    == "FINALIZADO"),

                            Goles =
                                grupo.Sum(partido =>
                                    (partido.GolesLocal ?? 0) +
                                    (partido.GolesVisitante ?? 0))
                        })
                    .OrderBy(grupo =>
                        grupo.Grupo)
                    .ToList();

            var ultimosResultados =
                partidosConMarcador
                    .OrderByDescending(partido =>
                        partido.FechaHora)
                    .Take(10)
                    .Select(partido =>
                        new ReportePartidoViewModel
                        {
                            PartidoId =
                                partido.Id,

                            SeleccionLocal =
                                partido.SeleccionLocal?.Nombre
                                ?? "Por definir",

                            SeleccionVisitante =
                                partido.SeleccionVisitante?.Nombre
                                ?? "Por definir",

                            GolesLocal =
                                partido.GolesLocal ?? 0,

                            GolesVisitante =
                                partido.GolesVisitante ?? 0,

                            FechaHora =
                                partido.FechaHora,

                            Fase =
                                NormalizarFase(
                                    partido.Fase)
                        })
                    .ToList();

            var modelo =
                new ReporteEstadisticasViewModel
                {
                    ApiDisponible = true,

                    TotalPartidos =
                        partidosApi.Count,

                    Programados =
                        partidosApi.Count(partido =>
                            NormalizarEstado(
                                partido.Estado)
                            == "PROGRAMADO"),

                    EnJuego =
                        partidosApi.Count(partido =>
                            NormalizarEstado(
                                partido.Estado)
                            == "EN JUEGO"),

                    Finalizados =
                        partidosFinalizados.Count,

                    TotalGoles =
                        totalGoles,

                    PromedioGoles =
                        promedioGoles,

                    PartidoMasGoleador =
                        partidoMasGoleador is null
                            ? "Sin información"
                            : CrearNombrePartido(
                                partidoMasGoleador),

                    GolesPartidoMasGoleador =
                        partidoMasGoleador is null
                            ? 0
                            : (partidoMasGoleador
                                .GolesLocal ?? 0)
                              +
                              (partidoMasGoleador
                                .GolesVisitante ?? 0),

                    ResumenPorGrupo =
                        resumenPorGrupo,

                    UltimosResultados =
                        ultimosResultados
                };

            return View(modelo);
        }

        private static string NormalizarEstado(
            string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return "PROGRAMADO";
            }

            return estado
                .Trim()
                .ToUpperInvariant()
                .Replace("_", " ");
        }

        private static string NormalizarGrupo(
            string? grupo)
        {
            if (string.IsNullOrWhiteSpace(grupo))
            {
                return string.Empty;
            }

            return grupo
                .Replace(
                    "GRUPO_",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "Grupo",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarFase(
            string? fase)
        {
            if (string.IsNullOrWhiteSpace(fase))
            {
                return "Por definir";
            }

            string faseLimpia =
                fase.Trim()
                    .ToLowerInvariant()
                    .Replace("_", " ");

            return char.ToUpper(faseLimpia[0])
                   + faseLimpia[1..];
        }

        private static string CrearNombrePartido(
            PartidoApiDto partido)
        {
            string local =
                partido.SeleccionLocal?.Nombre
                ?? "Por definir";

            string visitante =
                partido.SeleccionVisitante?.Nombre
                ?? "Por definir";

            int golesLocal =
                partido.GolesLocal ?? 0;

            int golesVisitante =
                partido.GolesVisitante ?? 0;

            return
                $"{local} {golesLocal} - " +
                $"{golesVisitante} {visitante}";
        }
    }
}