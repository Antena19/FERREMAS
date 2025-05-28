using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class RefreshTokenDTO
    {
        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; }
    }
} 