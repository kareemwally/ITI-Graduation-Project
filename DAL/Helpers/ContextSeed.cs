using DAL.Data;
using DAL.Helpers;
using DAL.Models;
using DAL.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString() // <-- تم الإضافة هنا لضمان استقرار الـ Identity
                };

                var createAdminResult = await userManager.CreateAsync(admin, "FayedAdmin@2026");

                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        /// <summary>
        /// Development-only convenience: a ready-to-use Factory owner (email confirmed) with a factory
        /// row, so the AI verification endpoints can be tested via login -> extract without going
        /// through registration (Cloudinary uploads) and email confirmation. Idempotent.
        /// Credentials: factory@test.com / Factory@2026.
        /// </summary>
        public static async Task SeedDevFactoryAsync(UserManager<User> userManager, FayedDbContext context)
        {
            const string email = "factory@test.com";
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new User
            {
                UserName = "test_factory",
                Email = email,
                Name = "Ahmed Mohamed Hassan",
                NationalId = "29001011200345",
                EmailConfirmed = true,
                VerificationStatus = VerificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString() // <-- تم الإضافة هنا أيضاً
            };

            var result = await userManager.CreateAsync(user, "Factory@2026");
            if (!result.Succeeded)
                return;

            await userManager.AddToRoleAsync(user, "Factory");

            // Only add the factory if this user doesn't already own one.
            var alreadyHasFactory = await context.Set<Factory>().AnyAsync(f => f.UserId == user.Id);
            if (!alreadyHasFactory)
            {
                context.Set<Factory>().Add(new Factory
                {
                    UserId = user.Id,
                    LegalName = "Nile Steel Recycling Co.",
                    CommercialRegistryNo = "123456",
                    TaxCardNo = "789-456-123",
                    Address = "14 El-Sawah St, El-Marg, Cairo",
                    Sector = "Metal scrap trading",
                    VerificationStatus = VerificationStatus.Pending
                });
                await context.SaveChangesAsync();
            }
        }
    }
}