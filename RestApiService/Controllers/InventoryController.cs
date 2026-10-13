using Microsoft.AspNetCore.Mvc;
using RestApiService.Data;
using RestApiService.Models;

namespace RestApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("check/{productId}/{quantity}")]
        public IActionResult CheckStock(string productId, int quantity)
        {
            // Validar si la cantidad solicitada es lógica
            if (quantity <= 0)
            {
                return BadRequest(new { Success = false, Message = "La cantidad a consultar debe ser mayor a 0." });
            }

            var product = _context.Products.Find(productId);

            if (product != null)
            {
                bool isAvailable = product.Stock >= quantity;
                return Ok(new { ProductId = productId, Available = isAvailable, CurrentStock = product.Stock }); // 200 OK
            }

            return NotFound(new { Success = false, Message = $"Product {productId} not found." }); // 404 Not Found
        }

        [HttpPost("order")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest(new { Success = false, Message = "La cantidad del pedido debe ser mayor a 0." }); // 400 Bad Request
            }

            var product = _context.Products.Find(request.ProductId);

            if (product == null)
            {
                return NotFound(new { Success = false, Message = "Product not found." }); // 404 Not Found
            }

            if (product.Stock < request.Quantity)
            {
                return BadRequest(new { Success = false, Message = "Insufficient stock." }); // 400 Bad Request por regla de negocio
            }

            product.Stock -= request.Quantity;
            _context.SaveChanges();

            return Ok(new { Success = true, Message = "Order created.", RemainingStock = product.Stock }); // 200 OK
        }

        [HttpPut("update")]
        public IActionResult UpdateStock([FromBody] UpdateRequest request)
        {
            if (request.NewQuantity < 0)
            {
                return BadRequest(new { Success = false, Message = "El stock no puede ser un valor negativo." }); // 400 Bad Request
            }

            var product = _context.Products.Find(request.ProductId);
            bool isNew = false;

            if (product == null)
            {
                // Si no existe, lo creamos (Upsert)
                isNew = true;
                product = new ProductEntity { Id = request.ProductId, Stock = request.NewQuantity };
                _context.Products.Add(product);
            }
            else
            {
                product.Stock = request.NewQuantity;
            }

            _context.SaveChanges();

            if (isNew)
            {
                // 201 Created si el recurso se creó de cero
                return StatusCode(StatusCodes.Status201Created, new
                {
                    Success = true,
                    Message = $"Product {request.ProductId} created successfully.",
                    NewStock = request.NewQuantity
                });
            }

            // 200 OK si solo se actualizó uno existente
            return Ok(new
            {
                Success = true,
                Message = $"Stock updated for product {request.ProductId}.",
                NewStock = request.NewQuantity
            });
        }

        [HttpDelete("delete")]
        public IActionResult DeleteStock([FromBody] DeleteStockRequest request)
        {
            var product = _context.Products.Find(request.ProductId);

            if (product == null)
            {
                return NotFound(new { Success = false, Message = $"Product {request.ProductId} not found." }); // 404 Not Found
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            // 200 OK (o también se suele usar 204 No Content)
            return Ok(new
            {
                Success = true,
                Message = $"Stock deleted for product {request.ProductId}."
            });
        }
    }

    // --- DTOs ---

    public class OrderRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class UpdateRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public int NewQuantity { get; set; }
    }

    public class DeleteStockRequest
    {
        public string ProductId { get; set; } = string.Empty;
    }
}