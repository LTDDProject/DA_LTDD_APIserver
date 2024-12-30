using Microsoft.AspNetCore.Mvc;
using QLTours.Models;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace QLTours.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly UserDAO _userDAO;

        public PasswordController(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }

        // Quên mật khẩu
        [HttpPost("forgot")]
        public IActionResult ForgotPassword([FromBody] ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Vui lòng nhập địa chỉ email hợp lệ." });
            }

            var user = _userDAO.GetUserByEmail(model.Email);
            if (user != null)
            {
                // Tạo mã reset mật khẩu
                var resetCode = Guid.NewGuid().ToString();
                var expirationTime = DateTime.Now.AddMinutes(15);

                // Lưu thông tin reset mật khẩu (tuỳ vào hệ thống của bạn)
                user.PasswordResetCode = resetCode;
                user.PasswordResetExpiry = expirationTime;
                _userDAO.UpdateUser(user);

                // Gửi email
                try
                {
                    SendPasswordResetEmail(user.Email, resetCode);
                    return Ok(new { message = "Đã gửi email hướng dẫn thiết lập lại mật khẩu." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"Lỗi khi gửi email: {ex.Message}" });
                }
            }

            return NotFound(new { message = "Email không tồn tại trong hệ thống." });
        }

        private void SendPasswordResetEmail(string userEmail, string resetCode)
        {
            var resetLink = $"https://yourfrontendurl.com/reset-password?code={resetCode}&email={userEmail}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("System", "danghuuthanh112003@gmail.com"));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = "Thiết lập lại mật khẩu";

            var body = new TextPart("html")
            {
                Text = $"Chào bạn,<br/><br/>Nhấn vào liên kết dưới đây để thiết lập lại mật khẩu của bạn:<br/><a href='{resetLink}'>Thiết lập lại mật khẩu</a><br/><br/>Liên kết có hiệu lực trong 15 phút."
            };

            message.Body = body;

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("danghuuthanh112003@gmail.com", "gmwa mxva rloa qnrh"); // Thay bằng mật khẩu ứng dụng
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
