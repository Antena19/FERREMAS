using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class PedidoBodega
    {
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        public int BodegueroId { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "pendiente"; // pendiente, en_proceso, completado

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación
        public virtual Pedido Pedido { get; set; }
        public virtual Bodeguero Bodeguero { get; set; }
        public virtual ICollection<ItemPedidoBodega> Items { get; set; }
    }
} 