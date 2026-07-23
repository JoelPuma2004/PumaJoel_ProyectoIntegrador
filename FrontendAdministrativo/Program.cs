using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace FrontendAdministrativo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder =
                WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // ==========================================
            // API DE ESTADÍSTICAS
            // ==========================================

            builder.Services.AddHttpClient<EstadisticasApiService>(
                client =>
                {
                    string? baseUrl =
                        builder.Configuration[
                            "ApiEstadisticas:BaseUrl"];

                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        throw new InvalidOperationException(
                            "No se configuró la URL de la API de Estadísticas.");
                    }

                    client.BaseAddress =
                        new Uri(baseUrl.TrimEnd('/') + "/");

                    client.Timeout =
                        TimeSpan.FromSeconds(15);
                });

            // ==========================================
            // API UTNGOLCOIN
            // ==========================================

            builder.Services.AddHttpClient<UTNGolCoinApiService>(
                client =>
                {
                    string? baseUrl =
                        builder.Configuration[
                            "ApiUTNGolCoin:BaseUrl"];

                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        throw new InvalidOperationException(
                            "No se configuró la URL de la API UTNGolCoin.");
                    }

                    client.BaseAddress =
                        new Uri(baseUrl.TrimEnd('/') + "/");

                    client.Timeout =
                        TimeSpan.FromSeconds(15);
                });

            // ==========================================
            // AUTENTICACIÓN
            // ==========================================

            builder.Services
                .AddAuthentication(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath =
                        "/Auth/Login";

                    options.AccessDeniedPath =
                        "/Auth/AccesoDenegado";

                    options.Cookie.Name =
                        "UTNGolMundial.Admin";

                    options.Cookie.HttpOnly =
                        true;

                    // Permite guardar la cookie cuando se ingresa
                    // mediante HTTP usando la dirección IP.
                    options.Cookie.SecurePolicy =
                        CookieSecurePolicy.SameAsRequest;

                    options.Cookie.SameSite =
                        SameSiteMode.Lax;

                    options.ExpireTimeSpan =
                        TimeSpan.FromMinutes(30);

                    options.SlidingExpiration =
                        true;
                });

            // Todas las páginas quedan restringidas
            // al administrador.
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy =
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole("ADMINISTRADOR")
                        .Build();
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(
                    "/Home/Error");

                app.UseHsts();
            }

            // Se deja desactivado para poder ingresar mediante:
            // http://192.168.0.102:5203
            //
            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                    name: "default",
                    pattern:
                    "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}