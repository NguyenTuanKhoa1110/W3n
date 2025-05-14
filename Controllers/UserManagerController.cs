using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using W3_test.Domain.Models;

namespace W3_test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagerController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagerController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        

		public async Task<IActionResult> Index()
		{
			var users = _userManager.Users.ToList();
			var roles = _roleManager.Roles.ToList();

			var userRoles = new Dictionary<string, string>();
			foreach (var user in users)
			{
				var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
				userRoles[user.Id.ToString()] = currentRole ?? "";
			}

			ViewBag.UserRoles = userRoles;
			ViewBag.Roles = roles;

			return View("ManageUser", users); 
		}


		[HttpPost]
        public async Task<IActionResult> ChangeRole(Guid id, string role)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || string.IsNullOrEmpty(role)) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }
    }
}
