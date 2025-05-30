using Ferremas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    [Authorize(Roles = "Vendedor")]
    [ApiController]
    [Route("api/[controller]")]
    public class VendedorController : ControllerBase
    {
        private readonly IVendedorService _vendedorService;

        public VendedorController(IVendedorService vendedorService)
        {
            _vendedorService = vendedorService;
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _vendedorService.GetClientes();
            return Ok(clientes);
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetClienteById(int clienteId)
        {
            var cliente = await _vendedorService.GetClienteById(clienteId);
            if (cliente == null)
                return NotFound();
            return Ok(cliente);
        }

        [HttpGet("pedidos-asignados/{vendedorId}")]
        public async Task<IActionResult> GetPedidosAsignados(int vendedorId)
        {
            var pedidos = await _vendedorService.GetPedidosAsignados(vendedorId);
            return Ok(pedidos);
        }

        [HttpGet("pedido/{pedidoId}")]
        public async Task<IActionResult> GetPedidoById(int pedidoId)
        {
            var pedido = await _vendedorService.GetPedidoById(pedidoId);
            if (pedido == null)
                return NotFound();
            return Ok(pedido);
        }

        [HttpPost("pedido-bodega")]
        public async Task<IActionResult> CrearPedidoBodega([FromBody] PedidoBodegaRequest request)
        {
            try
            {
                var pedidoBodega = await _vendedorService.CrearPedidoBodega(request.PedidoId, request.VendedorId);
                return Ok(pedidoBodega);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("todos-pedidos")]
        public async Task<IActionResult> GetTodosLosPedidos()
        {
            var pedidos = await _vendedorService.GetTodosLosPedidos();
            return Ok(pedidos);
        }

        [HttpPut("pedido/{pedidoId}/estado")]
        public async Task<IActionResult> ActualizarEstadoPedido(int pedidoId, [FromBody] string estado)
        {
            try
            {
                var pedido = await _vendedorService.ActualizarEstadoPedido(pedidoId, estado);
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class PedidoBodegaRequest
    {
        public int PedidoId { get; set; }
        public int VendedorId { get; set; }
    }
} 