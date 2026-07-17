using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize]
    public class UTNGolCoinController : Controller
    {
        private static readonly UTNGolCoinViewModel Configuracion =
            new()
            {
                BonoInicial = 100,
                MonedasPorAcierto = 200,
                LimiteMaximoApuesta = 500,
                ApuestasHabilitadas = true,

                MonedasEnCirculacion = 12500,
                TotalApuestas = 148,
                PartidoMasApostado = "México vs. Sudáfrica"
            };

        [HttpGet]
        public IActionResult Index()
        {
            return View(CopiarConfiguracion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(
            UTNGolCoinViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.MonedasEnCirculacion =
                    Configuracion.MonedasEnCirculacion;

                modelo.TotalApuestas =
                    Configuracion.TotalApuestas;

                modelo.PartidoMasApostado =
                    Configuracion.PartidoMasApostado;

                return View("Index", modelo);
            }

            Configuracion.BonoInicial =
                modelo.BonoInicial;

            Configuracion.MonedasPorAcierto =
                modelo.MonedasPorAcierto;

            Configuracion.LimiteMaximoApuesta =
                modelo.LimiteMaximoApuesta;

            Configuracion.ApuestasHabilitadas =
                modelo.ApuestasHabilitadas;

            TempData["MensajeExito"] =
                "La configuración de UTNGolCoin fue actualizada.";

            return RedirectToAction(nameof(Index));
        }

        private static UTNGolCoinViewModel
            CopiarConfiguracion()
        {
            return new UTNGolCoinViewModel
            {
                BonoInicial =
                    Configuracion.BonoInicial,

                MonedasPorAcierto =
                    Configuracion.MonedasPorAcierto,

                LimiteMaximoApuesta =
                    Configuracion.LimiteMaximoApuesta,

                ApuestasHabilitadas =
                    Configuracion.ApuestasHabilitadas,

                MonedasEnCirculacion =
                    Configuracion.MonedasEnCirculacion,

                TotalApuestas =
                    Configuracion.TotalApuestas,

                PartidoMasApostado =
                    Configuracion.PartidoMasApostado
            };
        }
    }
}