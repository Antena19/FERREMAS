using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Bodeguero
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int SucursalId { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaContratacion { get; set; }

        public DateTime? FechaTermino { get; set; }

        // Propiedades de navegación
        public virtual Usuario Usuario { get; set; }
        public virtual Sucursal Sucursal { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
} 