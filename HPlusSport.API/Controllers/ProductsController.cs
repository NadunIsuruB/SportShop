using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _contex;

        public ProductsController(ShopContext contex)
        {
            _contex= contex;
            _contex.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            var products = await _contex.Products.ToArrayAsync();
            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _contex.Products.FindAsync(id);

            if(product == null)
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
        public async Task<ActionResult> DeleteMultiple([FromQuery]int[] ids)
        {
            var products = new List<Product>();
            foreach(var id in ids)
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
