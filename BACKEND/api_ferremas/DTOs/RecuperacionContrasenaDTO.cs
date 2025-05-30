using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class RecuperacionContrasenaDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class CambioContrasenaDTO
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NuevaPassword { get; set; }

        [Required]
        [Compare("NuevaPassword")]
        public string ConfirmarPassword { get; set; }
    }
} 