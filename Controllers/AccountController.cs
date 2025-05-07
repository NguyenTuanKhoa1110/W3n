using Microsoft.AspNetCore.Mvc;
using W3_test.Domain.Models;
using Microsoft.AspNetCore.Identity;

using AutoMapper;

namespace W3_test.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;

		public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		
		public async Task<IActionResult> CreateRoles()
		{
			var roleNames = new[] { "Admin", "User", "Staff" };

			foreach (var roleName in roleNames)
			{
				var roleExist = await _roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					await _roleManager.CreateAsync(new AppRole(roleName));
				}
			}

			return Ok("Roles created successfully.");
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new AppUser
				{
					UserName = model.Email,
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
					Age = model.Age,
					Address = model.Address
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					
					await _userManager.AddToRoleAsync(user, "Staff");
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
					if (result.Succeeded)
					{
						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			}
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}
