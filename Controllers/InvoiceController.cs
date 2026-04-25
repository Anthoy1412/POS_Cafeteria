using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_Cafeteria.Models;

namespace POS_Cafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly PosContext _context;

        public InvoiceController(PosContext context)
        {
            _context = context;
        }

        // GET: api/invoice/5 -> Obtener datos completos para la factura
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Details)          // Incluye los renglones de la venta
                .ThenInclude(d => d.Product)      // De cada renglón, trae la info del producto (nombre)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound(new { message = "La factura no existe" });
            }

            // Formateamos la respuesta para que el Frontend la lea fácil
            var result = new
            {
                Folio = sale.SaleId,
                Fecha = sale.Date.ToString("dd/MM/yyyy HH:mm"),
                Total = sale.Total,
                Productos = sale.Details.Select(d => new
                {
                    Nombre = d.Product?.Name ?? "Producto eliminado",
                    Cantidad = d.Quantity,
                    PrecioUnitario = d.Price,
                    Subtotal = d.Quantity * d.Price
                })
            };

            return Ok(result);
        }
    }
}