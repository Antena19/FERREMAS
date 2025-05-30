using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.Modelos
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(255)]
        public string Direccion { get; set; }

        [Required]
        [StringLength(100)]
        public string Comuna { get; set; }

        [Required]
        [StringLength(100)]
        public string Region { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        public bool EsPrincipal { get; set; } = false;

        public bool Activo { get; set; } = true;
    }
} 