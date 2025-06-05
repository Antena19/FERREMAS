using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IBodegueroService
    {
        Task<IEnumerable<Inventario>> GetInventarioSucursal(int sucursalId);
        Task<IEnumerable<Inventario>> GetAllInventario();
        Task<IEnumerable<PedidoBodega>> GetPedidosBodegaAsignados(int bodegueroId);
        Task<PedidoBodega> GetPedidoBodegaById(int pedidoBodegaId);
        Task<EntregaBodega> CrearEntregaBodega(int pedidoBodegaId);
        Task<Producto> ActualizarProducto(Producto producto);
        Task<Inventario> ActualizarInventario(Inventario inventario);
        Task<IEnumerable<Producto>> GetProductosSucursal(int sucursalId);
        Task<PedidoBodega> ActualizarEstadoPedidoBodega(int pedidoBodegaId, string estado);
        Task<Inventario> ActualizarStockInventario(int sucursalId, int productoId, int cantidad);
    }
} 