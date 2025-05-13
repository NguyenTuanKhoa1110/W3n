using Microsoft.AspNetCore.Mvc.Rendering;

namespace W3_test.Domain.Models
{
	public class EditUserViewModel
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }

		public List<string> SelectedRoles { get; set; } = new();
		public List<SelectListItem> AllRoles { get; set; } = new();
	}

}
