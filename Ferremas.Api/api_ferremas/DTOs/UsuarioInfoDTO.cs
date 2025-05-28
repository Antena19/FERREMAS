using System.Collections.Generic;

namespace Ferremas.Api.DTOs
{
    public class UsuarioInfoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public List<string> Roles { get; set; }
    }
} 