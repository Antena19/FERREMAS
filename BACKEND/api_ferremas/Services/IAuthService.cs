using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IAuthService
    {
        Task<(bool success, string token, string message, UsuarioDTO usuario)> LoginAsync(LoginDTO loginDTO);
        Task<(bool success, string message)> RegistroAsync(RegistroDTO registroDTO);
        Task<(bool success, string message)> RecuperarContrasenaAsync(RecuperacionContrasenaDTO recuperacionDTO);
        Task<(bool success, string message)> CambiarContrasenaAsync(CambioContrasenaDTO cambioDTO);
        Task<(bool success, string message)> LogoutAsync(string token);
        Task<(bool success, string newToken, string message)> RefreshTokenAsync(string refreshToken);
        Task<UsuarioDTO> ObtenerUsuarioPorEmailAsync(string email);
    }
} 