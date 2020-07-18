using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GazeManager.Infrastructures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Services;
using Microsoft.Extensions.Configuration;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ConfigurationsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Configurations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Configuration>>> GetConfiguration()
        {
            return await _context.Configuration.ToListAsync();
        }

        [HttpGet("latest")]
        public async Task<ActionResult<Configuration>> GetLatestConfiguration()
        {
            var configuration = await _context.Configuration.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
            if (configuration == null)
            {
                return NotFound();
            }

            return Ok(configuration);
        }

        // GET: api/Configurations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Configuration>> GetConfiguration(long id)
        {
            var configuration = await _context.Configuration.FindAsync(id);

            if (configuration == null)
            {
                return NotFound();
            }

            return configuration;
        }

        // PUT: api/Configurations/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConfiguration(long id, Configuration configuration)
        {
            if (id != configuration.Id)
            {
                return BadRequest();
            }

            _context.Entry(configuration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfigurationExists(id))
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

        // POST: api/Configurations
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Configuration>> PostConfiguration(Configuration configuration)
        {
            _context.Configuration.Add(configuration);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConfiguration", new { id = configuration.Id }, configuration);
        }

        // DELETE: api/Configurations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Configuration>> DeleteConfiguration(long id)
        {
            var configuration = await _context.Configuration.FindAsync(id);
            if (configuration == null)
            {
                return NotFound();
            }

            _context.Configuration.Remove(configuration);
            await _context.SaveChangesAsync();

            return configuration;
        }

        [HttpGet("init")]
        public async Task<ActionResult<Configuration>> Initialize()
        {
            await _context.Database.EnsureDeletedAsync();

            await DatabaseSeeder.InitializeAsync(_context, _configuration);

            return Ok();
        }

        private bool ConfigurationExists(long id)
        {
            return _context.Configuration.Any(e => e.Id == id);
        }
    }
}
