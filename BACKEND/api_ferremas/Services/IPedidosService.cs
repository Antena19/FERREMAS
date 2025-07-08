using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IPedidosService
    {
        Task<IEnumerable<PedidoDTO>> GetAllPedidosAsync();
        Task<PedidoDTO> GetPedidoByIdAsync(int id);
        Task<IEnumerable<PedidoDTO>> GetPedidosByUsuarioIdAsync(int usuarioId);
        Task<PedidoDTO> UpdatePedidoAsync(int id, PedidoUpdateDTO pedidoUpdateDTO);
        Task<PedidoDTO> UpdatePedidoEstadoAsync(int id, string estado);
        Task<bool> DeletePedidoAsync(int id);
        Task<IEnumerable<PedidoDTO>> GetPedidosPendientesAsync();
        Task<PedidoDTO> CreatePedidoAsync(Pedido pedido);
        Task<IEnumerable<PedidoDTO>> GetHistorialComprasClienteAsync(int clienteId);
        Task<IEnumerable<PedidoDTO>> GetHistorialComprasUsuarioAsync(int usuarioId);
    }
}