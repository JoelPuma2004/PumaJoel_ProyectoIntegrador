using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {
        private static readonly List<AuditoriaViewModel>
            RegistrosTemporales = new()
            {
                new AuditoriaViewModel
                {
                    Id = 1,
                    Usuario = "Administrador UTN",
                    Accion = "Inicio de sesión",
                    Modulo = "Seguridad",
                    Descripcion =
                        "Ingreso correcto al panel administrativo.",
                    FechaHora = DateTime.Now.AddMinutes(-55),
                    DireccionIp = "127.0.0.1"
                },

                new AuditoriaViewModel
                {
                    Id = 2,
                    Usuario = "Administrador UTN",
                    Accion = "Consultó partidos",
                    Modulo = "Partidos",
                    Descripcion =
                        "Consultó los partidos programados del torneo.",
                    FechaHora = DateTime.Now.AddMinutes(-40),
                    DireccionIp = "127.0.0.1"
                },

                new AuditoriaViewModel
                {
                    Id = 3,
                    Usuario = "Administrador UTN",
                    Accion = "Registró resultado",
                    Modulo = "Partidos",
                    Descripcion =
                        "Actualizó el marcador de un partido.",
                    FechaHora = DateTime.Now.AddMinutes(-30),
                    DireccionIp = "127.0.0.1"
                },

                new AuditoriaViewModel
                {
                    Id = 4,
                    Usuario = "Administrador UTN",
                    Accion = "Modificó usuario",
                    Modulo = "Usuarios",
                    Descripcion =
                        "Cambió el rol de un usuario registrado.",
                    FechaHora = DateTime.Now.AddMinutes(-20),
                    DireccionIp = "127.0.0.1"
                },

                new AuditoriaViewModel
                {
                    Id = 5,
                    Usuario = "Administrador UTN",
                    Accion = "Desactivó usuario",
                    Modulo = "Usuarios",
                    Descripcion =
                        "Desactivó temporalmente una cuenta.",
                    FechaHora = DateTime.Now.AddMinutes(-10),
                    DireccionIp = "127.0.0.1"
                }
            };

        [HttpGet]
        public IActionResult Index(
            string? buscar,
            string? modulo)
        {
            IEnumerable<AuditoriaViewModel> registros =
                RegistrosTemporales;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                registros = registros.Where(r =>
                    r.Usuario.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase) ||

                    r.Accion.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase) ||

                    r.Descripcion.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                registros = registros.Where(r =>
                    r.Modulo.Equals(
                        modulo,
                        StringComparison.OrdinalIgnoreCase));
            }

            List<AuditoriaViewModel> resultado =
                registros
                    .OrderByDescending(r => r.FechaHora)
                    .ToList();

            AuditoriaIndexViewModel modelo = new()
            {
                Registros = resultado,
                Buscar = buscar,
                ModuloSeleccionado = modulo,
                TotalRegistros = resultado.Count,
                UltimaActividad = resultado
                    .FirstOrDefault()?.FechaHora
            };

            return View(modelo);
        }
    }
}