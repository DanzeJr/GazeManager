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
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetProduct(int pageIndex = -1, int pageSize = -1)
        {
            List<Product> products;
            if (pageIndex >= 0 && pageSize > 0)
            {
                int totalPages = await _context.Product.CountAsync();
                products = await _context.Product.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync();
                return Ok(new Pagination<Product>
                {
                    TotalPages = totalPages,
                    Data = products
                });
            }

            products = await _context.Product.ToListAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(long id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(long id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return product;
        }

        [HttpGet("generate")]
        public async Task<ActionResult<List<Product>>> Generate()
        {
            List<Category> categories = await _context.Category.ToListAsync();

            if (categories.Any())
            {
                List<Product> products = await _context.Product.ToListAsync();
                if (products.Count > 15)
                {
                    return Ok(products);
                }

                Faker faker = new Faker();
                products = Product.Faker.Generate((faker.Random.Number(5, 100)));

                foreach (var product in products)
                {
                    for (int i = 0; i < faker.Random.Number(1, 5); i++)
                    {
                        if (i == 0 || faker.Random.Bool())
                        {
                            product.ProductCategories.Add(new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = categories[i].Id
                            });
                        }
                    }
                }

                await _context.Product.AddRangeAsync(products);
                await _context.SaveChangesAsync();
                products = await _context.Product.ToListAsync();

                return CreatedAtAction(nameof(Generate), null, products);
            }

            return BadRequest("Please initialize category first.");
        }

        private bool ProductExists(long id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
