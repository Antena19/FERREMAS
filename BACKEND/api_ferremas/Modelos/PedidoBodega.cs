using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class PedidoBodega
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        public int VendedorId { get; set; }

        [Required]
        public int BodegueroId { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaPreparacion { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } // pendiente, asignado, en_preparacion, preparado, entregado

        public string Notas { get; set; }

        // Propiedades de navegación
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        [ForeignKey("VendedorId")]
        public virtual Usuario Vendedor { get; set; }

        [ForeignKey("BodegueroId")]
        public virtual Usuario Bodeguero { get; set; }

        public virtual ICollection<ItemPedidoBodega> Items { get; set; }
    }
} 