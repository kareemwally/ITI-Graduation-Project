using DAL.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Helpers
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAndAdminAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roleNames = { "Admin", "Factory" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                }
            }

            var defaultAdminEmail = "admin@fayed.com";
            
            var adminUser = await userManager.FindByEmailAsync(defaultAdminEmail);
            if (adminUser == null)
            {
                var admin = new User
                {
                    UserName = "fayed_admin",
                    Email = defaultAdminEmail,
                    Name = "Fayed Admin",
                    NationalId = "30157261502525",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(admin, "FayedAdmin@2026");

                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
