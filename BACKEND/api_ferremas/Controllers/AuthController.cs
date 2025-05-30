using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;
using System.Security.Claims;

namespace Ferremas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var (success, token, message, usuario) = await _authService.LoginAsync(loginDTO);
            
            if (success)
            {
                return Ok(new { token, usuario, message });
            }
            
            return BadRequest(new { message });
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroDTO registroDTO)
        {
            var (success, message) = await _authService.RegistroAsync(registroDTO);
            
            if (success)
            {
                return Ok(new { message });
            }
            
            return BadRequest(new { message });
        }

        [HttpPost("recuperar-contrasena")]
        public async Task<IActionResult> RecuperarContrasena([FromBody] RecuperacionContrasenaDTO recuperacionDTO)
        {
            var (success, message) = await _authService.RecuperarContrasenaAsync(recuperacionDTO);
            
            if (success)
            {
                return Ok(new { message });
            }
            
            return BadRequest(new { message });
        }

        [HttpPost("cambiar-contrasena")]
        public async Task<IActionResult> CambiarContrasena([FromBody] CambioContrasenaDTO cambioDTO)
        {
            var (success, message) = await _authService.CambiarContrasenaAsync(cambioDTO);
            
            if (success)
            {
                return Ok(new { message });
            }
            
            return BadRequest(new { message });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var (success, message) = await _authService.LogoutAsync(token);
            
            if (success)
            {
                return Ok(new { message });
            }
            
            return BadRequest(new { message });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            var (success, newToken, message) = await _authService.RefreshTokenAsync(refreshTokenDTO.RefreshToken);
            
            if (success)
            {
                return Ok(new { token = newToken, message });
            }
            
            return BadRequest(new { message });
        }

        // GET: api/Auth/validar
        [HttpGet("validar")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioInfoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidarToken()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var usuario = await _authService.ObtenerUsuarioPorEmailAsync(email);
            if (usuario == null)
            {
                return Unauthorized();
            }

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new UsuarioInfoDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Roles = roles
            });
        }
    }
} 