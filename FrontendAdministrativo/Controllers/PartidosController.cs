using FrontendAdministrativo.Models;
using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;



namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class PartidosController : Controller
    {
        private readonly EstadisticasApiService _estadisticasApiService;

        public PartidosController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService = estadisticasApiService;
        }

        // Datos de respaldo cuando la API no está disponible.
        private static readonly List<PartidoDto> PartidosTemporales = new()
        {
            new PartidoDto
            {
                Id = 1,
                NumeroPartidoFifa = 1,
                Fase = "Fase de grupos",
                Grupo = "A",
                SeleccionLocal = "México",
                SeleccionVisitante = "Sudáfrica",
                FechaHora = new DateTime(2026, 6, 11, 14, 0, 0),
                Sede = "Estadio Azteca",
                Estado = "PROGRAMADO"
            },

            new PartidoDto
            {
                Id = 2,
                NumeroPartidoFifa = 2,
                Fase = "Fase de grupos",
                Grupo = "A",
                SeleccionLocal = "Corea del Sur",
                SeleccionVisitante = "República Checa",
                FechaHora = new DateTime(2026, 6, 11, 19, 0, 0),
                Sede = "Estadio Akron",
                Estado = "PROGRAMADO"
            },

            new PartidoDto
            {
                Id = 3,
                NumeroPartidoFifa = 3,
                Fase = "Fase de grupos",
                Grupo = "B",
                SeleccionLocal = "Canadá",
                SeleccionVisitante = "Suiza",
                FechaHora = new DateTime(2026, 6, 12, 16, 0, 0),
                Sede = "BMO Field",
                Estado = "EN JUEGO",
                GolesLocal = 1,
                GolesVisitante = 0
            },

            new PartidoDto
            {
                Id = 4,
                NumeroPartidoFifa = 4,
                Fase = "Fase de grupos",
                Grupo = "B",
                SeleccionLocal = "Qatar",
                SeleccionVisitante = "Bosnia y Herzegovina",
                FechaHora = new DateTime(2026, 6, 12, 20, 0, 0),
                Sede = "BC Place",
                Estado = "PROGRAMADO"
            },

            new PartidoDto
            {
                Id = 5,
                NumeroPartidoFifa = 5,
                Fase = "Fase de grupos",
                Grupo = "C",
                SeleccionLocal = "Brasil",
                SeleccionVisitante = "Marruecos",
                FechaHora = new DateTime(2026, 6, 13, 15, 0, 0),
                Sede = "MetLife Stadium",
                Estado = "FINALIZADO",
                GolesLocal = 2,
                GolesVisitante = 1
            },

            new PartidoDto
            {
                Id = 6,
                NumeroPartidoFifa = 6,
                Fase = "Fase de grupos",
                Grupo = "C",
                SeleccionLocal = "Haití",
                SeleccionVisitante = "Escocia",
                FechaHora = new DateTime(2026, 6, 13, 19, 0, 0),
                Sede = "Gillette Stadium",
                Estado = "FINALIZADO",
                GolesLocal = 0,
                GolesVisitante = 0
            }
        };

        // =========================================================
        // LISTADO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? grupo,
            string? estado)
        {
            string? grupoParaApi =
                ConvertirGrupoParaApi(grupo);

            string? estadoParaApi =
                ConvertirEstadoParaApi(estado);

            List<PartidoApiDto>? partidosApi =
                await _estadisticasApiService
                    .ObtenerPartidosAsync(
                        grupoParaApi,
                        estadoParaApi);

            List<PartidoDto> partidos;

            if (partidosApi is not null)
            {
                partidos = partidosApi
                    .Select(ConvertirPartidoApi)
                    .OrderBy(partido => partido.FechaHora)
                    .ToList();

                ViewBag.UsandoDatosTemporales = false;
            }
            else
            {
                IEnumerable<PartidoDto> consulta =
                    PartidosTemporales;

                if (!string.IsNullOrWhiteSpace(grupo))
                {
                    consulta = consulta.Where(partido =>
                        string.Equals(
                            partido.Grupo,
                            grupo,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    string estadoNormalizado =
                        ConvertirEstadoParaVista(estado);

                    consulta = consulta.Where(partido =>
                        string.Equals(
                            ConvertirEstadoParaVista(
                                partido.Estado),
                            estadoNormalizado,
                            StringComparison.OrdinalIgnoreCase));
                }

                partidos = consulta
                    .OrderBy(partido => partido.FechaHora)
                    .ToList();

                ViewBag.UsandoDatosTemporales = true;
            }

            var modelo = new PartidosIndexViewModel
            {
                Partidos = partidos,
                GrupoSeleccionado = grupo,
                EstadoSeleccionado = estado,
                TotalPartidos = partidos.Count,

                Programados = partidos.Count(partido =>
                    partido.Estado == "PROGRAMADO"),

                EnJuego = partidos.Count(partido =>
                    partido.Estado == "EN JUEGO"),

                Finalizados = partidos.Count(partido =>
                    partido.Estado == "FINALIZADO")
            };

            return View(modelo);
        }

        // =========================================================
        // DETALLES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
           
            PartidoApiDto? partidoApi =
                await _estadisticasApiService
                    .ObtenerPartidoPorIdAsync(id);

            if (partidoApi is null)
            {
                List<PartidoApiDto>? todosLosPartidos =
                    await _estadisticasApiService
                        .ObtenerPartidosAsync();

                partidoApi = todosLosPartidos?
                    .FirstOrDefault(partido =>
                        partido.Id == id);
            }

            if (partidoApi is not null)
            {
                PartidoDto partido =
                    ConvertirPartidoApi(partidoApi);

                return View(partido);
            }

            // Respaldo temporal cuando la API está apagada.
            PartidoDto? partidoTemporal =
                PartidosTemporales.FirstOrDefault(
                    partido => partido.Id == id);

            if (partidoTemporal is null)
            {
                return NotFound();
            }

            return View(partidoTemporal);
        }

        // =========================================================
        // CREAR PARTIDO TEMPORAL
        // =========================================================

        [HttpGet]
        public IActionResult Crear()
        {
            int siguienteNumero =
                PartidosTemporales.Count == 0
                    ? 1
                    : PartidosTemporales.Max(partido =>
                        partido.NumeroPartidoFifa) + 1;

            var modelo = new PartidoFormViewModel
            {
                NumeroPartidoFifa = siguienteNumero,
                Fase = "Fase de grupos",
                Grupo = "A",
                FechaHora = DateTime.Now.AddDays(1),
                Estado = "PROGRAMADO"
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            PartidoFormViewModel modelo)
        {
            bool numeroRepetido =
                PartidosTemporales.Any(partido =>
                    partido.NumeroPartidoFifa ==
                    modelo.NumeroPartidoFifa);

            if (numeroRepetido)
            {
                ModelState.AddModelError(
                    nameof(modelo.NumeroPartidoFifa),
                    "Ya existe un partido con ese número.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            int nuevoId =
                PartidosTemporales.Count == 0
                    ? 1
                    : PartidosTemporales.Max(partido =>
                        partido.Id) + 1;

            var nuevoPartido = new PartidoDto
            {
                Id = nuevoId,

                NumeroPartidoFifa =
                    modelo.NumeroPartidoFifa,

                Fase = modelo.Fase.Trim(),

                Grupo =
                    string.IsNullOrWhiteSpace(modelo.Grupo)
                        ? string.Empty
                        : modelo.Grupo
                            .Trim()
                            .ToUpperInvariant(),

                SeleccionLocal =
                    modelo.SeleccionLocal.Trim(),

                SeleccionVisitante =
                    modelo.SeleccionVisitante.Trim(),

                FechaHora = modelo.FechaHora,
                Sede = modelo.Sede.Trim(),

                Estado = ConvertirEstadoParaVista(
                    modelo.Estado)
            };

            PartidosTemporales.Add(nuevoPartido);

            TempData["MensajeExito"] =
                "El partido temporal fue creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDITAR PARTIDO TEMPORAL
        // =========================================================

        [HttpGet]
        public IActionResult Editar(int id)
        {
            PartidoDto? partido =
                PartidosTemporales.FirstOrDefault(
                    elemento => elemento.Id == id);

            if (partido is null)
            {
                return NotFound();
            }

            var modelo = new PartidoFormViewModel
            {
                Id = partido.Id,

                NumeroPartidoFifa =
                    partido.NumeroPartidoFifa,

                Fase = partido.Fase,
                Grupo = partido.Grupo,

                SeleccionLocal =
                    partido.SeleccionLocal,

                SeleccionVisitante =
                    partido.SeleccionVisitante,

                FechaHora = partido.FechaHora,
                Sede = partido.Sede,
                Estado = partido.Estado
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(
            PartidoFormViewModel modelo)
        {
            PartidoDto? partido =
                PartidosTemporales.FirstOrDefault(
                    elemento =>
                        elemento.Id == modelo.Id);

            if (partido is null)
            {
                return NotFound();
            }

            bool numeroRepetido =
                PartidosTemporales.Any(elemento =>
                    elemento.Id != modelo.Id &&
                    elemento.NumeroPartidoFifa ==
                    modelo.NumeroPartidoFifa);

            if (numeroRepetido)
            {
                ModelState.AddModelError(
                    nameof(modelo.NumeroPartidoFifa),
                    "Ya existe otro partido con ese número.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            partido.NumeroPartidoFifa =
                modelo.NumeroPartidoFifa;

            partido.Fase =
                modelo.Fase.Trim();

            partido.Grupo =
                string.IsNullOrWhiteSpace(modelo.Grupo)
                    ? string.Empty
                    : modelo.Grupo
                        .Trim()
                        .ToUpperInvariant();

            partido.SeleccionLocal =
                modelo.SeleccionLocal.Trim();

            partido.SeleccionVisitante =
                modelo.SeleccionVisitante.Trim();

            partido.FechaHora =
                modelo.FechaHora;

            partido.Sede =
                modelo.Sede.Trim();

            partido.Estado =
                ConvertirEstadoParaVista(
                    modelo.Estado);

            TempData["MensajeExito"] =
                "El partido temporal fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // REGISTRAR RESULTADO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> RegistrarResultado(
            int id)
        {
            PartidoApiDto? partidoApi =
                await _estadisticasApiService
                    .ObtenerPartidoPorIdAsync(id);

            if (partidoApi is not null)
            {
                PartidoDto partido =
                    ConvertirPartidoApi(partidoApi);

                var modeloApi =
                    new RegistrarResultadoViewModel
                    {
                        PartidoId = partido.Id,

                        NumeroPartidoFifa =
                            partido.NumeroPartidoFifa,

                        SeleccionLocal =
                            partido.SeleccionLocal,

                        SeleccionVisitante =
                            partido.SeleccionVisitante,

                        GolesLocal =
                            partido.GolesLocal ?? 0,

                        GolesVisitante =
                            partido.GolesVisitante ?? 0,

                        EsDatoApi = true
                    };

                return View(modeloApi);
            }

            PartidoDto? partidoTemporal =
                PartidosTemporales.FirstOrDefault(
                    partido => partido.Id == id);

            if (partidoTemporal is null)
            {
                return NotFound();
            }

            var modeloTemporal =
                new RegistrarResultadoViewModel
                {
                    PartidoId =
                        partidoTemporal.Id,

                    NumeroPartidoFifa =
                        partidoTemporal.NumeroPartidoFifa,

                    SeleccionLocal =
                        partidoTemporal.SeleccionLocal,

                    SeleccionVisitante =
                        partidoTemporal.SeleccionVisitante,

                    GolesLocal =
                        partidoTemporal.GolesLocal ?? 0,

                    GolesVisitante =
                        partidoTemporal.GolesVisitante ?? 0,

                    EsDatoApi = false
                };

            return View(modeloTemporal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(
            RegistrarResultadoViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            // El partido proviene de la API real.
            if (modelo.EsDatoApi)
            {
                bool guardado =
                    await _estadisticasApiService
                        .RegistrarResultadoAsync(
                            modelo.PartidoId,
                            modelo.GolesLocal,
                            modelo.GolesVisitante);

                if (!guardado)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "La API no pudo registrar el resultado. " +
                        "Revise que el servicio de Andrea esté activo.");

                    return View(modelo);
                }

                TempData["MensajeExito"] =
                    $"Resultado registrado correctamente: " +
                    $"{modelo.SeleccionLocal} " +
                    $"{modelo.GolesLocal} - " +
                    $"{modelo.GolesVisitante} " +
                    $"{modelo.SeleccionVisitante}.";

                return RedirectToAction(nameof(Index));
            }

            // Respaldo temporal cuando la API está apagada.
            PartidoDto? partidoTemporal =
                PartidosTemporales.FirstOrDefault(
                    partido =>
                        partido.Id == modelo.PartidoId);

            if (partidoTemporal is null)
            {
                return NotFound();
            }

            partidoTemporal.GolesLocal =
                modelo.GolesLocal;

            partidoTemporal.GolesVisitante =
                modelo.GolesVisitante;

            partidoTemporal.Estado =
                "FINALIZADO";

            TempData["MensajeExito"] =
                $"Resultado temporal registrado: " +
                $"{partidoTemporal.SeleccionLocal} " +
                $"{modelo.GolesLocal} - " +
                $"{modelo.GolesVisitante} " +
                $"{partidoTemporal.SeleccionVisitante}.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CONVERSIONES ENTRE LA API Y EL FRONTEND
        // =========================================================

        private static PartidoDto ConvertirPartidoApi(
     PartidoApiDto partidoApi)
        {
            return new PartidoDto
            {
                Id = partidoApi.Id,
                NumeroPartidoFifa = partidoApi.Id,

                SeleccionLocal =
                    partidoApi.SeleccionLocal?.Nombre
                    ?? "Por definir",

                CodigoBanderaLocal =
                    ConvertirCodigoBandera(
                        partidoApi.SeleccionLocal?.CodigoPais),

                SeleccionVisitante =
                    partidoApi.SeleccionVisitante?.Nombre
                    ?? "Por definir",

                CodigoBanderaVisitante =
                    ConvertirCodigoBandera(
                        partidoApi.SeleccionVisitante?.CodigoPais),

                FechaHora = partidoApi.FechaHora,
                Sede = partidoApi.Sede,

                Fase = ConvertirFaseParaVista(
                    partidoApi.Fase),

                Grupo = ConvertirGrupoParaVista(
                    partidoApi.Grupo),

                Estado = ConvertirEstadoParaVista(
                    partidoApi.Estado),

                GolesLocal = partidoApi.GolesLocal,
                GolesVisitante = partidoApi.GolesVisitante
            };
        }

        private static string? ConvertirGrupoParaApi(
            string? grupo)
        {
            if (string.IsNullOrWhiteSpace(grupo))
            {
                return null;
            }

            string grupoLimpio = grupo
                .Replace(
                    "Grupo",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                .Trim()
                .ToUpperInvariant();

            return $"Grupo {grupoLimpio}";
        }

        private static string ConvertirGrupoParaVista(
            string? grupo)
        {
            if (string.IsNullOrWhiteSpace(grupo))
            {
                return "-";
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

        private static string? ConvertirEstadoParaApi(
            string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return null;
            }

            return estado
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "_");
        }

        private static string ConvertirEstadoParaVista(
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

        private static string ConvertirFaseParaVista(
            string? fase)
        {
            if (string.IsNullOrWhiteSpace(fase))
            {
                return "Por definir";
            }

            string faseLimpia = fase
                .Trim()
                .ToLowerInvariant()
                .Replace("_", " ");

            return faseLimpia switch
            {
                "fase de grupos" =>
                    "Fase de grupos",

                "dieciseisavos de final" =>
                    "Dieciseisavos de final",

                "octavos de final" =>
                    "Octavos de final",

                "cuartos de final" =>
                    "Cuartos de final",

                "semifinal" =>
                    "Semifinal",

                "tercer puesto" =>
                    "Tercer puesto",

                "final" =>
                    "Final",

                _ => char.ToUpper(faseLimpia[0])
                     + faseLimpia[1..]
            };
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

            // Códigos FIFA que son diferentes de los códigos ISO.
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
                    // Continúa buscando.
                }
            }

            return string.Empty;
        }
    }
}