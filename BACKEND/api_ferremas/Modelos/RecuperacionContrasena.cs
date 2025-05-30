using System;

namespace Ferremas.Api.Modelos
{
    public class RecuperacionContrasena
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Utilizado { get; set; }

        // Relación con Usuario
        public Usuario Usuario { get; set; }
    }
} 