using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3_test.Data.Entities;
using W3_test.Domain.Models;
using W3_test.Repositories;
using AutoMapper;
using W3_test.Domain.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace W3_test.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly IBookRepository _bookRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppRole> _roleManager;

		public AdminController(
			IBookRepository bookRepository,
			IOrderRepository orderRepository,
			IUserRepository userRepository,
			IMapper mapper,
			UserManager<AppUser> userManager,
			RoleManager<AppRole> roleManager)
		{
			_bookRepository = bookRepository;
			_orderRepository = orderRepository;
			_userRepository = userRepository;
			_mapper = mapper;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public IActionResult Index() => View();

		// BOOKS
		public async Task<IActionResult> ManageBooks()
		{
			var books = await _bookRepository.GetAllAsync();
			var bookModels = _mapper.Map<IEnumerable<BookDTO>>(books);
			return View(bookModels);
		}

		[HttpGet]
		public IActionResult CreateBook() => View();

		[HttpPost]
		public async Task<IActionResult> CreateBook(Book model, IFormFile ImageFile)
		{
			if (!ModelState.IsValid)
				return View(model);

			try
			{
				if (ImageFile != null && ImageFile.Length > 0)
				{
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
					var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
					if (!allowedExtensions.Contains(fileExtension))
					{
						ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
						return View(model);
					}

					var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
					if (!Directory.Exists(imagesDir))
						Directory.CreateDirectory(imagesDir);

					var filePath = Path.Combine(imagesDir, ImageFile.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await ImageFile.CopyToAsync(stream);
					}

					model.ImageUrl = "/images/" + ImageFile.FileName;
				}

				var bookEntity = _mapper.Map<BookEntity>(model);
				await _bookRepository.AddAsync(bookEntity);
				return RedirectToAction("ManageBooks");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> EditBook(Guid id)
		{
			var bookEntity = await _bookRepository.GetByIdAsync(id);
			if (bookEntity == null) return NotFound();

			var bookDto = _mapper.Map<BookDTO>(bookEntity);
			return View(bookDto);
		}

		[HttpPost]
		public async Task<IActionResult> EditBook(Book model)
		{
			if (!ModelState.IsValid) return View(model);

			var bookEntity = await _bookRepository.GetByIdAsync(model.Id);
			if (bookEntity == null) return NotFound();

			_mapper.Map(model, bookEntity);
			await _bookRepository.UpdateAsync(bookEntity);
			return RedirectToAction("ManageBooks");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteBook(Guid id)
		{
			var success = await _bookRepository.DeleteAsync(id);
			return success ? RedirectToAction("ManageBooks") : NotFound();
		}

		// ORDERS
		public async Task<IActionResult> ManageOrders(string status)
		{
			var orders = await _orderRepository.GetAllAsync();
			if (!string.IsNullOrEmpty(status))
				orders = orders.Where(o => o.Status == status);

			var orderModels = _mapper.Map<IEnumerable<OrderDTO>>(orders);
			var statuses = new List<string> { "Pending", "Shipped", "Delivered", "Cancelled" };
			ViewBag.statuses = new SelectList(statuses, status);
			return View(orderModels);
		}

		[HttpGet]
		public async Task<IActionResult> EditOrder(Guid id)
		{
			var orderEntity = await _orderRepository.GetByIdAsync(id);
			if (orderEntity == null) return NotFound();

			var orderModel = _mapper.Map<OrderDTO>(orderEntity);
			return View(orderModel);
		}

		[HttpPost]
		public async Task<IActionResult> EditOrder(OrderDTO model)
		{
			if (!ModelState.IsValid) return View(model);

			var orderEntity = await _orderRepository.GetByIdAsync(model.Id);
			if (orderEntity == null) return NotFound();

			_mapper.Map(model, orderEntity);
			await _orderRepository.UpdateAsync(orderEntity);
			return RedirectToAction("ManageOrders");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			var success = await _orderRepository.DeleteAsync(id);
			return success ? RedirectToAction("ManageOrders") : NotFound();
		}

		// USERS
		public async Task<IActionResult> ManageUsers(string query)
		{
			var users = await _userRepository.GetAllAsync();
			if (!string.IsNullOrEmpty(query))
			{
				query = query.ToLower();
				users = users.Where(u => u.Username.ToLower().Contains(query) || u.Email.ToLower().Contains(query));
			}

			var userModels = _mapper.Map<IEnumerable<AppUserDTO>>(users);
			ViewData["SearchQuery"] = query;
			return View(userModels);
		}

		[HttpGet]
		public async Task<IActionResult> EditUser(Guid id)
		{
			var user = await _userRepository.GetByIdAsync(id);
			if (user == null) return NotFound();

			var userModel = _mapper.Map<AppUserDTO>(user);

			var allRoles = await _roleManager.Roles.ToListAsync();
			ViewBag.AllRoles = allRoles;

			var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
			var userRoles = await _userManager.GetRolesAsync(appUser);
			ViewBag.UserRoles = userRoles;

			return View(userModel);
		}

		[HttpPost]
		public async Task<IActionResult> EditUser(AppUserDTO model, string[] selectedRoles)
		{
			if (!ModelState.IsValid) return View(model);

			var user = await _userManager.FindByIdAsync(model.Id.ToString());
			if (user == null) return NotFound();
			user.UserName = model.Username;
			user.Email = model.Email;
			var updateResult = await _userManager.UpdateAsync(user);
			if (!updateResult.Succeeded)
			{
				foreach (var error in updateResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View(model);
			}
			var currentRoles = await _userManager.GetRolesAsync(user);
			var rolesToRemove = currentRoles.Except(selectedRoles);
			var rolesToAdd = selectedRoles.Except(currentRoles);

			await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
			await _userManager.AddToRolesAsync(user, rolesToAdd);


			return RedirectToAction("ManageUsers");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteUser(Guid id)
		{
			var success = await _userRepository.DeleteAsync(id);
			return success ? RedirectToAction("ManageUsers") : NotFound();
		}
	}
}
