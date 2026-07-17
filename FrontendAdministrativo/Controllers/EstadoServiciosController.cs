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

        private readonly IConfiguration _configuration;

        public EstadoServiciosController(
            EstadisticasApiService estadisticasApiService,
            IConfiguration configuration)
        {
            _estadisticasApiService =
                estadisticasApiService;

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
                    "ApiUtnGolCoin:BaseUrl"
                ] ?? string.Empty;

            bool estadisticasConfigurado =
                !string.IsNullOrWhiteSpace(
                    urlEstadisticas);

            bool estadisticasDisponible =
                estadisticasConfigurado &&
                await _estadisticasApiService
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
                                    !string.IsNullOrWhiteSpace(
                                        urlUtnGolCoin),

                                Disponible = false
                            }
                        }
                };

            return View(modelo);
        }
    }
}