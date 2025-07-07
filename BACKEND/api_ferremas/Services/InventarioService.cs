using Ferremas.Api.Modelos;
using MySql.Data.MySqlClient;
using System.Data;

namespace Ferremas.Api.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly string _connectionString;

        public InventarioService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Inventario>> GetInventarioBySucursal(int sucursalId)
        {
            var inventario = new List<Inventario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT i.*, p.* 
                    FROM inventario i
                    INNER JOIN productos p ON i.producto_id = p.id
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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio"),
                            ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen_url")) ? null : reader.GetString("imagen_url")
                        }
                    });
                }
            }
            return inventario;
        }

        public async Task<Inventario> GetInventarioByProducto(int productoId, int sucursalId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT i.*, p.* 
                    FROM inventario i
                    INNER JOIN productos p ON i.producto_id = p.id
                    WHERE i.producto_id = @productoId AND i.sucursal_id = @sucursalId", connection);

                command.Parameters.AddWithValue("@productoId", productoId);
                command.Parameters.AddWithValue("@sucursalId", sucursalId);

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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio"),
                            ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen_url")) ? null : reader.GetString("imagen_url")
                        }
                    };
                }
            }
            return null;
        }

        public async Task<Inventario> UpdateStock(int inventarioId, int cantidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE inventario 
                    SET stock = @cantidad,
                        ultima_salida = CASE WHEN @cantidad < stock THEN NOW() ELSE ultima_salida END,
                        ultimo_ingreso = CASE WHEN @cantidad > stock THEN NOW() ELSE ultimo_ingreso END
                    WHERE id = @inventarioId;
                    SELECT * FROM inventario WHERE id = @inventarioId", connection);

                command.Parameters.AddWithValue("@cantidad", cantidad);
                command.Parameters.AddWithValue("@inventarioId", inventarioId);

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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida")
                    };
                }
            }
            return null;
        }

        public async Task<Inventario> UpdatePrecios(int inventarioId, decimal precioCompra, decimal precioVenta)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE inventario 
                    SET precio_compra = @precioCompra,
                        precio_venta = @precioVenta
                    WHERE id = @inventarioId;
                    SELECT * FROM inventario WHERE id = @inventarioId", connection);

                command.Parameters.AddWithValue("@precioCompra", precioCompra);
                command.Parameters.AddWithValue("@precioVenta", precioVenta);
                command.Parameters.AddWithValue("@inventarioId", inventarioId);

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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida")
                    };
                }
            }
            return null;
        }

        public async Task<IEnumerable<Inventario>> GetProductosBajoStock(int sucursalId)
        {
            var inventario = new List<Inventario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT i.*, p.* 
                    FROM inventario i
                    INNER JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @sucursalId 
                    AND i.stock <= i.stock_minimo", connection);

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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio"),
                            ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen_url")) ? null : reader.GetString("imagen_url")
                        }
                    });
                }
            }
            return inventario;
        }

        public async Task<IEnumerable<Inventario>> GetProductosSobreStock(int sucursalId)
        {
            var inventario = new List<Inventario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT i.*, p.* 
                    FROM inventario i
                    INNER JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @sucursalId 
                    AND i.stock >= i.stock_maximo", connection);

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
                        UltimoIngreso = reader.IsDBNull(reader.GetOrdinal("ultimo_ingreso")) ? null : reader.GetDateTime("ultimo_ingreso"),
                        UltimaSalida = reader.IsDBNull(reader.GetOrdinal("ultima_salida")) ? null : reader.GetDateTime("ultima_salida"),
                        Producto = new Producto
                        {
                            Id = reader.GetInt32("producto_id"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion"),
                            Precio = reader.GetDecimal("precio"),
                            ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen_url")) ? null : reader.GetString("imagen_url")
                        }
                    });
                }
            }
            return inventario;
        }
    }
} 