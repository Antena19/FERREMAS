using System;
using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class PagoCreateDTO
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [RegularExpression("tarjeta_debito|tarjeta_credito|transferencia|mercadopago",
            ErrorMessage = "El método de pago debe ser: tarjeta_debito, tarjeta_credito, transferencia o mercadopago")]
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
        
        // Campos específicos de Mercado Pago
        public string MercadoPagoPaymentId { get; set; }
        public string MercadoPagoPreferenceId { get; set; }
        public string MercadoPagoStatus { get; set; }
        public string MercadoPagoStatusDetail { get; set; }
        public string MercadoPagoPaymentMethodId { get; set; }
        public string MercadoPagoPaymentTypeId { get; set; }
        public int? MercadoPagoInstallments { get; set; }
        public string MercadoPagoCardNumber { get; set; }
        
        // URL para redirección a Mercado Pago
        public string UrlPasarela { get; set; }
    }

    public class PagoConfirmacionDTO
    {
        public string MercadoPagoPaymentId { get; set; }
        public string Estado { get; set; }  // completado, fallido, reembolsado
        public string MercadoPagoStatus { get; set; }
        public string MercadoPagoStatusDetail { get; set; }
        public string MercadoPagoPaymentMethodId { get; set; }
        public string MercadoPagoPaymentTypeId { get; set; }
        public int? MercadoPagoInstallments { get; set; }
        public string MercadoPagoCardNumber { get; set; }
    }
}