using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class DatosPersonalesDTO
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        public string TipoCliente { get; set; } = "particular";
    }
} 