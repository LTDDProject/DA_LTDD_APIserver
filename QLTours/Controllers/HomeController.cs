using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using QLTours.Services;
using QLTours.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using System.Net;
using SmtpClient = System.Net.Mail.SmtpClient;
using QLTours.Data;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace QLTours.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailSender _emailSender;

        private readonly QuanLyTourContext _ctx;
        private readonly TourDAO _tourDAO;
        private readonly UserDAO _userDAO;
        private readonly BookingDAO _bookingDAO;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVnPayService _vnPayService;
        private readonly ContactDAO _contactDAO;

        // Constructor cho HomeController
        public HomeController(QuanLyTourContext ctx,
                              TourDAO tourDAO,
                              UserDAO userDAO,
                              IHttpContextAccessor httpContextAccessor,
                              BookingDAO bookingDAO,
                              IVnPayService vnPayService,
                              IEmailSender emailSender,
                              ContactDAO contactDAO)
        {
            _ctx = ctx;
            _tourDAO = tourDAO;
            _userDAO = userDAO;
            _bookingDAO = bookingDAO;
            _httpContextAccessor = httpContextAccessor;
            _vnPayService = vnPayService;
            _emailSender = emailSender;
            _contactDAO = contactDAO;
        }

		// Phương thức gửi email thông báo thanh toán thành công
		private void SendPaymentSuccessEmail(string userEmail, Booking booking)
		{
			var smtpServer = "smtp.gmail.com"; // Thay thế bằng thông tin SMTP server của bạn
			var smtpPort = 587; // Thay thế bằng cổng SMTP của bạn (thông thường là 587 hoặc 465)
			var smtpUsername = "voquocthang107@gmail.com"; // Thay thế bằng tên đăng nhập SMTP của bạn
			var smtpPassword = "axmx xdsz bzqi hgst"; // Thay thế bằng mật khẩu SMTP của bạn

			var smtp = new SmtpClient
			{
				Host = smtpServer,
				Port = smtpPort,
				EnableSsl = true, // Bật kết nối SSL
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(smtpUsername, smtpPassword)
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress("voquocthang107@gmail.com"),
				Subject = "Thanh toán thành công", // Tiêu đề email
				Body = $"Chúc mừng! Thanh toán của bạn cho đơn đặt chỗ ID: {booking.BookingId} đã thành công. Cảm ơn bạn đã mua sắm tại chúng tôi!<br>" +
					   $"Tổng số tiền: {booking.Total} VND<br>" +
					   $"Ngày đặt: {booking.BookingDate}<br>" +
					   $"Trạng thái: {booking.Status}",
				IsBodyHtml = true
			};

			mailMessage.To.Add(userEmail); // Thay thế bằng địa chỉ email của người nhận

			try
			{
				smtp.Send(mailMessage);
			}
			catch (Exception ex)
			{
				// Xử lý lỗi khi gửi email
				throw ex;
			}
		}
        public IActionResult Index()
        {
            // Truy vấn danh sách category từ cơ sở dữ liệu
            var categories = _ctx.Categories.ToList();

            // Truy vấn danh sách tour từ cơ sở dữ liệu
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .ToList();

            // Truyền danh sách category và danh sách tour đến view
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(tours);
        }
        public IActionResult List()
        {
            // Lấy danh mục
            var categories = _ctx.Categories.ToList();

            // Lấy danh sách TourId
            var tourNames = _ctx.Tours.Select(t => t.TourId).Distinct().ToList();
            ViewBag.TourNames = new SelectList(tourNames);

            // Lấy tất cả Tour với các liên kết cần thiết
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.Reviews)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle) 
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .ToList();

            // Truyền dữ liệu vào ViewBag
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(tours); // Truyền danh sách tour đến view
        }


        // Thêm action để hiển thị chi tiết tour

        public IActionResult Search(int? categoryId, DateTime? startDate, DateTime? endDate, decimal? price, string tourName)
        {
            var tours = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.Reviews) // Load thông tin đánh giá
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .ToList();
            var categories = _ctx.Categories.ToList();
            var toursQuery = _ctx.Tours.Include(t => t.Category).AsQueryable();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            if (categoryId.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.CategoryId == categoryId);
            }

            if (startDate.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.StartDate == startDate);
            }

            if (endDate.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.EndDate == endDate);
            }

            if (price.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.Price <= price);
            }

            if (!string.IsNullOrEmpty(tourName))
            {
                toursQuery = toursQuery.Where(t => t.TourName.ToLower().Contains(tourName.ToLower()));
            }

            var searchedTours = toursQuery.ToList();

            return View(searchedTours);
        }

         // Phương thức GET cho trang liên hệ
    [HttpGet]
    public IActionResult SendMessage()
    {
        return View();
    }

        // Phương thức POST cho form liên hệ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(string username, string email, string subject, string messageContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var message = new Contact
                    {
                        Username = username,
                        Email = email,
                        Subject = subject,
                        Message = messageContent,
                        Status = "Chưa xem",
                    };

                    _contactDAO.SaveContactMessage(message);

                    // Lưu thông báo thành công vào ViewBag
                    ViewBag.Message = "Your message has been sent successfully!";
                }
                catch (Exception ex)
                {
                    // Lưu thông báo lỗi vào ViewBag
                    ViewBag.ErrorMessage = $"Error sending message: {ex.Message}";
                }
            }
            else
            {
                // Thông báo khi model không hợp lệ
                ViewBag.ErrorMessage = "Failed to send message. Please try again.";
            }

            
            return View("SendMessage");
        }


        public IActionResult TourDetail(int id)
        {
            var tour = _ctx.Tours
                .Include(t => t.Category)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Hotel)
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.Vehicle)
                .Include(t => t.Itineraries)
                    .ThenInclude(i => i.DetailItineraries)
                .Include(t => t.Itineraries)
                    .ThenInclude(i => i.ItineraryImages)  
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(t => t.TourId == id);

            if (tour == null)
            {
                return NotFound();
            }

            // Tính trung bình sao
            double? averageRating = 0.0;
            if (tour.Reviews != null && tour.Reviews.Any())
            {
                averageRating = tour.Reviews.Average(r => r.Rating);
            }

            // Truyền trung bình sao vào ViewBag để sử dụng trong view
            ViewBag.AverageRating = averageRating;
            return View(tour);
        }




        private bool IsValidEmail(string email)
        {
            // Sử dụng regular expression để kiểm tra định dạng email
            string pattern = @"^[A-Za-z0-9._%+-]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }
        // Mã hóa mật khẩu khi người dùng đăng ký
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        // Kiểm tra mật khẩu khi người dùng đăng nhập
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
		[HttpPost]
		[Route("api/register")]
		public IActionResult Register([FromBody] RegisterRequest request)
		{
			// Kiểm tra mật khẩu và xác nhận mật khẩu
			if (request.Password != request.ConfirmPassword)
			{
				return BadRequest(new { message = "Mật khẩu và xác nhận mật khẩu không trùng nhau" });
			}

			// Kiểm tra email đã tồn tại
			if (_userDAO.GetUserByEmail(request.Email) != null)
			{
				return BadRequest(new { message = "Email đã tồn tại" });
			}

			// Kiểm tra các trường khác
			if (string.IsNullOrWhiteSpace(request.Username))
			{
				return BadRequest(new { message = "Tên đăng nhập là bắt buộc" });
			}

			if (string.IsNullOrWhiteSpace(request.Phone))
			{
				return BadRequest(new { message = "Số điện thoại là bắt buộc" });
			}

			if (!IsValidEmail(request.Email))
			{
				return BadRequest(new { message = "Định dạng email không hợp lệ" });
			}

			// Mã hóa mật khẩu trước khi lưu
			string hashedPassword = HashPassword(request.Password);

			// Tạo đối tượng người dùng mới
			var user = new User
			{
				Username = request.Username,
				Email = request.Email,
				Phone = request.Phone,
				Password = hashedPassword
			};

			// Thêm người dùng mới vào cơ sở dữ liệu
			_userDAO.AddUser(user);

			return Ok(new { message = "Đăng ký thành công" });
		}

		[HttpPost]
		[Route("api/login")]
		public async Task<IActionResult> Login([FromBody] User user)
		{
			var existingUser = _userDAO.GetUserByEmail(user.Email);
			if (existingUser == null || !VerifyPassword(user.Password, existingUser.Password))
			{
				return Unauthorized(new { message = "Email hoặc mật khẩu không hợp lệ" });
			}

			var claims = new[]
			{
		new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString()),
		new Claim(ClaimTypes.Name, existingUser.Username),
		new Claim(ClaimTypes.Email, existingUser.Email)
	};
			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

			var token = GenerateJwtToken(existingUser);
			return Ok(new
			{
				userId = existingUser.UserId,
				username = existingUser.Username,
				email = existingUser.Email,
				phone = existingUser.Phone,
				accessToken = token
			});
		}


		private string GenerateJwtToken(User user)
		{
			var claims = new[]
			{
		new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
		new Claim(ClaimTypes.Name, user.Username),
		new Claim(ClaimTypes.Email, user.Email)
	};
			var secretKey = JwtKeyGenerator.GenerateSecureKey();
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: "AppDatTour",
				audience: "AppDatTour",
				claims: claims,
				expires: DateTime.Now.AddHours(1),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		[HttpPost]
		[Route("api/logout")]
		public IActionResult Logout()
		{
			// Xóa token trong SharedPreferences hoặc Session
			var prefs = HttpContext.RequestServices.GetService<IHttpContextAccessor>().HttpContext.Session;
			prefs?.Remove("authToken"); // Xóa token khỏi Session

			return Ok(new { message = "Đăng xuất thành công" });
		}





		private int GetUserId()
		{
			var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

			if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
			{
				throw new UnauthorizedAccessException("UserId not found in claims.");
			}

			return userId;
		}



		[HttpGet]
		[Route("api/CheckAuth")]
		public IActionResult GetTourDetails(int id)
		{
			// Kiểm tra người dùng đã đăng nhập chưa
			if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
			{
				return Unauthorized(); // Trả về lỗi 401 nếu chưa đăng nhập
			}

			// Lấy thông tin tour từ database
			Tour tour = _tourDAO.GetTourDetail(id);

			if (tour == null)
			{
				return NotFound(); // Trả về lỗi 404 nếu không tìm thấy tour
			}

			// Trả về thông tin tour
			return Ok(new
			{
				tourId = tour.TourId,
				tourName = tour.TourName,
				price = tour.Price,
				img = tour.Img,
				quantity = 1,  // Mặc định số lượng là 1
				discountCode = string.Empty  // Mặc định không có mã giảm giá
			});
		}


		// Phương thức hiển thị danh sách mã khuyến mãi
		[HttpGet]
        public IActionResult Promotions()
        {
            // Lấy danh sách các khuyến mãi đang hoạt động
            var activePromotions = _ctx.Promotions
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                .ToList();

            // Truyền danh sách khuyến mãi đến view
            return View(activePromotions);
        }

		[HttpPost]
		[Route("api/v1/payment/create")]
		public IActionResult CreatePaymentUrl([FromBody] PaymentRequest request)
		{
			try
			{
				// Kiểm tra dữ liệu đầu vào
				if (!_ctx.Tours.Any(t => t.TourId == request.TourId))
				{
					return BadRequest($"TourId {request.TourId} không tồn tại.");
				}

				if (!_ctx.Users.Any(u => u.UserId == request.UserId))
				{
					return BadRequest($"UserId {request.UserId} không tồn tại.");
				}

				// Tạo bản ghi booking
				var booking = new Booking
				{
					TourId = request.TourId,
					UserId = request.UserId,
					Total = request.Total,
					BookingDate = DateTime.Now,
					Status = "Đang xác nhận"
				};

				// Lưu vào cơ sở dữ liệu
				_ctx.Bookings.Add(booking);

				try
				{
					_ctx.SaveChanges();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Lỗi khi lưu booking: {ex.Message}");
					return StatusCode(500, "Lỗi khi lưu thông tin booking vào cơ sở dữ liệu.");
				}

				// Tạo URL thanh toán
				try
				{
					var paymentUrl = _vnPayService.CreatePaymentUrl(booking, HttpContext);
					return Ok(new { paymentUrl });
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Lỗi khi tạo URL thanh toán: {ex.Message}");
					return StatusCode(500, "Lỗi khi tạo URL thanh toán.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Lỗi không xác định: {ex.Message}");
				return StatusCode(500, "Đã xảy ra lỗi không xác định.");
			}
		}

		[HttpGet]
		[Route("api/v1/payment/callback")]
		public IActionResult PaymentCallback()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);

			// Lấy booking ID từ tham số hoặc Session (tùy cách truyền)
			int bookingId = int.Parse(Request.Query["vnp_TxnRef"]);

			// Kiểm tra trạng thái thanh toán
			var booking = _ctx.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
			if (booking == null) return NotFound();

			if (response.VnPayResponseCode == "00")
			{
				booking.Status = "Đã thanh toán";
				_ctx.SaveChanges();
				SendPaymentSuccessEmail(booking.User.Email, booking);
				return Ok(new { message = "Thanh toán thành công" });
			}
			else
			{
				booking.Status = "Giao dịch bị hủy";
				_ctx.SaveChanges();
				return BadRequest(new { message = "Thanh toán thất bại" });
			}
		}


		public IActionResult PaymentFailed()
		{
			return View();
		}
		[HttpPost]
		[Route("api/v1/book-tour")]
		public IActionResult BookTour([FromBody] BookTourRequest request)
		{
			int userId = GetUserId(); // Lấy userId từ session hoặc context
			Tour tour = _tourDAO.GetTourDetail(request.TourId);

			if (tour == null)
			{
				return NotFound();
			}

			decimal totalCost = tour.Price * request.Quantity;

			// Nếu ModelState không hợp lệ, trả về View với viewModel
			if (!ModelState.IsValid)
			{
				var viewModel = new BookTourViewModel
				{
					TourId = tour.TourId,
					TourName = tour.TourName,
					Price = tour.Price,
					Img = tour.Img,
					Quantity = request.Quantity
				};

				return BadRequest(new { message = "Invalid data", viewModel });
			}

			// Tạo đối tượng đặt tour
			Booking newBooking = new Booking
			{
				TourId = request.TourId,
				UserId = userId,
				Total = totalCost,
				BookingDate = DateTime.Now,
				Status = "Đang xác nhận"
			};

			_ctx.Bookings.Add(newBooking);
			_ctx.SaveChanges();
			HttpContext.Session.SetInt32("BookingId", newBooking.BookingId);

			// Gửi trả về kết quả từ server về Client (đổi lại cần thiết lập PaymentService hoặc gửi Payment URL đến Flutter)
			return Ok(new { bookingId = newBooking.BookingId });
		}










		// Action hiển thị thông tin booking của người dùng
		public IActionResult BookingDetails()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login");
            }
            int userId = GetUserId();
            List<Booking> cartItems = _bookingDAO.GetItemsByUserId(userId);

            // Passing data
            return View(cartItems);
        }

        // Action hủy booking
        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {


            // Lấy thông tin người dùng hiện tại
            int userId = GetUserId();

            // Kiểm tra xem booking có thuộc về người dùng hiện tại không
            Booking booking = _ctx.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
            {
                // Xử lý trường hợp booking không tồn tại hoặc không thuộc về người dùng hiện tại
                return NotFound();
            }

            // Thực hiện logic hủy booking ở đây (ví dụ: đặt trạng thái booking thành "Cancelled")
            booking.Status = "Đã hủy";
            _ctx.SaveChanges();

            // Chuyển hướng đến trang thông báo hoặc trang lịch sử đặt tour của người dùng
            return RedirectToAction("BookingDetails", "Home");
        }

        public IActionResult AddReview(int id)
        {
            // Lấy thông tin tour dựa trên id được truyền vào
            Tour tour = _tourDAO.GetTourDetail(id);

            if (tour == null)
            {
                return NotFound();
            }

            // Hiển thị trang đặt tour với thông tin của tour
            return View(tour);
        }

        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            // Get the user ID of the currently logged-in user
            int userId = GetUserId();

            // Check if the user has booked the tour
            bool hasBooked = _ctx.Bookings.Any(b => b.TourId == review.TourId && b.UserId == userId);

            if (!hasBooked)
            {
                // Set TempData with the message
                TempData["BookingMessage"] = "Bạn chưa đặt tour này!";
                return RedirectToAction("TourDetail", new { id = review.TourId });
            }

            // Set the user ID for the review
            review.UserId = userId;

            // Add the review to the database
            _ctx.Reviews.Add(review);
            _ctx.SaveChanges();

            // Redirect to the tour details page after adding the review
            return RedirectToAction("TourDetail", new { id = review.TourId });
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}