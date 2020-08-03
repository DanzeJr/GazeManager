using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GazeManager.Models;
using GazeManager.Models.Requests;
using GazeManager.Models.Responses;
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
        public async Task<IActionResult> GetProduct([FromQuery] FilterProduct request = null)
        {
            IQueryable<Product> queryable;
            List<Product> products;

            if (request?.CategoryId != null)
            {
                queryable = _context.Product.Include(p => p.ProductOptions).Where(x => x.ProductCategories.Any(pc => pc.CategoryId == request.CategoryId));

            }
            else
            {
                queryable = _context.Product.Include(p => p.ProductOptions).AsQueryable();
            }

            if (!string.IsNullOrEmpty(request?.Search))
            {
                queryable = queryable.Where(x => EF.Functions.Like(x.Name, $"{request.Search}%"));
            }

            if (request?.PageIndex > 0 && request.PageSize > 0)
            {
                int? totalPages = await queryable.CountAsync();
                queryable = queryable.Skip(request.PageSize.Value * (request.PageIndex.Value - 1)).Take(request.PageSize.Value);

                products = await queryable.ToListAsync();
                return Ok(new Pagination<Product>
                {
                    TotalPages = totalPages.Value,
                    Data = products
                });
            }

            products = await queryable.ToListAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(long id)
        {
            var product = await _context.Product
                .Include(p => p.ProductOptions)
                .Include(x => x.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pg => pg.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("options/{id}")]
        public async Task<ActionResult<ProductOption>> GetProductOption(long id)
        {
            var productOption = await _context.ProductOption
                .Include(po => po.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productOption == null)
            {
                return NotFound();
            }

            return productOption;
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
                products = Product.Faker.Generate((faker.Random.Number(5, 50)));

                foreach (var product in products)
                {
                    for (int i = 0; i < faker.Random.Number(1, categories.Count); i++)
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

                    for (int i = 0; i < faker.Random.Number(1, 5); i++)
                    {
                        string color = faker.Commerce.Color().ToUpperInvariant();
                        for (int j = 0; j < faker.Random.Number(1, 3); j++)
                        {
                            ProductOption option = ProductOption.Faker.Generate();
                            option.Color = color;
                            option.Size = j;
                            product.ProductOptions.Add(option);
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        ProductImage productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            Name = faker.Image.LoremFlickrUrl(keywords: "sunglass, beauty", matchAllKeywords: true,
                                width: 1000, height: 1000)
                        };
                        _context.ProductImage.Add(productImage);

                        product.ProductImages.Add(productImage);
                    }

                    _context.Product.Add(product);
                }
                await _context.SaveChangesAsync();

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
