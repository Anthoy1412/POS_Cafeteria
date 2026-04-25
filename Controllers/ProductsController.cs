using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_Cafeteria.Models;

namespace POS_Cafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly PosContext _context;

        public ProductsController(PosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStock()
        {
            return await _context.Products
                .Where(p => p.Stock < 5)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducts), new { id = product.ProductId }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            try {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return NoContent();
            } 
            catch (DbUpdateException) {
                return BadRequest("No se puede eliminar un producto con ventas registradas. Ajuste su stock a 0.");
            }
        }

        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Stock = newQuantity;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Stock actualizado", nuevoStock = product.Stock });
        }
    }
}