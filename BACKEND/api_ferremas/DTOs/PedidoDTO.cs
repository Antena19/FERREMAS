using System;
using System.Collections.Generic;

namespace Ferremas.Api.DTOs
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public string Estado { get; set; }
        public string TipoEntrega { get; set; }
        public int? SucursalId { get; set; }
        public string SucursalNombre { get; set; }
        public int? DireccionId { get; set; }
        public string DireccionTexto { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Notas { get; set; }
        public int? VendedorId { get; set; }
        public string VendedorNombre { get; set; }
        public int? BodegueroId { get; set; }
        public string BodegueroNombre { get; set; }
        public List<PedidoItemResponseDTO> Items { get; set; }
    }

    public class PedidoItemResponseDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}