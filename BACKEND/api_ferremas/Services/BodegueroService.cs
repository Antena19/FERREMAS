using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;

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
                    SELECT 
                        i.id,
                        i.producto_id,
                        i.sucursal_id,
                        i.stock,
                        i.stock_minimo,
                        i.ultimo_ingreso,
                        i.ultima_salida,
                        p.codigo,
                        p.nombre,
                        p.descripcion,
                        p.marca_id,
                        m.nombre as nombre_marca,
                        s.nombre as nombre_sucursal,
                        s.direccion as direccion_sucursal,
                        s.comuna as comuna_sucursal,
                        s.region as region_sucursal
                    FROM inventario i 
                    INNER JOIN productos p ON i.producto_id = p.id
                    INNER JOIN marcas m ON p.marca_id = m.id
                    INNER JOIN sucursales s ON i.sucursal_id = s.id
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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : (DateTime?)reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : (DateTime?)reader.GetDateTime("ultima_salida"),
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
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("sucursal_id"),
                            Nombre = reader.GetString("nombre_sucursal"),
                            Direccion = reader.GetString("direccion_sucursal"),
                            Comuna = reader.GetString("comuna_sucursal"),
                            Region = reader.GetString("region_sucursal")
                        }
                    });
                }
            }
            return inventario;
        }

        public async Task<IEnumerable<Inventario>> GetAllInventario()
        {
            var inventario = new List<Inventario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT 
                        i.id,
                        i.producto_id,
                        i.sucursal_id,
                        i.stock,
                        i.stock_minimo,
                        i.ultimo_ingreso,
                        i.ultima_salida,
                        p.codigo,
                        p.nombre,
                        p.descripcion,
                        p.precio,
                        p.categoria_id,
                        p.marca_id,
                        p.imagen_url,
                        p.especificaciones,
                        p.fecha_creacion,
                        p.fecha_modificacion,
                        p.activo as producto_activo,
                        m.id as marca_id,
                        m.nombre as nombre_marca,
                        m.descripcion as marca_descripcion,
                        m.logo_url,
                        m.activo as marca_activo,
                        s.nombre as nombre_sucursal,
                        s.direccion as direccion_sucursal,
                        s.comuna as comuna_sucursal,
                        s.region as region_sucursal,
                        s.telefono,
                        s.es_principal,
                        s.activo as sucursal_activo
                    FROM inventario i 
                    INNER JOIN productos p ON i.producto_id = p.id
                    INNER JOIN marcas m ON p.marca_id = m.id
                    INNER JOIN sucursales s ON i.sucursal_id = s.id
                    ORDER BY s.nombre, p.nombre", connection);
                
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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : (DateTime?)reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : (DateTime?)reader.GetDateTime("ultima_salida"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Codigo = reader.GetString("codigo"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio"),
                            CategoriaId = reader.IsDBNull(reader.GetOrdinal("categoria_id")) ? null : (int?)reader.GetInt32("categoria_id"),
                            MarcaId = reader.GetInt32("marca_id"),
                            ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen_url")) ? null : reader.GetString("imagen_url"),
                            Especificaciones = reader.IsDBNull(reader.GetOrdinal("especificaciones")) ? null : reader.GetString("especificaciones"),
                            FechaCreacion = reader.GetDateTime("fecha_creacion"),
                            FechaModificacion = reader.IsDBNull(reader.GetOrdinal("fecha_modificacion")) ? null : (DateTime?)reader.GetDateTime("fecha_modificacion"),
                            Activo = reader.GetBoolean("producto_activo"),
                            Marca = new Marca
                            {
                                Id = reader.GetInt32("marca_id"),
                                Nombre = reader.GetString("nombre_marca"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("marca_descripcion")) ? null : reader.GetString("marca_descripcion"),
                                LogoUrl = reader.IsDBNull(reader.GetOrdinal("logo_url")) ? null : reader.GetString("logo_url"),
                                Activo = reader.GetBoolean("marca_activo")
                            }
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("sucursal_id"),
                            Nombre = reader.GetString("nombre_sucursal"),
                            Direccion = reader.GetString("direccion_sucursal"),
                            Comuna = reader.GetString("comuna_sucursal"),
                            Region = reader.GetString("region_sucursal"),
                            Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                            EsPrincipal = reader.GetBoolean("es_principal"),
                            Activo = reader.GetBoolean("sucursal_activo")
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
                    SELECT 
                        pb.id,
                        pb.pedido_id,
                        pb.vendedor_id,
                        pb.bodeguero_id,
                        pb.fecha_creacion,
                        pb.estado as estado_pedido_bodega,
                        pb.fecha_preparacion,
                        pb.notas,
                        p.id as pedido_id,
                        p.usuario_id,
                        p.fecha_pedido,
                        p.estado as estado_pedido,
                        p.tipo_entrega,
                        p.sucursal_id,
                        p.direccion_id,
                        p.subtotal,
                        p.costo_envio,
                        p.impuestos,
                        p.total,
                        p.notas as notas_pedido,
                        p.vendedor_id as pedido_vendedor_id,
                        p.bodeguero_id as pedido_bodeguero_id,
                        s.nombre as nombre_sucursal,
                        s.direccion as direccion_sucursal,
                        s.comuna as comuna_sucursal,
                        s.region as region_sucursal,
                        pi.id as item_id,
                        pi.producto_id,
                        pi.cantidad,
                        pi.precio_unitario,
                        pi.subtotal as item_subtotal,
                        pr.nombre as nombre_producto,
                        pr.codigo as codigo_producto,
                        pr.descripcion as descripcion_producto,
                        pr.precio as precio_producto,
                        pr.categoria_id,
                        pr.marca_id,
                        pr.imagen_url,
                        pr.especificaciones,
                        pr.fecha_creacion as fecha_creacion_producto,
                        pr.fecha_modificacion,
                        pr.activo as producto_activo,
                        m.id as marca_id,
                        m.nombre as nombre_marca,
                        m.descripcion as marca_descripcion,
                        m.logo_url,
                        m.activo as marca_activo
                    FROM pedidos_bodega pb
                    INNER JOIN pedidos p ON pb.pedido_id = p.id
                    INNER JOIN sucursales s ON p.sucursal_id = s.id
                    INNER JOIN pedido_items pi ON p.id = pi.pedido_id
                    INNER JOIN productos pr ON pi.producto_id = pr.id
                    INNER JOIN marcas m ON pr.marca_id = m.id
                    WHERE pb.bodeguero_id = @bodegueroId AND pb.estado != 'entregado'
                    ORDER BY pb.fecha_creacion DESC", connection);
                
                command.Parameters.AddWithValue("@bodegueroId", bodegueroId);
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var pedidoBodega = new PedidoBodega
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        VendedorId = reader.GetInt32("vendedor_id"),
                        BodegueroId = reader.GetInt32("bodeguero_id"),
                        Estado = reader.GetString("estado_pedido_bodega"),
                        FechaCreacion = reader.GetDateTime("fecha_creacion"),
                        FechaPreparacion = reader.IsDBNull(reader.GetOrdinal("fecha_preparacion")) ? null : reader.GetDateTime("fecha_preparacion"),
                        Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            UsuarioId = reader.GetInt32("usuario_id"),
                            FechaPedido = reader.GetDateTime("fecha_pedido"),
                            Estado = reader.GetString("estado_pedido"),
                            TipoEntrega = reader.GetString("tipo_entrega"),
                            SucursalId = reader.GetInt32("sucursal_id"),
                            DireccionId = reader.IsDBNull(reader.GetOrdinal("direccion_id")) ? null : (int?)reader.GetInt32("direccion_id"),
                            Subtotal = reader.GetDecimal("subtotal"),
                            CostoEnvio = reader.GetDecimal("costo_envio"),
                            Impuestos = reader.GetDecimal("impuestos"),
                            Total = reader.GetDecimal("total"),
                            Notas = reader.IsDBNull(reader.GetOrdinal("notas_pedido")) ? null : reader.GetString("notas_pedido"),
                            VendedorId = reader.IsDBNull(reader.GetOrdinal("pedido_vendedor_id")) ? null : (int?)reader.GetInt32("pedido_vendedor_id"),
                            BodegueroId = reader.IsDBNull(reader.GetOrdinal("pedido_bodeguero_id")) ? null : (int?)reader.GetInt32("pedido_bodeguero_id"),
                            Sucursal = new Sucursal
                            {
                                Id = reader.GetInt32("sucursal_id"),
                                Nombre = reader.GetString("nombre_sucursal"),
                                Direccion = reader.GetString("direccion_sucursal"),
                                Comuna = reader.GetString("comuna_sucursal"),
                                Region = reader.GetString("region_sucursal")
                            }
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
                                    Codigo = reader.GetString("codigo_producto"),
                                    Nombre = reader.GetString("nombre_producto"),
                                    Descripcion = reader.GetString("descripcion_producto"),
                                    Precio = reader.GetDecimal("precio_producto"),
                                    Marca = new Marca
                                    {
                                        Nombre = reader.GetString("nombre_marca")
                                    }
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
                    SELECT 
                        pb.id,
                        pb.pedido_id,
                        pb.vendedor_id,
                        pb.bodeguero_id,
                        pb.fecha_creacion,
                        pb.estado as estado_pedido_bodega,
                        pb.fecha_preparacion,
                        pb.notas,
                        p.estado,
                        p.tipo_entrega,
                        p.sucursal_id,
                        pi.id as item_id,
                        pi.producto_id,
                        pi.cantidad,
                        pr.nombre as nombre_producto,
                        pr.codigo as codigo_producto,
                        pr.descripcion as descripcion_producto,
                        pr.precio as precio_producto,
                        m.nombre as nombre_marca
                    FROM pedidos_bodega pb
                    INNER JOIN pedidos p ON pb.pedido_id = p.id
                    INNER JOIN pedido_items pi ON p.id = pi.pedido_id
                    INNER JOIN productos pr ON pi.producto_id = pr.id
                    INNER JOIN marcas m ON pr.marca_id = m.id
                    WHERE pb.id = @pedidoBodegaId", connection);
                
                command.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                
                var pedidoBodega = new PedidoBodega();
                var items = new List<ItemPedidoBodega>();
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (pedidoBodega.Id == 0) // Primera fila
                    {
                        pedidoBodega = new PedidoBodega
                        {
                            Id = reader.GetInt32("id"),
                            PedidoId = reader.GetInt32("pedido_id"),
                            VendedorId = reader.GetInt32("vendedor_id"),
                            BodegueroId = reader.GetInt32("bodeguero_id"),
                            Estado = reader.GetString("estado_pedido_bodega"),
                            FechaCreacion = reader.GetDateTime("fecha_creacion"),
                            FechaPreparacion = reader.IsDBNull(reader.GetOrdinal("fecha_preparacion")) ? null : reader.GetDateTime("fecha_preparacion"),
                            Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                            Pedido = new Pedido
                            {
                                Id = reader.GetInt32("pedido_id"),
                                Estado = reader.GetString("estado"),
                                TipoEntrega = reader.GetString("tipo_entrega"),
                                SucursalId = reader.GetInt32("sucursal_id")
                            }
                        };
                    }

                    items.Add(new ItemPedidoBodega
                    {
                        Id = reader.GetInt32("item_id"),
                        ProductoId = reader.GetInt32("producto_id"),
                        Cantidad = reader.GetInt32("cantidad"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Codigo = reader.GetString("codigo_producto"),
                            Nombre = reader.GetString("nombre_producto"),
                            Descripcion = reader.GetString("descripcion_producto"),
                            Precio = reader.GetDecimal("precio_producto"),
                            Marca = new Marca
                            {
                                Nombre = reader.GetString("nombre_marca")
                            }
                        }
                    });
                }

                if (pedidoBodega.Id != 0)
                {
                    pedidoBodega.Items = items;
                    return pedidoBodega;
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

                    // Verificar que todos los items estén preparados
                    using var commandVerificarItems = new MySqlCommand(@"
                        SELECT COUNT(*) as total, 
                               SUM(CASE WHEN estado = 'preparado' THEN 1 ELSE 0 END) as preparados
                        FROM items_pedido_bodega
                        WHERE pedido_bodega_id = @pedidoBodegaId", connection, transaction);

                    commandVerificarItems.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    using var readerItems = await commandVerificarItems.ExecuteReaderAsync();
                    if (!await readerItems.ReadAsync())
                        throw new Exception("Error al verificar items del pedido");

                    var totalItems = readerItems.GetInt32("total");
                    var itemsPreparados = readerItems.GetInt32("preparados");
                    await readerItems.CloseAsync();

                    if (totalItems != itemsPreparados)
                        throw new Exception("No todos los items del pedido están preparados");

                    // Crear entrega bodega
                    using var commandEntrega = new MySqlCommand(@"
                        INSERT INTO entregas_bodega (
                            pedido_bodega_id, 
                            fecha_entrega, 
                            estado, 
                            tipo_entrega
                        )
                        VALUES (
                            @pedidoBodegaId, 
                            NOW(), 
                            'preparada', 
                            @tipoEntrega
                        );
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    commandEntrega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    commandEntrega.Parameters.AddWithValue("@tipoEntrega", pedidoBodega.Pedido.TipoEntrega);

                    var entregaId = Convert.ToInt32(await commandEntrega.ExecuteScalarAsync());

                    // Actualizar estado del pedido bodega
                    using var commandPedidoBodega = new MySqlCommand(@"
                        UPDATE pedidos_bodega 
                        SET estado = 'preparado',
                            fecha_preparacion = NOW()
                        WHERE id = @pedidoBodegaId", connection, transaction);

                    commandPedidoBodega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                    await commandPedidoBodega.ExecuteNonQueryAsync();

                    // Actualizar estado del pedido según tipo de entrega
                    using var commandPedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = CASE 
                            WHEN tipo_entrega = 'envio' THEN 'en_entrega'
                            ELSE 'entregado'
                        END
                        WHERE id = @pedidoId", connection, transaction);

                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoBodega.PedidoId);
                    await commandPedido.ExecuteNonQueryAsync();

                    // Actualizar estado en pedidos_vendedor
                    using var commandPedidoVendedor = new MySqlCommand(@"
                        UPDATE pedidos_vendedor 
                        SET estado = 'completado' 
                        WHERE pedido_id = @pedidoId", connection, transaction);

                    commandPedidoVendedor.Parameters.AddWithValue("@pedidoId", pedidoBodega.PedidoId);
                    await commandPedidoVendedor.ExecuteNonQueryAsync();

                    // Actualizar inventario
                    foreach (var item in pedidoBodega.Items)
                    {
                        using var commandInventario = new MySqlCommand(@"
                            UPDATE inventario 
                            SET stock = stock - @cantidad 
                            WHERE producto_id = @productoId 
                            AND sucursal_id = @sucursalId", connection, transaction);

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

        public async Task<PedidoBodega> ActualizarEstadoPedidoBodega(int pedidoBodegaId, string estado)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Primero obtenemos el pedido_id y la sucursal_id
                    int pedidoId;
                    int sucursalId;
                    using (var commandGetPedido = new MySqlCommand(@"
                        SELECT pb.pedido_id, p.sucursal_id 
                        FROM pedidos_bodega pb
                        JOIN pedidos p ON p.id = pb.pedido_id
                        WHERE pb.id = @pedidoBodegaId", connection, transaction))
                    {
                        commandGetPedido.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                        using var reader = await commandGetPedido.ExecuteReaderAsync();
                        if (!await reader.ReadAsync())
                        {
                            await transaction.RollbackAsync();
                            return null;
                        }
                        pedidoId = reader.GetInt32("pedido_id");
                        sucursalId = reader.GetInt32("sucursal_id");
                    }
                    
                    // Actualizamos el estado en pedidos_bodega
                    using (var commandUpdateBodega = new MySqlCommand(@"
                        UPDATE pedidos_bodega 
                        SET estado = @estado,
                            fecha_preparacion = CASE WHEN @estado = 'preparado' THEN NOW() ELSE fecha_preparacion END
                        WHERE id = @pedidoBodegaId", connection, transaction))
                    {
                        commandUpdateBodega.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                        commandUpdateBodega.Parameters.AddWithValue("@estado", estado);
                        await commandUpdateBodega.ExecuteNonQueryAsync();
                    }

                    // Si el estado es 'preparado', actualizamos también el estado en la tabla pedidos y el stock
                    if (estado == "preparado")
                    {
                        // Actualizamos el estado en pedidos
                        using (var commandUpdatePedido = new MySqlCommand(@"
                            UPDATE pedidos 
                            SET estado = 'preparado'
                            WHERE id = @pedidoId", connection, transaction))
                        {
                            commandUpdatePedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                            await commandUpdatePedido.ExecuteNonQueryAsync();
                        }

                        // Actualizamos el stock de los productos
                        using (var commandUpdateStock = new MySqlCommand(@"
                            UPDATE inventario i
                            INNER JOIN pedido_items pi ON i.producto_id = pi.producto_id
                            INNER JOIN pedidos_bodega pb ON pb.pedido_id = pi.pedido_id
                            SET i.stock = i.stock - pi.cantidad,
                                i.ultima_salida = NOW()
                            WHERE pb.id = @pedidoBodegaId
                            AND i.sucursal_id = @sucursalId", connection, transaction))
                        {
                            commandUpdateStock.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                            commandUpdateStock.Parameters.AddWithValue("@sucursalId", sucursalId);
                            await commandUpdateStock.ExecuteNonQueryAsync();
                        }
                    }

                    // Obtenemos el pedido actualizado
                    PedidoBodega pedidoBodega = null;
                    using (var commandGetUpdated = new MySqlCommand(@"
                        SELECT * FROM pedidos_bodega WHERE id = @pedidoBodegaId", connection, transaction))
                    {
                        commandGetUpdated.Parameters.AddWithValue("@pedidoBodegaId", pedidoBodegaId);
                        using var reader = await commandGetUpdated.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            pedidoBodega = new PedidoBodega
                            {
                                Id = reader.GetInt32("id"),
                                PedidoId = reader.GetInt32("pedido_id"),
                                VendedorId = reader.GetInt32("vendedor_id"),
                                BodegueroId = reader.GetInt32("bodeguero_id"),
                                Estado = reader.GetString("estado"),
                                FechaCreacion = reader.GetDateTime("fecha_creacion"),
                                FechaPreparacion = reader.IsDBNull(reader.GetOrdinal("fecha_preparacion")) ? null : reader.GetDateTime("fecha_preparacion"),
                                Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas")
                            };
                        }
                    }

                    if (pedidoBodega != null)
                    {
                        await transaction.CommitAsync();
                        return pedidoBodega;
                    }

                    await transaction.RollbackAsync();
                    return null;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Inventario> ActualizarStockInventario(int sucursalId, int productoId, int cantidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE inventario 
                    SET stock = stock + @cantidad,
                        ultima_salida = CASE WHEN @cantidad < 0 THEN NOW() ELSE ultima_salida END,
                        ultimo_ingreso = CASE WHEN @cantidad > 0 THEN NOW() ELSE ultimo_ingreso END
                    WHERE sucursal_id = @sucursalId 
                    AND producto_id = @productoId;
                    SELECT * FROM inventario 
                    WHERE sucursal_id = @sucursalId 
                    AND producto_id = @productoId", connection);
                
                command.Parameters.AddWithValue("@sucursalId", sucursalId);
                command.Parameters.AddWithValue("@productoId", productoId);
                command.Parameters.AddWithValue("@cantidad", cantidad);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Inventario
                    {
                        Id = reader.GetInt32("id"),
                        ProductoId = reader.GetInt32("producto_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Stock = reader.GetInt32("stock"),
                        StockMinimo = reader.GetInt32("stock_minimo"),
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : (DateTime?)reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : (DateTime?)reader.GetDateTime("ultima_salida")
                    };
                }
            }
            return null;
        }
    }
} 