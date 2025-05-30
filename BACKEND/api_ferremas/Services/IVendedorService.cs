using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IVendedorService
    {
        Task<IEnumerable<Cliente>> GetClientes();
        Task<Cliente> GetClienteById(int clienteId);
        Task<IEnumerable<Pedido>> GetPedidosAsignados(int vendedorId);
        Task<Pedido> GetPedidoById(int pedidoId);
        Task<PedidoBodega> CrearPedidoBodega(int pedidoId, int vendedorId);
        Task<IEnumerable<Pedido>> GetTodosLosPedidos();
        Task<Pedido> ActualizarEstadoPedido(int pedidoId, string estado);
    }
} 