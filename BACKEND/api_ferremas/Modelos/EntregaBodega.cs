using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class EntregaBodega
    {
        public int Id { get; set; }

        [Required]
        public int PedidoBodegaId { get; set; }

        [Required]
        public DateTime FechaEntrega { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } // preparada, entregada

        [Required]
        [StringLength(20)]
        public string TipoEntrega { get; set; } // retiro_tienda, despacho_domicilio

        // Propiedades de navegación
        public virtual PedidoBodega PedidoBodega { get; set; }
    }
} 