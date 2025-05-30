using System;
using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Rut { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        public string Rol { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; }
    }
} 