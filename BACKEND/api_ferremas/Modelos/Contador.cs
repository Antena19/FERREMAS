using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Contador
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaContratacion { get; set; }

        public DateTime? FechaTermino { get; set; }

        // Propiedades de navegación
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
    }
} 