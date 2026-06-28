using DAL.Helpers;
using DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace Fayed_API.Extensions
{
    public static class AppExtensions
    {
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

                // نداء ميثود السيرفر اللي عملناها في الـ DAL
                await ContextSeed.SeedRolesAndAdminAsync(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("AppExtensions");
                logger.LogError(ex, "حدث خطأ أثناء عمل Seeding للبيانات الأساسية.");
            }
        }
    }
}
