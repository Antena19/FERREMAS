using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;

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
    }
} 