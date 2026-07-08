using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FrontendAdministrativo.Controllers

{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Datos temporales para construir y probar el frontend.
            // Posteriormente serán reemplazados por datos de las APIs REST.
            var dashboard = new DashboardViewModel
            {
                TotalSelecciones = 48,
                TotalPartidos = 72,
                PartidosFinalizados = 0,
                TotalUsuarios = 3,

                ApiEstadisticasDisponible = false,
                ApiUtnGolCoinDisponible = false,

                ActividadesRecientes = new List<ActividadRecienteViewModel>
                {
                    new ActividadRecienteViewModel
                    {
                        Descripcion = "Proyecto administrativo MVC creado",
                        Tipo = "Sistema",
                        Fecha = DateTime.Now
                    },
                    new ActividadRecienteViewModel
                    {
                        Descripcion = "Dashboard administrativo configurado",
                        Tipo = "Interfaz",
                        Fecha = DateTime.Now
                    },
                    new ActividadRecienteViewModel
                    {
                        Descripcion = "Datos iniciales preparados para pruebas",
                        Tipo = "Información",
                        Fecha = DateTime.Now
                    }
                }
            };

            return View(dashboard);
        }
    }
}