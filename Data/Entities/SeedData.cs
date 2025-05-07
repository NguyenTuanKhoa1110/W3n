using Microsoft.AspNetCore.Identity;
using W3_test.Domain.Models;

namespace W3_test.Data
{
	public static class SeedData
	{
		public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
		{
			if (!await roleManager.RoleExistsAsync("Admin"))
				await roleManager.CreateAsync(new AppRole("Admin")
			{
				Name = "Admin",
				Description = "Admin role"
			});

			if (!await roleManager.RoleExistsAsync("Staff"))
				await roleManager.CreateAsync(new AppRole("Staff")
				{
			Name = "Staff",
			Description = "Staff role"
		});
		}
		

		public static async Task SeedAdminAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
		{
			var adminEmail = "admin@example.com";
			var admin = await userManager.FindByEmailAsync(adminEmail);

			if (admin == null)
			{
				admin = new AppUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					FirstName = "Admin",
					LastName = "User",
					Age = 30,
					Address = "Admin City",
					EmailConfirmed = true
				};

				var createResult = await userManager.CreateAsync(admin, "Admin@123");

				if (createResult.Succeeded)
				{
					await userManager.AddToRoleAsync(admin, "Admin");
				}
			}
		}
	}
}
