using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class SeleccionesController : Controller
    {
        private readonly EstadisticasApiService _estadisticasApiService;

        public SeleccionesController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService = estadisticasApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            string? grupo)
        {
            List<SeleccionApiDto>? respuestaApi =
                await _estadisticasApiService
                    .ObtenerSeleccionesAsync();

            ViewBag.ApiDisponible =
                respuestaApi is not null;

            List<SeleccionApiDto> selecciones =
                respuestaApi ?? new List<SeleccionApiDto>();
            foreach (SeleccionApiDto seleccion in selecciones)
            {
                seleccion.CodigoBandera =
                    ConvertirCodigoBandera(
                        seleccion.CodigoPais);
            }

            // Lista de grupos para el filtro.
            ViewBag.Grupos = selecciones
                .Select(seleccion =>
                    NormalizarGrupo(seleccion.Grupo))
                .Where(grupoActual =>
                    !string.IsNullOrWhiteSpace(grupoActual))
                .Distinct()
                .OrderBy(grupoActual => grupoActual)
                .ToList();

            IEnumerable<SeleccionApiDto> consulta =
                selecciones;

            // Buscar por nombre o código.
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(seleccion =>
                    seleccion.Nombre.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    seleccion.CodigoPais.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por grupo.
            if (!string.IsNullOrWhiteSpace(grupo))
            {
                consulta = consulta.Where(seleccion =>
                    string.Equals(
                        NormalizarGrupo(seleccion.Grupo),
                        grupo,
                        StringComparison.OrdinalIgnoreCase));
            }

            List<SeleccionApiDto> modelo = consulta
                .OrderBy(seleccion =>
                    NormalizarGrupo(seleccion.Grupo))
                .ThenBy(seleccion =>
                    seleccion.Nombre)
                .ToList();

            ViewBag.Buscar = buscar;
            ViewBag.GrupoSeleccionado = grupo;
            ViewBag.TotalSelecciones = modelo.Count;

            return View(modelo);
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
        private static string ConvertirCodigoBandera(
    string? codigoPais)
        {
            if (string.IsNullOrWhiteSpace(codigoPais))
            {
                return string.Empty;
            }

            string codigo = codigoPais
                .Trim()
                .ToUpperInvariant();

            var codigosEspeciales =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["ENG"] = "gb-eng",
                    ["SCO"] = "gb-sct",
                    ["WAL"] = "gb-wls",
                    ["NIR"] = "gb-nir",

                    ["RSA"] = "za",
                    ["SUI"] = "ch",
                    ["GER"] = "de",
                    ["NED"] = "nl",
                    ["CRO"] = "hr",
                    ["GRE"] = "gr",
                    ["POR"] = "pt",
                    ["DEN"] = "dk",

                    ["PAR"] = "py",
                    ["URU"] = "uy",
                    ["CHI"] = "cl",
                    ["CRC"] = "cr",
                    ["HAI"] = "ht",
                    ["HON"] = "hn",
                    ["TRI"] = "tt",

                    ["KSA"] = "sa",
                    ["UAE"] = "ae",
                    ["CZE"] = "cz",
                    ["SVN"] = "si",
                    ["BIH"] = "ba",

                    ["MAR"] = "ma",
                    ["ALG"] = "dz",
                    ["NGA"] = "ng",
                    ["CMR"] = "cm",
                    ["CGO"] = "cg",
                    ["COD"] = "cd",
                    ["KOS"] = "xk"
                };

            if (codigosEspeciales.TryGetValue(
                codigo,
                out string? codigoEspecial))
            {
                return codigoEspecial;
            }

            foreach (CultureInfo cultura in
                CultureInfo.GetCultures(
                    CultureTypes.SpecificCultures))
            {
                try
                {
                    var region = new RegionInfo(
                        cultura.Name);

                    if (string.Equals(
                        region.ThreeLetterISORegionName,
                        codigo,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return region
                            .TwoLetterISORegionName
                            .ToLowerInvariant();
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return string.Empty;
        }
    }
}