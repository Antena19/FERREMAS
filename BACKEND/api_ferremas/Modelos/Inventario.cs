using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Inventario
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int SucursalId { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int StockMinimo { get; set; } = 5;

        public DateTime? UltimoIngreso { get; set; }

        public DateTime? UltimaSalida { get; set; }

        [Required]
        public int StockMaximo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual Producto Producto { get; set; }
        public virtual Sucursal Sucursal { get; set; }
    }
} 