	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using W3_test.Data.Entities;
	using W3_test.Domain.Models;
	using W3_test.Repositories;
	using AutoMapper;
	using W3_test.Domain.DTOs;

	namespace W3_test.Controllers
	{
		[Authorize(Roles = "Admin,Staff")]
		public class AdminController : Controller
		{
			private readonly IBookRepository _bookRepository;
			private readonly IOrderRepository _orderRepository;
			private readonly IUserRepository _userRepository;
			private readonly IMapper _mapper;

			public AdminController(
				IBookRepository bookRepository,
				IOrderRepository orderRepository,
				IUserRepository userRepository,
				IMapper mapper)
			{
				_bookRepository = bookRepository;
				_orderRepository = orderRepository;
				_userRepository = userRepository;
				_mapper = mapper;
			}

			public IActionResult Index()
			{
				return View();
			}

			// Hiển thị danh sách sách (ManageBooks)
			public async Task<IActionResult> ManageBooks()
			{
				var books = await _bookRepository.GetAllAsync();
				var bookModels = _mapper.Map<IEnumerable<BookDTO>>(books);
				return View(bookModels);
			}

			[HttpGet]
			public IActionResult CreateBook()
			{
				return View();
			}

			[HttpPost]
			public async Task<IActionResult> CreateBook(Book model, IFormFile ImageFile)
			{
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
					{
						Directory.CreateDirectory(imagesDir);
					}

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
				ModelState.AddModelError(string.Empty, $"An error occurred while creating the book: {ex.Message}");
			}
			return View(model);
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
				if (ModelState.IsValid)
				{
					var bookEntity = await _bookRepository.GetByIdAsync(model.Id);
					if (bookEntity == null) return NotFound();

					_mapper.Map(model, bookEntity);
					await _bookRepository.UpdateAsync(bookEntity);
					return RedirectToAction("ManageBooks");
				}
				return View(model);
			}

			[HttpPost]
			public async Task<IActionResult> DeleteBook(Guid id)
			{
				var success = await _bookRepository.DeleteAsync(id);
				if (!success)
				{
					return NotFound();
				}
				return RedirectToAction("ManageBooks");
			}

			// Hiển thị danh sách đơn hàng (ManageOrders)
			public async Task<IActionResult> ManageOrders(string status)
			{
				var ordersQuery = await _orderRepository.GetAllAsync();
				if (!string.IsNullOrEmpty(status))
				{
					ordersQuery = ordersQuery.Where(o => o.Status == status);
				}
				var orderModels = _mapper.Map<IEnumerable<OrderDTO>>(ordersQuery);
				ViewData["StatusFilter"] = status;
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
				if (ModelState.IsValid)
				{
					var orderEntity = await _orderRepository.GetByIdAsync(model.Id);
					if (orderEntity == null) return NotFound();

					_mapper.Map(model, orderEntity);
					await _orderRepository.UpdateAsync(orderEntity);
					return RedirectToAction("ManageOrders");
				}
				return View(model);
			}

			[HttpPost]
			public async Task<IActionResult> DeleteOrder(Guid id)
			{
				var success = await _orderRepository.DeleteAsync(id);
				if (!success)
				{
					return NotFound();
				}
				return RedirectToAction("ManageOrders");
			}


			// Hiển thị danh sách người dùng (ManageUsers)
			public async Task<IActionResult> ManageUsers(string query)
			{
				var usersQuery = await _userRepository.GetAllAsync();
				if (!string.IsNullOrEmpty(query))
				{
					query = query.ToLower();
					usersQuery = usersQuery.Where(u => u.Username.ToLower().Contains(query) || u.Email.ToLower().Contains(query));
				}
				var userModels = _mapper.Map<IEnumerable<AppUserDTO>>(usersQuery);
				ViewData["SearchQuery"] = query;
				return View(userModels);
			}

			[HttpGet]
			public async Task<IActionResult> EditUser(Guid id)
			{
				var user = await _userRepository.GetByIdAsync(id);
				if (user == null) return NotFound();

				var userModel = _mapper.Map<AppUserDTO>(user);
				return View(userModel);
			}

			[HttpPost]
			public async Task<IActionResult> EditUser(AppUserDTO model)
			{
				if (ModelState.IsValid)
				{
					var user = await _userRepository.GetByIdAsync(model.Id);
					if (user == null) return NotFound();

					_mapper.Map(model, user);
					await _userRepository.UpdateAsync(user);
					return RedirectToAction("ManageUsers");
				}
				return View(model);
			}

			[HttpPost]
			public async Task<IActionResult> DeleteUser(Guid id)
			{
				var success = await _userRepository.DeleteAsync(id);
				if (!success)
				{
					return NotFound();
				}
				return RedirectToAction("ManageUsers");
			}
		}
	}