using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class PedidoCreateDTO
    {
        public int UsuarioId { get; set; }
        public string TipoEntrega { get; set; } // "retiro_tienda" o "despacho_domicilio"
        public int? SucursalId { get; set; }
        public int? DireccionId { get; set; }
        public string Notas { get; set; }
    }

    public class PedidoItemDTO
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}