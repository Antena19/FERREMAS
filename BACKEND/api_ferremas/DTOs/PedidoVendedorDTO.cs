using System;
using System.Collections.Generic;

namespace Ferremas.Api.DTOs
{
    public class PedidoVendedorDTO
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; }
        public string TipoEntrega { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Notas { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string EstadoAsignacion { get; set; }
        public decimal? ComisionCalculada { get; set; }
        public ClientePedidoDTO Cliente { get; set; }
        public SucursalDTO Sucursal { get; set; }
        public DireccionDTO DireccionEntrega { get; set; }
        public List<ItemPedidoDTO> Items { get; set; }
    }

    public class ClientePedidoDTO
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
    }

    public class SucursalDTO
    {
        public string Nombre { get; set; }
    }

    public class ItemPedidoDTO
    {
        public ProductoDTO Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
} 