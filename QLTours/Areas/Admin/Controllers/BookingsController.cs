using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QLTours.Areas.Admin.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingsApiController : ControllerBase
	{
		private readonly QuanLyTourContext _context;

		public BookingsApiController(QuanLyTourContext context)
		{
			_context = context;
		}

		// GET: api/Bookings/Revenue
		[HttpGet("Revenue")]
		public IActionResult GetRevenue()
		{
			var bookings = _context.Bookings.Where(b => b.Status == "Đã thanh toán").ToList();

			var revenueByMonthAndYear = from booking in bookings
										group booking by new { booking.BookingDate.Year, booking.BookingDate.Month } into grouped
										select new
										{
											Year = grouped.Key.Year,
											Month = grouped.Key.Month,
											TotalRevenue = grouped.Sum(b => b.Total)
										};

			return Ok(revenueByMonthAndYear);
		}

		// GET: api/Bookings/QuarterlyRevenue
		[HttpGet("QuarterlyRevenue")]
		public IActionResult GetQuarterlyRevenue()
		{
			var bookings = _context.Bookings.Where(b => b.Status == "Đã thanh toán").ToList();

			if (!bookings.Any())
			{
				return NotFound("No bookings found.");
			}

			var revenueByQuarter = from booking in bookings
								   group booking by new
								   {
									   Year = booking.BookingDate.Year,
									   Quarter = (booking.BookingDate.Month - 1) / 3 + 1 // Determine the quarter
								   } into grouped
								   select new
								   {
									   Quarter = grouped.Key.Quarter switch
									   {
										   1 => $"Q1 (Jan-Mar) - {grouped.Key.Year}",
										   2 => $"Q2 (Apr-Jun) - {grouped.Key.Year}",
										   3 => $"Q3 (Jul-Sep) - {grouped.Key.Year}",
										   4 => $"Q4 (Oct-Dec) - {grouped.Key.Year}",
										   _ => $"Unknown Quarter - {grouped.Key.Year}"
									   },
									   TotalRevenue = grouped.Sum(b => b.Total)
								   };

			return Ok(revenueByQuarter);
		}

		// GET: api/Bookings
		[HttpGet]
		public async Task<IActionResult> GetBookings()
		{
			var bookings = await _context.Bookings
										 .Include(b => b.Tour)
										 .Include(b => b.User)
										 .Select(b => new BookingDTO
										 {
											 BookingId = b.BookingId,
											 Status = b.Status,
											 UserId = b.User.UserId,  
											 TourId = b.Tour.TourId,  
											 Total = b.Total,
											 BookingDate = b.BookingDate
										 })
										 .ToListAsync();

			return Ok(bookings);
		}



		// GET: api/Bookings/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetBooking(int id)
		{
			var booking = await _context.Bookings
										 .Include(b => b.Tour)
										 .Include(b => b.User)
										 .FirstOrDefaultAsync(m => m.BookingId == id);

			if (booking == null)
			{
				return NotFound();
			}

			return Ok(booking);
		}

		// POST: api/Bookings
		[HttpPost]
		public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();
			return CreatedAtAction("GetBooking", new { id = booking.BookingId }, booking);
		}

		// PUT: api/Bookings/5
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking booking)
		{
			if (id != booking.BookingId)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			_context.Entry(booking).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return NoContent();
		}

		// DELETE: api/Bookings/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBooking(int id)
		{
			var booking = await _context.Bookings.FindAsync(id);
			if (booking == null)
			{
				return NotFound();
			}

			_context.Bookings.Remove(booking);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
