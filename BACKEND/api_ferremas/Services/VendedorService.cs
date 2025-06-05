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
                                        COALESCE(pr.id, 0), '|',
                                        COALESCE(pr.codigo, ''), '|',
                                        COALESCE(pr.nombre, ''), '|',
                                        COALESCE(pr.descripcion, ''), '|',
                                        COALESCE(pr.precio, 0), '|',
                                        COALESCE(pr.categoria_id, 0), '|',
                                        COALESCE(pr.marca_id, 0), '|',
                                        COALESCE(pr.imagen_url, ''), '|',
                                        COALESCE(pr.especificaciones, ''), '|',
                                        COALESCE(pr.fecha_creacion, ''), '|',
                                        COALESCE(pr.activo, 0), '|',
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
                        GROUP BY 
                            p.id, p.fecha_pedido, p.estado, p.tipo_entrega, 
                            p.subtotal, p.costo_envio, p.impuestos, p.total, 
                            p.notas, u.nombre, u.apellido, u.email, u.telefono,
                            s.nombre, d.calle, d.numero, d.departamento, 
                            d.comuna, d.region, d.codigo_postal,
                            pv.fecha_asignacion, pv.estado, pv.comision_calculada
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
                                        if (partes.Length == 14)
                                        {
                                            pedido.Items.Add(new ItemPedidoDTO
                                            {
                                                Producto = new ProductoDTO 
                                                { 
                                                    Id = int.Parse(partes[0]),
                                                    Codigo = partes[1],
                                                    Nombre = partes[2],
                                                    Descripcion = partes[3],
                                                    Precio = decimal.Parse(partes[4]),
                                                    CategoriaId = int.Parse(partes[5]),
                                                    MarcaId = int.Parse(partes[6]),
                                                    ImagenUrl = partes[7],
                                                    Especificaciones = partes[8],
                                                    FechaCreacion = DateTime.Parse(partes[9]),
                                                    Activo = partes[10] == "1"
                                                },
                                                Cantidad = int.Parse(partes[11]),
                                                PrecioUnitario = decimal.Parse(partes[12]),
                                                Subtotal = decimal.Parse(partes[13])
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
                var sql = @"
                    SELECT 
                        p.*,
                        u.id as usuario_id,
                        u.nombre as usuario_nombre,
                        u.apellido as usuario_apellido,
                        u.email as usuario_email,
                        u.telefono as usuario_telefono,
                        s.id as sucursal_id,
                        s.nombre as sucursal_nombre,
                        d.id as direccion_id,
                        d.calle,
                        d.numero,
                        d.departamento,
                        d.comuna,
                        d.region,
                        d.codigo_postal,
                        pi.id as item_id,
                        pi.producto_id,
                        pi.cantidad,
                        pi.precio_unitario,
                        pi.subtotal,
                        pr.codigo as producto_codigo,
                        pr.nombre as producto_nombre,
                        pr.descripcion as producto_descripcion,
                        pr.precio as producto_precio,
                        pr.categoria_id as producto_categoria_id,
                        pr.marca_id as producto_marca_id,
                        pr.imagen_url as producto_imagen_url,
                        pr.especificaciones as producto_especificaciones,
                        pr.fecha_creacion as producto_fecha_creacion,
                        pr.activo as producto_activo
                    FROM pedidos p
                    INNER JOIN usuarios u ON p.usuario_id = u.id
                    LEFT JOIN sucursales s ON p.sucursal_id = s.id
                    LEFT JOIN direcciones d ON p.direccion_id = d.id
                    LEFT JOIN pedido_items pi ON p.id = pi.pedido_id
                    LEFT JOIN productos pr ON pi.producto_id = pr.id
                    WHERE p.id = @pedidoId";

                using var command = new MySqlCommand(sql, connection);
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
                            FechaPedido = reader.GetDateTime("fecha_pedido"),
                            Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? null : reader.GetString("estado"),
                            TipoEntrega = reader.IsDBNull(reader.GetOrdinal("tipo_entrega")) ? null : reader.GetString("tipo_entrega"),
                            Subtotal = reader.IsDBNull(reader.GetOrdinal("subtotal")) ? 0 : reader.GetDecimal("subtotal"),
                            CostoEnvio = reader.IsDBNull(reader.GetOrdinal("costo_envio")) ? 0 : reader.GetDecimal("costo_envio"),
                            Impuestos = reader.IsDBNull(reader.GetOrdinal("impuestos")) ? 0 : reader.GetDecimal("impuestos"),
                            Total = reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : reader.GetDecimal("total"),
                            Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                            VendedorId = reader.IsDBNull(reader.GetOrdinal("vendedor_id")) ? 0 : reader.GetInt32("vendedor_id"),
                            BodegueroId = reader.IsDBNull(reader.GetOrdinal("bodeguero_id")) ? 0 : reader.GetInt32("bodeguero_id"),
                            Usuario = new Usuario
                            {
                                Id = reader.GetInt32("usuario_id"),
                                Nombre = reader.IsDBNull(reader.GetOrdinal("usuario_nombre")) ? null : reader.GetString("usuario_nombre"),
                                Apellido = reader.IsDBNull(reader.GetOrdinal("usuario_apellido")) ? null : reader.GetString("usuario_apellido"),
                                Email = reader.IsDBNull(reader.GetOrdinal("usuario_email")) ? null : reader.GetString("usuario_email"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("usuario_telefono")) ? null : reader.GetString("usuario_telefono")
                            },
                            Sucursal = reader.IsDBNull(reader.GetOrdinal("sucursal_id")) ? null : new Sucursal
                            {
                                Id = reader.GetInt32("sucursal_id"),
                                Nombre = reader.IsDBNull(reader.GetOrdinal("sucursal_nombre")) ? null : reader.GetString("sucursal_nombre")
                            },
                            Direccion = reader.IsDBNull(reader.GetOrdinal("direccion_id")) ? null : new Direccion
                            {
                                Id = reader.GetInt32("direccion_id"),
                                Calle = reader.IsDBNull(reader.GetOrdinal("calle")) ? null : reader.GetString("calle"),
                                Numero = reader.IsDBNull(reader.GetOrdinal("numero")) ? null : reader.GetString("numero"),
                                Departamento = reader.IsDBNull(reader.GetOrdinal("departamento")) ? null : reader.GetString("departamento"),
                                Comuna = reader.IsDBNull(reader.GetOrdinal("comuna")) ? null : reader.GetString("comuna"),
                                Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString("region"),
                                CodigoPostal = reader.IsDBNull(reader.GetOrdinal("codigo_postal")) ? null : reader.GetString("codigo_postal")
                            },
                            Items = new List<PedidoItem>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("item_id")))
                    {
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
                                Codigo = reader.IsDBNull(reader.GetOrdinal("producto_codigo")) ? null : reader.GetString("producto_codigo"),
                                Nombre = reader.IsDBNull(reader.GetOrdinal("producto_nombre")) ? null : reader.GetString("producto_nombre"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("producto_descripcion")) ? null : reader.GetString("producto_descripcion"),
                                Precio = reader.IsDBNull(reader.GetOrdinal("producto_precio")) ? 0 : reader.GetDecimal("producto_precio"),
                                CategoriaId = reader.IsDBNull(reader.GetOrdinal("producto_categoria_id")) ? 0 : reader.GetInt32("producto_categoria_id"),
                                MarcaId = reader.IsDBNull(reader.GetOrdinal("producto_marca_id")) ? 0 : reader.GetInt32("producto_marca_id"),
                                ImagenUrl = reader.IsDBNull(reader.GetOrdinal("producto_imagen_url")) ? null : reader.GetString("producto_imagen_url"),
                                Especificaciones = reader.IsDBNull(reader.GetOrdinal("producto_especificaciones")) ? null : reader.GetString("producto_especificaciones"),
                                FechaCreacion = reader.IsDBNull(reader.GetOrdinal("producto_fecha_creacion")) ? DateTime.MinValue : reader.GetDateTime("producto_fecha_creacion"),
                                Activo = reader.IsDBNull(reader.GetOrdinal("producto_activo")) ? false : reader.GetBoolean("producto_activo")
                            }
                        });
                    }
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
                        SELECT p.*, pv.vendedor_id, v.sucursal_id 
                        FROM pedidos p
                        INNER JOIN pedidos_vendedor pv ON p.id = pv.pedido_id
                        INNER JOIN vendedores v ON pv.vendedor_id = v.id
                        WHERE p.id = @pedidoId 
                        AND pv.vendedor_id = @vendedorId
                        AND p.estado = 'asignado_vendedor'
                        AND pv.estado = 'asignado'", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPedido.Parameters.AddWithValue("@vendedorId", vendedorId);

                    using var readerPedido = await commandPedido.ExecuteReaderAsync();
                    if (!await readerPedido.ReadAsync())
                    {
                        throw new Exception("Pedido no encontrado, no asignado al vendedor o estado incorrecto");
                    }

                    var sucursalId = readerPedido.GetInt32("sucursal_id");
                    var tipoEntrega = readerPedido.GetString("tipo_entrega");
                    await readerPedido.CloseAsync();

                    // Verificar que el pedido tenga tipo de entrega válido
                    if (string.IsNullOrEmpty(tipoEntrega) || (tipoEntrega != "retiro_tienda" && tipoEntrega != "despacho_domicilio"))
                        throw new Exception("El pedido debe tener un tipo de entrega válido (retiro_tienda o despacho_domicilio)");

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
                        FROM pedido_items
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

                    // Obtener el pedido bodega creado con todos sus datos relacionados
                    using var commandGetPedidoBodega = new MySqlCommand(@"
                        SELECT 
                            pb.*,
                            p.fecha_pedido,
                            p.estado as estado_pedido,
                            p.tipo_entrega,
                            p.subtotal,
                            p.costo_envio,
                            p.impuestos,
                            p.total,
                            p.notas as notas_pedido,
                            u.nombre as nombre_vendedor,
                            u.apellido as apellido_vendedor,
                            u.email as email_vendedor,
                            u.telefono as telefono_vendedor,
                            b.nombre as nombre_bodeguero,
                            b.apellido as apellido_bodeguero,
                            b.email as email_bodeguero,
                            b.telefono as telefono_bodeguero,
                            s.nombre as nombre_sucursal,
                            COALESCE(
                                GROUP_CONCAT(
                                    CONCAT(
                                        pi.id, '|',
                                        pi.producto_id, '|',
                                        pr.nombre, '|',
                                        pi.cantidad, '|',
                                        pi.precio_unitario, '|',
                                        pi.subtotal
                                    )
                                ),
                                ''
                            ) as items_pedido
                        FROM pedidos_bodega pb
                        INNER JOIN pedidos p ON pb.pedido_id = p.id
                        INNER JOIN vendedores v ON pb.vendedor_id = v.id
                        INNER JOIN usuarios u ON v.usuario_id = u.id
                        INNER JOIN bodegueros b ON pb.bodeguero_id = b.id
                        INNER JOIN sucursales s ON v.sucursal_id = s.id
                        LEFT JOIN pedido_items pi ON p.id = pi.pedido_id
                        LEFT JOIN productos pr ON pi.producto_id = pr.id
                        WHERE pb.id = @pedidoBodegaId
                        GROUP BY pb.id", connection);

                    commandGetPedidoBodega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);

                    using var readerPedidoBodega = await commandGetPedidoBodega.ExecuteReaderAsync();
                    if (await readerPedidoBodega.ReadAsync())
                    {
                        var pedidoBodega = new PedidoBodega
                        {
                            Id = readerPedidoBodega.GetInt32("id"),
                            PedidoId = readerPedidoBodega.GetInt32("pedido_id"),
                            VendedorId = readerPedidoBodega.GetInt32("vendedor_id"),
                            BodegueroId = readerPedidoBodega.GetInt32("bodeguero_id"),
                            Estado = readerPedidoBodega.GetString("estado"),
                            FechaCreacion = readerPedidoBodega.GetDateTime("fecha_creacion"),
                            FechaPreparacion = readerPedidoBodega.IsDBNull(readerPedidoBodega.GetOrdinal("fecha_preparacion")) ? null : (DateTime?)readerPedidoBodega.GetDateTime("fecha_preparacion"),
                            Notas = readerPedidoBodega.IsDBNull(readerPedidoBodega.GetOrdinal("notas")) ? null : readerPedidoBodega.GetString("notas"),
                            Pedido = new Pedido
                            {
                                Id = readerPedidoBodega.GetInt32("pedido_id"),
                                FechaPedido = readerPedidoBodega.GetDateTime("fecha_pedido"),
                                Estado = readerPedidoBodega.GetString("estado_pedido"),
                                TipoEntrega = readerPedidoBodega.GetString("tipo_entrega"),
                                Subtotal = readerPedidoBodega.GetDecimal("subtotal"),
                                CostoEnvio = readerPedidoBodega.GetDecimal("costo_envio"),
                                Impuestos = readerPedidoBodega.GetDecimal("impuestos"),
                                Total = readerPedidoBodega.GetDecimal("total"),
                                Notas = readerPedidoBodega.IsDBNull(readerPedidoBodega.GetOrdinal("notas_pedido")) ? null : readerPedidoBodega.GetString("notas_pedido")
                            },
                            Vendedor = new Usuario
                            {
                                Id = readerPedidoBodega.GetInt32("vendedor_id"),
                                Nombre = readerPedidoBodega.GetString("nombre_vendedor"),
                                Apellido = readerPedidoBodega.GetString("apellido_vendedor"),
                                Email = readerPedidoBodega.GetString("email_vendedor"),
                                Telefono = readerPedidoBodega.GetString("telefono_vendedor")
                            },
                            Bodeguero = new Usuario
                            {
                                Id = readerPedidoBodega.GetInt32("bodeguero_id"),
                                Nombre = readerPedidoBodega.GetString("nombre_bodeguero"),
                                Apellido = readerPedidoBodega.GetString("apellido_bodeguero"),
                                Email = readerPedidoBodega.GetString("email_bodeguero"),
                                Telefono = readerPedidoBodega.GetString("telefono_bodeguero")
                            },
                            Items = new List<ItemPedidoBodega>()
                        };

                        var itemsPedidoStr = readerPedidoBodega.GetString("items_pedido");
                        if (!string.IsNullOrEmpty(itemsPedidoStr))
                        {
                            var itemsPedido = itemsPedidoStr.Split(',');
                            foreach (var item in itemsPedido)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    var partes = item.Split('|');
                                    if (partes.Length == 6)
                                    {
                                        pedidoBodega.Items.Add(new ItemPedidoBodega
                                        {
                                            Id = int.Parse(partes[0]),
                                            PedidoBodegaId = pedidoBodegaId,
                                            ProductoId = int.Parse(partes[1]),
                                            Producto = new Producto { Nombre = partes[2] },
                                            Cantidad = int.Parse(partes[3]),
                                            Estado = "pendiente"
                                        });
                                    }
                                }
                            }
                        }

                        return pedidoBodega;
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
                    INNER JOIN usuarios c ON p.usuario_id = c.id
                    INNER JOIN pedido_items i ON p.id = i.pedido_id
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
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Verificar el estado actual del pedido
                    using var commandVerificar = new MySqlCommand(@"
                        SELECT p.estado, pb.estado as estado_bodega
                        FROM pedidos p
                        LEFT JOIN pedidos_bodega pb ON p.id = pb.pedido_id
                        WHERE p.id = @pedidoId", connection, transaction);

                    commandVerificar.Parameters.AddWithValue("@pedidoId", pedidoId);
                    using var reader = await commandVerificar.ExecuteReaderAsync();
                    
                    if (!await reader.ReadAsync())
                        throw new Exception("Pedido no encontrado");

                    var estadoActual = reader.GetString("estado");
                    var estadoBodega = reader.IsDBNull(reader.GetOrdinal("estado_bodega")) ? null : reader.GetString("estado_bodega");
                    await reader.CloseAsync();

                    // Validar que se pueda cambiar al nuevo estado
                    if (estado.ToLower() == "entregado" && estadoBodega != "preparado")
                        throw new Exception("El pedido debe estar preparado por el bodeguero antes de marcarlo como entregado");
                    
                    if (estado.ToLower() == "en_entrega" && estadoBodega != "preparado")
                        throw new Exception("El pedido debe estar preparado por el bodeguero antes de marcarlo como en_entrega");

                    // Actualizar estado del pedido
                    using var commandPedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = @estado 
                        WHERE id = @pedidoId", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@estado", estado);
                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandPedido.ExecuteNonQueryAsync();

                    // Actualizar estado en pedidos_vendedor según el nuevo estado
                    using var commandPedidoVendedor = new MySqlCommand(@"
                        UPDATE pedidos_vendedor 
                        SET estado = CASE 
                            WHEN @estado = 'entregado' THEN 'completado'
                            WHEN @estado = 'en_entrega' THEN 'en_entrega'
                            WHEN @estado = 'cancelado' THEN 'cancelado'
                            ELSE estado
                        END
                        WHERE pedido_id = @pedidoId", connection, transaction);

                    commandPedidoVendedor.Parameters.AddWithValue("@estado", estado);
                    commandPedidoVendedor.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandPedidoVendedor.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();

                    // Obtener el pedido actualizado
                    using var commandGetPedido = new MySqlCommand(@"
                        SELECT * FROM pedidos WHERE id = @pedidoId", connection);

                    commandGetPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    using var readerPedido = await commandGetPedido.ExecuteReaderAsync();
                    
                    if (await readerPedido.ReadAsync())
                    {
                        return new Pedido
                        {
                            Id = readerPedido.GetInt32("id"),
                            UsuarioId = readerPedido.GetInt32("usuario_id"),
                            FechaPedido = readerPedido.GetDateTime("fecha_pedido"),
                            Estado = readerPedido.GetString("estado"),
                            TipoEntrega = readerPedido.GetString("tipo_entrega"),
                            Subtotal = readerPedido.GetDecimal("subtotal"),
                            CostoEnvio = readerPedido.GetDecimal("costo_envio"),
                            Impuestos = readerPedido.GetDecimal("impuestos"),
                            Total = readerPedido.GetDecimal("total"),
                            Notas = readerPedido.IsDBNull(readerPedido.GetOrdinal("notas")) ? null : readerPedido.GetString("notas")
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
    }
} 