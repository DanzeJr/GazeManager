using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using GazeManager.Infrastructures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notification = GazeManager.Models.Notification;

namespace GazeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public OrdersController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
            return await _context.Order.Include(o => o.Cart).ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(long id)
        {
            var userId = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            var order = await _context.Order.FindAsync(id);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            var productOrders = await _context.ProductOrder
                .Include(po => po.Option)
                .ThenInclude(o => o.Product)
                .Where(po => po.OrderId == order.Id)
                .ToListAsync();

            foreach (var productOrder in productOrders)
            {
                productOrder.Option.Color = productOrder.Color;
                productOrder.Option.Size = productOrder.Size;
                productOrder.Option.Price = productOrder.Price;
                productOrder.Option.Discount = productOrder.Discount;
                productOrder.Option.Product.ProductOptions = null;
            }

            order.Cart = productOrders;

            return Ok(order);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(long id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        [HttpPut("{id}/status")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] int status)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Order.Update(order);
            await _context.SaveChangesAsync();

            string content = "";
            switch (status)
            {
                case 0:
                {
                    content = $"Order {order.Code} has been submitted";
                    break;
                }
                case 1:
                {
                    content = $"Order {order.Code} has been confirmed and on delivery";
                    break;
                }
                case 2:
                {
                    content = $"Order {order.Code} has been completed";
                    break;
                }
                case 3:
                {
                    content = $"Order {order.Code} has been cancelled";
                    break;
                }
            }

            var userDevice = await _context.DeviceInfo.Where(x => x.UserId == order.UserId && x.Status == 1).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
            if (userDevice != null)
            {
                var notification = new Notification
                {
                    Title = "Order Notification",
                    UserId = order.UserId,
                    Content = content,
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

        [HttpPost]
        [Authorize(Roles = Role.Customer)]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var id = long.Parse(User.Claims.FirstOrDefault(x => x.Type == "gazeId")?.Value);
            order.UserId = id;

            foreach (var productOrder in order.Cart)
            {
                ProductOption option = await _context.ProductOption.FindAsync(productOrder.OptionId);
                if (option == null)
                {
                    return BadRequest();
                }

                productOrder.Id = 0;
                productOrder.Color = option.Color;
                productOrder.Size = option.Size;
                productOrder.Discount = option.Discount;
                productOrder.Price = option.Price;
                productOrder.Option = null;
            }

            Configuration configuration = await _context.Configuration.OrderByDescending(c => c.CreatedDate).FirstOrDefaultAsync();
            order.Code = $"#{DateTime.Now:yyMMddHHmmssfff}";
            order.ShippingFee = order.ShippingOption == 1
                ? configuration.StandardShippingFee
                : configuration.PremiumShippingFee;
            order.Status = (int) OrderStatus.Submitted;

            _context.Order.Add(order);

            List<CartItem> cart = await _context.Cart.Where(c => c.UserId == id).ToListAsync();
            _context.Cart.RemoveRange(cart);

            await _context.SaveChangesAsync();

            var userDevice = await _context.DeviceInfo.Where(x => x.UserId == id).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
            if (userDevice != null)
            {
                var notification = new Notification
                {
                    Title = "Order Submitted",
                    UserId = id,
                    Content = "Your order has been submitted!",
                    ObjectId = order.Id,
                    Type = (int) NotificationType.Order,
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

            return CreatedAtAction(nameof(PostOrder), new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(long id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private bool OrderExists(long id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
    }
}
