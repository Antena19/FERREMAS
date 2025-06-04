using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class ItemPedidoBodega
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoBodegaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public int CantidadPreparada { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } // pendiente, preparado

        // Propiedades de navegación
        [ForeignKey("PedidoBodegaId")]
        public virtual PedidoBodega PedidoBodega { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
} 