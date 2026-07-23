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

            List<PartidoDto> partidos =
                partidosApi?
                    .Select(ConvertirPartidoApi)
                    .OrderBy(partido => partido.NumeroPartidoFifa)
                    .ToList()
                ?? new List<PartidoDto>();

            var modelo = new PartidosIndexViewModel
            {
                ApiDisponible = partidosApi is not null,
                Partidos = partidos,
                GrupoSeleccionado = grupo ?? string.Empty,
                EstadoSeleccionado = estado ?? string.Empty,
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

            TempData["MensajeError"] =
                "El partido no está disponible en el Servicio de Estadísticas.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CREAR PARTIDO REAL EN LA API
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            List<PartidoApiDto>? partidosExistentes =
                await _estadisticasApiService.ObtenerPartidosAsync();

            if (partidosExistentes is null)
            {
                TempData["MensajeError"] =
                    "No es posible crear partidos mientras la API no esté disponible.";

                return RedirectToAction(nameof(Index));
            }

            HashSet<int> numerosUtilizados =
                partidosExistentes
                    .Select(partido => partido.NumeroPartidoFifa)
                    .ToHashSet();

            int siguienteNumero =
                Enumerable.Range(89, 16)
                    .FirstOrDefault(numero =>
                        !numerosUtilizados.Contains(numero));

            if (siguienteNumero == 0)
            {
                TempData["MensajeError"] =
                    "Los partidos desde octavos hasta la final (89 a 104) ya están registrados.";

                return RedirectToAction(nameof(Index));
            }

            var modelo = new PartidoFormViewModel
            {
                NumeroPartidoFifa = siguienteNumero,

                Fase = ObtenerFasePorNumero(siguienteNumero),

                FechaHora = DateTime.Now.AddDays(1),
                Estado = "PROGRAMADO",

                CuotaLocal = 1.50m,
                CuotaEmpate = 2.50m,
                CuotaVisitante = 1.80m
            };

            await CargarCatalogosAsync(modelo);
            PrepararEquiposClasificados(
                modelo,
                partidosExistentes);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            PartidoFormViewModel modelo)
        {
            await CargarCatalogosAsync(modelo);

            List<PartidoApiDto>? partidosExistentes =
                await _estadisticasApiService.ObtenerPartidosAsync();

            if (partidosExistentes is null ||
                !modelo.CatalogosDisponibles)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La API no está disponible. No se guardó ningún partido.");

                return View(modelo);
            }

            if (!PrepararEquiposClasificados(
                    modelo,
                    partidosExistentes))
            {
                ModelState.AddModelError(
                    string.Empty,
                    modelo.MensajeClasificacion
                    ?? "No fue posible determinar los equipos clasificados.");

                return View(modelo);
            }

            // La fase siempre se calcula en el servidor.
            // No se confía en el valor enviado por el navegador.
            modelo.Fase = ObtenerFasePorNumero(
                modelo.NumeroPartidoFifa);

            if (string.IsNullOrWhiteSpace(modelo.Fase))
            {
                ModelState.AddModelError(
                    nameof(modelo.NumeroPartidoFifa),
                    "El número del partido no es válido.");
            }

            bool numeroRepetido =
    partidosExistentes.Any(partido =>
        partido.NumeroPartidoFifa ==
        modelo.NumeroPartidoFifa);

            if (numeroRepetido)
            {
                ModelState.AddModelError(
                    nameof(modelo.NumeroPartidoFifa),
                    "Ya existe un partido con ese número.");
            }
            // Verifica que los equipos existan.
            SeleccionApiDto? equipo1 =
                modelo.SeleccionesLocalDisponibles.FirstOrDefault(
                    seleccion =>
                        seleccion.Id ==
                        modelo.SeleccionLocalId);

            SeleccionApiDto? equipo2 =
                modelo.SeleccionesVisitanteDisponibles.FirstOrDefault(
                    seleccion =>
                        seleccion.Id ==
                        modelo.SeleccionVisitanteId);

            if (modelo.SeleccionLocalId.HasValue &&
                equipo1 is null)
            {
                ModelState.AddModelError(
                    nameof(modelo.SeleccionLocalId),
                    "El Equipo 1 no clasificó para este partido.");
            }

            if (modelo.SeleccionVisitanteId.HasValue &&
                equipo2 is null)
            {
                ModelState.AddModelError(
                    nameof(modelo.SeleccionVisitanteId),
                    "El Equipo 2 no clasificó para este partido.");
            }

            // Los dos equipos deben ser diferentes.
            if (modelo.SeleccionLocalId.HasValue &&
                modelo.SeleccionVisitanteId.HasValue &&
                modelo.SeleccionLocalId.Value ==
                modelo.SeleccionVisitanteId.Value)
            {
                ModelState.AddModelError(
                    nameof(modelo.SeleccionVisitanteId),
                    "Una selección no puede jugar contra sí misma.");
            }

            // Verifica que la sede exista.
            bool sedeExiste =
                modelo.SedeId.HasValue &&
                modelo.Sedes.Any(sede =>
                    sede.Id == modelo.SedeId.Value);

            if (modelo.SedeId.HasValue &&
                !sedeExiste)
            {
                ModelState.AddModelError(
                    nameof(modelo.SedeId),
                    "La sede seleccionada no existe.");
            }

            bool esFaseDeGrupos =
                modelo.NumeroPartidoFifa >= 1 &&
                modelo.NumeroPartidoFifa <= 72;

            if (esFaseDeGrupos)
            {
                if (!modelo.GrupoId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(modelo.GrupoId),
                        "Debe seleccionar un grupo.");
                }
                else
                {
                    GrupoApiDto? grupoSeleccionado =
                        modelo.Grupos.FirstOrDefault(
                            grupo =>
                                grupo.Id ==
                                modelo.GrupoId.Value);

                    if (grupoSeleccionado is null)
                    {
                        ModelState.AddModelError(
                            nameof(modelo.GrupoId),
                            "El grupo seleccionado no existe.");
                    }
                    else
                    {
                        string grupoEsperado =
                            ConvertirGrupoParaVista(
                                grupoSeleccionado.Nombre);

                        if (equipo1 is not null &&
                            ConvertirGrupoParaVista(
                                equipo1.Grupo) != grupoEsperado)
                        {
                            ModelState.AddModelError(
                                nameof(modelo.SeleccionLocalId),
                                $"El Equipo 1 no pertenece al Grupo " +
                                $"{grupoEsperado}.");
                        }

                        if (equipo2 is not null &&
                            ConvertirGrupoParaVista(
                                equipo2.Grupo) != grupoEsperado)
                        {
                            ModelState.AddModelError(
                                nameof(modelo.SeleccionVisitanteId),
                                $"El Equipo 2 no pertenece al Grupo " +
                                $"{grupoEsperado}.");
                        }
                    }
                }
            }
            else
            {
                // En eliminatorias no existe grupo.
                modelo.GrupoId = null;
                ModelState.Remove(
                    nameof(modelo.GrupoId));
            }

            // Evita que un equipo tenga dos partidos
            // exactamente en la misma fecha y hora.
            if (modelo.SeleccionLocalId.HasValue &&
                modelo.SeleccionVisitanteId.HasValue)
            {
                bool equipoOcupado =
                    partidosExistentes.Any(partido =>
                        partido.FechaHora ==
                        modelo.FechaHora &&
                        (
                            partido.SeleccionLocal?.Id ==
                                modelo.SeleccionLocalId.Value ||

                            partido.SeleccionVisitante?.Id ==
                                modelo.SeleccionLocalId.Value ||

                            partido.SeleccionLocal?.Id ==
                                modelo.SeleccionVisitanteId.Value ||

                            partido.SeleccionVisitante?.Id ==
                                modelo.SeleccionVisitanteId.Value
                        ));

                if (equipoOcupado)
                {
                    ModelState.AddModelError(
                        nameof(modelo.FechaHora),
                        "Uno de los equipos ya tiene otro partido " +
                        "en esa fecha y hora.");
                }
            }

            if (modelo.FechaHora == default)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaHora),
                    "Ingrese una fecha y hora válidas.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var nuevoPartido =
                new CrearPartidoApiDto
                {
                    NumeroPartidoFifa =
                        modelo.NumeroPartidoFifa,

                    SeleccionLocalId =
                        modelo.SeleccionLocalId!.Value,

                    SeleccionVisitanteId =
                        modelo.SeleccionVisitanteId!.Value,

                    SedeId =
                        modelo.SedeId!.Value,

                    GrupoId =
                        modelo.GrupoId,

                    FechaHora =
                        modelo.FechaHora,

                    Fase =
                        modelo.Fase,

                    CuotaLocal =
                        modelo.CuotaLocal,

                    CuotaEmpate =
                        modelo.CuotaEmpate,

                    CuotaVisitante =
                        modelo.CuotaVisitante
                };

            bool creado =
                await _estadisticasApiService
                    .CrearPartidoAsync(nuevoPartido);

            if (!creado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La API no pudo crear el partido. " +
                    "Revise los datos y confirme que el servicio local " +
                    "esté disponible.");

                return View(modelo);
            }

            TempData["MensajeExito"] =
                $"El partido {modelo.NumeroPartidoFifa} " +
                "fue creado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // REGISTRAR RESULTADO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> RegistrarResultado(int id)
        {
            PartidoApiDto? partidoApi =
                await _estadisticasApiService
                    .ObtenerPartidoPorIdAsync(id);

            // Si el endpoint individual no encuentra el partido,
            // buscamos el ID dentro del listado completo.
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
                            partido.GolesVisitante ?? 0
                    };

                return View(modeloApi);
            }

            TempData["MensajeError"] =
                "No fue posible consultar el partido en la API. " +
                "No se puede registrar un resultado sin conexión.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(
            RegistrarResultadoViewModel modelo)
        {
            PartidoApiDto? partidoApi =
                await _estadisticasApiService
                    .ObtenerPartidoPorIdAsync(modelo.PartidoId);

            if (partidoApi is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible confirmar el partido en la API. " +
                    "No se guardó el resultado.");

                return View(modelo);
            }

            modelo.NumeroPartidoFifa = partidoApi.NumeroPartidoFifa;
            modelo.SeleccionLocal =
                partidoApi.SeleccionLocal?.Nombre ?? "Por definir";
            modelo.SeleccionVisitante =
                partidoApi.SeleccionVisitante?.Nombre ?? "Por definir";

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

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
                    "No se aplicó ningún cambio local.");

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
        private static string ObtenerFasePorNumero(
    int numeroPartido)
        {
            return numeroPartido switch
            {
                >= 89 and <= 96 =>
                    "OCTAVOS",

                >= 97 and <= 100 =>
                    "CUARTOS",

                >= 101 and <= 102 =>
                    "SEMIFINAL",

                103 =>
                    "TERCER_PUESTO",

                104 =>
                    "FINAL",

                _ =>
                    string.Empty
            };
        }

        private async Task CargarCatalogosAsync(
            PartidoFormViewModel modelo)
        {
            List<SeleccionApiDto>? selecciones =
                await _estadisticasApiService
                    .ObtenerSeleccionesAsync();

            List<SedeApiDto>? sedes =
                await _estadisticasApiService
                    .ObtenerSedesAsync();

            modelo.CatalogosDisponibles =
                selecciones is not null &&
                sedes is not null;

            modelo.Selecciones = selecciones ?? new();
            modelo.Sedes = sedes ?? new();
            modelo.Grupos = new();

            modelo.Selecciones =
                modelo.Selecciones
                    .OrderBy(seleccion =>
                        seleccion.Nombre)
                    .ToList();

            modelo.Sedes =
                modelo.Sedes
                    .OrderBy(sede =>
                        sede.Nombre)
                    .ToList();

            modelo.Grupos =
                modelo.Grupos
                    .OrderBy(grupo =>
                        grupo.Id)
                    .ToList();
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
                NumeroPartidoFifa =
        partidoApi.NumeroPartidoFifa,

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            PartidoApiDto? partido =
                await _estadisticasApiService
                    .ObtenerPartidoPorIdAsync(id);

            if (partido is null)
            {
                TempData["MensajeError"] =
                    "El partido no existe o ya fue eliminado.";

                return RedirectToAction(nameof(Index));
            }

            if (!string.Equals(
                    partido.Estado,
                    "PROGRAMADO",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["MensajeError"] =
                    $"El partido #{partido.NumeroPartidoFifa} " +
                    "no puede eliminarse porque no está programado.";

                return RedirectToAction(nameof(Index));
            }

            (bool exito, string mensaje) =
                await _estadisticasApiService
                    .EliminarPartidoAsync(id);

            if (!exito)
            {
                TempData["MensajeError"] =
                    mensaje;

                return RedirectToAction(nameof(Index));
            }

            TempData["MensajeExito"] =
                $"El partido #{partido.NumeroPartidoFifa} " +
                "fue eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        private static bool PrepararEquiposClasificados(
            PartidoFormViewModel modelo,
            List<PartidoApiDto> partidos)
        {
            var origenes =
                ObtenerPartidosDeOrigen(
                    modelo.NumeroPartidoFifa);

            if (origenes is null)
            {
                modelo.PuedeCrearPartido = false;
                modelo.MensajeClasificacion =
                    "No existe una llave de clasificación para este partido.";
                return false;
            }

            PartidoApiDto? partidoOrigen1 =
                partidos.FirstOrDefault(partido =>
                    partido.NumeroPartidoFifa ==
                    origenes.Value.Partido1);

            PartidoApiDto? partidoOrigen2 =
                partidos.FirstOrDefault(partido =>
                    partido.NumeroPartidoFifa ==
                    origenes.Value.Partido2);

            if (partidoOrigen1 is null ||
                partidoOrigen2 is null)
            {
                modelo.PuedeCrearPartido = false;

                modelo.MensajeClasificacion =
                    $"Primero deben existir los partidos FIFA " +
                    $"#{origenes.Value.Partido1} y " +
                    $"#{origenes.Value.Partido2}.";

                return false;
            }

            if (!PartidoTieneResultado(partidoOrigen1) ||
                !PartidoTieneResultado(partidoOrigen2))
            {
                modelo.PuedeCrearPartido = false;

                modelo.MensajeClasificacion =
                    $"Primero deben registrarse los resultados de los " +
                    $"partidos FIFA #{origenes.Value.Partido1} y " +
                    $"#{origenes.Value.Partido2}.";

                return false;
            }

            List<SeleccionApiDto> equipos1 =
                ObtenerSeleccionesPorResultado(
                    partidoOrigen1,
                    origenes.Value.UsarGanador1);

            List<SeleccionApiDto> equipos2 =
                ObtenerSeleccionesPorResultado(
                    partidoOrigen2,
                    origenes.Value.UsarGanador2);

            if (equipos1.Count == 0 || equipos2.Count == 0)
            {
                modelo.PuedeCrearPartido = false;

                modelo.MensajeClasificacion =
                    "No se pudieron obtener los equipos de los partidos anteriores.";

                return false;
            }

            modelo.SeleccionesLocalDisponibles =
                equipos1.OrderBy(equipo => equipo.Nombre).ToList();

            modelo.SeleccionesVisitanteDisponibles =
                equipos2.OrderBy(equipo => equipo.Nombre).ToList();

            if (equipos1.Count == 1)
            {
                modelo.SeleccionLocalId = equipos1[0].Id;
                modelo.SeleccionLocal = equipos1[0].Nombre;
            }

            if (equipos2.Count == 1)
            {
                modelo.SeleccionVisitanteId = equipos2[0].Id;
                modelo.SeleccionVisitante = equipos2[0].Nombre;
            }

            bool requiereSeleccionPorPenales =
                equipos1.Count > 1 ||
                equipos2.Count > 1;

            modelo.EquiposAsignadosAutomaticamente =
                !requiereSeleccionPorPenales;
            modelo.PuedeCrearPartido = true;

            modelo.MensajeClasificacion = requiereSeleccionPorPenales
                ? $"Solo se muestran equipos de los partidos FIFA " +
                  $"#{origenes.Value.Partido1} y " +
                  $"#{origenes.Value.Partido2}. Seleccione manualmente " +
                  "el clasificado del partido empatado, porque la API " +
                  "no registra el resultado por penales."
                : $"Equipos clasificados automáticamente desde los " +
                  $"partidos FIFA #{origenes.Value.Partido1} y " +
                  $"#{origenes.Value.Partido2}.";

            return true;
        }

        private static bool PartidoTieneResultado(
            PartidoApiDto partido)
        {
            return string.Equals(
                       partido.Estado,
                       "FINALIZADO",
                       StringComparison.OrdinalIgnoreCase) &&
                   partido.GolesLocal.HasValue &&
                   partido.GolesVisitante.HasValue;
        }

        private static List<SeleccionApiDto> ObtenerSeleccionesPorResultado(
            PartidoApiDto partido,
            bool usarGanador)
        {
            if (!partido.GolesLocal.HasValue ||
                !partido.GolesVisitante.HasValue)
            {
                return new();
            }

            if (partido.GolesLocal.Value ==
                partido.GolesVisitante.Value)
            {
                return new[]
                    {
                        partido.SeleccionLocal,
                        partido.SeleccionVisitante
                    }
                    .OfType<SeleccionApiDto>()
                    .DistinctBy(seleccion => seleccion.Id)
                    .ToList();
            }

            bool ganoLocal =
                partido.GolesLocal.Value >
                partido.GolesVisitante.Value;

            if (!usarGanador)
            {
                ganoLocal = !ganoLocal;
            }

            SeleccionApiDto? seleccion = ganoLocal
                ? partido.SeleccionLocal
                : partido.SeleccionVisitante;

            return seleccion is null
                ? new()
                : new() { seleccion };
        }

        private static (
            int Partido1,
            bool UsarGanador1,
            int Partido2,
            bool UsarGanador2)?
            ObtenerPartidosDeOrigen(
                int numeroPartidoFifa)
        {
            return numeroPartidoFifa switch
            {
                89 => (74, true, 77, true),
                90 => (73, true, 75, true),
                91 => (76, true, 78, true),
                92 => (79, true, 80, true),
                93 => (83, true, 84, true),
                94 => (81, true, 82, true),
                95 => (86, true, 88, true),
                96 => (85, true, 87, true),

                97 => (89, true, 90, true),
                98 => (93, true, 94, true),
                99 => (91, true, 92, true),
                100 => (95, true, 96, true),

                101 => (97, true, 98, true),
                102 => (99, true, 100, true),

                103 => (101, false, 102, false),
                104 => (101, true, 102, true),

                _ => null
            };
        }
    }
}