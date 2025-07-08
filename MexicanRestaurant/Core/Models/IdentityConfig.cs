using Microsoft.AspNetCore.Identity;

namespace MexicanRestaurant.Core.Models
{
    public class IdentityConfig
    {
        public static async Task CreateAdminUserAsync(IServiceProvider provider)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            string userName = Environment.GetEnvironmentVariable("AdminEmail") ?? "";
            string password = Environment.GetEnvironmentVariable("AdminPassword") ?? "";
            string roleName = Environment.GetEnvironmentVariable("AdminRole") ?? "";

            if (await roleManager.FindByNameAsync(roleName) == null)
                await roleManager.CreateAsync(new IdentityRole(roleName));
            
            if (await userManager.FindByNameAsync(userName) == null)
            {
                var user = new ApplicationUser
                {
                    FirstName = "Ahmed",
                    LastName = "Eldabaa",
                    Gender = "Male",
                    PhoneNumber = "010154473",
                    DateOfBirth = new DateTime(1999, 1, 18, 0, 0, 0),
                    UserName = userName,
                    Email = userName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
