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

		public UserManagerController(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var users = _userManager.Users.ToList();
			return View(users);
		}

		public async Task<IActionResult> Delete(Guid id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null) return NotFound();

			var roles = await _userManager.GetRolesAsync(user);
			if (roles.Contains("Admin"))
			{
				return Forbid(); 
			}

			await _userManager.DeleteAsync(user);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> ChangeRole(Guid id, string role)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null) return NotFound();

			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles); 
			await _userManager.AddToRoleAsync(user, role); 

			return RedirectToAction(nameof(Index));
		}
	}
}
