﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoriesApiController : ControllerBase
	{
		private readonly QuanLyTourContext _context;

		public CategoriesApiController(QuanLyTourContext context)
		{
			_context = context;
		}

		// GET: api/Categories
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
		{
			return await _context.Categories.ToListAsync();
		}

		// GET: api/Categories/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Category>> GetCategory(int id)
		{
			var category = await _context.Categories.FindAsync(id);

			if (category == null)
			{
				return NotFound();
			}

			return category;
		}

		// POST: api/Categories
		[HttpPost]
		public async Task<ActionResult<Category>> PostCategory(Category category)
		{
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
		}

		// PUT: api/Categories/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutCategory(int id, Category category)
		{
			if (id != category.CategoryId)
			{
				return BadRequest();
			}

			_context.Entry(category).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CategoryExists(id))
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

		// DELETE: api/Categories/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			_context.Categories.Remove(category);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool CategoryExists(int id)
		{
			return _context.Categories.Any(e => e.CategoryId == id);
		}
	}
}
