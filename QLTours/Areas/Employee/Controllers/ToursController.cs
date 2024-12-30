using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using QLTours.Services;

namespace QLTours.Areas.Employee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ToursController : ControllerBase
    {
        private readonly QuanLyTourContext _context;
        private readonly ImageTourService _imageTourService;

        public ToursController(QuanLyTourContext context, ImageTourService imageTourService)
        {
            _context = context;
            _imageTourService = imageTourService;
        }

		// GET: api/Tours
		[HttpGet]
		public async Task<IActionResult> GetTours(string? search)
		{
			var tours = _context.Tours.AsQueryable();

			// Nếu có từ khóa tìm kiếm, lọc danh sách tours theo tên
			if (!string.IsNullOrEmpty(search))
			{
				tours = tours.Where(t => t.TourName.Contains(search));
			}

			// Lấy kết quả và trả về
			var result = await tours.ToListAsync();
			return Ok(result);
		}



		// GET: api/Tours/5
		[HttpGet("{id}")]
        public async Task<IActionResult> GetTour(int id)
        {
            var tour = await _context.Tours
                                      .Include(t => t.Category) // Optional: Include related entities
                                      .FirstOrDefaultAsync(t => t.TourId == id);

            if (tour == null)
            {
                return NotFound();
            }

            return Ok(tour);
        }

		
		// POST: api/Tours
		[HttpPost]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<Tour>> CreateTour([Bind("TourId,TourName,Description,Price,CategoryId,Quantity,StartDate,EndDate,Img")] Tour tour, IFormFile img)
		{
			if (ModelState.IsValid)
			{
				// Nếu có ảnh được gửi lên, lưu ảnh
				if (img != null && img.Length > 0)
				{
					tour.Img = await _imageTourService.SaveImageAsync(img);
				}

				// Thêm tour mới vào cơ sở dữ liệu
				_context.Add(tour);
				await _context.SaveChangesAsync();

				// Trả về tour đã tạo với đường dẫn đến phương thức GetTour
				return CreatedAtAction(nameof(GetTour), new { id = tour.TourId }, tour);
			}

			// Nếu có lỗi trong ModelState, trả về BadRequest
			return BadRequest(ModelState);
		}

		// PUT: api/Tours/5
		[HttpPut("{id}")]
        public async Task<IActionResult> EditTour(int id, [Bind("TourId,TourName,Description,Price,CategoryId,Quantity,StartDate,EndDate,Img")] Tour tour, IFormFile img)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (img != null && img.Length > 0)
                    {
                        tour.Img = await _imageTourService.SaveImageAsync(img);
                    }

                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return NoContent();
            }

            return BadRequest(ModelState);
        }

        // DELETE: api/Tours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TourExists(int id)
        {
            return (_context.Tours?.Any(e => e.TourId == id)).GetValueOrDefault();
        }
    }
}
