using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class PedidoDesdeCarritoDTO
    {
        [Required]
        public string TipoEntrega { get; set; } // "retiro_tienda" o "despacho_domicilio"
        public int? SucursalId { get; set; }
        public int? DireccionId { get; set; }
        public string Notas { get; set; }
    }
} 