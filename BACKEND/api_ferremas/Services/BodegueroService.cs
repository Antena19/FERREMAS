using Ferremas.Api.Modelos;
using MySql.Data.MySqlClient;
using System.Data;

namespace Ferremas.Api.Services
{
    public class BodegueroService : IBodegueroService
    {
        private readonly string _connectionString;

        public BodegueroService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Inventario>> GetInventarioSucursal(int sucursalId)
        {
            var inventario = new List<Inventario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT i.*, p.*, m.* 
                    FROM inventario i 
                    INNER JOIN productos p ON i.producto_id = p.id
                    INNER JOIN marcas m ON p.marca_id = m.id
                    WHERE i.sucursal_id = @sucursalId", connection);
                
                command.Parameters.AddWithValue("@sucursalId", sucursalId);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    inventario.Add(new Inventario
                    {
                        Id = reader.GetInt32("id"),
                        ProductoId = reader.GetInt32("producto_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Stock = reader.GetInt32("stock"),
                        StockMinimo = reader.GetInt32("stock_minimo"),
                        StockMaximo = reader.GetInt32("stock_maximo"),
                        PrecioCompra = reader.GetDecimal("precio_compra"),
                        PrecioVenta = reader.GetDecimal("precio_venta"),
                        Activo = reader.GetBoolean("activo"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Codigo = reader.GetString("codigo"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Marca = new Marca
                            {
                                Id = reader.GetInt32("marca_id"),
                                Nombre = reader.GetString("nombre_marca")
                            }
                        }
                    });
                }
            }
            return inventario;
        }

        public async Task<IEnumerable<PedidoBodega>> GetPedidosBodegaAsignados(int bodegueroId)
        {
            var pedidos = new List<PedidoBodega>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT pb.*, p.*, i.*, pr.* 
                    FROM pedidos_bodega pb
                    INNER JOIN pedidos p ON pb.pedido_id = p.id
                    INNER JOIN items_pedido_bodega i ON pb.id = i.pedido_bodega_id
                    INNER JOIN productos pr ON i.producto_id = pr.id
                    WHERE pb.bodeguero_id = @bodegueroId AND pb.estado != 'entregado'", connection);
                
                command.Parameters.AddWithValue("@bodegueroId", bodegueroId);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var pedidoBodega = new PedidoBodega
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        BodegueroId = reader.GetInt32("bodeguero_id"),
                        Estado = reader.GetString("estado"),
                        FechaCreacion = reader.GetDateTime("fecha_creacion"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        },
                        Items = new List<ItemPedidoBodega>
                        {
                            new ItemPedidoBodega
                            {
                                Id = reader.GetInt32("item_id"),
                                ProductoId = reader.GetInt32("producto_id"),
                                Cantidad = reader.GetInt32("cantidad"),
                                Producto = new Producto
                                {
                                    Id = reader.GetInt32("producto_id"),
                                    Nombre = reader.GetString("nombre_producto")
                                }
                            }
                        }
                    };
                    pedidos.Add(pedidoBodega);
                }
            }
            return pedidos;
        }

        public async Task<PedidoBodega> GetPedidoBodegaById(int pedidoBodegaId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT pb.*, p.*, i.*, pr.* 
                    FROM pedidos_bodega pb
                    INNER JOIN pedidos p ON pb.pedido_id = p.id
                    INNER JOIN items_pedido_bodega i ON pb.id = i.pedido_bodega_id
                    INNER JOIN productos pr ON i.producto_id = pr.id
                    WHERE pb.id = @pedidoBodegaId", connection);
                
                command.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PedidoBodega
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        BodegueroId = reader.GetInt32("bodeguero_id"),
                        Estado = reader.GetString("estado"),
                        FechaCreacion = reader.GetDateTime("fecha_creacion"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        },
                        Items = new List<ItemPedidoBodega>
                        {
                            new ItemPedidoBodega
                            {
                                Id = reader.GetInt32("item_id"),
                                ProductoId = reader.GetInt32("producto_id"),
                                Cantidad = reader.GetInt32("cantidad"),
                                Producto = new Producto
                                {
                                    Id = reader.GetInt32("producto_id"),
                                    Nombre = reader.GetString("nombre_producto")
                                }
                            }
                        }
                    };
                }
            }
            return null;
        }

        public async Task<EntregaBodega> CrearEntregaBodega(int pedidoBodegaId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var pedidoBodega = await GetPedidoBodegaById(pedidoBodegaId);
                    if (pedidoBodega == null)
                        throw new Exception("Pedido bodega no encontrado");

                    // Crear entrega bodega
                    using var commandEntrega = new MySqlCommand(@"
                        INSERT INTO entregas_bodega (pedido_bodega_id, fecha_entrega, estado, tipo_entrega)
                        VALUES (@pedidoBodegaId, NOW(), 'preparada', @tipoEntrega);
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    commandEntrega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    commandEntrega.Parameters.AddWithValue("@tipoEntrega", pedidoBodega.Pedido.TipoEntrega);

                    var entregaId = Convert.ToInt32(await commandEntrega.ExecuteScalarAsync());

                    // Actualizar estado del pedido bodega
                    using var commandPedidoBodega = new MySqlCommand(@"
                        UPDATE pedidos_bodega 
                        SET estado = 'preparado' 
                        WHERE id = @pedidoBodegaId", connection, transaction);

                    commandPedidoBodega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    await commandPedidoBodega.ExecuteNonQueryAsync();

                    // Actualizar estado del pedido
                    using var commandPedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = 'preparado' 
                        WHERE id = @pedidoId", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoBodega.PedidoId);
                    await commandPedido.ExecuteNonQueryAsync();

                    // Actualizar inventario
                    foreach (var item in pedidoBodega.Items)
                    {
                        using var commandInventario = new MySqlCommand(@"
                            UPDATE inventario 
                            SET stock = stock - @cantidad 
                            WHERE producto_id = @productoId AND sucursal_id = @sucursalId", connection, transaction);

                        commandInventario.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        commandInventario.Parameters.AddWithValue("@productoId", item.ProductoId);
                        commandInventario.Parameters.AddWithValue("@sucursalId", pedidoBodega.Pedido.SucursalId);
                        await commandInventario.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();

                    return new EntregaBodega
                    {
                        Id = entregaId,
                        PedidoBodegaId = pedidoBodegaId,
                        FechaEntrega = DateTime.Now,
                        Estado = "preparada",
                        TipoEntrega = pedidoBodega.Pedido.TipoEntrega
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Producto> ActualizarProducto(Producto producto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE productos 
                    SET codigo = @codigo, nombre = @nombre, descripcion = @descripcion,
                        marca_id = @marcaId, categoria_id = @categoriaId, precio = @precio
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", producto.Id);
                command.Parameters.AddWithValue("@codigo", producto.Codigo);
                command.Parameters.AddWithValue("@nombre", producto.Nombre);
                command.Parameters.AddWithValue("@descripcion", producto.Descripcion);
                command.Parameters.AddWithValue("@marcaId", producto.MarcaId);
                command.Parameters.AddWithValue("@categoriaId", producto.CategoriaId);
                command.Parameters.AddWithValue("@precio", producto.Precio);

                await command.ExecuteNonQueryAsync();
                return producto;
            }
        }

        public async Task<Inventario> ActualizarInventario(Inventario inventario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE inventario 
                    SET stock = @stock, stock_minimo = @stockMinimo, stock_maximo = @stockMaximo,
                        precio_compra = @precioCompra, precio_venta = @precioVenta, activo = @activo
                    WHERE producto_id = @productoId AND sucursal_id = @sucursalId", connection);

                command.Parameters.AddWithValue("@productoId", inventario.ProductoId);
                command.Parameters.AddWithValue("@sucursalId", inventario.SucursalId);
                command.Parameters.AddWithValue("@stock", inventario.Stock);
                command.Parameters.AddWithValue("@stockMinimo", inventario.StockMinimo);
                command.Parameters.AddWithValue("@stockMaximo", inventario.StockMaximo);
                command.Parameters.AddWithValue("@precioCompra", inventario.PrecioCompra);
                command.Parameters.AddWithValue("@precioVenta", inventario.PrecioVenta);
                command.Parameters.AddWithValue("@activo", inventario.Activo);

                await command.ExecuteNonQueryAsync();
                return inventario;
            }
        }

        public async Task<IEnumerable<Producto>> GetProductosSucursal(int sucursalId)
        {
            var productos = new List<Producto>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, m.* 
                    FROM productos p
                    INNER JOIN marcas m ON p.marca_id = m.id
                    WHERE EXISTS (
                        SELECT 1 FROM inventario i 
                        WHERE i.producto_id = p.id AND i.sucursal_id = @sucursalId
                    )", connection);
                
                command.Parameters.AddWithValue("@sucursalId", sucursalId);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    productos.Add(new Producto
                    {
                        Id = reader.GetInt32("id"),
                        Codigo = reader.GetString("codigo"),
                        Nombre = reader.GetString("nombre"),
                        Descripcion = reader.GetString("descripcion"),
                        MarcaId = reader.GetInt32("marca_id"),
                        CategoriaId = reader.GetInt32("categoria_id"),
                        Precio = reader.GetDecimal("precio"),
                        Marca = new Marca
                        {
                            Id = reader.GetInt32("marca_id"),
                            Nombre = reader.GetString("nombre_marca")
                        }
                    });
                }
            }
            return productos;
        }
    }
} 