using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using GazeManager.Infrastructures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Models.Requests;
using GazeManager.Models.Responses;
using GazeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using PhoneNumbers;
using Notification = GazeManager.Models.Notification;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly BlobService blobService;

        public UsersController(AppDbContext context, IConfiguration configuration, BlobService blobService)
        {
            _context = context;
            _configuration = configuration;
            this.blobService = blobService;
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

        [HttpGet("profile")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult<User>> GetProfile()
        {
            var id = User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value;
            var user = await _context.User.FindAsync(long.Parse(id));

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
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            string formattedPhoneNumber = phoneNumberUtil.Format(phoneNumberUtil.Parse(user.Phone, "VN"), PhoneNumberFormat.E164);
            bool existEmail = await _context.User.AnyAsync(x => x.Email == user.Email);
            bool existPhoneNumber = await _context.User.AnyAsync(x => x.Phone == formattedPhoneNumber);
            if (existEmail && existPhoneNumber)
            {
                return Conflict(2);
            } else if (existEmail)
            {
                return Conflict(0);
            } else if (existPhoneNumber)
            {
                return Conflict(1);
            }
            string avatarUrl = await blobService.UploadFile("avatars", DateTime.Now.Ticks + ".jpg", user.Avatar);
            UserRecordArgs userRecordArgs = new UserRecordArgs
            {
                DisplayName = user.Name,
                Email = user.Email,
                PhoneNumber = formattedPhoneNumber,
                Password = user.Password,
                PhotoUrl = avatarUrl,
                Disabled = false
            };
            UserRecord cloudUser = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

            user.Role = Role.Customer;
            user.CreatedDate = DateTime.Now;
            user.Avatar = avatarUrl;
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(cloudUser.Uid, new Dictionary<string, object>
            {
                {"gazeId", user.Id},
                {ClaimTypes.Role, Role.Customer},
                {ClaimTypes.Email, user.Email}
            });

            return CreatedAtAction(nameof(PostUser), new { id = user.Id }, user);
        }

        // POST: api/Users/register
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult<User>> Register()
        {
            var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(Request.Headers[HeaderNames.Authorization].ToString().Substring("Bearer ".Length));
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

            _context.User.Add(user);
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

        [HttpPost("cartitem")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> AddCartItem([FromBody] CartItem item)
        {
            ProductOption productOption = await _context.ProductOption.FindAsync(item.OptionId);
            if (productOption == null)
            {
                return BadRequest();
            }
            var id = User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value;
            item.UserId = long.Parse(id);

            CartItem origin = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == item.UserId && c.OptionId == item.OptionId);

            if (origin != null)
            {
                origin.Quantity += item.Quantity;
                _context.Cart.Update(origin);
                await _context.SaveChangesAsync();

                return Ok(origin);
            }

            item.CreatedDate = DateTime.Now;
            _context.Cart.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AddCartItem), null, item);
        }

        [HttpPost("cart")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> AddCart([FromBody] List<CartItem> newCart)
        {
            var id = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var cart = await _context.Cart.Where(c => c.UserId == id).ToListAsync();
            _context.Cart.RemoveRange(cart);
            foreach (var item in newCart)
            {
                ProductOption productOption = await _context.ProductOption.FindAsync(item.OptionId);
                if (productOption == null)
                {
                    return BadRequest();
                }
                item.UserId = id;

                item.Id = 0;
                item.CreatedDate = DateTime.Now;
                _context.Cart.Add(item);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AddCartItem), null, newCart);
        }

        [HttpGet("cart")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> GetCart()
        {
            var id = User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value;
            var cart = await _context.Cart
                .Include(c => c.Option)
                .ThenInclude(o => o.Product)
                .Where(x => x.UserId == long.Parse(id))
                .ToListAsync();
            foreach (var item in cart)
            {
                item.Option.Product.ProductOptions = null;
            }
            return Ok(cart);
        }

        [HttpPost("guests/cart")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCart([FromBody] IEnumerable<CartItem> items)
        {
            List<CartItem> cart = new List<CartItem>();
            foreach (var item in items)
            {
                item.Option = await _context.ProductOption.Include(po => po.Product).FirstOrDefaultAsync(po => po.Id == item.OptionId);
                if (item.Option != null)
                {
                    item.Option.Product.ProductOptions = null;
                    cart.Add(item);
                }
            }
            return Ok(cart);
        }

        [HttpPut("cart")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> UpdateCart([FromBody] List<CartItem> items)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);

            var cart = await _context.Cart.Where(c => c.UserId == userId).ToListAsync();

            foreach (var cartItem in cart)
            {
                CartItem updated = items.FirstOrDefault(c => c.OptionId == cartItem.OptionId);
                if (updated != null && updated.Quantity > 0)
                {
                    cartItem.Quantity = updated.Quantity;

                    _context.Cart.Update(cartItem);
                }
                else
                {
                    _context.Cart.Remove(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(items);
        }

        [HttpDelete("cart/{id}")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> RemoveFromCart(long id)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var cartItem = await _context.Cart.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (cartItem != null)
            {
                _context.Cart.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpDelete("cart")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> ClearCart()
        {
            var id = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var cart = await _context.Cart.Where(c => c.UserId == id).ToListAsync();
            _context.Cart.RemoveRange(cart);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("orders")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult<List<Order>>> GetOrder([FromQuery] int? status)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            IQueryable<Order> queryable = _context.Order.Include(o => o.Cart).Where(o => o.UserId == userId );
            if (status != null)
            {
                queryable = queryable.Where(o => o.Status == status);
            }

            List<Order> orders = await queryable.ToListAsync();

            foreach (var order in orders)
            {
                foreach (var productOrder in order.Cart)
                {
                    productOrder.Option = new ProductOption
                    {
                        Price = productOrder.Price,
                        Discount = productOrder.Discount
                    };
                }
            }

            return Ok(orders);
        }

        [HttpDelete("orders/{id}")]
        [Authorize(Roles = Role.Customer)]
        public async Task<IActionResult> UpdateStatus(long id)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var order = await _context.Order.FindAsync(id);
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            order.Status = (int) OrderStatus.Cancelled;
            _context.Order.Update(order);
            await _context.SaveChangesAsync();

            var userDevice = await _context.DeviceInfo.Where(x => x.UserId == order.UserId && x.Status == 1).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
            if (userDevice != null)
            {
                var notification = new Notification
                {
                    Title = "Order Notification",
                    UserId = order.UserId,
                    Content = $"Order {order.Code} has been cancelled successfully",
                    ObjectId = order.Id,
                    Type = (int)NotificationType.Order,
                    Status = 0,
                    CreatedDate = DateTime.Now
                };

                _context.Notification.Add(notification);
                await _context.SaveChangesAsync();

                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Data = notification.GetData(),
                    Token = userDevice.RegId,
                };

                // Send a message to the device corresponding to the provided
                // registration token.
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
            }

            return Ok(order);
        }

        [HttpGet("notifications")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult> GetNotification([FromQuery] BaseFilter request)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);

            List<Notification> notifications = new List<Notification>();
            IQueryable<Notification> queryable = _context.Notification.Where(x => x.UserId == userId);

            if (request?.PageIndex > 0 && request.PageSize > 0)
            {
                queryable = queryable.Skip(request.PageSize.Value * (request.PageIndex.Value - 1)).Take(request.PageSize.Value);
            }

            int? totalPages = await queryable.CountAsync();
            notifications = await queryable.ToListAsync();

            return Ok(new Pagination<Notification>
            {
                TotalPages = totalPages.Value,
                Data = notifications
            });
        }

        [HttpGet("notifications/size")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult> GetNotificationSize([FromQuery] int? status)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);

            List<Notification> notifications = new List<Notification>();
            IQueryable<Notification> queryable = _context.Notification.Where(x => x.UserId == userId);

            if (status != null)
            {
                queryable = queryable.Where(x => x.Status == status);
            }

            int? totalPages = await queryable.CountAsync();

            return Ok(totalPages);
        }

        [HttpDelete("notifications")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult> DeleteAllNotification()
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var notifications = await _context.Notification.Where(x => x.UserId == userId).ToListAsync();

            _context.Notification.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("notifications/{id}")]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult<Notification>> DeleteNotification(long id)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var notification = await _context.Notification.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
