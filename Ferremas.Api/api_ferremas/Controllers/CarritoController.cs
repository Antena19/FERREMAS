using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Controllers
{
    [ApiController]
    [Route("api/carrito")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoService _carritoService;

        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        // POST: api/carrito/agregar
        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarAlCarrito([FromBody] AgregarItemCarritoDTO dto)
        {
            Console.WriteLine($"Agregar al carrito: usuarioId={dto.UsuarioId}, productoId={dto.ProductoId}, cantidad={dto.Cantidad}");
            if (dto == null || dto.UsuarioId <= 0 || dto.ProductoId <= 0 || dto.Cantidad <= 0)
                return BadRequest("Datos inválidos");

            var carrito = await _carritoService.AgregarItem(dto.UsuarioId, dto.ProductoId, dto.Cantidad);
            return Ok(carrito);
        }

        // GET: api/carrito/{usuarioId}
        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> ObtenerCarrito(int usuarioId)
        {
            var carrito = await _carritoService.ObtenerCarrito(usuarioId);
            if (carrito == null)
                return NotFound();
            return Ok(carrito);
        }

        // PUT: api/carrito/actualizar-cantidad
        [HttpPut("actualizar-cantidad")]
        public async Task<IActionResult> ActualizarCantidad([FromBody] ActualizarCantidadDTO dto)
        {
            if (dto == null || dto.UsuarioId <= 0 || dto.ItemId <= 0 || dto.Cantidad <= 0)
                return BadRequest("Datos inválidos");

            var carrito = await _carritoService.ActualizarCantidad(dto.UsuarioId, dto.ItemId, dto.Cantidad);
            return Ok(carrito);
        }

        // DELETE: api/carrito/eliminar-item
        [HttpDelete("eliminar-item")]
        public async Task<IActionResult> EliminarItem([FromBody] EliminarItemDTO dto)
        {
            if (dto == null || dto.UsuarioId <= 0 || dto.ItemId <= 0)
                return BadRequest("Datos inválidos");

            var carrito = await _carritoService.EliminarItem(dto.UsuarioId, dto.ItemId);
            return Ok(carrito);
        }

        // DELETE: api/carrito/vaciar/{usuarioId}
        [HttpDelete("vaciar/{usuarioId}")]
        public async Task<IActionResult> VaciarCarrito(int usuarioId)
        {
            if (usuarioId <= 0)
                return BadRequest("Usuario inválido");

            await _carritoService.VaciarCarrito(usuarioId);
            return Ok(new { mensaje = "Carrito vaciado" });
        }
    }

    // DTO para agregar item al carrito
    public class AgregarItemCarritoDTO
    {
        public int UsuarioId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    // DTO para actualizar cantidad
    public class ActualizarCantidadDTO
    {
        public int UsuarioId { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
    }

    // DTO para eliminar item
    public class EliminarItemDTO
    {
        public int UsuarioId { get; set; }
        public int ItemId { get; set; }
    }
} 