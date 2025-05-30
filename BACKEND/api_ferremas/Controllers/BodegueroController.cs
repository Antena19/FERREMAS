using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    [Authorize(Roles = "Bodeguero")]
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
    }
} 