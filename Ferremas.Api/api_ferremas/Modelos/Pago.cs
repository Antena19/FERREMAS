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

        // Campos específicos de Mercado Pago
        [MaxLength(100)]
        public string MercadoPagoPaymentId { get; set; }

        [MaxLength(100)]
        public string MercadoPagoPreferenceId { get; set; }

        [MaxLength(50)]
        public string MercadoPagoStatus { get; set; }

        [MaxLength(100)]
        public string MercadoPagoStatusDetail { get; set; }

        [MaxLength(50)]
        public string MercadoPagoPaymentMethodId { get; set; }

        [MaxLength(50)]
        public string MercadoPagoPaymentTypeId { get; set; }

        public int? MercadoPagoInstallments { get; set; }

        [MaxLength(20)]
        public string MercadoPagoCardNumber { get; set; }

        [MaxLength(255)]
        public string ReferenciaTransaccion { get; set; }

        public string Notas { get; set; }

        public int? ContadorId { get; set; }

        [ForeignKey("ContadorId")]
        public Usuario Contador { get; set; }

        public string UrlRetorno { get; set; }
    }
}