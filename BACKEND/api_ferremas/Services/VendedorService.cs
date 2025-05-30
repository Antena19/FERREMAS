using Ferremas.Api.Modelos;
using MySql.Data.MySqlClient;
using System.Data;

namespace Ferremas.Api.Services
{
    public class VendedorService : IVendedorService
    {
        private readonly string _connectionString;

        public VendedorService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Cliente>> GetClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT c.*, d.* 
                    FROM clientes c
                    LEFT JOIN direcciones d ON c.id = d.cliente_id", connection);

                using var reader = await command.ExecuteReaderAsync();
                var clienteDict = new Dictionary<int, Cliente>();

                while (await reader.ReadAsync())
                {
                    var clienteId = reader.GetInt32("id");
                    if (!clienteDict.ContainsKey(clienteId))
                    {
                        clienteDict[clienteId] = new Cliente
                        {
                            Id = clienteId,
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            CorreoElectronico = reader.GetString("correo_electronico"),
                            Telefono = reader.GetString("telefono"),
                            Direcciones = new List<Direccion>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("direccion_id")))
                    {
                        clienteDict[clienteId].Direcciones.Add(new Direccion
                        {
                            Id = reader.GetInt32("direccion_id"),
                            UsuarioId = clienteId,
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.GetString("codigo_postal")
                        });
                    }
                }

                clientes.AddRange(clienteDict.Values);
            }
            return clientes;
        }

        public async Task<Cliente> GetClienteById(int clienteId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT c.*, d.* 
                    FROM clientes c
                    LEFT JOIN direcciones d ON c.id = d.cliente_id
                    WHERE c.id = @clienteId", connection);

                command.Parameters.AddWithValue("@clienteId", clienteId);

                using var reader = await command.ExecuteReaderAsync();
                Cliente cliente = null;

                while (await reader.ReadAsync())
                {
                    if (cliente == null)
                    {
                        cliente = new Cliente
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            CorreoElectronico = reader.GetString("correo_electronico"),
                            Telefono = reader.GetString("telefono"),
                            Direcciones = new List<Direccion>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("direccion_id")))
                    {
                        cliente.Direcciones.Add(new Direccion
                        {
                            Id = reader.GetInt32("direccion_id"),
                            UsuarioId = clienteId,
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.GetString("codigo_postal")
                        });
                    }
                }

                return cliente;
            }
        }

        public async Task<IEnumerable<Pedido>> GetPedidosAsignados(int vendedorId)
        {
            var pedidos = new List<Pedido>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, c.*, i.*, pr.* 
                    FROM pedidos p
                    INNER JOIN clientes c ON p.cliente_id = c.id
                    INNER JOIN items_pedido i ON p.id = i.pedido_id
                    INNER JOIN productos pr ON i.producto_id = pr.id
                    WHERE p.vendedor_id = @vendedorId", connection);

                command.Parameters.AddWithValue("@vendedorId", vendedorId);

                using var reader = await command.ExecuteReaderAsync();
                var pedidoDict = new Dictionary<int, Pedido>();

                while (await reader.ReadAsync())
                {
                    var pedidoId = reader.GetInt32("pedido_id");
                    if (!pedidoDict.ContainsKey(pedidoId))
                    {
                        pedidoDict[pedidoId] = new Pedido
                        {
                            Id = pedidoId,
                            UsuarioId = reader.GetInt32("usuario_id"),
                            VendedorId = reader.GetInt32("vendedor_id"),
                            Estado = reader.GetString("estado"),
                            Total = reader.GetDecimal("total"),
                            FechaPedido = reader.GetDateTime("fecha_pedido"),
                            Usuario = new Usuario
                            {
                                Id = reader.GetInt32("usuario_id"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido")
                            },
                            Items = new List<PedidoItem>()
                        };
                    }

                    pedidoDict[pedidoId].Items.Add(new PedidoItem
                    {
                        Id = reader.GetInt32("item_id"),
                        PedidoId = pedidoId,
                        ProductoId = reader.GetInt32("producto_id"),
                        Cantidad = reader.GetInt32("cantidad"),
                        PrecioUnitario = reader.GetDecimal("precio_unitario"),
                        Subtotal = reader.GetDecimal("subtotal"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio")
                        }
                    });
                }

                pedidos.AddRange(pedidoDict.Values);
            }
            return pedidos;
        }

        public async Task<Pedido> GetPedidoById(int pedidoId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, c.*, i.*, pr.* 
                    FROM pedidos p
                    INNER JOIN clientes c ON p.cliente_id = c.id
                    INNER JOIN items_pedido i ON p.id = i.pedido_id
                    INNER JOIN productos pr ON i.producto_id = pr.id
                    WHERE p.id = @pedidoId", connection);

                command.Parameters.AddWithValue("@pedidoId", pedidoId);

                using var reader = await command.ExecuteReaderAsync();
                Pedido pedido = null;

                while (await reader.ReadAsync())
                {
                    if (pedido == null)
                    {
                        pedido = new Pedido
                        {
                            Id = pedidoId,
                            UsuarioId = reader.GetInt32("usuario_id"),
                            VendedorId = reader.GetInt32("vendedor_id"),
                            Estado = reader.GetString("estado"),
                            Total = reader.GetDecimal("total"),
                            FechaPedido = reader.GetDateTime("fecha_pedido"),
                            Usuario = new Usuario
                            {
                                Id = reader.GetInt32("usuario_id"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido")
                            },
                            Items = new List<PedidoItem>()
                        };
                    }

                    pedido.Items.Add(new PedidoItem
                    {
                        Id = reader.GetInt32("item_id"),
                        PedidoId = pedidoId,
                        ProductoId = reader.GetInt32("producto_id"),
                        Cantidad = reader.GetInt32("cantidad"),
                        PrecioUnitario = reader.GetDecimal("precio_unitario"),
                        Subtotal = reader.GetDecimal("subtotal"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio")
                        }
                    });
                }

                return pedido;
            }
        }

        public async Task<PedidoBodega> CrearPedidoBodega(int pedidoId, int vendedorId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    // Verificar pedido y vendedor
                    using var commandPedido = new MySqlCommand(@"
                        SELECT p.*, v.sucursal_id 
                        FROM pedidos p
                        INNER JOIN vendedores v ON p.vendedor_id = v.id
                        WHERE p.id = @pedidoId AND p.vendedor_id = @vendedorId", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPedido.Parameters.AddWithValue("@vendedorId", vendedorId);

                    using var readerPedido = await commandPedido.ExecuteReaderAsync();
                    if (!await readerPedido.ReadAsync())
                        throw new Exception("Pedido no encontrado o no asignado al vendedor");

                    var sucursalId = readerPedido.GetInt32("sucursal_id");
                    await readerPedido.CloseAsync();

                    // Buscar bodeguero activo
                    using var commandBodeguero = new MySqlCommand(@"
                        SELECT id FROM bodegueros 
                        WHERE sucursal_id = @sucursalId AND activo = 1 
                        ORDER BY RAND() LIMIT 1", connection, transaction);

                    commandBodeguero.Parameters.AddWithValue("@sucursalId", sucursalId);

                    var bodegueroId = await commandBodeguero.ExecuteScalarAsync();
                    if (bodegueroId == null)
                        throw new Exception("No hay bodegueros disponibles en la sucursal");

                    // Crear pedido bodega
                    using var commandPedidoBodega = new MySqlCommand(@"
                        INSERT INTO pedidos_bodega (pedido_id, bodeguero_id, estado, fecha_creacion)
                        VALUES (@pedidoId, @bodegueroId, 'pendiente', NOW());
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    commandPedidoBodega.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPedidoBodega.Parameters.AddWithValue("@bodegueroId", bodegueroId);

                    var pedidoBodegaId = Convert.ToInt32(await commandPedidoBodega.ExecuteScalarAsync());

                    // Crear items pedido bodega
                    using var commandItems = new MySqlCommand(@"
                        INSERT INTO items_pedido_bodega (pedido_bodega_id, producto_id, cantidad)
                        SELECT @pedidoBodegaId, producto_id, cantidad
                        FROM items_pedido
                        WHERE pedido_id = @pedidoId", connection, transaction);

                    commandItems.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    commandItems.Parameters.AddWithValue("@pedidoId", pedidoId);

                    await commandItems.ExecuteNonQueryAsync();

                    // Actualizar estado del pedido
                    using var commandUpdatePedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = 'en_bodega' 
                        WHERE id = @pedidoId", connection, transaction);

                    commandUpdatePedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandUpdatePedido.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();

                    // Obtener el pedido bodega creado
                    using var commandGetPedidoBodega = new MySqlCommand(@"
                        SELECT * FROM pedidos_bodega WHERE id = @pedidoBodegaId", connection);

                    commandGetPedidoBodega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);

                    using var readerPedidoBodega = await commandGetPedidoBodega.ExecuteReaderAsync();
                    if (await readerPedidoBodega.ReadAsync())
                    {
                        return new PedidoBodega
                        {
                            Id = readerPedidoBodega.GetInt32("id"),
                            PedidoId = readerPedidoBodega.GetInt32("pedido_id"),
                            BodegueroId = readerPedidoBodega.GetInt32("bodeguero_id"),
                            Estado = readerPedidoBodega.GetString("estado"),
                            FechaCreacion = readerPedidoBodega.GetDateTime("fecha_creacion")
                        };
                    }
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            return null;
        }

        public async Task<IEnumerable<Pedido>> GetTodosLosPedidos()
        {
            var pedidos = new List<Pedido>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, c.*, i.*, pr.* 
                    FROM pedidos p
                    INNER JOIN clientes c ON p.cliente_id = c.id
                    INNER JOIN items_pedido i ON p.id = i.pedido_id
                    INNER JOIN productos pr ON i.producto_id = pr.id", connection);

                using var reader = await command.ExecuteReaderAsync();
                var pedidoDict = new Dictionary<int, Pedido>();

                while (await reader.ReadAsync())
                {
                    var pedidoId = reader.GetInt32("pedido_id");
                    if (!pedidoDict.ContainsKey(pedidoId))
                    {
                        pedidoDict[pedidoId] = new Pedido
                        {
                            Id = pedidoId,
                            UsuarioId = reader.GetInt32("usuario_id"),
                            VendedorId = reader.GetInt32("vendedor_id"),
                            Estado = reader.GetString("estado"),
                            Total = reader.GetDecimal("total"),
                            FechaPedido = reader.GetDateTime("fecha_pedido"),
                            Usuario = new Usuario
                            {
                                Id = reader.GetInt32("usuario_id"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido")
                            },
                            Items = new List<PedidoItem>()
                        };
                    }

                    pedidoDict[pedidoId].Items.Add(new PedidoItem
                    {
                        Id = reader.GetInt32("item_id"),
                        PedidoId = pedidoId,
                        ProductoId = reader.GetInt32("producto_id"),
                        Cantidad = reader.GetInt32("cantidad"),
                        PrecioUnitario = reader.GetDecimal("precio_unitario"),
                        Subtotal = reader.GetDecimal("subtotal"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio")
                        }
                    });
                }

                pedidos.AddRange(pedidoDict.Values);
            }
            return pedidos;
        }

        public async Task<Pedido> ActualizarEstadoPedido(int pedidoId, string estado)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE pedidos 
                    SET estado = @estado 
                    WHERE id = @pedidoId;
                    SELECT * FROM pedidos WHERE id = @pedidoId", connection);

                command.Parameters.AddWithValue("@estado", estado);
                command.Parameters.AddWithValue("@pedidoId", pedidoId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Pedido
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        VendedorId = reader.GetInt32("vendedor_id"),
                        Estado = reader.GetString("estado"),
                        Total = reader.GetDecimal("total"),
                        FechaPedido = reader.GetDateTime("fecha_pedido")
                    };
                }
            }
            return null;
        }
    }
} 