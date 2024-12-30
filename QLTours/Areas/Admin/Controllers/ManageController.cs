using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using BCrypt.Net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLTours.Areas.Admin.Controllers
{
	[Route("api/admin/[controller]")]
	[ApiController]
	public class ManageController : ControllerBase
	{
		private readonly QuanLyTourContext _context;

		public ManageController(QuanLyTourContext context)
		{
			_context = context;
		}

		// GET: api/admin/manage
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Manage>>> GetAll()
		{
			var manages = await _context.Manages.AsNoTracking().ToListAsync();
			return Ok(manages);
		}

		// GET: api/admin/manage/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var account = await _context.Manages.AsNoTracking().FirstOrDefaultAsync(m => m.IdMng == id);
			if (account == null)
			{
				return NotFound();
			}

			return Ok(account);
		}

		// POST: api/admin/manage
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] Manage model)
		{
			if (ModelState.IsValid)
			{
				// Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
				model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
				model.Status = "Unlocked";  // Mặc định trạng thái là Unlocked

				_context.Manages.Add(model);
				await _context.SaveChangesAsync();

				// Trả về tài khoản vừa tạo thay vì toàn bộ danh sách
				return CreatedAtAction(nameof(GetById), new { id = model.IdMng }, model);
			}

			return BadRequest("Invalid data.");
		}

		// PUT: api/admin/manage/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Edit(int id, [FromBody] Manage model)
		{
			if (id != model.IdMng)
			{
				return BadRequest("ID mismatch");
			}

			var account = await _context.Manages.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}

			// Chỉ mã hóa lại mật khẩu nếu có thay đổi
			if (!string.IsNullOrEmpty(model.Password))
			{
				account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
			}

			account.Username = model.Username;
			account.Role = model.Role;
			account.Status = model.Status;

			_context.Update(account);
			await _context.SaveChangesAsync();

			// Lấy danh sách cập nhật
			var updatedList = await _context.Manages.AsNoTracking().ToListAsync();
			return Ok(updatedList);
		}

		// POST: api/admin/manage/toggle-status/{id}
		[HttpPost("toggle-status/{id}")]
		public async Task<IActionResult> ToggleStatus(int id)
		{
			var account = await _context.Manages.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}

			account.Status = (account.Status == "Locked") ? "Unlocked" : "Locked"; // Toggle status
			_context.Update(account);
			await _context.SaveChangesAsync();

			return Ok(account);
		}

		// DELETE: api/admin/manage/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var account = await _context.Manages.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}

			_context.Manages.Remove(account);
			await _context.SaveChangesAsync();

			return NoContent();  // Return NoContent after successful deletion
		}
	}
}
