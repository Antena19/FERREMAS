using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IInventarioService
    {
        Task<IEnumerable<Inventario>> GetInventarioBySucursal(int sucursalId);
        Task<Inventario> GetInventarioByProducto(int productoId, int sucursalId);
        Task<Inventario> UpdateStock(int inventarioId, int cantidad);
        Task<Inventario> UpdatePrecios(int inventarioId, decimal precioCompra, decimal precioVenta);
        Task<IEnumerable<Inventario>> GetProductosBajoStock(int sucursalId);
        Task<IEnumerable<Inventario>> GetProductosSobreStock(int sucursalId);
    }
} 