using System;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Ferremas.Api.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace Ferremas.Api.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly string _connectionString;
        private readonly IProductoService _productoService;

        public CarritoService(IConfiguration configuration, IProductoService productoService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _productoService = productoService;
        }

        public async Task<Carrito> ObtenerCarrito(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Obtener el carrito
                var carrito = await ObtenerCarritoActivo(connection, usuarioId);
                if (carrito == null)
                {
                    // Crear un nuevo carrito si no existe
                    carrito = await CrearCarrito(connection, usuarioId);
                }

                // Obtener los items del carrito
                carrito.Items = await ObtenerItemsCarrito(connection, carrito.Id);

                // Calcular totales
                return await CalcularTotales(carrito);
            }
        }

        public async Task<Carrito> AgregarItem(int usuarioId, int productoId, int cantidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Console.WriteLine($"[AgregarItem] usuarioId={usuarioId}, productoId={productoId}, cantidad={cantidad}");

                        // Obtener o crear carrito activo
                        var carrito = await ObtenerCarritoActivo(connection, usuarioId, transaction);
                        if (carrito == null)
                        {
                            Console.WriteLine("[AgregarItem] Carrito no existe, creando uno nuevo...");
                            carrito = await CrearCarrito(connection, usuarioId, transaction);
                        }
                        Console.WriteLine($"[AgregarItem] CarritoId={carrito.Id}");

                        // Verificar si el producto ya existe en el carrito
                        var itemExistente = await ObtenerItemCarrito(connection, carrito.Id, productoId, transaction);
                        if (itemExistente != null)
                        {
                            Console.WriteLine("[AgregarItem] Item ya existe, actualizando cantidad...");
                            itemExistente.Cantidad += cantidad;
                            await ActualizarItemCarrito(connection, itemExistente, transaction);
                        }
                        else
                        {
                            Console.WriteLine("[AgregarItem] Item no existe, obteniendo producto...");
                            var producto = await _productoService.ObtenerPorIdAsync(productoId);
                            if (producto == null)
                            {
                                Console.WriteLine("[AgregarItem] Producto no encontrado");
                                throw new Exception("Producto no encontrado");
                            }

                            Console.WriteLine("[AgregarItem] Creando nuevo item...");
                            var nuevoItem = new ItemCarrito
                            {
                                CarritoId = carrito.Id,
                                ProductoId = productoId,
                                Cantidad = cantidad,
                                PrecioUnitario = producto.Precio,
                                Subtotal = producto.Precio * cantidad
                            };

                            await CrearItemCarrito(connection, nuevoItem, transaction);
                        }

                        // Actualizar totales del carrito
                        var updateCommand = new MySqlCommand(
                            @"UPDATE carritos
                              SET subtotal = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId),
                                  impuestos = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 0.19,
                                  total = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 1.19
                              WHERE id = @CarritoId",
                            connection,
                            transaction);
                        updateCommand.Parameters.AddWithValue("@CarritoId", carrito.Id);
                        await updateCommand.ExecuteNonQueryAsync();

                        transaction.Commit();

                        Console.WriteLine("[AgregarItem] Obteniendo carrito actualizado...");
                        return await ObtenerCarrito(usuarioId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AgregarItem] ERROR: {ex.Message}");
                        if (transaction.Connection != null)
                        {
                            try { transaction.Rollback(); } catch { }
                        }
                        throw;
                    }
                }
            }
        }

        public async Task<Carrito> ActualizarCantidad(int usuarioId, int itemId, int cantidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var carrito = await ObtenerCarritoActivo(connection, usuarioId, transaction);
                        if (carrito == null)
                        {
                            throw new Exception("Carrito no encontrado");
                        }

                        var item = await ObtenerItemCarritoPorId(connection, itemId, transaction);
                        if (item == null || item.CarritoId != carrito.Id)
                        {
                            throw new Exception("Item no encontrado en el carrito");
                        }

                        item.Cantidad = cantidad;
                        item.Subtotal = item.PrecioUnitario * cantidad;

                        await ActualizarItemCarrito(connection, item, transaction);

                        // Actualizar totales del carrito
                        var updateCommand = new MySqlCommand(
                            @"UPDATE carritos
                              SET subtotal = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId),
                                  impuestos = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 0.19,
                                  total = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 1.19
                              WHERE id = @CarritoId",
                            connection,
                            transaction);
                        updateCommand.Parameters.AddWithValue("@CarritoId", carrito.Id);
                        await updateCommand.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return await ObtenerCarrito(usuarioId);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<Carrito> EliminarItem(int usuarioId, int itemId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var carrito = await ObtenerCarritoActivo(connection, usuarioId, transaction);
                        if (carrito == null)
                        {
                            throw new Exception("Carrito no encontrado");
                        }

                        var item = await ObtenerItemCarritoPorId(connection, itemId, transaction);
                        if (item == null || item.CarritoId != carrito.Id)
                        {
                            throw new Exception("Item no encontrado en el carrito");
                        }

                        await EliminarItemCarrito(connection, itemId, transaction);

                        // Actualizar totales del carrito
                        var updateCommand = new MySqlCommand(
                            @"UPDATE carritos
                              SET subtotal = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId),
                                  impuestos = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 0.19,
                                  total = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 1.19
                              WHERE id = @CarritoId",
                            connection,
                            transaction);
                        updateCommand.Parameters.AddWithValue("@CarritoId", carrito.Id);
                        await updateCommand.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return await ObtenerCarrito(usuarioId);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task VaciarCarrito(int usuarioId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var carrito = await ObtenerCarritoActivo(connection, usuarioId, transaction);
                        if (carrito != null)
                        {
                            await EliminarItemsCarrito(connection, carrito.Id, transaction);

                            // Actualizar totales del carrito
                            var updateCommand = new MySqlCommand(
                                @"UPDATE carritos
                                  SET subtotal = 0, impuestos = 0, descuentos = 0, total = 0
                                  WHERE id = @CarritoId",
                                connection,
                                transaction);
                            updateCommand.Parameters.AddWithValue("@CarritoId", carrito.Id);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<Carrito> CalcularTotales(Carrito carrito)
        {
            carrito.Subtotal = carrito.Items.Sum(i => i.Subtotal);
            carrito.Impuestos = carrito.Subtotal * 0.19m; // 19% IVA
            carrito.Descuentos = 0; // Implementar lógica de descuentos si es necesario
            carrito.Total = carrito.Subtotal + carrito.Impuestos - carrito.Descuentos;

            return carrito;
        }

        public async Task<Carrito> SincronizarCarrito(int usuarioId, List<ItemSincronizarDTO> items)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Obtener o crear carrito activo
                        var carrito = await ObtenerCarritoActivo(connection, usuarioId, transaction);
                        if (carrito == null)
                        {
                            carrito = await CrearCarrito(connection, usuarioId, transaction);
                        }
                        // Eliminar todos los items actuales
                        await EliminarItemsCarrito(connection, carrito.Id, transaction);
                        // Agregar los nuevos items
                        foreach (var item in items)
                        {
                            var producto = await _productoService.ObtenerPorIdAsync(item.ProductoId);
                            if (producto == null) continue;
                            var nuevoItem = new ItemCarrito
                            {
                                CarritoId = carrito.Id,
                                ProductoId = item.ProductoId,
                                Cantidad = item.Cantidad,
                                PrecioUnitario = producto.Precio,
                                Subtotal = producto.Precio * item.Cantidad
                            };
                            await CrearItemCarrito(connection, nuevoItem, transaction);
                        }
                        // Actualizar totales del carrito
                        var updateCommand = new MySqlCommand(
                            @"UPDATE carritos
                              SET subtotal = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId),
                                  impuestos = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 0.19,
                                  total = (SELECT COALESCE(SUM(subtotal),0) FROM items_carrito WHERE carrito_id = @CarritoId) * 1.19
                              WHERE id = @CarritoId",
                            connection,
                            transaction);
                        updateCommand.Parameters.AddWithValue("@CarritoId", carrito.Id);
                        await updateCommand.ExecuteNonQueryAsync();
                        transaction.Commit();
                        return await ObtenerCarrito(usuarioId);
                    }
                    catch (Exception ex)
                    {
                        if (transaction.Connection != null)
                        {
                            try { transaction.Rollback(); } catch { }
                        }
                        throw;
                    }
                }
            }
        }

        #region Métodos Privados

        private async Task<Carrito> ObtenerCarritoActivo(MySqlConnection connection, int usuarioId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"SELECT id, usuario_id, fecha_creacion, fecha_actualizacion, subtotal, impuestos, descuentos, total 
                  FROM carritos 
                  WHERE usuario_id = @UsuarioId AND activo = 1
                  ORDER BY id DESC
                  LIMIT 1
                  FOR UPDATE",
                connection,
                transaction);

            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Carrito
                    {
                        Id = reader.GetInt32(0),
                        UsuarioId = reader.GetInt32(1),
                        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                        FechaActualizacion = reader.IsDBNull(reader.GetOrdinal("fecha_actualizacion")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("fecha_actualizacion")),
                        Subtotal = reader.GetDecimal(4),
                        Impuestos = reader.GetDecimal(5),
                        Descuentos = reader.GetDecimal(6),
                        Total = reader.GetDecimal(7)
                    };
                }
            }

            return null;
        }

        private async Task<Carrito> CrearCarrito(MySqlConnection connection, int usuarioId, MySqlTransaction transaction = null)
        {
            // Verificar de nuevo si ya existe un carrito activo (condición de carrera)
            var carritoExistente = await ObtenerCarritoActivo(connection, usuarioId, transaction);
            if (carritoExistente != null)
                return carritoExistente;

            var command = new MySqlCommand(
                @"INSERT INTO carritos (usuario_id, fecha_creacion, activo) 
                  VALUES (@UsuarioId, @FechaCreacion, 1);
                  SELECT LAST_INSERT_ID();",
                connection,
                transaction);

            command.Parameters.AddWithValue("@UsuarioId", usuarioId);
            command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);

            var carritoId = Convert.ToInt32(await command.ExecuteScalarAsync());

            return new Carrito
            {
                Id = carritoId,
                UsuarioId = usuarioId,
                FechaCreacion = DateTime.Now,
                Items = new List<ItemCarrito>()
            };
        }

        private async Task<List<ItemCarrito>> ObtenerItemsCarrito(MySqlConnection connection, int carritoId, MySqlTransaction transaction = null)
        {
            var items = new List<ItemCarrito>();
            var command = new MySqlCommand(
                @"SELECT i.id, i.carrito_id, i.producto_id, i.cantidad, i.precio_unitario, i.subtotal,
                         p.nombre, p.descripcion, p.precio, p.imagen_url
                  FROM items_carrito i
                  INNER JOIN productos p ON i.producto_id = p.id
                  WHERE i.carrito_id = @CarritoId",
                connection,
                transaction);

            command.Parameters.AddWithValue("@CarritoId", carritoId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    items.Add(new ItemCarrito
                    {
                        Id = reader.GetInt32(0),
                        CarritoId = reader.GetInt32(1),
                        ProductoId = reader.GetInt32(2),
                        Cantidad = reader.GetInt32(3),
                        PrecioUnitario = reader.GetDecimal(4),
                        Subtotal = reader.GetDecimal(5),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32(2),
                            Nombre = reader.GetString(6),
                            Descripcion = reader.GetString(7),
                            Precio = reader.GetDecimal(8),
                            ImagenUrl = reader.GetString(9)
                        }
                    });
                }
            }

            return items;
        }

        private async Task<ItemCarrito> ObtenerItemCarrito(MySqlConnection connection, int carritoId, int productoId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"SELECT id, carrito_id, producto_id, cantidad, precio_unitario, subtotal
                  FROM items_carrito
                  WHERE carrito_id = @CarritoId AND producto_id = @ProductoId",
                connection,
                transaction);

            command.Parameters.AddWithValue("@CarritoId", carritoId);
            command.Parameters.AddWithValue("@ProductoId", productoId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new ItemCarrito
                    {
                        Id = reader.GetInt32(0),
                        CarritoId = reader.GetInt32(1),
                        ProductoId = reader.GetInt32(2),
                        Cantidad = reader.GetInt32(3),
                        PrecioUnitario = reader.GetDecimal(4),
                        Subtotal = reader.GetDecimal(5)
                    };
                }
            }

            return null;
        }

        private async Task<ItemCarrito> ObtenerItemCarritoPorId(MySqlConnection connection, int itemId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"SELECT id, carrito_id, producto_id, cantidad, precio_unitario, subtotal
                  FROM items_carrito
                  WHERE id = @ItemId",
                connection,
                transaction);

            command.Parameters.AddWithValue("@ItemId", itemId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new ItemCarrito
                    {
                        Id = reader.GetInt32(0),
                        CarritoId = reader.GetInt32(1),
                        ProductoId = reader.GetInt32(2),
                        Cantidad = reader.GetInt32(3),
                        PrecioUnitario = reader.GetDecimal(4),
                        Subtotal = reader.GetDecimal(5)
                    };
                }
            }

            return null;
        }

        private async Task CrearItemCarrito(MySqlConnection connection, ItemCarrito item, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"INSERT INTO items_carrito (carrito_id, producto_id, cantidad, precio_unitario, subtotal)
                  VALUES (@CarritoId, @ProductoId, @Cantidad, @PrecioUnitario, @Subtotal);
                  SELECT LAST_INSERT_ID();",
                connection,
                transaction);

            command.Parameters.AddWithValue("@CarritoId", item.CarritoId);
            command.Parameters.AddWithValue("@ProductoId", item.ProductoId);
            command.Parameters.AddWithValue("@Cantidad", item.Cantidad);
            command.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
            command.Parameters.AddWithValue("@Subtotal", item.Subtotal);

            item.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        private async Task ActualizarItemCarrito(MySqlConnection connection, ItemCarrito item, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"UPDATE items_carrito 
                  SET cantidad = @Cantidad, subtotal = @Subtotal
                  WHERE id = @Id",
                connection,
                transaction);

            command.Parameters.AddWithValue("@Id", item.Id);
            command.Parameters.AddWithValue("@Cantidad", item.Cantidad);
            command.Parameters.AddWithValue("@Subtotal", item.Subtotal);

            await command.ExecuteNonQueryAsync();
        }

        private async Task EliminarItemCarrito(MySqlConnection connection, int itemId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                "DELETE FROM items_carrito WHERE id = @Id",
                connection,
                transaction);

            command.Parameters.AddWithValue("@Id", itemId);

            await command.ExecuteNonQueryAsync();
        }

        private async Task EliminarItemsCarrito(MySqlConnection connection, int carritoId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                "DELETE FROM items_carrito WHERE carrito_id = @CarritoId",
                connection,
                transaction);

            command.Parameters.AddWithValue("@CarritoId", carritoId);

            await command.ExecuteNonQueryAsync();
        }

        #endregion
    }
} 