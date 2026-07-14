using FrontendAdministrativo.Models;
using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    public class PartidosController : Controller
    {
        // Datos temporales mientras la API de Estadísticas está en desarrollo.
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
                Estado = "EN_JUEGO",
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

        [HttpGet]
        public IActionResult Index(string? grupo, string? estado)
        {
            IEnumerable<PartidoDto> partidosFiltrados = PartidosTemporales;

            if (!string.IsNullOrWhiteSpace(grupo))
            {
                partidosFiltrados = partidosFiltrados.Where(partido =>
                    partido.Grupo.Equals(
                        grupo,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                partidosFiltrados = partidosFiltrados.Where(partido =>
                    partido.Estado.Equals(
                        estado,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            var modelo = new PartidosIndexViewModel
            {
                Partidos = partidosFiltrados
                    .OrderBy(partido => partido.FechaHora)
                    .ToList(),

                GrupoSeleccionado = grupo ?? string.Empty,
                EstadoSeleccionado = estado ?? string.Empty,

                TotalPartidos = PartidosTemporales.Count,

                Programados = PartidosTemporales.Count(
                    partido => partido.Estado == "PROGRAMADO"
                ),

                EnJuego = PartidosTemporales.Count(
                    partido => partido.Estado == "EN_JUEGO"
                ),

                Finalizados = PartidosTemporales.Count(
                    partido => partido.Estado == "FINALIZADO"
                )
            };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult Detalles(int id)
        {
            var partido = PartidosTemporales.FirstOrDefault(
                partido => partido.Id == id
            );

            if (partido == null)
            {
                return NotFound();
            }

            return View(partido);
        }
        [HttpGet]
        public IActionResult RegistrarResultado(int id)
        {
            var partido = PartidosTemporales.FirstOrDefault(
                partido => partido.Id == id
            );

            if (partido == null)
            {
                return NotFound();
            }

            var modelo = new RegistrarResultadoViewModel
            {
                PartidoId = partido.Id,
                NumeroPartidoFifa = partido.NumeroPartidoFifa,
                SeleccionLocal = partido.SeleccionLocal,
                SeleccionVisitante = partido.SeleccionVisitante,
                GolesLocal = partido.GolesLocal ?? 0,
                GolesVisitante = partido.GolesVisitante ?? 0
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarResultado(
            RegistrarResultadoViewModel modelo
        )
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var partido = PartidosTemporales.FirstOrDefault(
                partido => partido.Id == modelo.PartidoId
            );

            if (partido == null)
            {
                return NotFound();
            }

            partido.GolesLocal = modelo.GolesLocal;
            partido.GolesVisitante = modelo.GolesVisitante;
            partido.Estado = "FINALIZADO";

            TempData["MensajeExito"] =
                $"Resultado registrado correctamente: " +
                $"{partido.SeleccionLocal} {modelo.GolesLocal} - " +
                $"{modelo.GolesVisitante} {partido.SeleccionVisitante}.";

            return RedirectToAction(nameof(Index));
        }

    }

}