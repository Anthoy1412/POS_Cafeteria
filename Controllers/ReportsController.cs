using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_Cafeteria.Models;

namespace POS_Cafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly PosContext _context;

        public ReportsController(PosContext context)
        {
            _context = context;
        }

        // 1. REPORTE: Productos más vendidos (Top 5)
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts()
        {
            var topProducts = await _context.SaleDetails
                .GroupBy(sd => sd.ProductId)
                .Select(g => new
                {
                    // Obtenemos el nombre buscando directamente en la tabla PRODUCT
                    ProductName = _context.Products
                                    .Where(p => p.ProductId == g.Key)
                                    .Select(p => p.Name)
                                    .FirstOrDefault() ?? "Producto Desconocido",
                    TotalSold = g.Sum(sd => sd.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            return Ok(topProducts);
        }

        // 2. REPORTE: Ventas totales del día (Para el cuadro de resumen)
        [HttpGet("daily-sales")]
        public async Task<IActionResult> GetDailySales()
        {
            var today = DateTime.Today;
            // Sumamos el total de todas las ventas de hoy
            var total = await _context.Sales
                .Where(s => s.Date >= today)
                .SumAsync(s => s.Total);

            return Ok(new { TotalAmount = total });
        }
    }
}