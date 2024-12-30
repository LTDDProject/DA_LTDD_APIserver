using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using QLTours.Models.QLTours.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLTours.Areas.Employee.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class TourDetailsController : ControllerBase
	{
		private readonly QuanLyTourContext _context;

		public TourDetailsController(QuanLyTourContext context)
		{
			_context = context;
		}

		// GET: api/Employee/TourDetails
		[HttpGet]
		public async Task<IActionResult> GetTourDetails()
		{
			var tourDetails = await _context.TourDetails
				.Include(td => td.Tour)   // Bao gồm thông tin Tour
				.Include(td => td.Hotel)  // Bao gồm thông tin Hotel
				.Include(td => td.Vehicle) // Bao gồm thông tin Vehicle
				.ToListAsync();

			// Lấy thông tin TourName, VehicleName, HotelName từ TourDetail
			var result = tourDetails.Select(td => new
			{
				TourName = td.Tour?.TourName ?? "No Tour",
				VehicleName = td.Vehicle?.VehicleName ?? "No Vehicle",
				HotelName = td.Hotel?.HotelName ?? "No Hotel"
			}).ToList();

			return Ok(result);
		}


		// GET: api/Employee/TourDetails/5
		[HttpGet("{id}")]
		public async Task<ActionResult<TourDetail>> GetTourDetail(int id)
		{
			var tourDetail = await _context.TourDetails
				.Include(td => td.Tour)
				.Include(td => td.Hotel)
				.Include(td => td.Vehicle)
				.FirstOrDefaultAsync(m => m.TourDetailId == id);

			if (tourDetail == null)
			{
				return NotFound();
			}

			return Ok(tourDetail);
		}

		// POST: api/Employee/TourDetails
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<TourDetail>> PostTourDetail(TourDetail tourDetail)
		{
			if (ModelState.IsValid)
			{
				_context.TourDetails.Add(tourDetail);
				await _context.SaveChangesAsync();
				return CreatedAtAction(nameof(GetTourDetail), new { id = tourDetail.TourDetailId }, tourDetail);
			}
			return BadRequest(ModelState);
		}

		// PUT: api/Employee/TourDetails/5
		[HttpPut("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PutTourDetail(int id, TourDetail tourDetail)
		{
			if (id != tourDetail.TourDetailId)
			{
				return BadRequest();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(tourDetail);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!TourDetailExists(id))
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

		// DELETE: api/Employee/TourDetails/5
		[HttpDelete("{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteTourDetail(int id)
		{
			var tourDetail = await _context.TourDetails.FindAsync(id);
			if (tourDetail == null)
			{
				return NotFound();
			}

			_context.TourDetails.Remove(tourDetail);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TourDetailExists(int id)
		{
			return _context.TourDetails.Any(e => e.TourDetailId == id);
		}
	}
}
