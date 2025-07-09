using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Repositories
{
    public interface IPagoRepository
    {
        Task<IEnumerable<Pago>> ObtenerTodosAsync();
        Task<Pago> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Pago>> ObtenerPorPedidoAsync(int pedidoId);
        Task<int> CrearPagoAsync(Pago pago);
        Task<bool> ActualizarPagoAsync(Pago pago);
        Task<bool> ActualizarEstadoPagoAsync(int id, string estado, string notas);
        Task<Pago> ObtenerPorTokenPasarelaAsync(string tokenPasarela);
        Task<Pago> ObtenerPorWebpayTokenAsync(string webpayToken);
        Task<Pago> ObtenerPorWebpayBuyOrderAsync(string webpayBuyOrder);
        Task<bool> PagoExisteAsync(int id);
        Task<bool> ActualizarTokenYBuyOrderAsync(int pedidoId, string token, string buyOrder);
    }
}