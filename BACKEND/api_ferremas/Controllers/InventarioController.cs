using Ferremas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    [Authorize(Roles = "Administrador,Bodeguero")]
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        [HttpGet("sucursal/{sucursalId}")]
        public async Task<IActionResult> GetInventarioBySucursal(int sucursalId)
        {
            var inventario = await _inventarioService.GetInventarioBySucursal(sucursalId);
            return Ok(inventario);
        }

        [HttpGet("producto/{productoId}/sucursal/{sucursalId}")]
        public async Task<IActionResult> GetInventarioByProducto(int productoId, int sucursalId)
        {
            var inventario = await _inventarioService.GetInventarioByProducto(productoId, sucursalId);
            if (inventario == null)
                return NotFound();
            return Ok(inventario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int cantidad)
        {
            try
            {
                var inventario = await _inventarioService.UpdateStock(id, cantidad);
                return Ok(inventario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}/precios")]
        public async Task<IActionResult> UpdatePrecios(int id, [FromBody] UpdatePreciosRequest request)
        {
            try
            {
                var inventario = await _inventarioService.UpdatePrecios(id, request.PrecioCompra, request.PrecioVenta);
                return Ok(inventario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("bajo-stock/{sucursalId}")]
        public async Task<IActionResult> GetProductosBajoStock(int sucursalId)
        {
            var productos = await _inventarioService.GetProductosBajoStock(sucursalId);
            return Ok(productos);
        }

        [HttpGet("sobre-stock/{sucursalId}")]
        public async Task<IActionResult> GetProductosSobreStock(int sucursalId)
        {
            var productos = await _inventarioService.GetProductosSobreStock(sucursalId);
            return Ok(productos);
        }
    }

    public class UpdatePreciosRequest
    {
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
    }
} 