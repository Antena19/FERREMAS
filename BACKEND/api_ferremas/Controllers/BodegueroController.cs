using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    [Authorize(Roles = "bodeguero")]
    [ApiController]
    [Route("api/[controller]")]
    public class BodegueroController : ControllerBase
    {
        private readonly IBodegueroService _bodegueroService;

        public BodegueroController(IBodegueroService bodegueroService)
        {
            _bodegueroService = bodegueroService;
        }

        [HttpGet("inventario/{sucursalId}")]
        public async Task<IActionResult> GetInventarioSucursal(int sucursalId)
        {
            var inventario = await _bodegueroService.GetInventarioSucursal(sucursalId);
            return Ok(inventario);
        }

        [HttpGet("inventario")]
        public async Task<IActionResult> GetAllInventario()
        {
            var inventario = await _bodegueroService.GetAllInventario();
            return Ok(inventario);
        }

        [HttpGet("pedidos-asignados/{bodegueroId}")]
        public async Task<IActionResult> GetPedidosBodegaAsignados(int bodegueroId)
        {
            var pedidos = await _bodegueroService.GetPedidosBodegaAsignados(bodegueroId);
            return Ok(pedidos);
        }

        [HttpGet("pedido-bodega/{pedidoBodegaId}")]
        public async Task<IActionResult> GetPedidoBodegaById(int pedidoBodegaId)
        {
            var pedido = await _bodegueroService.GetPedidoBodegaById(pedidoBodegaId);
            if (pedido == null)
                return NotFound();
            return Ok(pedido);
        }

        [HttpPost("entrega-bodega/{pedidoBodegaId}")]
        public async Task<IActionResult> CrearEntregaBodega(int pedidoBodegaId)
        {
            try
            {
                var entrega = await _bodegueroService.CrearEntregaBodega(pedidoBodegaId);
                return Ok(entrega);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("producto")]
        public async Task<IActionResult> ActualizarProducto([FromBody] Producto producto)
        {
            try
            {
                var productoActualizado = await _bodegueroService.ActualizarProducto(producto);
                return Ok(productoActualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("inventario")]
        public async Task<IActionResult> ActualizarInventario([FromBody] Inventario inventario)
        {
            try
            {
                var inventarioActualizado = await _bodegueroService.ActualizarInventario(inventario);
                return Ok(inventarioActualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("productos/{sucursalId}")]
        public async Task<IActionResult> GetProductosSucursal(int sucursalId)
        {
            var productos = await _bodegueroService.GetProductosSucursal(sucursalId);
            return Ok(productos);
        }

        [HttpPut("pedido-bodega/{pedidoBodegaId}/estado")]
        public async Task<IActionResult> ActualizarEstadoPedidoBodega(int pedidoBodegaId, [FromBody] string estado)
        {
            try
            {
                var pedido = await _bodegueroService.ActualizarEstadoPedidoBodega(pedidoBodegaId, estado);
                if (pedido == null)
                    return NotFound(new { message = "Pedido no encontrado" });

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el estado del pedido", error = ex.Message });
            }
        }

        [HttpPut("inventario/{sucursalId}/{productoId}")]
        public async Task<IActionResult> ActualizarStockInventario(int sucursalId, int productoId, [FromBody] int cantidad)
        {
            try
            {
                var inventario = await _bodegueroService.ActualizarStockInventario(sucursalId, productoId, cantidad);
                if (inventario == null)
                    return NotFound(new { message = "Producto no encontrado en el inventario de la sucursal" });

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el stock", error = ex.Message });
            }
        }
    }
} 