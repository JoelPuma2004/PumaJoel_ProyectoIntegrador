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
                    .OrderBy(partido => partido.NumeroPartidoFifa)
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
                    .OrderBy(partido => partido.NumeroPartidoFifa)
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
        // CREAR PARTIDO REAL EN LA API
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            List<PartidoApiDto> partidosExistentes =
                await _estadisticasApiService.ObtenerPartidosAsync()
                ?? new List<PartidoApiDto>();

            HashSet<int> numerosUtilizados =
                partidosExistentes
                    .Select(partido => partido.NumeroPartidoFifa)
                    .ToHashSet();

            int siguienteNumero =
                Enumerable.Range(1, 104)
                    .FirstOrDefault(numero =>
                        !numerosUtilizados.Contains(numero));

            if (siguienteNumero == 0)
            {
                TempData["MensajeError"] =
                    "Los 104 partidos del torneo ya están registrados.";

                return RedirectToAction(nameof(Index));
            }

            var modelo = new PartidoFormViewModel
            {
                NumeroPartidoFifa = siguienteNumero,

                Fase = ObtenerFasePorNumero(
                    siguienteNumero),

                FechaHora = DateTime.Now.AddDays(1),
                Estado = "PROGRAMADO",

                CuotaLocal = 1.50m,
                CuotaEmpate = 2.50m,
                CuotaVisitante = 1.80m
            };

            await CargarCatalogosAsync(modelo);

            if (modelo.NumeroPartidoFifa >= 97 &&
                modelo.NumeroPartidoFifa <= 104)
            {
                AsignarEquiposClasificados(
                    modelo,
                    partidosExistentes);
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            PartidoFormViewModel modelo)
        {
            await CargarCatalogosAsync(modelo);

            List<PartidoApiDto> partidosExistentes =
                await _estadisticasApiService.ObtenerPartidosAsync()
                ?? new List<PartidoApiDto>();

            // La fase siempre se calcula en el servidor.
            // No se confía en el valor enviado por el navegador.
            modelo.Fase =
                ObtenerFasePorNumero(
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
            if (modelo.NumeroPartidoFifa >= 97 &&
    modelo.NumeroPartidoFifa <= 104)
            {
                ModelState.Remove(
                    nameof(modelo.SeleccionLocalId));

                ModelState.Remove(
                    nameof(modelo.SeleccionVisitanteId));

                bool equiposAsignados =
                    AsignarEquiposClasificados(
                        modelo,
                        partidosExistentes);

                if (!equiposAsignados)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        modelo.MensajeClasificacion ??
                        "No se pudieron determinar los equipos clasificados.");
                }
            }

            // Verifica que los equipos existan.
            SeleccionApiDto? equipo1 =
                modelo.Selecciones.FirstOrDefault(
                    seleccion =>
                        seleccion.Id ==
                        modelo.SeleccionLocalId);

            SeleccionApiDto? equipo2 =
                modelo.Selecciones.FirstOrDefault(
                    seleccion =>
                        seleccion.Id ==
                        modelo.SeleccionVisitanteId);

            if (modelo.SeleccionLocalId.HasValue &&
                equipo1 is null)
            {
                ModelState.AddModelError(
                    nameof(modelo.SeleccionLocalId),
                    "El Equipo 1 seleccionado no existe.");
            }

            if (modelo.SeleccionVisitanteId.HasValue &&
                equipo2 is null)
            {
                ModelState.AddModelError(
                    nameof(modelo.SeleccionVisitanteId),
                    "El Equipo 2 seleccionado no existe.");
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
                    "Revise los datos y confirme que Andrea " +
                    "tenga el servicio encendido.");

                return View(modelo);
            }

            TempData["MensajeExito"] =
                $"El partido {modelo.NumeroPartidoFifa} " +
                "fue creado correctamente.";

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
                            partido.GolesVisitante ?? 0,

                        EsDatoApi = true
                    };

                return View(modeloApi);
            }

            // Respaldo temporal cuando la API no está disponible.
            PartidoDto? partidoTemporal =
                PartidosTemporales.FirstOrDefault(
                    partido => partido.Id == id);

            if (partidoTemporal is null)
            {
                TempData["MensajeError"] =
                    "No fue posible encontrar el partido seleccionado.";

                return RedirectToAction(nameof(Index));
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
        private static string ObtenerFasePorNumero(
    int numeroPartido)
        {
            return numeroPartido switch
            {
                >= 1 and <= 72 =>
                    "GRUPOS",

                >= 73 and <= 88 =>
                    "DIECISEISAVOS",

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
            modelo.Selecciones =
                await _estadisticasApiService
                    .ObtenerSeleccionesAsync()
                ?? new List<SeleccionApiDto>();

            modelo.Sedes =
                await _estadisticasApiService
                    .ObtenerSedesAsync()
                ?? new List<SedeApiDto>();

            modelo.Grupos =
                await _estadisticasApiService
                    .ObtenerGruposAsync()
                ?? new List<GrupoApiDto>();

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
        private static bool AsignarEquiposClasificados(
    PartidoFormViewModel modelo,
    List<PartidoApiDto> partidos)
        {
            var origenes =
                ObtenerPartidosDeOrigen(
                    modelo.NumeroPartidoFifa);

            if (origenes is null)
            {
                modelo.PuedeCrearPartido = true;
                return true;
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

            SeleccionApiDto? equipo1 =
                ObtenerSeleccionPorResultado(
                    partidoOrigen1,
                    origenes.Value.UsarGanador1);

            SeleccionApiDto? equipo2 =
                ObtenerSeleccionPorResultado(
                    partidoOrigen2,
                    origenes.Value.UsarGanador2);

            if (equipo1 is null || equipo2 is null)
            {
                modelo.PuedeCrearPartido = false;

                modelo.MensajeClasificacion =
                    "No se puede determinar el clasificado porque " +
                    "uno de los partidos terminó empatado y la API " +
                    "no indica el ganador por penales.";

                return false;
            }

            modelo.SeleccionLocalId = equipo1.Id;
            modelo.SeleccionVisitanteId = equipo2.Id;

            modelo.SeleccionLocal = equipo1.Nombre;
            modelo.SeleccionVisitante = equipo2.Nombre;

            modelo.EquiposAsignadosAutomaticamente = true;
            modelo.PuedeCrearPartido = true;

            modelo.MensajeClasificacion =
                $"Equipos asignados automáticamente según los " +
                $"resultados de los partidos FIFA " +
                $"#{origenes.Value.Partido1} y " +
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

        private static SeleccionApiDto? ObtenerSeleccionPorResultado(
            PartidoApiDto partido,
            bool usarGanador)
        {
            if (!partido.GolesLocal.HasValue ||
                !partido.GolesVisitante.HasValue)
            {
                return null;
            }

            if (partido.GolesLocal.Value ==
                partido.GolesVisitante.Value)
            {
                return null;
            }

            bool ganoLocal =
                partido.GolesLocal.Value >
                partido.GolesVisitante.Value;

            if (!usarGanador)
            {
                ganoLocal = !ganoLocal;
            }

            return ganoLocal
                ? partido.SeleccionLocal
                : partido.SeleccionVisitante;
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