using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class ItemPedidoBodega
    {
        public int Id { get; set; }

        [Required]
        public int PedidoBodegaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        // Propiedades de navegación
        public virtual PedidoBodega PedidoBodega { get; set; }
        public virtual Producto Producto { get; set; }
    }
} 