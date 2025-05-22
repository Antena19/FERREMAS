using System;
using System.Collections.Generic;

namespace Ferremas.Api.Modelos
{
    public class Carrito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Descuentos { get; set; }
        public decimal Total { get; set; }
        public List<ItemCarrito> Items { get; set; } = new List<ItemCarrito>();
    }

    public class ItemCarrito
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public Producto Producto { get; set; }
    }
} 