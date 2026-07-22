using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class HomeController : Controller
    {
        private readonly EstadisticasApiService
            _estadisticasApiService;

        private readonly UTNGolCoinApiService
            _utnGolCoinApiService;

        public HomeController(
            EstadisticasApiService estadisticasApiService,
            UTNGolCoinApiService utnGolCoinApiService)
        {
            _estadisticasApiService =
                estadisticasApiService;

            _utnGolCoinApiService =
                utnGolCoinApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tareaPartidos =
                _estadisticasApiService
                    .ObtenerPartidosAsync();

            var tareaSelecciones =
                _estadisticasApiService
                    .ObtenerSeleccionesAsync();

            var tareaUtnGolCoin =
                _utnGolCoinApiService
                    .EstaDisponibleAsync();

            await Task.WhenAll(
                tareaPartidos,
                tareaSelecciones,
                tareaUtnGolCoin);

            var partidos =
                await tareaPartidos;

            var selecciones =
                await tareaSelecciones;

            bool apiUtnGolCoinDisponible =
                await tareaUtnGolCoin;

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

            if (apiUtnGolCoinDisponible)
            {
                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            "Servicio UTNGolCoin conectado correctamente",
                        Tipo = "API",
                        Fecha = DateTime.Now
                    });
            }
            else
            {
                actividades.Add(
                    new ActividadRecienteViewModel
                    {
                        Descripcion =
                            "Servicio UTNGolCoin no disponible",
                        Tipo = "Advertencia",
                        Fecha = DateTime.Now
                    });
            }

            var dashboard =
                new DashboardViewModel
                {
                    TotalSelecciones =
                        totalSelecciones,

                    TotalPartidos =
                        totalPartidos,

                    PartidosFinalizados =
                        partidosFinalizados,

                    // Temporal hasta conectar la API de usuarios.
                    TotalUsuarios = 3,

                    ApiEstadisticasDisponible =
                        apiEstadisticasDisponible,

                    ApiUtnGolCoinDisponible =
                        apiUtnGolCoinDisponible,

                    ActividadesRecientes =
                        actividades
                };

            return View(dashboard);
        }
    }
}