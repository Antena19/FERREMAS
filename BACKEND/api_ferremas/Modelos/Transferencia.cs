using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Transferencia
    {
        public int Id { get; set; }
        public int PagoId { get; set; }
        public string BancoOrigen { get; set; }
        public string NumeroCuenta { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Notas { get; set; }

        // Propiedades de navegación
        public virtual Pago Pago { get; set; }
    }
} 