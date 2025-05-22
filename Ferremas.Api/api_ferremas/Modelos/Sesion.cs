using System;

namespace Ferremas.Api.Modelos
{
    public class Sesion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; }
        public string IpAddress { get; set; }
        public string Dispositivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activo { get; set; }

        // Relación con Usuario
        public Usuario Usuario { get; set; }
    }
} 