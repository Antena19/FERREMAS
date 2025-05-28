using System;

namespace Ferremas.Api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activo { get; set; }

        // Relación con Usuario
        public virtual Usuario Usuario { get; set; }
    }
} 