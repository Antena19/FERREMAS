using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using Ferremas.Api.Repositories;
using Ferremas.Api.Services;

namespace Ferremas.Api.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IAdminService _adminService;

        public ProductoService(IProductoRepository productoRepository, IAdminService adminService)
        {
            _productoRepository = productoRepository;
            _adminService = adminService;
        }

        public async Task<IEnumerable<ProductoDTO>> ObtenerTodosAsync(bool incluirInactivos = false)
        {
            var productos = await _productoRepository.ObtenerTodosAsync(incluirInactivos);
            return productos.Select(p => MapearADTO(p));
        }

        public async Task<IEnumerable<ProductoDTO>> ObtenerTodosAsync()
        {
            return await ObtenerTodosAsync(false);
        }

        public async Task<ProductoDTO> ObtenerPorIdAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return null;

            return MapearADTO(producto);
        }

        public async Task<IEnumerable<ProductoDTO>> BuscarProductosAsync(string termino, int? categoriaId, decimal? precioMin, decimal? precioMax)
        {
            var productos = await _productoRepository.BuscarProductosAsync(termino, categoriaId, precioMin, precioMax);
            return productos.Select(p => MapearADTO(p));
        }

        public async Task<int> CrearProductoAsync(ProductoCreateDTO productoDTO)
        {
            if (await _productoRepository.ProductoExisteAsync(productoDTO.Codigo))
            {
                throw new InvalidOperationException($"Ya existe un producto con el código {productoDTO.Codigo}");
            }

            var producto = new Producto
            {
                Codigo = productoDTO.Codigo,
                Nombre = productoDTO.Nombre,
                Descripcion = productoDTO.Descripcion,
                Precio = productoDTO.Precio,
                CategoriaId = productoDTO.CategoriaId,
                MarcaId = productoDTO.MarcaId,
                ImagenUrl = productoDTO.ImagenUrl,
                Especificaciones = productoDTO.Especificaciones,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            var id = await _productoRepository.CrearProductoAsync(producto);

            // Crear inventario para cada sucursal activa con stock 50
            var sucursales = await _adminService.GetAllSucursales();
            foreach (var sucursal in sucursales)
            {
                await _productoRepository.ActualizarInventarioAsync(id, sucursal.Id, 50);
            }

            return id;
        }

        public async Task<bool> ActualizarProductoAsync(int id, ProductoUpdateDTO productoDTO)
        {
            var productoExistente = await _productoRepository.ObtenerPorIdAsync(id);
            if (productoExistente == null) return false;

            // Actualizar propiedades
            productoExistente.Nombre = productoDTO.Nombre;
            productoExistente.Descripcion = productoDTO.Descripcion;
            productoExistente.Precio = productoDTO.Precio;
            productoExistente.CategoriaId = productoDTO.CategoriaId;
            productoExistente.MarcaId = productoDTO.MarcaId;
            productoExistente.ImagenUrl = productoDTO.ImagenUrl;
            productoExistente.Especificaciones = productoDTO.Especificaciones;
            productoExistente.Activo = productoDTO.Activo;
            productoExistente.FechaModificacion = DateTime.Now;

            return await _productoRepository.ActualizarProductoAsync(productoExistente);
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            if (!await _productoRepository.ProductoExisteAsync(id))
            {
                return false;
            }

            return await _productoRepository.EliminarProductoAsync(id);
        }

        public async Task<bool> ActualizarStockAsync(int productoId, int cantidad)
        {
            if (!await _productoRepository.ProductoExisteAsync(productoId))
            {
                return false;
            }

            return await _productoRepository.ActualizarStockAsync(productoId, cantidad);
        }

        public async Task<IEnumerable<ProductoDTO>> ObtenerPorCategoriaAsync(int categoriaId)
        {
            var productos = await _productoRepository.ObtenerPorCategoriaAsync(categoriaId);
            return productos.Select(p => MapearADTO(p));
        }

        public async Task<bool> ActualizarInventarioAsync(int productoId, InventarioUpdateDTO inventarioDTO)
        {
            if (!await _productoRepository.ProductoExisteAsync(productoId))
            {
                return false;
            }

            return await _productoRepository.ActualizarInventarioAsync(
                productoId,
                inventarioDTO.SucursalId,
                inventarioDTO.Stock
            );
        }

        public async Task<bool> ActualizarImagenAsync(int id, string nombreArchivo)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null)
                return false;
            producto.ImagenUrl = nombreArchivo;
            await _productoRepository.ActualizarProductoAsync(producto);
            return true;
        }

        public async Task<bool> CategoriaExisteAsync(int categoriaId)
        {
            // Suponiendo que hay un método en el repositorio de categorías
            return await _productoRepository.CategoriaExisteAsync(categoriaId);
        }

        public async Task<bool> MarcaExisteAsync(int marcaId)
        {
            // Suponiendo que hay un método en el repositorio de marcas
            return await _productoRepository.MarcaExisteAsync(marcaId);
        }

        public async Task<bool> ProductoCodigoExisteAsync(string codigo)
        {
            return await _productoRepository.ProductoExisteAsync(codigo);
        }

        // Método auxiliar para mapear de Modelo a DTO
        private ProductoDTO MapearADTO(Producto producto)
        {
            if (producto == null) return null;

            return new ProductoDTO
            {
                Id = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                CategoriaId = producto.CategoriaId,
                CategoriaNombre = producto.Categoria?.Nombre,
                MarcaId = producto.MarcaId,
                MarcaNombre = producto.Marca?.Nombre,
                ImagenUrl = producto.ImagenUrl,
                Especificaciones = producto.Especificaciones,
                FechaCreacion = producto.FechaCreacion,
                Activo = producto.Activo
            };
        }
    }
}