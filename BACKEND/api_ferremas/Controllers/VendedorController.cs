using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MySql.Data.MySqlClient;

namespace Ferremas.Api.Controllers
{
    [Authorize(Policy = "RequireVendedorRole")]
    [ApiController]
    [Route("api/[controller]")]
    public class VendedorController : ControllerBase
    {
        private readonly IVendedorService _vendedorService;

        public VendedorController(IVendedorService vendedorService)
        {
            _vendedorService = vendedorService;
        }

        private int GetVendedorId()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            using (var connection = new MySqlConnection(_vendedorService.GetConnectionString()))
            {
                connection.Open();
                using var command = new MySqlCommand("SELECT id FROM vendedores WHERE usuario_id = @usuarioId", connection);
                command.Parameters.AddWithValue("@usuarioId", usuarioId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        [HttpGet("clientes")]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _vendedorService.GetClientes();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<Cliente>> GetClienteById(int clienteId)
        {
            try
            {
                var cliente = await _vendedorService.GetClienteById(clienteId);
                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {clienteId} no encontrado");
                }
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("pedidos-asignados")]
        public async Task<ActionResult<IEnumerable<PedidoVendedorDTO>>> GetPedidosAsignados()
        {
            try
            {
                var vendedorId = GetVendedorId();
                if (vendedorId == 0)
                {
                    return NotFound("Vendedor no encontrado");
                }

                var pedidos = await _vendedorService.GetPedidosAsignados(vendedorId);
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<Pedido>> GetPedidoById(int pedidoId)
        {
            try
            {
                var pedido = await _vendedorService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound($"Pedido con ID {pedidoId} no encontrado");
                }
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("pedido/{pedidoId}/crear-pedido-bodega")]
        public async Task<ActionResult<PedidoBodega>> CrearPedidoBodega(int pedidoId)
        {
            try
            {
                var vendedorId = GetVendedorId();
                if (vendedorId == 0)
                {
                    return NotFound("Vendedor no encontrado");
                }

                var pedidoBodega = await _vendedorService.CrearPedidoBodega(pedidoId, vendedorId);
                if (pedidoBodega == null)
                {
                    return NotFound($"No se pudo crear el pedido de bodega para el pedido {pedidoId}");
                }
                return Ok(pedidoBodega);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("pedidos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetTodosLosPedidos()
        {
            try
            {
                var pedidos = await _vendedorService.GetTodosLosPedidos();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("pedido/{pedidoId}/estado")]
        public async Task<ActionResult<Pedido>> ActualizarEstadoPedido(int pedidoId, [FromBody] string estado)
        {
            try
            {
                var pedido = await _vendedorService.ActualizarEstadoPedido(pedidoId, estado);
                if (pedido == null)
                {
                    return NotFound($"Pedido con ID {pedidoId} no encontrado");
                }
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
} 