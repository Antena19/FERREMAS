using System;
using System.Collections.Generic;

namespace Ferremas.Api.DTOs
{
    public class PerfilUsuarioDTO
    {
        // Datos personales y contacto
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rut { get; set; }
        public string Telefono { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        // Datos adicionales del cliente (si existe)
        public string TipoCliente { get; set; }
        public decimal TotalCompras { get; set; }
        public int NumeroCompras { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public bool Newsletter { get; set; }

        // Historial de compras
        public List<PedidoDTO> HistorialCompras { get; set; } = new List<PedidoDTO>();

        // Direcciones de envío
        public List<DireccionDTO> Direcciones { get; set; } = new List<DireccionDTO>();
    }
} 