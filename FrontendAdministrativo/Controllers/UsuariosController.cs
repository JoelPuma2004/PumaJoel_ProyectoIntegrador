using FrontendAdministrativo.Models.Api;
using FrontendAdministrativo.Models.ViewModels;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UsuariosController : Controller
    {
        private readonly EstadisticasApiService
            _estadisticasApiService;

        public UsuariosController(
            EstadisticasApiService estadisticasApiService)
        {
            _estadisticasApiService =
                estadisticasApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            string? rol,
            string? estado)
        {
            List<UsuarioApiDto>? respuestaApi =
                await _estadisticasApiService
                    .ObtenerUsuariosAsync();

            if (respuestaApi is null)
            {
                return View(new UsuariosIndexViewModel
                {
                    ApiDisponible = false,
                    Buscar = buscar,
                    RolSeleccionado = rol,
                    EstadoSeleccionado = estado
                });
            }

            List<UsuarioAdminViewModel> todosLosUsuarios =
                respuestaApi
                    .Select(ConvertirUsuario)
                    .ToList();

            IEnumerable<UsuarioAdminViewModel> consulta =
                todosLosUsuarios;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(usuario =>
                    usuario.NombreCompleto.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    usuario.Correo.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                consulta = consulta.Where(usuario =>
                    string.Equals(
                        usuario.Rol,
                        rol,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (estado == "ACTIVO")
            {
                consulta = consulta.Where(usuario =>
                    usuario.Activo);
            }
            else if (estado == "INACTIVO")
            {
                consulta = consulta.Where(usuario =>
                    !usuario.Activo);
            }

            List<UsuarioAdminViewModel> usuariosFiltrados =
                consulta
                    .OrderBy(usuario =>
                        usuario.NombreCompleto)
                    .ToList();

            var modelo = new UsuariosIndexViewModel
            {
                ApiDisponible = true,
                Usuarios = usuariosFiltrados,

                Buscar = buscar,

                RolSeleccionado = rol,

                EstadoSeleccionado = estado,

                TotalUsuarios =
                    todosLosUsuarios.Count,

                UsuariosActivos =
                    todosLosUsuarios.Count(usuario =>
                        usuario.Activo),

                Administradores =
                    todosLosUsuarios.Count(usuario =>
                        usuario.Rol == "ADMINISTRADOR")
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            List<UsuarioApiDto>? usuarios =
                await _estadisticasApiService
                    .ObtenerUsuariosAsync();

            UsuarioApiDto? usuarioApi =
                usuarios?.FirstOrDefault(usuario =>
                    usuario.Id == id);

            if (usuarioApi is null)
            {
                TempData["MensajeError"] =
                    "No fue posible consultar el usuario en la API.";

                return RedirectToAction(nameof(Index));
            }

            UsuarioAdminViewModel usuario =
                ConvertirUsuario(usuarioApi);

            var modelo = new EditarUsuarioViewModel
            {
                Id = usuario.Id,

                NombreCompleto =
                    usuario.NombreCompleto,

                Correo =
                    usuario.Correo,

                Rol =
                    usuario.Rol,

                Activo =
                    usuario.Activo
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            EditarUsuarioViewModel modelo)
        {
            modelo.Rol =
                modelo.Rol
                    .Trim()
                    .ToUpperInvariant();

            if (modelo.Rol != "ADMINISTRADOR" &&
                modelo.Rol != "USUARIO")
            {
                ModelState.AddModelError(
                    nameof(modelo.Rol),
                    "El rol seleccionado no es válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            List<UsuarioApiDto>? usuarios =
                await _estadisticasApiService
                    .ObtenerUsuariosAsync();

            UsuarioApiDto? usuarioActual =
                usuarios?.FirstOrDefault(usuario =>
                    usuario.Id == modelo.Id);

            if (usuarioActual is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible consultar el usuario en la API.");

                return View(modelo);
            }

            int nuevoRolId =
                modelo.Rol == "ADMINISTRADOR"
                    ? 1
                    : 2;

            bool rolActualizado = true;
            bool estadoActualizado = true;

            if (usuarioActual.Rol?.Id != nuevoRolId)
            {
                rolActualizado =
                    await _estadisticasApiService
                        .CambiarRolUsuarioAsync(
                            modelo.Id,
                            nuevoRolId);
            }

            if (usuarioActual.Activo != modelo.Activo)
            {
                estadoActualizado =
                    await _estadisticasApiService
                        .CambiarEstadoUsuarioAsync(
                            modelo.Id,
                            modelo.Activo);
            }

            if (!rolActualizado ||
                !estadoActualizado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible guardar todos los cambios. " +
                    "Revise que el servicio esté disponible.");

                return View(modelo);
            }

            TempData["MensajeExito"] =
                $"El usuario {modelo.NombreCompleto} " +
                "fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id)
        {
            List<UsuarioApiDto>? usuarios =
                await _estadisticasApiService
                    .ObtenerUsuariosAsync();

            UsuarioApiDto? usuarioApi =
                usuarios?.FirstOrDefault(usuario =>
                    usuario.Id == id);

            if (usuarioApi is null)
            {
                TempData["MensajeError"] =
                    "No fue posible consultar el usuario en la API. " +
                    "No se realizó ningún cambio.";

                return RedirectToAction(nameof(Index));
            }

            bool nuevoEstado =
                !usuarioApi.Activo;

            bool actualizado =
                await _estadisticasApiService
                    .CambiarEstadoUsuarioAsync(
                        id,
                        nuevoEstado);

            if (!actualizado)
            {
                TempData["MensajeError"] =
                    "No fue posible cambiar el estado del usuario.";

                return RedirectToAction(nameof(Index));
            }

            string nombre =
                ObtenerNombreVisible(usuarioApi);

            TempData["MensajeExito"] =
                nuevoEstado
                    ? $"El usuario {nombre} fue activado."
                    : $"El usuario {nombre} fue desactivado.";

            return RedirectToAction(nameof(Index));
        }

        private static UsuarioAdminViewModel ConvertirUsuario(
            UsuarioApiDto usuarioApi)
        {
            return new UsuarioAdminViewModel
            {
                Id =
                    usuarioApi.Id,

                NombreCompleto =
                    ObtenerNombreVisible(usuarioApi),

                Correo =
                    ObtenerCorreoVisible(usuarioApi),

                Rol =
                    usuarioApi.Rol?.Nombre?
                        .Trim()
                        .ToUpperInvariant()
                    ?? "USUARIO",

                Activo =
                    usuarioApi.Activo,

                FechaRegistro =
                    ConvertirFecha(
                        usuarioApi.FechaCreacion),

                // Andrea no devuelve último acceso.
                UltimoAcceso =
                    null
            };
        }

        private static string ObtenerNombreVisible(
            UsuarioApiDto usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                return usuario.Nombre.Trim();
            }

            if (!string.IsNullOrWhiteSpace(usuario.Username))
            {
                return usuario.Username.Trim();
            }

            return $"Usuario {usuario.Id}";
        }

        private static string ObtenerCorreoVisible(
            UsuarioApiDto usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Email))
            {
                return usuario.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(usuario.Username) &&
                usuario.Username.Contains('@'))
            {
                return usuario.Username.Trim();
            }

            return "No registrado";
        }

        private static DateTime ConvertirFecha(
            List<int>? valores)
        {
            if (valores is null ||
                valores.Count < 3)
            {
                return DateTime.MinValue;
            }

            try
            {
                int anio =
                    valores[0];

                int mes =
                    valores[1];

                int dia =
                    valores[2];

                int hora =
                    valores.Count > 3
                        ? valores[3]
                        : 0;

                int minuto =
                    valores.Count > 4
                        ? valores[4]
                        : 0;

                int segundo =
                    valores.Count > 5
                        ? valores[5]
                        : 0;

                int milisegundo =
                    valores.Count > 6
                        ? valores[6] / 1_000_000
                        : 0;

                return new DateTime(
                    anio,
                    mes,
                    dia,
                    hora,
                    minuto,
                    segundo,
                    milisegundo,
                    DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }
    }
}