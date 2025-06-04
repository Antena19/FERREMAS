using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Ferremas.Api.Modelos;
using Ferremas.Api.DTOs;

namespace Ferremas.Api.Services
{
    public class VendedorService : IVendedorService
    {
        private readonly string _connectionString;

        public VendedorService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public async Task<IEnumerable<Cliente>> GetClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
                    SELECT 
                        c.id,
                        c.nombre,
                        c.apellido,
                        c.email,
                        c.telefono,
                        d.calle,
                        d.numero,
                        d.departamento,
                        d.comuna,
                        d.region,
                        d.codigo_postal
                    FROM usuarios c
                    LEFT JOIN direcciones d ON c.id = d.usuario_id
                    WHERE c.rol = 'cliente'";

                using var command = new MySqlCommand(sql, connection);
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
                            CorreoElectronico = reader.GetString("email"),
                            Telefono = reader.GetString("telefono"),
                            Direcciones = new List<Direccion>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("calle")))
                    {
                        clienteDict[clienteId].Direcciones.Add(new Direccion
                        {
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.IsDBNull(reader.GetOrdinal("codigo_postal")) ? null : reader.GetString("codigo_postal")
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
                var sql = @"
                    SELECT 
                        c.id,
                        c.nombre,
                        c.apellido,
                        c.email,
                        c.telefono,
                        d.calle,
                        d.numero,
                        d.departamento,
                        d.comuna,
                        d.region,
                        d.codigo_postal
                    FROM usuarios c
                    LEFT JOIN direcciones d ON c.id = d.usuario_id
                    WHERE c.id = @clienteId AND c.rol = 'cliente'";

                using var command = new MySqlCommand(sql, connection);
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
                            CorreoElectronico = reader.GetString("email"),
                            Telefono = reader.GetString("telefono"),
                            Direcciones = new List<Direccion>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("calle")))
                    {
                        cliente.Direcciones.Add(new Direccion
                        {
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.IsDBNull(reader.GetOrdinal("codigo_postal")) ? null : reader.GetString("codigo_postal")
                        });
                    }
                }

                return cliente;
            }
        }

        public async Task<IEnumerable<PedidoVendedorDTO>> GetPedidosAsignados(int vendedorId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var sql = @"
                        SELECT 
                            p.id,
                            p.fecha_pedido,
                            p.estado,
                            p.tipo_entrega,
                            p.subtotal,
                            p.costo_envio,
                            p.impuestos,
                            p.total,
                            p.notas,
                            u.nombre as nombre_cliente,
                            u.apellido as apellido_cliente,
                            u.email as email_cliente,
                            u.telefono as telefono_cliente,
                            s.nombre as nombre_sucursal,
                            d.calle,
                            d.numero,
                            d.departamento,
                            d.comuna,
                            d.region,
                            d.codigo_postal,
                            pv.fecha_asignacion,
                            pv.estado as estado_asignacion,
                            pv.comision_calculada,
                            COALESCE(
                                GROUP_CONCAT(
                                    CONCAT(
                                        COALESCE(pr.nombre, ''), '|',
                                        COALESCE(pi.cantidad, 0), '|',
                                        COALESCE(pi.precio_unitario, 0), '|',
                                        COALESCE(pi.subtotal, 0)
                                    )
                                ),
                                ''
                            ) as items_pedido
                        FROM pedidos p
                        JOIN pedidos_vendedor pv ON p.id = pv.pedido_id
                        JOIN usuarios u ON p.usuario_id = u.id
                        LEFT JOIN sucursales s ON p.sucursal_id = s.id
                        LEFT JOIN direcciones d ON p.direccion_id = d.id
                        LEFT JOIN pedido_items pi ON p.id = pi.pedido_id
                        LEFT JOIN productos pr ON pi.producto_id = pr.id
                        WHERE pv.vendedor_id = @vendedorId
                        GROUP BY p.id
                        ORDER BY pv.fecha_asignacion DESC";

                    using var command = new MySqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@vendedorId", vendedorId);
                    using var reader = await command.ExecuteReaderAsync();

                    var pedidos = new List<PedidoVendedorDTO>();

                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            var pedido = new PedidoVendedorDTO
                            {
                                Id = reader.GetInt32("id"),
                                FechaPedido = reader.GetDateTime("fecha_pedido"),
                                Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? null : reader.GetString("estado"),
                                TipoEntrega = reader.IsDBNull(reader.GetOrdinal("tipo_entrega")) ? null : reader.GetString("tipo_entrega"),
                                Subtotal = reader.IsDBNull(reader.GetOrdinal("subtotal")) ? 0 : reader.GetDecimal("subtotal"),
                                CostoEnvio = reader.IsDBNull(reader.GetOrdinal("costo_envio")) ? 0 : reader.GetDecimal("costo_envio"),
                                Impuestos = reader.IsDBNull(reader.GetOrdinal("impuestos")) ? 0 : reader.GetDecimal("impuestos"),
                                Total = reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : reader.GetDecimal("total"),
                                Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                                FechaAsignacion = reader.GetDateTime("fecha_asignacion"),
                                EstadoAsignacion = reader.IsDBNull(reader.GetOrdinal("estado_asignacion")) ? null : reader.GetString("estado_asignacion"),
                                ComisionCalculada = reader.IsDBNull(reader.GetOrdinal("comision_calculada")) ? null : (decimal?)reader.GetDecimal("comision_calculada"),
                                Cliente = new ClientePedidoDTO
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("nombre_cliente")) ? null : reader.GetString("nombre_cliente"),
                                    Apellido = reader.IsDBNull(reader.GetOrdinal("apellido_cliente")) ? null : reader.GetString("apellido_cliente"),
                                    CorreoElectronico = reader.IsDBNull(reader.GetOrdinal("email_cliente")) ? null : reader.GetString("email_cliente"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono_cliente")) ? null : reader.GetString("telefono_cliente")
                                },
                                Sucursal = reader.IsDBNull(reader.GetOrdinal("nombre_sucursal")) ? null : new SucursalDTO
                                {
                                    Nombre = reader.GetString("nombre_sucursal")
                                },
                                DireccionEntrega = reader.IsDBNull(reader.GetOrdinal("calle")) ? null : new DireccionDTO
                                {
                                    Calle = reader.IsDBNull(reader.GetOrdinal("calle")) ? null : reader.GetString("calle"),
                                    Numero = reader.IsDBNull(reader.GetOrdinal("numero")) ? null : reader.GetString("numero"),
                                    Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                                    Comuna = reader.IsDBNull(reader.GetOrdinal("comuna")) ? null : reader.GetString("comuna"),
                                    Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString("region"),
                                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("codigo_postal")) ? null : reader.GetString("codigo_postal")
                                },
                                Items = new List<ItemPedidoDTO>()
                            };

                            var itemsPedidoStr = reader.GetString("items_pedido");
                            if (!string.IsNullOrEmpty(itemsPedidoStr))
                            {
                                var itemsPedido = itemsPedidoStr.Split(',');
                                foreach (var item in itemsPedido)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        var partes = item.Split('|');
                                        if (partes.Length == 4)
                                        {
                                            pedido.Items.Add(new ItemPedidoDTO
                                            {
                                                Producto = new ProductoDTO { Nombre = partes[0] },
                                                Cantidad = int.Parse(partes[1]),
                                                PrecioUnitario = decimal.Parse(partes[2]),
                                                Subtotal = decimal.Parse(partes[3])
                                            });
                                        }
                                    }
                                }
                            }

                            pedidos.Add(pedido);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error al procesar el pedido {reader.GetInt32("id")}: {ex.Message}", ex);
                        }
                    }

                    return pedidos;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener pedidos asignados para el vendedor {vendedorId}: {ex.Message}", ex);
            }
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
                        WHERE p.id = @pedidoId 
                        AND p.vendedor_id = @vendedorId
                        AND p.estado = 'asignado_vendedor'", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPedido.Parameters.AddWithValue("@vendedorId", vendedorId);

                    using var readerPedido = await commandPedido.ExecuteReaderAsync();
                    if (!await readerPedido.ReadAsync())
                        throw new Exception("Pedido no encontrado, no asignado al vendedor o estado incorrecto");

                    var sucursalId = readerPedido.GetInt32("sucursal_id");
                    var tipoEntrega = readerPedido.GetString("tipo_entrega");
                    await readerPedido.CloseAsync();

                    // Verificar que el pedido tenga tipo de entrega válido
                    if (string.IsNullOrEmpty(tipoEntrega) || (tipoEntrega != "envio" && tipoEntrega != "retiro"))
                        throw new Exception("El pedido debe tener un tipo de entrega válido (envio o retiro)");

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
                        INSERT INTO pedidos_bodega (
                            pedido_id, 
                            vendedor_id,
                            bodeguero_id, 
                            estado, 
                            fecha_creacion
                        )
                        VALUES (
                            @pedidoId, 
                            @vendedorId,
                            @bodegueroId, 
                            'pendiente', 
                            NOW()
                        );
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    commandPedidoBodega.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPedidoBodega.Parameters.AddWithValue("@vendedorId", vendedorId);
                    commandPedidoBodega.Parameters.AddWithValue("@bodegueroId", bodegueroId);

                    var pedidoBodegaId = Convert.ToInt32(await commandPedidoBodega.ExecuteScalarAsync());

                    // Crear items pedido bodega
                    using var commandItems = new MySqlCommand(@"
                        INSERT INTO items_pedido_bodega (
                            pedido_bodega_id, 
                            producto_id, 
                            cantidad,
                            estado
                        )
                        SELECT 
                            @pedidoBodegaId, 
                            producto_id, 
                            cantidad,
                            'pendiente'
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

                    // Actualizar estado en pedidos_vendedor
                    using var commandUpdatePedidoVendedor = new MySqlCommand(@"
                        UPDATE pedidos_vendedor 
                        SET estado = 'en_proceso' 
                        WHERE pedido_id = @pedidoId", connection, transaction);

                    commandUpdatePedidoVendedor.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandUpdatePedidoVendedor.ExecuteNonQueryAsync();

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