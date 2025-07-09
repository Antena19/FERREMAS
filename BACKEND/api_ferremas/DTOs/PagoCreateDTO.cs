using System;
using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class PagoCreateDTO
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [RegularExpression("tarjeta_debito|tarjeta_credito|transferencia|webpay",
            ErrorMessage = "El método de pago debe ser: tarjeta_debito, tarjeta_credito, transferencia o webpay")]
        public string Metodo { get; set; }

        public string UrlRetorno { get; set; }  // URL para retornar después del pago (para pasarelas online)
    }

    public class PagoResponseDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Estado { get; set; }
        public string Metodo { get; set; }
        public string UrlPasarela { get; set; }
    }
}