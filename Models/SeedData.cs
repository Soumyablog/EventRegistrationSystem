using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace EventRegistrationSystem.Models
{
    public class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // --------------------------------------------
            // Create Admin role
            // --------------------------------------------

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("Admin"));
            }

            // --------------------------------------------
            // Create User role
            // --------------------------------------------

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(
                    new IdentityRole("User"));
            }

            // --------------------------------------------
            // Create Admin account
            // --------------------------------------------

            var adminEmail = "admin@eventsystem.com";

            var admin =
                await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    "Admin@123");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

    }
}
