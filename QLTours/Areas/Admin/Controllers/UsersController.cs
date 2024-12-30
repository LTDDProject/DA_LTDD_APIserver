using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	
	public class UsersController : ControllerBase
	{
		private readonly QuanLyTourContext _context;

		public UsersController(QuanLyTourContext context)
		{
			_context = context;
		}

		
		// GET: api/Admin/Users
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			if (_context.Users == null)
			{
				return Problem("Entity set 'QuanLyTourContext.Users' is null.");
			}

			var users = await _context.Users.AsNoTracking().ToListAsync(); // Lấy toàn bộ danh sách người dùng.

			return Ok(users); // Trả về danh sách người dùng.
		}

		// GET: api/Admin/Users/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetUserDetails(int id)
		{
			if (_context.Users == null)
			{
				return NotFound();
			}

			var user = await _context.Users
				.FirstOrDefaultAsync(m => m.UserId == id);

			if (user == null)
			{
				return NotFound();
			}

			return Ok(user);
		}
	}
}
