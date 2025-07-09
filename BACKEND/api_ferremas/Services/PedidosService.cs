using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using Ferremas.Api.Repositories;
using MySql.Data.MySqlClient;

namespace Ferremas.Api.Services
{
    public class PedidosService : IPedidosService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;

        public PedidosService(IPedidoRepository pedidoRepository,
                             IProductoRepository productoRepository,
                             IClienteRepository clienteRepository)
        {
            _pedidoRepository = pedidoRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;
        }

        public async Task<IEnumerable<PedidoDTO>> GetAllPedidosAsync()
        {
            var pedidos = await _pedidoRepository.GetAllPedidosAsync();
            return pedidos.Select(p => MapPedidoToDTO(p));
        }

        public async Task<PedidoDTO> GetPedidoByIdAsync(int id)
        {
            var pedido = await _pedidoRepository.GetPedidoByIdAsync(id);
            if (pedido == null)
                return null;

            return MapPedidoToDTO(pedido);
        }

        public async Task<IEnumerable<PedidoDTO>> GetPedidosByUsuarioIdAsync(int usuarioId)
        {
            var pedidos = await _pedidoRepository.GetPedidosByUsuarioIdAsync(usuarioId);
            return pedidos.Select(p => MapPedidoToDTO(p));
        }

        public async Task<IEnumerable<PedidoDTO>> GetPedidosPendientesAsync()
        {
            var pedidos = await _pedidoRepository.GetPedidosPendientesAsync();
            return pedidos.Select(p => MapPedidoToDTO(p));
        }

        public async Task<PedidoDTO> CreatePedidoAsync(Pedido pedido)
        {
            var pedidoCreado = await _pedidoRepository.CreatePedidoAsync(pedido);
            if (pedidoCreado == null)
                return null;

            return MapPedidoToDTO(pedidoCreado);
        }

        public async Task<PedidoDTO> UpdatePedidoAsync(int id, PedidoUpdateDTO pedidoUpdateDTO)
        {
            var pedidoExistente = await _pedidoRepository.GetPedidoByIdAsync(id);
            if (pedidoExistente == null)
                return null;

            pedidoExistente.Estado = pedidoUpdateDTO.Estado;

            if (pedidoUpdateDTO.Items != null && pedidoUpdateDTO.Items.Count > 0)
            {
                decimal totalPedido = 0;
                var itemsExistentes = pedidoExistente.Items.ToList();
                var itemsActualizados = new List<PedidoItem>();

                foreach (var itemDTO in pedidoUpdateDTO.Items)
                {
                    PedidoItem item;

                    if (itemDTO.Id.HasValue)
                    {
                        item = itemsExistentes.FirstOrDefault(i => i.Id == itemDTO.Id.Value);
                        if (item == null)
                            throw new KeyNotFoundException($"No se encontró el item con ID {itemDTO.Id.Value}");

                        var producto = await _productoRepository.ObtenerPorIdAsync(itemDTO.ProductoId);
                        if (producto == null)
                            throw new KeyNotFoundException($"No se encontró el producto con ID {itemDTO.ProductoId}");

                        item.ProductoId = itemDTO.ProductoId;
                        item.Cantidad = itemDTO.Cantidad;
                        item.PrecioUnitario = producto.Precio;
                        item.Subtotal = producto.Precio * itemDTO.Cantidad;
                    }
                    else
                    {
                        var producto = await _productoRepository.ObtenerPorIdAsync(itemDTO.ProductoId);
                        if (producto == null)
                            throw new KeyNotFoundException($"No se encontró el producto con ID {itemDTO.ProductoId}");

                        item = new PedidoItem
                        {
                            PedidoId = id,
                            ProductoId = itemDTO.ProductoId,
                            Cantidad = itemDTO.Cantidad,
                            PrecioUnitario = producto.Precio,
                            Subtotal = producto.Precio * itemDTO.Cantidad
                        };
                        pedidoExistente.Items.Add(item);
                    }

                    itemsActualizados.Add(item);
                    totalPedido += item.Subtotal;
                }

                var itemsAEliminar = itemsExistentes
                    .Where(i => !itemsActualizados.Any(ia => ia.Id == i.Id))
                    .ToList();

                foreach (var item in itemsAEliminar)
                {
                    pedidoExistente.Items.Remove(item);
                }

                pedidoExistente.Total = totalPedido;
            }

            var pedidoActualizado = await _pedidoRepository.UpdatePedidoAsync(id, pedidoExistente);
            return MapPedidoToDTO(pedidoActualizado);
        }

        public async Task<PedidoDTO> UpdatePedidoEstadoAsync(int id, string estado)
        {
            var pedidoActualizado = await _pedidoRepository.UpdatePedidoEstadoAsync(id, estado);
            if (pedidoActualizado == null)
                return null;

            return MapPedidoToDTO(pedidoActualizado);
        }

        public async Task<bool> DeletePedidoAsync(int id)
        {
            return await _pedidoRepository.DeletePedidoAsync(id);
        }

        public async Task<IEnumerable<PedidoDTO>> GetHistorialComprasClienteAsync(int clienteId)
        {
            var pedidos = await _pedidoRepository.GetHistorialComprasClienteAsync(clienteId);
            return pedidos.Select(p => MapPedidoToDTO(p));
        }

        public async Task<IEnumerable<PedidoDTO>> GetHistorialComprasUsuarioAsync(int usuarioId)
        {
            var pedidos = await _pedidoRepository.GetHistorialComprasUsuarioAsync(usuarioId);
            return pedidos.Select(p => MapPedidoToDTO(p));
        }

        private PedidoDTO MapPedidoToDTO(Pedido pedido)
        {
            return new PedidoDTO
            {
                Id = pedido.Id,
                Fecha = pedido.FechaPedido,
                UsuarioId = pedido.UsuarioId,
                UsuarioNombre = pedido.UsuarioNombre,
                Estado = pedido.Estado,
                TipoEntrega = pedido.TipoEntrega,
                SucursalId = pedido.SucursalId,
                SucursalNombre = pedido.SucursalNombre,
                DireccionId = pedido.DireccionId,
                DireccionTexto = pedido.Direccion != null ? $"{pedido.Direccion.Calle} {pedido.Direccion.Numero}{(string.IsNullOrEmpty(pedido.Direccion.Departamento) ? "" : ", Depto. " + pedido.Direccion.Departamento)}, {pedido.Direccion.Comuna}, {pedido.Direccion.Region}" : null,
                Subtotal = pedido.Subtotal,
                CostoEnvio = pedido.CostoEnvio,
                Impuestos = pedido.Impuestos,
                Total = pedido.Total,
                Notas = pedido.Notas,
                VendedorId = pedido.VendedorId,
                VendedorNombre = pedido.VendedorNombre,
                BodegueroId = pedido.BodegueroId,
                BodegueroNombre = pedido.BodegueroNombre,
                Items = pedido.Items?.Select(item => new PedidoItemResponseDTO
                {
                    Id = item.Id,
                    ProductoId = item.ProductoId,
                    ProductoNombre = item.ProductoNombre,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                }).ToList()
            };
        }
    }
}