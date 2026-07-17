using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class HomeController : Controller
    {
        private readonly EstadisticasApiService _estadisticasApiService;

        public HomeController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService = estadisticasApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tareaPartidos =
                _estadisticasApiService.ObtenerPartidosAsync();

            var tareaSelecciones =
                _estadisticasApiService.ObtenerSeleccionesAsync();

            await Task.WhenAll(
                tareaPartidos,
                tareaSelecciones);

            var partidos = await tareaPartidos;
            var selecciones = await tareaSelecciones;

            bool apiEstadisticasDisponible =
                partidos is not null &&
                selecciones is not null;

            int totalPartidos =
                partidos?.Count ?? 0;

            int partidosFinalizados =
                partidos?.Count(partido =>
                    string.Equals(
                        partido.Estado,
                        "FINALIZADO",
                        StringComparison.OrdinalIgnoreCase))
                ?? 0;

            int totalSelecciones =
                selecciones?.Count ?? 0;

            var actividades =
                new List<ActividadRecienteViewModel>();

            if (apiEstadisticasDisponible)
            {
                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            "Datos cargados desde el Servicio de Estadísticas",
                        Tipo = "API",
                        Fecha = DateTime.Now
                    });

                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            $"{totalPartidos} partidos registrados",
                        Tipo = "Partidos",
                        Fecha = DateTime.Now
                    });

                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            $"{partidosFinalizados} partidos finalizados",
                        Tipo = "Resultados",
                        Fecha = DateTime.Now
                    });
            }
            else
            {
                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            "Servicio de Estadísticas no disponible",
                        Tipo = "Advertencia",
                        Fecha = DateTime.Now
                    });
            }

            var dashboard = new DashboardViewModel
            {
                TotalSelecciones = totalSelecciones,
                TotalPartidos = totalPartidos,
                PartidosFinalizados = partidosFinalizados,

                // Temporal hasta conectar la API de usuarios.
                TotalUsuarios = 3,

                ApiEstadisticasDisponible =
                    apiEstadisticasDisponible,

                // Permanecerá apagado hasta conectar
                // el servicio de UTNGolCoin.
                ApiUtnGolCoinDisponible = false,

                ActividadesRecientes = actividades
            };

            return View(dashboard);
        }
    }
}