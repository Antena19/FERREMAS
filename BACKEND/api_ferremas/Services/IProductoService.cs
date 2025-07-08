using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDTO>> ObtenerTodosAsync();
        Task<IEnumerable<ProductoDTO>> ObtenerTodosAsync(bool incluirInactivos);
        Task<ProductoDTO> ObtenerPorIdAsync(int id);
        Task<IEnumerable<ProductoDTO>> ObtenerPorCategoriaAsync(int categoriaId);
        Task<IEnumerable<ProductoDTO>> BuscarProductosAsync(string termino, int? categoriaId, decimal? precioMin, decimal? precioMax);
        Task<int> CrearProductoAsync(ProductoCreateDTO productoDTO);
        Task<bool> ActualizarProductoAsync(int id, ProductoUpdateDTO productoDTO);
        Task<bool> EliminarProductoAsync(int id);
        Task<bool> ActualizarStockAsync(int productoId, int cantidad);
        Task<bool> ActualizarInventarioAsync(int productoId, InventarioUpdateDTO inventarioDTO);
        Task<bool> ActualizarImagenAsync(int id, string nombreArchivo);
        Task<bool> CategoriaExisteAsync(int categoriaId);
        Task<bool> MarcaExisteAsync(int marcaId);
        Task<bool> ProductoCodigoExisteAsync(string codigo);
    }
}