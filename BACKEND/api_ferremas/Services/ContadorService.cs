using Ferremas.Api.Modelos;
using MySql.Data.MySqlClient;
using System.Data;

namespace Ferremas.Api.Services
{
    public class ContadorService : IContadorService
    {
        private readonly string _connectionString;

        public ContadorService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Pago> AprobarPagoTransferencia(int pedidoId, int contadorId, string bancoOrigen, string numeroCuenta)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Obtener el pedido
                    using var commandPedido = new MySqlCommand(@"
                        SELECT * FROM pedidos WHERE id = @pedidoId", connection, transaction);
                    commandPedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    
                    using var readerPedido = await commandPedido.ExecuteReaderAsync();
                    if (!await readerPedido.ReadAsync())
                        throw new Exception("Pedido no encontrado");

                    var monto = readerPedido.GetDecimal("total");
                    await readerPedido.CloseAsync();

                    // Crear el pago
                    using var commandPago = new MySqlCommand(@"
                        INSERT INTO pagos (pedido_id, metodo, monto, estado, fecha_pago, referencia_transaccion, contador_id)
                        VALUES (@pedidoId, 'transferencia', @monto, 'completado', NOW(), @referencia, @contadorId);
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    var referencia = $"TRF-{bancoOrigen}-{numeroCuenta}-{DateTime.Now:yyyyMMddHHmmss}";
                    commandPago.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandPago.Parameters.AddWithValue("@monto", monto);
                    commandPago.Parameters.AddWithValue("@referencia", referencia);
                    commandPago.Parameters.AddWithValue("@contadorId", contadorId);

                    var pagoId = Convert.ToInt32(await commandPago.ExecuteScalarAsync());

                    // Actualizar estado del pedido
                    using var commandUpdatePedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = 'pagado' 
                        WHERE id = @pedidoId", connection, transaction);
                    commandUpdatePedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandUpdatePedido.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();

                    // Obtener el pago creado
                    return await GetPagoById(pagoId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Pago>> GetHistorialPagos(DateTime fechaInicio, DateTime fechaFin)
        {
            var pagos = new List<Pago>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, pe.* 
                    FROM pagos p
                    INNER JOIN pedidos pe ON p.pedido_id = pe.id
                    WHERE p.fecha_pago BETWEEN @fechaInicio AND @fechaFin
                    ORDER BY p.fecha_pago DESC", connection);

                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    pagos.Add(new Pago
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        Metodo = reader.GetString("metodo"),
                        Monto = reader.GetDecimal("monto"),
                        Estado = reader.GetString("estado"),
                        FechaPago = reader.GetDateTime("fecha_pago"),
                        ReferenciaTransaccion = reader.GetString("referencia_transaccion"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        }
                    });
                }
            }
            return pagos;
        }

        public async Task<IEnumerable<Pago>> GetPagosPendientes()
        {
            var pagos = new List<Pago>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, pe.* 
                    FROM pagos p
                    INNER JOIN pedidos pe ON p.pedido_id = pe.id
                    WHERE p.estado = 'pendiente'
                    ORDER BY p.fecha_pago DESC", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    pagos.Add(new Pago
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        Metodo = reader.GetString("metodo"),
                        Monto = reader.GetDecimal("monto"),
                        Estado = reader.GetString("estado"),
                        FechaPago = reader.GetDateTime("fecha_pago"),
                        ReferenciaTransaccion = reader.GetString("referencia_transaccion"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        }
                    });
                }
            }
            return pagos;
        }

        public async Task<Pago> GetPagoById(int pagoId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, pe.* 
                    FROM pagos p
                    INNER JOIN pedidos pe ON p.pedido_id = pe.id
                    WHERE p.id = @pagoId", connection);

                command.Parameters.AddWithValue("@pagoId", pagoId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Pago
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        Metodo = reader.GetString("metodo"),
                        Monto = reader.GetDecimal("monto"),
                        Estado = reader.GetString("estado"),
                        FechaPago = reader.GetDateTime("fecha_pago"),
                        ReferenciaTransaccion = reader.GetString("referencia_transaccion"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        }
                    };
                }
            }
            return null;
        }

        public async Task<IEnumerable<Pedido>> GetPedidosPendientesPago()
        {
            var pedidos = new List<Pedido>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT p.*, u.* 
                    FROM pedidos p
                    INNER JOIN usuarios u ON p.usuario_id = u.id
                    WHERE p.estado = 'pendiente_pago'
                    ORDER BY p.fecha_pedido DESC", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    pedidos.Add(new Pedido
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        Total = reader.GetDecimal("total"),
                        Estado = reader.GetString("estado"),
                        FechaPedido = reader.GetDateTime("fecha_pedido"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("usuario_id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido")
                        }
                    });
                }
            }
            return pedidos;
        }
    }
} 