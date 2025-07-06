using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;
using System.Security.Claims;

namespace Ferremas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "cliente,Administrador")]
    public class PerfilController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public PerfilController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Obtiene el perfil completo del usuario autenticado
        /// </summary>
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> GetMiPerfil()
        {
            try
            {
                // Debug: Imprimir todos los claims del usuario
                Console.WriteLine("=== Claims del usuario ===");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }

                // Obtener el ID del usuario desde el token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                Console.WriteLine($"UserIdClaim: {userIdClaim?.Type} = {userIdClaim?.Value}");
                
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("No se pudo obtener el userId del token");
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                Console.WriteLine($"UserId obtenido: {userId}");

                var perfil = await _adminService.GetPerfilUsuario(userId);
                if (perfil == null)
                    return NotFound(new { message = "No se encontró el perfil del usuario" });
                
                return Ok(new { 
                    success = true, 
                    message = "Perfil obtenido exitosamente", 
                    data = perfil 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetMiPerfil: {ex.Message}");
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al obtener perfil: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar el token
        /// </summary>
        [HttpGet("test-token")]
        public IActionResult TestToken()
        {
            try
            {
                // Obtener el ID del usuario desde el token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = User.FindFirst(ClaimTypes.Email);
                var roleClaim = User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "❌ No se encontró el ID del usuario en el token" });
                }

                var userId = int.Parse(userIdClaim.Value);

                return Ok(new
                {
                    message = "✅ Token válido",
                    userId = userId,
                    email = emailClaim?.Value,
                    role = roleClaim?.Value,
                    claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"❌ Error al validar token: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene las direcciones del usuario autenticado
        /// </summary>
        [HttpGet("direcciones")]
        public async Task<IActionResult> GetMisDirecciones()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var direcciones = await _adminService.GetDireccionesUsuario(userId);
                return Ok(new { 
                    success = true, 
                    message = "Direcciones obtenidas exitosamente", 
                    data = direcciones 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al obtener direcciones: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Agrega una nueva dirección al usuario autenticado
        /// </summary>
        [HttpPost("direcciones")]
        public async Task<IActionResult> AgregarDireccion([FromBody] DireccionDTO direccion)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                direccion.UsuarioId = userId;
                var nuevaDireccion = await _adminService.CrearDireccion(direccion);
                
                return Ok(new { 
                    success = true, 
                    message = "Dirección agregada exitosamente", 
                    data = nuevaDireccion 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al agregar dirección: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Actualiza una dirección del usuario autenticado
        /// </summary>
        [HttpPut("direcciones/{id}")]
        public async Task<IActionResult> ActualizarDireccion(int id, [FromBody] DireccionDTO direccion)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                // Verificar que la dirección pertenece al usuario
                var direccionExistente = await _adminService.GetDireccionById(id);
                if (direccionExistente == null || direccionExistente.UsuarioId != userId)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                direccion.UsuarioId = userId;
                var direccionActualizada = await _adminService.ActualizarDireccion(id, direccion);
                
                return Ok(new { 
                    success = true, 
                    message = "Dirección actualizada exitosamente", 
                    data = direccionActualizada 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al actualizar dirección: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Elimina una dirección del usuario autenticado
        /// </summary>
        [HttpDelete("direcciones/{id}")]
        public async Task<IActionResult> EliminarDireccion(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                // Verificar que la dirección pertenece al usuario
                var direccionExistente = await _adminService.GetDireccionById(id);
                if (direccionExistente == null || direccionExistente.UsuarioId != userId)
                {
                    return NotFound(new { message = "Dirección no encontrada" });
                }

                var resultado = await _adminService.EliminarDireccion(id);
                if (!resultado)
                {
                    return BadRequest(new { message = "No se pudo eliminar la dirección" });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Dirección eliminada exitosamente" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al eliminar dirección: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Actualiza los datos personales del usuario autenticado
        /// </summary>
        [HttpPut("datos-personales")]
        public async Task<IActionResult> ActualizarDatosPersonales([FromBody] DatosPersonalesDTO datos)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                var resultado = await _adminService.ActualizarDatosPersonales(userId, datos);
                if (!resultado)
                {
                    return NotFound(new { message = "No se pudo actualizar los datos personales" });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Datos personales actualizados exitosamente" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"Error al actualizar datos personales: {ex.Message}" 
                });
            }
        }
    }
} 