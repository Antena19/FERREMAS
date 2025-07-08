﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Repositories
{
    public interface IPedidoRepository
    {
        Task<IEnumerable<Pedido>> GetAllPedidosAsync();
        Task<Pedido> GetPedidoByIdAsync(int id);
        Task<IEnumerable<Pedido>> GetPedidosByUsuarioIdAsync(int usuarioId);
        Task<Pedido> CreatePedidoAsync(Pedido pedido);
        Task<Pedido> UpdatePedidoAsync(int id, Pedido pedido);
        Task<Pedido> UpdatePedidoEstadoAsync(int id, string estado);
        Task<bool> DeletePedidoAsync(int id);
        Task<bool> PedidoExistsAsync(int id);
        Task<IEnumerable<Pedido>> GetPedidosPendientesAsync();
        Task<IEnumerable<Pedido>> GetHistorialComprasClienteAsync(int clienteId);
        Task<IEnumerable<Pedido>> GetHistorialComprasUsuarioAsync(int usuarioId);
    }
}