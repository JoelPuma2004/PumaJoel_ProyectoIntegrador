using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UTNGolCoinController : Controller
    {
        private readonly UTNGolCoinApiService
            _utnGolCoinApiService;

        public UTNGolCoinController(
            UTNGolCoinApiService utnGolCoinApiService)
        {
            _utnGolCoinApiService =
                utnGolCoinApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var modelo = new UTNGolCoinViewModel();

            await CargarDatosRealesAsync(modelo);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
            UTNGolCoinViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                await CargarRankingAsync(modelo);

                modelo.ConfiguracionDisponible = true;

                return View("Index", modelo);
            }

            var configuracion =
                new ConfiguracionUTNGolCoinApiDto
                {
                    BonoInicial =
                        modelo.BonoInicial,

                    MonedasPorAcierto =
                        modelo.MonedasPorAcierto,

                    LimiteMaximoApuesta =
                        modelo.LimiteMaximoApuesta,

                    ApuestasHabilitadas =
                        modelo.ApuestasHabilitadas
                };

            bool actualizado =
                await _utnGolCoinApiService
                    .ActualizarConfiguracionAsync(
                        configuracion);

            if (!actualizado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible actualizar la configuración " +
                    "en la API de Mayra.");

                await CargarRankingAsync(modelo);

                modelo.ConfiguracionDisponible = false;

                return View("Index", modelo);
            }

            TempData["MensajeExito"] =
                "La configuración de UTNGolCoin fue " +
                "actualizada correctamente en la API.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatosRealesAsync(
            UTNGolCoinViewModel modelo)
        {
            List<RankingUTNGolCoinApiDto>? ranking =
                await _utnGolCoinApiService
                    .ObtenerRankingAsync();

            ConfiguracionUTNGolCoinApiDto? configuracion =
                await _utnGolCoinApiService
                    .ObtenerConfiguracionAsync();

            if (ranking is not null)
            {
                modelo.ServicioDisponible = true;

                modelo.Ranking = ranking
                    .OrderByDescending(usuario =>
                        usuario.Saldo)
                    .ToList();

                modelo.TotalBilleteras =
                    ranking.Count;

                modelo.MonedasEnCirculacion =
                    ranking.Sum(usuario =>
                        usuario.Saldo);
            }
            else
            {
                modelo.ServicioDisponible = false;
                modelo.Ranking = new();
                modelo.TotalBilleteras = 0;
                modelo.MonedasEnCirculacion = 0;
            }

            if (configuracion is not null)
            {
                modelo.ConfiguracionDisponible = true;

                modelo.BonoInicial =
                    configuracion.BonoInicial;

                modelo.MonedasPorAcierto =
                    configuracion.MonedasPorAcierto;

                modelo.LimiteMaximoApuesta =
                    configuracion.LimiteMaximoApuesta;

                modelo.ApuestasHabilitadas =
                    configuracion.ApuestasHabilitadas;
            }
            else
            {
                modelo.ConfiguracionDisponible = false;
            }
        }

        private async Task CargarRankingAsync(
            UTNGolCoinViewModel modelo)
        {
            List<RankingUTNGolCoinApiDto>? ranking =
                await _utnGolCoinApiService
                    .ObtenerRankingAsync();

            if (ranking is null)
            {
                modelo.ServicioDisponible = false;
                modelo.Ranking = new();
                modelo.TotalBilleteras = 0;
                modelo.MonedasEnCirculacion = 0;

                return;
            }

            modelo.ServicioDisponible = true;

            modelo.Ranking = ranking
                .OrderByDescending(usuario =>
                    usuario.Saldo)
                .ToList();

            modelo.TotalBilleteras =
                ranking.Count;

            modelo.MonedasEnCirculacion =
                ranking.Sum(usuario =>
                    usuario.Saldo);
        }
    }
}