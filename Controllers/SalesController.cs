using Microsoft.AspNetCore.Mvc;
using POS_Cafeteria.Models;

namespace POS_Cafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly PosContext _context;

        public SalesController(PosContext context)
        {
            _context = context;
        }

        // POST: api/sales -> Registrar una venta
        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleRequest request)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var sale = new Sale { UserId = request.UserId, Total = request.Total };
                    _context.Sales.Add(sale);
                    _context.SaveChanges();

                    foreach (var item in request.Details)
                    {
                        var detail = new SaleDetail
                        {
                            SaleId = sale.SaleId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        _context.SaleDetails.Add(detail);
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return Ok(new { message = "Venta registrada con éxito", saleId = sale.SaleId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest("Error al procesar: " + ex.Message);
                }
            }
        }
    }

    public class SaleRequest
    {
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public List<SaleDetail> Details { get; set; } = new();
    }
}