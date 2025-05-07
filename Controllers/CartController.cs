using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3_test.Data.Entities;
using W3_test.Repositories;
using W3_test.Domain.DTOs;
using System.Linq;

namespace W3_test.Controllers
{
	[Authorize]
	public class CartController : Controller
	{
		private readonly ICartRepository _cartRepository;
		private readonly IBookRepository _bookRepository;

		public CartController(ICartRepository cartRepository, IBookRepository bookRepository)
		{
			_cartRepository = cartRepository;
			_bookRepository = bookRepository;
		}

		public async Task<IActionResult> Index()
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null)
			{
				cart = new CartEntity { Id = Guid.NewGuid(), UserId = userId };
				await _cartRepository.AddAsync(cart);
			}

			var cartDTO = new CartDTO
			{
				Id = cart.Id,
				UserId = cart.UserId,
				Items = cart.Items?.Select(item => new CartItemDTO
				{
					Id = item.Id,
					BookId = item.BookId,
					BookTitle = item.Book?.Title ?? "",
					Quantity = item.Quantity,
					Price = item.Price
				}).ToList() ?? new List<CartItemDTO>()
			};

			return View(cartDTO); 
		}

		
		[HttpPost]
		public async Task<IActionResult> AddToCart(Guid bookId, int quantity) 
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			var book = await _bookRepository.GetByIdAsync(bookId); 

			if (book == null || quantity <= 0 || book.Stock < quantity)
			{
				return BadRequest("Invalid book or quantity.");
			}

			if (cart == null)
			{
				cart = new CartEntity { Id = Guid.NewGuid(), UserId = userId };
				await _cartRepository.AddAsync(cart);
			}

			var cartItem = cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
			if (cartItem == null)
			{
				cartItem = new CartItemEntity
				{
					Id = Guid.NewGuid(),
					CartId = cart.Id,
					BookId = bookId, // bookId is Guid here
					Quantity = quantity,
					Price = book.Price
				};
				cart.Items.Add(cartItem);
			}
			else
			{
				cartItem.Quantity += quantity;
			}

			await _cartRepository.UpdateAsync(cart);
			return RedirectToAction("Index");
		}

		
		[HttpPost]
		public async Task<IActionResult> RemoveFromCart(Guid cartItemId) 
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null) return NotFound();

			var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId); 
			if (cartItem != null)
			{
				cart.Items.Remove(cartItem);
				await _cartRepository.UpdateAsync(cart);
			}
			return RedirectToAction("Index");
		}
		[HttpPost]
		public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null) return NotFound();

			var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
			if (cartItem != null && quantity > 0)
			{
				cartItem.Quantity = quantity;
				await _cartRepository.UpdateAsync(cart);
			}
			return RedirectToAction("Index");
		}
																															
	}
}
