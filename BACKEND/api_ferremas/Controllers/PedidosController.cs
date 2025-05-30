using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Dapper;

namespace Ferremas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidosService _pedidosService;
        private readonly IDbConnection _db;

        public PedidosController(IPedidosService pedidosService, IDbConnection db)
        {
            _pedidosService = pedidosService;
            _db = db;
        }

        /// <summary>
        /// Obtiene todos los pedidos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PedidoDTO>>> GetPedidos()
        {
            var pedidos = await _pedidosService.GetAllPedidosAsync();
            return Ok(pedidos);
        }

        /// <summary>
        /// Obtiene un pedido por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoDTO>> GetPedido(int id)
        {
            var pedido = await _pedidosService.GetPedidoByIdAsync(id);

            if (pedido == null)
                return NotFound($"No se encontró el pedido con ID {id}");

            return Ok(pedido);
        }

        /// <summary>
        /// Obtiene los pedidos de un usuario específico
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PedidoDTO>>> GetPedidosByUsuario(int usuarioId)
        {
            var pedidos = await _pedidosService.GetPedidosByUsuarioIdAsync(usuarioId);
            return Ok(pedidos);
        }

        /// <summary>
        /// Crea un nuevo pedido
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearPedido([FromBody] PedidoCreateDTO dto)
        {
            try
            {
                var pedido = new Pedido
                {
                    UsuarioId = dto.UsuarioId,
                    TipoEntrega = dto.TipoEntrega,
                    SucursalId = dto.SucursalId,
                    DireccionId = dto.DireccionId,
                    Notas = dto.Notas,
                    Estado = "pendiente",
                    FechaPedido = DateTime.Now
                };

                var pedidoCreado = await _pedidosService.CreatePedidoAsync(pedido);
                if (pedidoCreado == null)
                    return BadRequest("No se pudo crear el pedido. Verifica que el carrito esté activo y los datos sean correctos.");

                return Ok(pedidoCreado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "Error al crear el pedido: " + ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un pedido existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PedidoDTO>> UpdatePedido(int id, PedidoUpdateDTO pedidoUpdateDTO)
        {
            try
            {
                var pedidoActualizado = await _pedidosService.UpdatePedidoAsync(id, pedidoUpdateDTO);

                if (pedidoActualizado == null)
                    return NotFound($"No se encontró el pedido con ID {id}");

                return Ok(pedidoActualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el estado de un pedido
        /// </summary>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoDTO>> UpdateEstadoPedido(int id, [FromBody] string estado)
        {
            if (string.IsNullOrEmpty(estado) || !new[] { "pendiente", "aprobado", "preparando", "enviado", "entregado", "cancelado" }.Contains(estado.ToLower()))
                return BadRequest("Estado no válido. Debe ser: pendiente, aprobado, preparando, enviado, entregado o cancelado");

            var pedidoActualizado = await _pedidosService.UpdatePedidoEstadoAsync(id, estado);

            if (pedidoActualizado == null)
                return NotFound($"No se encontró el pedido con ID {id}");

            return Ok(pedidoActualizado);
        }

        /// <summary>
        /// Elimina un pedido
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var deleted = await _pedidosService.DeletePedidoAsync(id);

            if (!deleted)
                return NotFound($"No se encontró el pedido con ID {id}");

            return NoContent();
        }

        /// <summary>
        /// Obtiene todos los pedidos pendientes
        /// </summary>
        [HttpGet("pendientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "administrador,vendedor,bodeguero")]
        public async Task<ActionResult<IEnumerable<PedidoDTO>>> GetPedidosPendientes()
        {
            try
            {
                var pedidos = await _pedidosService.GetPedidosPendientesAsync();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "Error al obtener los pedidos pendientes: " + ex.Message);
            }
        }
    }
}