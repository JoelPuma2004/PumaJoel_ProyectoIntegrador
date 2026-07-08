using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace FrontendAdministrativo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Autenticación mediante cookies.
            builder.Services
                .AddAuthentication(
                    CookieAuthenticationDefaults.AuthenticationScheme
                )
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/AccesoDenegado";

                    options.Cookie.Name = "UTNGolMundial.Admin";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy =
                        CookieSecurePolicy.Always;
                    options.Cookie.SameSite =
                        SameSiteMode.Lax;

                    options.ExpireTimeSpan =
                        TimeSpan.FromMinutes(30);

                    options.SlidingExpiration = true;
                });

            // Todas las páginas quedan restringidas al administrador,
            // excepto las que tengan [AllowAnonymous].
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // El orden es importante.
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