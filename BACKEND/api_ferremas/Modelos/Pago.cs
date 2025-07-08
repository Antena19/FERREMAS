using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime? FechaPago { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; }  // pendiente, completado, fallido, reembolsado

        [Required]
        [MaxLength(50)]
        public string Metodo { get; set; }  // tarjeta_debito, tarjeta_credito, transferencia, mercadopago

        // Campos específicos de Webpay Plus
        public string WebpayToken { get; set; }
        public string WebpayBuyOrder { get; set; }
        public string WebpaySessionId { get; set; }
        public string WebpayAuthorizationCode { get; set; }
        public string WebpayPaymentTypeCode { get; set; }
        public int? WebpayResponseCode { get; set; }
        public string WebpayCardLastDigits { get; set; }
        public int? WebpayInstallmentsNumber { get; set; }
        public DateTime? WebpayTransactionDate { get; set; }
        public string WebpayStatus { get; set; }
        public string WebpayVci { get; set; }

        public string Notas { get; set; }

        public int? ContadorId { get; set; }

        [ForeignKey("ContadorId")]
        public Usuario Contador { get; set; }

        public string UrlRetorno { get; set; }
    }
}