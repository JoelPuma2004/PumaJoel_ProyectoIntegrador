using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class EstadoServiciosController : Controller
    {
        private readonly EstadisticasApiService
            _estadisticasApiService;

        private readonly UTNGolCoinApiService
            _utnGolCoinApiService;

        private readonly IConfiguration
            _configuration;

        public EstadoServiciosController(
            EstadisticasApiService estadisticasApiService,
            UTNGolCoinApiService utnGolCoinApiService,
            IConfiguration configuration)
        {
            _estadisticasApiService =
                estadisticasApiService;

            _utnGolCoinApiService =
                utnGolCoinApiService;

            _configuration =
                configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string urlEstadisticas =
                _configuration[
                    "ApiEstadisticas:BaseUrl"
                ] ?? string.Empty;

            string urlUtnGolCoin =
                _configuration[
                    "ApiUTNGolCoin:BaseUrl"
                ] ?? string.Empty;

            bool estadisticasConfigurado =
                !string.IsNullOrWhiteSpace(
                    urlEstadisticas);

            bool utnGolCoinConfigurado =
                !string.IsNullOrWhiteSpace(
                    urlUtnGolCoin);

            bool estadisticasDisponible =
                estadisticasConfigurado &&
                await _estadisticasApiService
                    .EstaDisponibleAsync();

            bool utnGolCoinDisponible =
                utnGolCoinConfigurado &&
                await _utnGolCoinApiService
                    .EstaDisponibleAsync();

            var modelo =
                new EstadoServiciosViewModel
                {
                    FechaConsulta = DateTime.Now,

                    Servicios =
                        new List<EstadoServicioViewModel>
                        {
                            new EstadoServicioViewModel
                            {
                                Nombre =
                                    "Servicio de Estadísticas",

                                Tecnologia =
                                    "Jakarta EE, WildFly y PostgreSQL",

                                Url =
                                    urlEstadisticas,

                                Configurado =
                                    estadisticasConfigurado,

                                Disponible =
                                    estadisticasDisponible
                            },

                            new EstadoServicioViewModel
                            {
                                Nombre =
                                    "Servicio UTNGolCoin",

                                Tecnologia =
                                    "ASP.NET Core Web API y MySQL",

                                Url =
                                    urlUtnGolCoin,

                                Configurado =
                                    utnGolCoinConfigurado,

                                Disponible =
                                    utnGolCoinDisponible
                            }
                        }
                };

            return View(modelo);
        }
    }
}