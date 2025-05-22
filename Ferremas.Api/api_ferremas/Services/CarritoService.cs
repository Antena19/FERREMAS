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

                        // Obtener o crear carrito
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

                        transaction.Commit();

                        Console.WriteLine("[AgregarItem] Obteniendo carrito actualizado...");
                        return await ObtenerCarrito(usuarioId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AgregarItem] ERROR: {ex.Message}");
                        transaction.Rollback();
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

        #region Métodos Privados

        private async Task<Carrito> ObtenerCarritoActivo(MySqlConnection connection, int usuarioId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"SELECT Id, usuario_id, FechaCreacion, FechaActualizacion, Subtotal, Impuestos, Descuentos, Total 
                  FROM Carritos 
                  WHERE usuario_id = @UsuarioId AND Activo = 1",
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
                        FechaCreacion = reader.GetDateTime(2),
                        FechaActualizacion = reader.IsDBNull(3) ? null : (DateTime?)reader.GetDateTime(3),
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
            var command = new MySqlCommand(
                @"INSERT INTO Carritos (usuario_id, FechaCreacion, Activo) 
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
                @"SELECT i.Id, i.CarritoId, i.ProductoId, i.Cantidad, i.PrecioUnitario, i.Subtotal,
                         p.Nombre, p.Descripcion, p.Precio, p.Stock, p.ImagenUrl
                  FROM ItemsCarrito i
                  INNER JOIN Productos p ON i.ProductoId = p.Id
                  WHERE i.CarritoId = @CarritoId",
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
                            ImagenUrl = reader.GetString(10)
                        }
                    });
                }
            }

            return items;
        }

        private async Task<ItemCarrito> ObtenerItemCarrito(MySqlConnection connection, int carritoId, int productoId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                @"SELECT Id, CarritoId, ProductoId, Cantidad, PrecioUnitario, Subtotal
                  FROM ItemsCarrito
                  WHERE CarritoId = @CarritoId AND ProductoId = @ProductoId",
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
                @"SELECT Id, CarritoId, ProductoId, Cantidad, PrecioUnitario, Subtotal
                  FROM ItemsCarrito
                  WHERE Id = @ItemId",
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
                @"INSERT INTO ItemsCarrito (CarritoId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
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
                @"UPDATE ItemsCarrito 
                  SET Cantidad = @Cantidad, Subtotal = @Subtotal
                  WHERE Id = @Id",
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
                "DELETE FROM ItemsCarrito WHERE Id = @Id",
                connection,
                transaction);

            command.Parameters.AddWithValue("@Id", itemId);

            await command.ExecuteNonQueryAsync();
        }

        private async Task EliminarItemsCarrito(MySqlConnection connection, int carritoId, MySqlTransaction transaction = null)
        {
            var command = new MySqlCommand(
                "DELETE FROM ItemsCarrito WHERE CarritoId = @CarritoId",
                connection,
                transaction);

            command.Parameters.AddWithValue("@CarritoId", carritoId);

            await command.ExecuteNonQueryAsync();
        }

        #endregion
    }
} 