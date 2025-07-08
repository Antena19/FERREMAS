using System;

namespace Ferremas.Api.DTOs
{
    public class PagoConfirmacionDTO
    {
        public string Token { get; set; }
        public string BuyOrder { get; set; }
        public string SessionId { get; set; }
        public string AuthorizationCode { get; set; }
        public string PaymentTypeCode { get; set; }
        public int? ResponseCode { get; set; }
        public string CardLastDigits { get; set; }
        public int? InstallmentsNumber { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Status { get; set; }
        public string Vci { get; set; }
    }
} 