using System;

namespace Ferremas.Api.DTOs
{
    public class InventarioUpdateDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int SucursalId { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
    }
} 