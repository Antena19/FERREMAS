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
        /// Endpoint temporal para probar la consulta de usuarios
        /// </summary>
        [HttpGet("test-usuarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> TestUsuarios()
        {
            try
            {
                var sql = @"
                    SELECT 
                        u.id,
                        u.nombre,
                        u.apellido,
                        CONCAT(u.nombre, ' ', u.apellido) as NombreCompleto
                    FROM usuarios u
                    WHERE u.id IN (33, 34, 15)
                    ORDER BY u.id";

                var usuarios = await _db.QueryAsync(sql);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
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

        /// <summary>
        /// Obtiene el historial de compras de un cliente
        /// </summary>
        [HttpGet("cliente/{clienteId}/historial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PedidoDTO>>> GetHistorialComprasCliente(int clienteId)
        {
            try
            {
                var historial = await _pedidosService.GetHistorialComprasClienteAsync(clienteId);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "Error al obtener el historial de compras: " + ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el historial de compras del usuario autenticado (si es cliente)
        /// </summary>
        [HttpGet("mi-historial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PedidoDTO>>> GetMiHistorialCompras()
        {
            try
            {
                // Obtener el ID del usuario desde el token JWT
                var usuarioIdClaim = User.FindFirst("nameid") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("No se pudo identificar al usuario");
                }

                var historial = await _pedidosService.GetHistorialComprasUsuarioAsync(usuarioId);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "Error al obtener el historial de compras: " + ex.Message);
            }
        }

        /// <summary>
        /// Crea o actualiza el pedido pendiente del usuario autenticado a partir del carrito actual.
        /// </summary>
        [HttpPost("desde-carrito")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearOActualizarPedidoDesdeCarrito([FromBody] PedidoDesdeCarritoDTO dto)
        {
            try
            {
                // Obtener el ID del usuario desde el token JWT
                var usuarioIdClaim = User.FindFirst("nameid") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("No se pudo identificar al usuario");
                }

                var result = await _db.QueryAsync<dynamic>(
                    "sp_crear_o_actualizar_pedido_desde_carrito",
                    new
                    {
                        p_usuario_id = usuarioId,
                        p_tipo_entrega = dto.TipoEntrega,
                        p_sucursal_id = dto.SucursalId,
                        p_direccion_id = dto.DireccionId,
                        p_notas = dto.Notas
                    },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el pedido pendiente del usuario autenticado (si existe)
        /// </summary>
        [HttpGet("pendiente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPedidoPendiente()
        {
            var usuarioIdClaim = User.FindFirst("userId");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return BadRequest("No se pudo identificar al usuario");
            }

            var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM pedidos WHERE usuario_id = @usuarioId AND estado = 'pendiente' LIMIT 1",
                new { usuarioId }
            );
            return Ok(result);
        }
    }
}