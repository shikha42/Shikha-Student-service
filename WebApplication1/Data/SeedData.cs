using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Student", "Staff" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Primary staff: shikha@iubat.edu / P@ssW0rd (replaces aranna@iubat.edu)
            string staffEmail = "shikha@iubat.edu";
            const string staffPassword = "P@ssW0rd";

            var existingStaff = await userManager.FindByEmailAsync(staffEmail);
            if (existingStaff == null)
            {
                var staffUser = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FirstName = "Shikha",
                    LastName = "IUBAT",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(staffUser, staffPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }
            else
            {
                // Ensure Staff role
                if (!await userManager.IsInRoleAsync(existingStaff, "Staff"))
                {
                    await userManager.AddToRoleAsync(existingStaff, "Staff");
                }
                // Ensure password is P@ssW0rd (reset if needed) and name is Shikha
                bool needsUpdate = false;
                if (existingStaff.FirstName != "Shikha" || existingStaff.LastName != "IUBAT")
                {
                    existingStaff.FirstName = "Shikha";
                    existingStaff.LastName = "IUBAT";
                    needsUpdate = true;
                }
                if (needsUpdate)
                {
                    await userManager.UpdateAsync(existingStaff);
                }
                // Reset password if not valid
                if (!await userManager.CheckPasswordAsync(existingStaff, staffPassword))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existingStaff);
                    await userManager.ResetPasswordAsync(existingStaff, token, staffPassword);
                }
            }

            // Cleanup legacy staff account (replace semantics)
            string legacyEmail = "aranna@iubat.edu";
            if (!string.Equals(legacyEmail, staffEmail, StringComparison.OrdinalIgnoreCase))
            {
                var legacyUser = await userManager.FindByEmailAsync(legacyEmail);
                if (legacyUser != null)
                {
                    await userManager.DeleteAsync(legacyUser);
                }
            }
        }
    }
}
