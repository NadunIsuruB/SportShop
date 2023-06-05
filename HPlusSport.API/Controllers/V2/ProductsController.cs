using HPlusSport.API.Filters;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HPlusSport.API.Controllers.V2
{
    [ApiVersion("2.0")]
    //[Route("api/[controller]")]
    [Route("v{v:apiVersion}/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _contex;

        public ProductsController(ShopContext contex)
        {
            _contex = contex;
            _contex.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts([FromQuery] QueryParameters queryParameters)
        {
            IQueryable<Product> products = _contex.Products.Where(p => p.IsAvailable ==true);

            if (queryParameters.MinPrice != 0)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice
                    );
            }
            if (queryParameters.MaxPrice != 0)
            {
                products = products.Where(
                    p => p.Price <= queryParameters.MaxPrice
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(
                    p => p.Sku == queryParameters.Sku
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower())
                    );
            }
            if (!string.IsNullOrEmpty(queryParameters.SearchStr))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.SearchStr.ToLower()) ||
                    p.Sku.ToLower().Contains(queryParameters.SearchStr.ToLower()) ||
                    p.Price.ToString().Contains(queryParameters.SearchStr.ToLower())
                    );
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                }
            }

            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            if (products == null)
            {
                return NotFound();
            }
            return Ok(await products.ToArrayAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _contex.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            /*if (!ModelState.IsValid)
            {
                return BadRequest();
            }*/
            _contex.Products.Add(product);
            await _contex.SaveChangesAsync();
            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product
                );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _contex.Entry(product).State = EntityState.Modified;

            try
            {
                await _contex.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_contex.Products.Any(p => p.Id == id))
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

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _contex.Products.FindAsync(id);
            if (product == null) { return NotFound(); }
            _contex.Products.Remove(product);
            await _contex.SaveChangesAsync();

            return product;
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery] int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _contex.Products.FindAsync(id);
                if (product == null) { return NotFound(); }

                products.Add(product);
            }

            _contex.Products.RemoveRange(products);
            await _contex.SaveChangesAsync();

            return Ok(products);
        }
    }
}
