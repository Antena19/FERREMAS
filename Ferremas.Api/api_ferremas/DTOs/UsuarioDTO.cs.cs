namespace Ferremas.Api.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rut { get; set; }
        public string Telefono { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
    }
}
