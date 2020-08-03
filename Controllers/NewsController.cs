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
    public class NewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/News
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsInfo>>> GetNews()
        {
            return await _context.News.ToListAsync();
        }

        // GET: api/News/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsInfo>> GetNewsInfo(long id)
        {
            var newsInfo = await _context.News.FindAsync(id);

            if (newsInfo == null)
            {
                return NotFound();
            }

            return newsInfo;
        }

        // PUT: api/News/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNewsInfo(long id, NewsInfo newsInfo)
        {
            if (id != newsInfo.Id)
            {
                return BadRequest();
            }

            _context.Entry(newsInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsInfoExists(id))
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

        // POST: api/News
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<NewsInfo>> PostNewsInfo(NewsInfo newsInfo)
        {
            _context.News.Add(newsInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNewsInfo", new { id = newsInfo.Id }, newsInfo);
        }

        // DELETE: api/News/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<NewsInfo>> DeleteNewsInfo(long id)
        {
            var newsInfo = await _context.News.FindAsync(id);
            if (newsInfo == null)
            {
                return NotFound();
            }

            _context.News.Remove(newsInfo);
            await _context.SaveChangesAsync();

            return newsInfo;
        }

        [HttpGet("generate")]
        public async Task<ActionResult<List<NewsInfo>>> Generate()
        {
            List<NewsInfo> news = NewsInfo.Faker.Generate(10);
            _context.News.AddRange(news);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Generate), null, news);
        }

        private bool NewsInfoExists(long id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}
