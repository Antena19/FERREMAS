using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;

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
                        p.bodeguero_id as BodegueroId,
                        CONCAT(u.nombre, ' ', u.apellido) as UsuarioNombre,
                        s.nombre as SucursalNombre,
                        CONCAT(v.nombre, ' ', v.apellido) as VendedorNombre,
                        CONCAT(b.nombre, ' ', b.apellido) as BodegueroNombre
                    FROM pedidos p
                    LEFT JOIN usuarios u ON p.usuario_id = u.id
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN usuarios v ON p.vendedor_id = v.id
                    LEFT JOIN usuarios b ON p.bodeguero_id = b.id
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
                    SELECT 
                        p.*,
                        u.nombre as UsuarioNombre,
                        s.nombre as SucursalNombre,
                        v.nombre as VendedorNombre,
                        b.nombre as BodegueroNombre
                    FROM pedidos p
                    LEFT JOIN usuarios u ON p.usuario_id = u.id
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN usuarios v ON p.vendedor_id = v.id
                    LEFT JOIN usuarios b ON p.bodeguero_id = b.id
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
                        // Obtener el carrito activo del usuario
                        var carrito = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT * FROM carritos WHERE usuario_id = @UsuarioId AND activo = 1",
                            new { pedido.UsuarioId },
                            transaction
                        );

                        if (carrito == null)
                            throw new Exception("No hay carrito activo para este usuario");

                        // Crear pedido
                        var pedidoId = await connection.ExecuteScalarAsync<int>(
                            @"INSERT INTO pedidos (
                                usuario_id, tipo_entrega, sucursal_id, direccion_id,
                                subtotal, impuestos, costo_envio, total, notas, estado
                            ) VALUES (
                                @UsuarioId, @TipoEntrega, @SucursalId, @DireccionId,
                                @Subtotal, @Impuestos, @CostoEnvio, @Total, @Notas, 'pendiente'
                            );
                            SELECT LAST_INSERT_ID();",
                            pedido,
                            transaction
                        );

                        // Mover items del carrito a pedido_items
                        await connection.ExecuteAsync(
                            @"INSERT INTO pedido_items (
                                pedido_id, producto_id, cantidad, precio_unitario, subtotal
                            )
                            SELECT 
                                @PedidoId, producto_id, cantidad, precio_unitario, subtotal
                            FROM items_carrito
                            WHERE carrito_id = @CarritoId",
                            new { PedidoId = pedidoId, CarritoId = carrito.id },
                            transaction
                        );

                        // Desactivar carrito
                        await connection.ExecuteAsync(
                            "UPDATE carritos SET activo = 0 WHERE id = @CarritoId",
                            new { CarritoId = carrito.id },
                            transaction
                        );

                        await transaction.CommitAsync();

                        // Obtener el pedido creado con sus items
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
                        pi.subtotal,
                        p.id as ProductoId,
                        p.nombre as ProductoNombre,
                        p.precio as ProductoPrecio,
                        p.descripcion as ProductoDescripcion,
                        p.categoria_id as ProductoCategoriaId,
                        p.activo as ProductoActivo
                    FROM pedido_items pi
                    INNER JOIN productos p ON pi.producto_id = p.id
                    WHERE pi.pedido_id = @PedidoId";

                var items = await connection.QueryAsync<PedidoItem, Producto, PedidoItem>(
                    sql,
                    (pedidoItem, producto) =>
                    {
                        pedidoItem.Producto = producto;
                        return pedidoItem;
                    },
                    new { PedidoId = pedidoId },
                    splitOn: "ProductoId"
                );

                return items;
            }
        }

        public async Task<IEnumerable<Pedido>> GetPedidosPendientesAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        p.*,
                        u.nombre as UsuarioNombre,
                        s.nombre as SucursalNombre,
                        v.nombre as VendedorNombre,
                        b.nombre as BodegueroNombre
                    FROM pedidos p
                    LEFT JOIN usuarios u ON p.usuario_id = u.id
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN usuarios v ON p.vendedor_id = v.id
                    LEFT JOIN usuarios b ON p.bodeguero_id = b.id
                    WHERE p.estado IN ('pendiente', 'confirmado', 'asignado_vendedor', 'en_bodega', 'preparado', 'en_entrega')
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

        public async Task<IEnumerable<Pedido>> GetHistorialComprasClienteAsync(int clienteId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        p.*,
                        u.nombre as UsuarioNombre,
                        s.nombre as SucursalNombre,
                        v.nombre as VendedorNombre,
                        b.nombre as BodegueroNombre
                    FROM pedidos p
                    LEFT JOIN usuarios u ON p.usuario_id = u.id
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN usuarios v ON p.vendedor_id = v.id
                    LEFT JOIN usuarios b ON p.bodeguero_id = b.id
                    WHERE p.usuario_id IN (
                        SELECT u.id 
                        FROM usuarios u 
                        INNER JOIN clientes c ON u.rut = c.rut 
                        WHERE c.id = @ClienteId
                    )
                    ORDER BY p.fecha_pedido DESC";

                var pedidos = await connection.QueryAsync<Pedido>(sql, new { ClienteId = clienteId });

                foreach (var pedido in pedidos)
                {
                    var items = await GetPedidoItemsAsync(pedido.Id);
                    pedido.Items = items as ICollection<PedidoItem>;
                }

                return pedidos;
            }
        }

        // Clase auxiliar para mapear el resultado del query
        private class PedidoRaw
        {
            public int Id { get; set; }
            public int UsuarioId { get; set; }
            public DateTime FechaPedido { get; set; }
            public string Estado { get; set; }
            public string TipoEntrega { get; set; }
            public int? SucursalId { get; set; }
            public string SucursalNombre { get; set; }
            public int? DireccionId { get; set; }
            public string DireccionCalle { get; set; }
            public string DireccionNumero { get; set; }
            public string DireccionDepartamento { get; set; }
            public string DireccionComuna { get; set; }
            public string DireccionRegion { get; set; }
            public decimal Subtotal { get; set; }
            public decimal CostoEnvio { get; set; }
            public decimal Impuestos { get; set; }
            public decimal Total { get; set; }
            public string Notas { get; set; }
            public int? VendedorId { get; set; }
            public string VendedorNombre { get; set; }
            public int? BodegueroId { get; set; }
            public string BodegueroNombre { get; set; }
        }

        private class PedidoItemRaw
        {
            public int Id { get; set; }
            public int PedidoId { get; set; }
            public int ProductoId { get; set; }
            public string ProductoNombre { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
        }

        public async Task<IEnumerable<Pedido>> GetHistorialComprasUsuarioAsync(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        p.id, p.usuario_id as UsuarioId, p.fecha_pedido as FechaPedido, p.estado, p.tipo_entrega as TipoEntrega,
                        p.sucursal_id as SucursalId, s.nombre as SucursalNombre,
                        p.direccion_id as DireccionId, d.calle as DireccionCalle, d.numero as DireccionNumero, d.departamento as DireccionDepartamento, d.comuna as DireccionComuna, d.region as DireccionRegion,
                        p.subtotal, p.costo_envio as CostoEnvio, p.impuestos, p.total, p.notas,
                        p.vendedor_id as VendedorId, v.nombre as VendedorNombre,
                        p.bodeguero_id as BodegueroId, b.nombre as BodegueroNombre
                    FROM pedidos p
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN direcciones d ON p.direccion_id = d.id
                    LEFT JOIN usuarios v ON p.vendedor_id = v.id
                    LEFT JOIN usuarios b ON p.bodeguero_id = b.id
                    WHERE p.usuario_id = @UsuarioId
                    ORDER BY p.fecha_pedido DESC";

                var pedidosRaw = (await connection.QueryAsync<PedidoRaw>(sql, new { UsuarioId = usuarioId })).ToList();
                var pedidos = new List<Pedido>();

                foreach (var raw in pedidosRaw)
                {
                    var pedido = new Pedido
                    {
                        Id = raw.Id,
                        UsuarioId = raw.UsuarioId,
                        FechaPedido = raw.FechaPedido,
                        Estado = raw.Estado,
                        TipoEntrega = raw.TipoEntrega,
                        SucursalId = raw.SucursalId,
                        DireccionId = raw.DireccionId,
                        Subtotal = raw.Subtotal,
                        CostoEnvio = raw.CostoEnvio,
                        Impuestos = raw.Impuestos,
                        Total = raw.Total,
                        Notas = raw.Notas,
                        VendedorId = raw.VendedorId,
                        BodegueroId = raw.BodegueroId,
                        Sucursal = raw.SucursalId.HasValue ? new Sucursal { Id = raw.SucursalId.Value, Nombre = raw.SucursalNombre } : null,
                        Direccion = raw.DireccionId.HasValue ? new Direccion {
                            Id = raw.DireccionId.Value,
                            Calle = raw.DireccionCalle,
                            Numero = raw.DireccionNumero,
                            Departamento = raw.DireccionDepartamento,
                            Comuna = raw.DireccionComuna,
                            Region = raw.DireccionRegion
                        } : null,
                        Vendedor = raw.VendedorId.HasValue ? new Usuario { Id = raw.VendedorId.Value, Nombre = raw.VendedorNombre } : null,
                        Bodeguero = raw.BodegueroId.HasValue ? new Usuario { Id = raw.BodegueroId.Value, Nombre = raw.BodegueroNombre } : null
                    };

                    // Traer los items con nombre de producto
                    var itemsSql = @"
                        SELECT 
                            pi.id, pi.pedido_id as PedidoId, pi.producto_id as ProductoId, pr.nombre as ProductoNombre,
                            pi.cantidad, pi.precio_unitario as PrecioUnitario, pi.subtotal
                        FROM pedido_items pi
                        JOIN productos pr ON pi.producto_id = pr.id
                        WHERE pi.pedido_id = @PedidoId";

                    var itemsRaw = (await connection.QueryAsync<PedidoItemRaw>(itemsSql, new { PedidoId = pedido.Id })).ToList();
                    var items = new List<PedidoItem>();
                    foreach (var iraw in itemsRaw)
                    {
                        items.Add(new PedidoItem
                        {
                            Id = iraw.Id,
                            PedidoId = iraw.PedidoId,
                            ProductoId = iraw.ProductoId,
                            Cantidad = iraw.Cantidad,
                            PrecioUnitario = iraw.PrecioUnitario,
                            Subtotal = iraw.Subtotal,
                            Producto = new Producto { Id = iraw.ProductoId, Nombre = iraw.ProductoNombre }
                        });
                    }
                    pedido.Items = items;

                    pedidos.Add(pedido);
                }

                return pedidos;
            }
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(int pedidoId, string nuevoEstado)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "UPDATE pedidos SET estado = @NuevoEstado WHERE id = @PedidoId";
                var filas = await connection.ExecuteAsync(sql, new { PedidoId = pedidoId, NuevoEstado = nuevoEstado });
                return filas > 0;
            }
        }
    }
}