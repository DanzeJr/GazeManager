using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Services;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceInfo>>> GetDeviceInfo()
        {
            return await _context.DeviceInfo.ToListAsync();
        }

        // GET: api/Devices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceInfo>> GetDeviceInfo(long id)
        {
            var deviceInfo = await _context.DeviceInfo.FindAsync(id);

            if (deviceInfo == null)
            {
                return NotFound();
            }

            return deviceInfo;
        }

        // PUT: api/Devices/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeviceInfo(long id, DeviceInfo deviceInfo)
        {
            if (id != deviceInfo.Id)
            {
                return BadRequest();
            }

            _context.Entry(deviceInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceInfoExists(id))
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

        // POST: api/Devices
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<DeviceInfo>> PostDeviceInfo(DeviceInfo deviceInfo)
        {
            deviceInfo.CreatedDate = DateTime.Now;
            deviceInfo.LastUpdate = DateTime.Now;
            var id = User?.Claims?.FirstOrDefault(x => x.Type == "gazeId")?.Value;
            if (id != null)
            {
                deviceInfo.UserId = long.Parse(id);
                var userDevice = await _context.DeviceInfo.Where(x => x.UserId == deviceInfo.UserId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                if (deviceInfo.InstanceId.Equals(userDevice.InstanceId))
                {
                    deviceInfo.CreatedDate = userDevice.CreatedDate;
                    _context.DeviceInfo.Update(deviceInfo);
                    await _context.SaveChangesAsync();

                    return Ok(deviceInfo);
                }
            }

            await _context.DeviceInfo.AddAsync(deviceInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostDeviceInfo), null, deviceInfo);
        }

        // DELETE: api/Devices/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<DeviceInfo>> DeleteDeviceInfo(long id)
        {
            var deviceInfo = await _context.DeviceInfo.FindAsync(id);
            if (deviceInfo == null)
            {
                return NotFound();
            }

            _context.DeviceInfo.Remove(deviceInfo);
            await _context.SaveChangesAsync();

            return deviceInfo;
        }

        private bool DeviceInfoExists(long id)
        {
            return _context.DeviceInfo.Any(e => e.Id == id);
        }
    }
}
