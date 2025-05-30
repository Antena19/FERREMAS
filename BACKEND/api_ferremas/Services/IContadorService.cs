using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IContadorService
    {
        Task<Pago> AprobarPagoTransferencia(int pedidoId, int contadorId, string bancoOrigen, string numeroCuenta);
        Task<IEnumerable<Pago>> GetHistorialPagos(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<Pago>> GetPagosPendientes();
        Task<Pago> GetPagoById(int pagoId);
        Task<IEnumerable<Pedido>> GetPedidosPendientesPago();
    }
} 