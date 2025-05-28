using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Ferremas.Api.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly string _connectionString;

        public PedidoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Pedido>> GetAllPedidosAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        p.id,
                        p.usuario_id as UsuarioId,
                        p.fecha_pedido as FechaPedido,
                        p.estado,
                        p.tipo_entrega as TipoEntrega,
                        p.sucursal_id as SucursalId,
                        p.direccion_id as DireccionId,
                        p.subtotal,
                        p.costo_envio as CostoEnvio,
                        p.impuestos,
                        p.total,
                        p.notas,
                        p.vendedor_id as VendedorId,
                        p.bodeguero_id as BodegueroId
                    FROM pedidos p
                    ORDER BY p.fecha_pedido DESC";

                var pedidos = await connection.QueryAsync<Pedido>(sql);

                foreach (var pedido in pedidos)
                {
                    var items = await GetPedidoItemsAsync(pedido.Id);
                    pedido.Items = items as ICollection<PedidoItem>;
                }

                return pedidos;
            }
        }

        public async Task<Pedido> GetPedidoByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, u.nombre as UsuarioNombre
                    FROM pedidos p
                    JOIN usuarios u ON p.usuario_id = u.id
                    WHERE p.id = @Id";

                var pedido = await connection.QueryFirstOrDefaultAsync<Pedido>(sql, new { Id = id });

                if (pedido != null)
                {
                    var items = await GetPedidoItemsAsync(pedido.Id);
                    pedido.Items = items as ICollection<PedidoItem>;
                }

                return pedido;
            }
        }

        public async Task<IEnumerable<Pedido>> GetPedidosByUsuarioIdAsync(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, u.nombre as UsuarioNombre
                    FROM pedidos p
                    JOIN usuarios u ON p.usuario_id = u.id
                    WHERE p.usuario_id = @UsuarioId
                    ORDER BY p.fecha_pedido DESC";

                var pedidos = await connection.QueryAsync<Pedido>(sql, new { UsuarioId = usuarioId });

                foreach (var pedido in pedidos)
                {
                    var items = await GetPedidoItemsAsync(pedido.Id);
                    pedido.Items = items as ICollection<PedidoItem>;
                }

                return pedidos;
            }
        }

        public async Task<Pedido> CreatePedidoAsync(Pedido pedido)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var sqlPedido = @"
                            INSERT INTO pedidos (
                                usuario_id,
                                fecha_pedido,
                                estado,
                                tipo_entrega,
                                sucursal_id,
                                direccion_id,
                                subtotal,
                                costo_envio,
                                impuestos,
                                total,
                                notas,
                                vendedor_id,
                                bodeguero_id
                            ) VALUES (
                                @UsuarioId,
                                @FechaPedido,
                                @Estado,
                                @TipoEntrega,
                                @SucursalId,
                                @DireccionId,
                                @Subtotal,
                                @CostoEnvio,
                                @Impuestos,
                                @Total,
                                @Notas,
                                @VendedorId,
                                @BodegueroId
                            );
                            SELECT LAST_INSERT_ID();";

                        var pedidoId = await connection.ExecuteScalarAsync<int>(sqlPedido, new
                        {
                            pedido.UsuarioId,
                            pedido.FechaPedido,
                            pedido.Estado,
                            pedido.TipoEntrega,
                            pedido.SucursalId,
                            pedido.DireccionId,
                            pedido.Subtotal,
                            pedido.CostoEnvio,
                            pedido.Impuestos,
                            pedido.Total,
                            pedido.Notas,
                            pedido.VendedorId,
                            pedido.BodegueroId
                        }, transaction);

                        pedido.Id = pedidoId;

                        if (pedido.Items != null && pedido.Items.Count > 0)
                        {
                            var sqlItem = @"
                                INSERT INTO pedido_items (
                                    pedido_id,
                                    producto_id,
                                    cantidad,
                                    precio_unitario,
                                    subtotal
                                ) VALUES (
                                    @PedidoId,
                                    @ProductoId,
                                    @Cantidad,
                                    @PrecioUnitario,
                                    @Subtotal
                                );
                                SELECT LAST_INSERT_ID();";

                            foreach (var item in pedido.Items)
                            {
                                item.PedidoId = pedidoId;
                                var itemId = await connection.ExecuteScalarAsync<int>(sqlItem, new
                                {
                                    item.PedidoId,
                                    item.ProductoId,
                                    item.Cantidad,
                                    item.PrecioUnitario,
                                    item.Subtotal
                                }, transaction);

                                item.Id = itemId;
                            }
                        }

                        await transaction.CommitAsync();
                        return await GetPedidoByIdAsync(pedidoId);
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<Pedido> UpdatePedidoAsync(int id, Pedido pedido)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var sqlPedido = @"
                            UPDATE pedidos 
                            SET 
                                estado = @Estado,
                                tipo_entrega = @TipoEntrega,
                                sucursal_id = @SucursalId,
                                direccion_id = @DireccionId,
                                subtotal = @Subtotal,
                                costo_envio = @CostoEnvio,
                                impuestos = @Impuestos,
                                total = @Total,
                                notas = @Notas,
                                vendedor_id = @VendedorId,
                                bodeguero_id = @BodegueroId
                            WHERE id = @Id";

                        await connection.ExecuteAsync(sqlPedido, new
                        {
                            Id = id,
                            pedido.Estado,
                            pedido.TipoEntrega,
                            pedido.SucursalId,
                            pedido.DireccionId,
                            pedido.Subtotal,
                            pedido.CostoEnvio,
                            pedido.Impuestos,
                            pedido.Total,
                            pedido.Notas,
                            pedido.VendedorId,
                            pedido.BodegueroId
                        }, transaction);

                        if (pedido.Items != null && pedido.Items.Count > 0)
                        {
                            var sqlGetItems = "SELECT id FROM pedido_items WHERE pedido_id = @PedidoId";
                            var itemsActualesIds = await connection.QueryAsync<int>(sqlGetItems, new { PedidoId = id }, transaction);

                            var itemsNuevosIds = new HashSet<int>();
                            foreach (var item in pedido.Items)
                            {
                                if (item.Id > 0)
                                    itemsNuevosIds.Add(item.Id);
                            }

                            foreach (var itemId in itemsActualesIds)
                            {
                                if (!itemsNuevosIds.Contains(itemId))
                                {
                                    var sqlDeleteItem = "DELETE FROM pedido_items WHERE id = @Id";
                                    await connection.ExecuteAsync(sqlDeleteItem, new { Id = itemId }, transaction);
                                }
                            }

                            foreach (var item in pedido.Items)
                            {
                                if (item.Id > 0)
                                {
                                    var sqlUpdateItem = @"
                                        UPDATE pedido_items 
                                        SET 
                                            producto_id = @ProductoId,
                                            cantidad = @Cantidad,
                                            precio_unitario = @PrecioUnitario,
                                            subtotal = @Subtotal
                                        WHERE id = @Id";

                                    await connection.ExecuteAsync(sqlUpdateItem, new
                                    {
                                        item.Id,
                                        item.ProductoId,
                                        item.Cantidad,
                                        item.PrecioUnitario,
                                        item.Subtotal
                                    }, transaction);
                                }
                                else
                                {
                                    var sqlInsertItem = @"
                                        INSERT INTO pedido_items (
                                            pedido_id,
                                            producto_id,
                                            cantidad,
                                            precio_unitario,
                                            subtotal
                                        ) VALUES (
                                            @PedidoId,
                                            @ProductoId,
                                            @Cantidad,
                                            @PrecioUnitario,
                                            @Subtotal
                                        );
                                        SELECT LAST_INSERT_ID();";

                                    item.PedidoId = id;
                                    var itemId = await connection.ExecuteScalarAsync<int>(sqlInsertItem, new
                                    {
                                        item.PedidoId,
                                        item.ProductoId,
                                        item.Cantidad,
                                        item.PrecioUnitario,
                                        item.Subtotal
                                    }, transaction);

                                    item.Id = itemId;
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return await GetPedidoByIdAsync(id);
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<Pedido> UpdatePedidoEstadoAsync(int id, string estado)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "UPDATE pedidos SET estado = @Estado WHERE id = @Id";

                await connection.ExecuteAsync(sql, new { Id = id, Estado = estado });

                return await GetPedidoByIdAsync(id);
            }
        }

        public async Task<bool> DeletePedidoAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var sqlDeleteItems = "DELETE FROM pedido_items WHERE pedido_id = @PedidoId";
                        await connection.ExecuteAsync(sqlDeleteItems, new { PedidoId = id }, transaction);

                        var sqlDeletePedido = "DELETE FROM pedidos WHERE id = @Id";
                        var filasAfectadas = await connection.ExecuteAsync(sqlDeletePedido, new { Id = id }, transaction);

                        await transaction.CommitAsync();
                        return filasAfectadas > 0;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> PedidoExistsAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT COUNT(1) FROM pedidos WHERE id = @Id";

                var existe = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

                return existe > 0;
            }
        }

        private async Task<IEnumerable<PedidoItem>> GetPedidoItemsAsync(int pedidoId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        pi.id,
                        pi.pedido_id as PedidoId,
                        pi.producto_id as ProductoId,
                        pi.cantidad,
                        pi.precio_unitario as PrecioUnitario,
                        pi.subtotal
                    FROM pedido_items pi
                    WHERE pi.pedido_id = @PedidoId";

                var items = await connection.QueryAsync<PedidoItem>(sql, new { PedidoId = pedidoId });

                return items;
            }
        }

        public async Task<IEnumerable<Pedido>> GetPedidosPendientesAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, u.nombre as UsuarioNombre
                    FROM pedidos p
                    JOIN usuarios u ON p.usuario_id = u.id
                    WHERE p.estado = 'pendiente'
                    ORDER BY p.fecha_pedido DESC";

                var pedidos = await connection.QueryAsync<Pedido>(sql);

                foreach (var pedido in pedidos)
                {
                    var items = await GetPedidoItemsAsync(pedido.Id);
                    pedido.Items = items as ICollection<PedidoItem>;
                }

                return pedidos;
            }
        }
    }
}