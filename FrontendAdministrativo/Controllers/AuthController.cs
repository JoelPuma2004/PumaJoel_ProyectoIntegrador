using System.Security.Claims;
using FrontendAdministrativo.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel modelo
        )
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            string? usuarioConfigurado =
                _configuration["AdminDemo:Usuario"];

            string? contrasenaConfigurada =
                _configuration["AdminDemo:Contrasena"];

            if (string.IsNullOrWhiteSpace(usuarioConfigurado) ||
                string.IsNullOrWhiteSpace(contrasenaConfigurada))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Las credenciales temporales no están configuradas."
                );

                return View(modelo);
            }

            bool usuarioCorrecto =
                modelo.Usuario.Equals(
                    usuarioConfigurado,
                    StringComparison.OrdinalIgnoreCase
                );

            bool contrasenaCorrecta =
                modelo.Contrasena == contrasenaConfigurada;

            if (!usuarioCorrecto || !contrasenaCorrecta)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Correo o contraseña incorrectos."
                );

                return View(modelo);
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.Name,
                    "Administrador UTN"
                ),
                new(
                    ClaimTypes.Email,
                    modelo.Usuario
                ),
                new(
                    ClaimTypes.Role,
                    "ADMINISTRADOR"
                )
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme
            );

            var propiedades = new AuthenticationProperties
            {
                IsPersistent = modelo.Recordarme,
                ExpiresUtc = DateTimeOffset.UtcNow
                    .AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                propiedades
            );

            if (!string.IsNullOrWhiteSpace(modelo.ReturnUrl) &&
                Url.IsLocalUrl(modelo.ReturnUrl))
            {
                return LocalRedirect(modelo.ReturnUrl);
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}