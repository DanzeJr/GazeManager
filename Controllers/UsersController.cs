using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(long id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // POST: api/Users/register
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult<User>> Register()
        {
            var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(Request.Headers[HeaderNames.Authorization]);
            var fireBaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(token.Uid);
            var user = await _context.User.FirstOrDefaultAsync(x => x.Email == fireBaseUser.Email);
            if (user != null)
            {
                return Ok(user);
            }

            user = new User
            {
                Name = fireBaseUser.DisplayName,
                Avatar = fireBaseUser.PhotoUrl,
                Email = fireBaseUser.Email,
                Phone = fireBaseUser.PhoneNumber,
                Role = Role.Customer
            };

            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(fireBaseUser.Uid, new Dictionary<string, object>
            {
                {"gazeId", user.Id},
                {ClaimTypes.Role, Role.Customer},
                {ClaimTypes.Email, fireBaseUser.Email}
            });

            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(long id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        [HttpPost("{id}/devices")]
        public async Task<ActionResult<DeviceInfo>> RegisterDevice(long id, [FromBody] DeviceInfo device)
        {
            User user = await _context.User.FindAsync(id);
            if (user != null && device != null)
            {
                await _context.DeviceInfo.AddAsync(device);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(RegisterDevice), null, device);
            }

            return BadRequest();
        }

        // GET: api/Users
        [HttpGet("init")]
        public async Task<ActionResult<User>> InitAdmin()
        {
            var adminInfo = _configuration.GetSection("AppConfig:Firebase:Admin").Get<UserRecordArgs>();
            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(adminInfo.Email);
            if (user == null)
            {
                user = await FirebaseAuth.DefaultInstance.CreateUserAsync(adminInfo);
            }
            else
            {
                adminInfo.Uid = user.Uid;
                user = await FirebaseAuth.DefaultInstance.UpdateUserAsync(adminInfo);
            }

            var admin = await _context.User.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (admin == null)
            {
                admin = new User();
                admin.Email = user.Email;
                admin.Name = user.DisplayName;
                admin.Avatar = user.PhotoUrl;
                admin.Phone = user.PhoneNumber;
                admin.Role = Role.Admin;
                admin.CreatedDate = DateTime.Now;

                await _context.User.AddAsync(admin);
                await _context.SaveChangesAsync();

                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid, new Dictionary<string, object>
                {
                    {"gazeId", admin.Id},
                    {ClaimTypes.Role, Role.Admin},
                    {ClaimTypes.Email, user.Email}
                });

                return CreatedAtAction(nameof(InitAdmin), null, admin);
            }

            admin.Name = user.DisplayName;
            admin.Avatar = user.PhotoUrl;
            admin.Phone = user.PhoneNumber;
            admin.Role = Role.Admin;

            _context.User.Update(admin);
            await _context.SaveChangesAsync();

            return Ok(admin);
        }

        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
