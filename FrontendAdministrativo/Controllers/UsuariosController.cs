using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UsuariosController : Controller
    {
        // Datos temporales hasta conectar la API de usuarios.
        private static readonly List<UsuarioAdminViewModel>
            UsuariosTemporales = new()
            {
                new UsuarioAdminViewModel
                {
                    Id = 1,
                    NombreCompleto = "Administrador UTN",
                    Correo = "admin@utn.edu.ec",
                    Rol = "ADMINISTRADOR",
                    Activo = true,
                    FechaRegistro =
                        new DateTime(2026, 6, 1, 8, 30, 0),
                    UltimoAcceso = DateTime.Now
                },

                new UsuarioAdminViewModel
                {
                    Id = 2,
                    NombreCompleto = "Andrea Guacales",
                    Correo = "andrea@utn.edu.ec",
                    Rol = "ADMINISTRADOR",
                    Activo = true,
                    FechaRegistro =
                        new DateTime(2026, 6, 5, 10, 0, 0),
                    UltimoAcceso =
                        DateTime.Now.AddHours(-2)
                },

                new UsuarioAdminViewModel
                {
                    Id = 3,
                    NombreCompleto = "Carlos Andrade",
                    Correo = "carlos@utn.edu.ec",
                    Rol = "USUARIO",
                    Activo = true,
                    FechaRegistro =
                        new DateTime(2026, 6, 10, 14, 15, 0),
                    UltimoAcceso =
                        DateTime.Now.AddDays(-1)
                },

                new UsuarioAdminViewModel
                {
                    Id = 4,
                    NombreCompleto = "María López",
                    Correo = "maria@utn.edu.ec",
                    Rol = "USUARIO",
                    Activo = true,
                    FechaRegistro =
                        new DateTime(2026, 6, 12, 9, 40, 0),
                    UltimoAcceso =
                        DateTime.Now.AddHours(-5)
                },

                new UsuarioAdminViewModel
                {
                    Id = 5,
                    NombreCompleto = "Pedro Torres",
                    Correo = "pedro@utn.edu.ec",
                    Rol = "USUARIO",
                    Activo = false,
                    FechaRegistro =
                        new DateTime(2026, 6, 15, 16, 20, 0),
                    UltimoAcceso =
                        DateTime.Now.AddDays(-8)
                },

                new UsuarioAdminViewModel
                {
                    Id = 6,
                    NombreCompleto = "Lucía Morales",
                    Correo = "lucia@utn.edu.ec",
                    Rol = "USUARIO",
                    Activo = true,
                    FechaRegistro =
                        new DateTime(2026, 6, 18, 11, 10, 0),
                    UltimoAcceso =
                        DateTime.Now.AddDays(-2)
                }
            };

        [HttpGet]
        public IActionResult Index(
            string? buscar,
            string? rol,
            string? estado)
        {
            IEnumerable<UsuarioAdminViewModel> consulta =
                UsuariosTemporales;

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
                    .OrderBy(usuario => usuario.NombreCompleto)
                    .ToList();

            var modelo = new UsuariosIndexViewModel
            {
                Usuarios = usuariosFiltrados,
                Buscar = buscar,
                RolSeleccionado = rol,
                EstadoSeleccionado = estado,

                TotalUsuarios =
                    UsuariosTemporales.Count,

                UsuariosActivos =
                    UsuariosTemporales.Count(usuario =>
                        usuario.Activo),

                Administradores =
                    UsuariosTemporales.Count(usuario =>
                        usuario.Rol == "ADMINISTRADOR")
            };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            UsuarioAdminViewModel? usuario =
                UsuariosTemporales.FirstOrDefault(
                    usuario => usuario.Id == id);

            if (usuario is null)
            {
                return NotFound();
            }

            var modelo = new EditarUsuarioViewModel
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Activo = usuario.Activo
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(
            EditarUsuarioViewModel modelo)
        {
            UsuarioAdminViewModel? usuario =
                UsuariosTemporales.FirstOrDefault(
                    usuario => usuario.Id == modelo.Id);

            if (usuario is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.Rol =
                modelo.Rol.Trim().ToUpperInvariant();

            usuario.Activo =
                modelo.Activo;

            TempData["MensajeExito"] =
                $"El usuario {usuario.NombreCompleto} " +
                "fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id)
        {
            UsuarioAdminViewModel? usuario =
                UsuariosTemporales.FirstOrDefault(
                    usuario => usuario.Id == id);

            if (usuario is null)
            {
                return NotFound();
            }

            usuario.Activo = !usuario.Activo;

            TempData["MensajeExito"] =
                usuario.Activo
                    ? $"El usuario {usuario.NombreCompleto} fue activado."
                    : $"El usuario {usuario.NombreCompleto} fue desactivado.";

            return RedirectToAction(nameof(Index));
        }
    }
}