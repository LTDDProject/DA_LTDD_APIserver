using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using System.Security.Claims;
using System.Text.Json;
using BCrypt.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;

namespace QLTours.Areas.Admin.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly QuanLyTourContext _ctx;

		public AccountController(QuanLyTourContext ctx)
		{
			_ctx = ctx;
		}

		// Login POST (Web API)
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] QLTours.Models.LoginRequest loginRequest)
		{
			var user = await _ctx.Manages
								  .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

			if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password)) // Xác thực mật khẩu đã mã hóa
			{
				return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
			}

			if (user.Status == "Locked") // Kiểm tra trạng thái tài khoản
			{
				return BadRequest(new { message = "Tài khoản của bạn đã bị khóa." });
			}

			// Tạo claims cho người dùng
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Role, user.Role ? "Admin" : "Employee")  // Admin hoặc Employee
            };

			// Tạo identity từ các claims
			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			// Đăng nhập người dùng và lưu thông tin đăng nhập vào cookie
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

			// Lưu tên người dùng và vai trò vào session
			HttpContext.Session.SetString("Username", user.Username);
			HttpContext.Session.SetInt32("IdMng", user.IdMng);
			HttpContext.Session.SetString("Role", user.Role ? "Admin" : "Employee");

			return Ok(new { message = "Đăng nhập thành công!", role = user.Role ? "Admin" : "Employee" });
		}

		// Logout (Web API)
		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			HttpContext.Session.Clear();
			return Ok(new { message = "Đăng xuất thành công!" });
		}

		// ChangePassword POST (Web API)
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
		{
			var IdMng = HttpContext.Session.GetInt32("IdMng");
			if (!IdMng.HasValue)
			{
				return Unauthorized(new { message = "Bạn cần đăng nhập trước." });
			}

			var user = await _ctx.Manages.FirstOrDefaultAsync(u => u.IdMng == IdMng.Value);
			if (user == null)
			{
				return BadRequest(new { message = "Không tìm thấy người dùng." });
			}

			// Kiểm tra mật khẩu cũ (so sánh mật khẩu đã mã hóa)
			if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, user.Password))
			{
				return BadRequest(new { message = "Mật khẩu cũ không đúng." });
			}

			// Kiểm tra mật khẩu mới và xác nhận mật khẩu
			if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmPassword)
			{
				return BadRequest(new { message = "Mật khẩu mới và xác nhận không khớp." });
			}

			// Mã hóa mật khẩu mới trước khi lưu vào cơ sở dữ liệu
			user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);

			_ctx.Update(user);
			await _ctx.SaveChangesAsync();

			return Ok(new { message = "Đổi mật khẩu thành công!" });
		}
	}
}