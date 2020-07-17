using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Services;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategory(int pageIndex = -1, int pageSize = -1)
        {
            List<Category> categories;
            if (pageIndex >= 0 && pageSize > 0)
            {
                int totalPages = await _context.Category.CountAsync();
                categories = await _context.Category.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync();
                return Ok(new Pagination<Category>
                {
                    TotalPages = totalPages,
                    Data = categories
                });
            }

            categories = await _context.Category.ToListAsync();
            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(long id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(long id, Category category)
        {
            if (id != category.Id)
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

        // POST: api/Categories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(long id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return category;
        }

        [HttpGet("generate")]
        public async Task<ActionResult<List<Category>>> Generate()
        {
            List<Category> categories = await _context.Category.ToListAsync();
            if (categories.Count > 15)
            {
                return Ok(categories);
            }

            categories = Category.Faker.Generate((new Faker()).Random.Number(1, 15));
            await _context.Category.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            categories = await _context.Category.ToListAsync();

            return CreatedAtAction(nameof(Generate), null, categories);
        }

        private bool CategoryExists(long id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}
