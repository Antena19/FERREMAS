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

        public async Task<Pago> AprobarPagoTransferencia(int pedidoId, int contadorId, string bancoOrigen, string numeroCuenta, string estado, string notas)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Obtener el usuario_id del contador
                    using var commandContador = new MySqlCommand(@"
                        SELECT usuario_id 
                        FROM contadores 
                        WHERE id = @contadorId AND activo = 1", connection, transaction);
                    commandContador.Parameters.AddWithValue("@contadorId", contadorId);
                    
                    var usuarioId = await commandContador.ExecuteScalarAsync();
                    if (usuarioId == null)
                        throw new Exception("Contador no encontrado o inactivo");

                    // Obtener el pago pendiente
                    using var commandPago = new MySqlCommand(@"
                        SELECT * FROM pagos 
                        WHERE pedido_id = @pedidoId 
                        AND metodo = 'transferencia' 
                        AND estado = 'pendiente'", connection, transaction);
                    commandPago.Parameters.AddWithValue("@pedidoId", pedidoId);
                    
                    using var readerPago = await commandPago.ExecuteReaderAsync();
                    if (!await readerPago.ReadAsync())
                        throw new Exception("No se encontró un pago pendiente por transferencia para este pedido");

                    var pagoId = readerPago.GetInt32("id");
                    var monto = readerPago.GetDecimal("monto");
                    await readerPago.CloseAsync();

                    // Actualizar el pago
                    using var commandUpdatePago = new MySqlCommand(@"
                        UPDATE pagos 
                        SET estado = @estado,
                            contador_id = @usuarioId,
                            fecha_pago = NOW()
                        WHERE id = @pagoId", connection, transaction);

                    commandUpdatePago.Parameters.AddWithValue("@estado", estado);
                    commandUpdatePago.Parameters.AddWithValue("@usuarioId", usuarioId);
                    commandUpdatePago.Parameters.AddWithValue("@pagoId", pagoId);
                    await commandUpdatePago.ExecuteNonQueryAsync();

                    // Registrar la transferencia
                    using var commandTransferencia = new MySqlCommand(@"
                        INSERT INTO transferencias (
                            pedido_id,
                            contador_id,
                            monto,
                            banco_origen,
                            numero_cuenta,
                            fecha_transferencia,
                            estado,
                            notas
                        ) VALUES (
                            @pedidoId,
                            @contadorId,
                            @monto,
                            @bancoOrigen,
                            @numeroCuenta,
                            NOW(),
                            'confirmada',
                            @notas
                        )", connection, transaction);

                    commandTransferencia.Parameters.AddWithValue("@pedidoId", pedidoId);
                    commandTransferencia.Parameters.AddWithValue("@contadorId", contadorId);
                    commandTransferencia.Parameters.AddWithValue("@monto", monto);
                    commandTransferencia.Parameters.AddWithValue("@bancoOrigen", bancoOrigen);
                    commandTransferencia.Parameters.AddWithValue("@numeroCuenta", numeroCuenta);
                    commandTransferencia.Parameters.AddWithValue("@notas", notas);
                    await commandTransferencia.ExecuteNonQueryAsync();

                    // Actualizar estado del pedido
                    using var commandUpdatePedido = new MySqlCommand(@"
                        UPDATE pedidos 
                        SET estado = 'confirmado' 
                        WHERE id = @pedidoId", connection, transaction);

                    commandUpdatePedido.Parameters.AddWithValue("@pedidoId", pedidoId);
                    await commandUpdatePedido.ExecuteNonQueryAsync();

                    // Buscar vendedor disponible en la sucursal
                    using var commandVendedor = new MySqlCommand(@"
                        SELECT v.usuario_id 
                        FROM vendedores v
                        INNER JOIN pedidos p ON p.sucursal_id = v.sucursal_id
                        WHERE p.id = @pedidoId
                        AND v.activo = 1
                        ORDER BY RAND()
                        LIMIT 1", connection, transaction);

                    commandVendedor.Parameters.AddWithValue("@pedidoId", pedidoId);
                    var vendedorUsuarioId = await commandVendedor.ExecuteScalarAsync();

                    if (vendedorUsuarioId != null)
                    {
                        // Obtener el ID del vendedor
                        using var commandGetVendedorId = new MySqlCommand(@"
                            SELECT id 
                            FROM vendedores 
                            WHERE usuario_id = @usuarioId", connection, transaction);
                        commandGetVendedorId.Parameters.AddWithValue("@usuarioId", vendedorUsuarioId);
                        var vendedorId = await commandGetVendedorId.ExecuteScalarAsync();

                        // Asignar vendedor al pedido
                        using var commandAsignarVendedor = new MySqlCommand(@"
                            UPDATE pedidos 
                            SET vendedor_id = @vendedorUsuarioId,
                                estado = 'asignado_vendedor'
                            WHERE id = @pedidoId", connection, transaction);

                        commandAsignarVendedor.Parameters.AddWithValue("@vendedorUsuarioId", vendedorUsuarioId);
                        commandAsignarVendedor.Parameters.AddWithValue("@pedidoId", pedidoId);
                        await commandAsignarVendedor.ExecuteNonQueryAsync();

                        // Crear registro en pedidos_vendedor
                        using var commandPedidoVendedor = new MySqlCommand(@"
                            INSERT INTO pedidos_vendedor (
                                pedido_id, 
                                vendedor_id, 
                                fecha_asignacion, 
                                estado
                            ) VALUES (
                                @pedidoId,
                                @vendedorId,
                                NOW(),
                                'asignado'
                            )", connection, transaction);

                        commandPedidoVendedor.Parameters.AddWithValue("@pedidoId", pedidoId);
                        commandPedidoVendedor.Parameters.AddWithValue("@vendedorId", vendedorId);
                        await commandPedidoVendedor.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();

                    // Obtener el pago actualizado
                    using var commandGetPago = new MySqlCommand(@"
                        SELECT p.*, t.banco_origen, t.numero_cuenta, t.fecha_registro, t.notas
                        FROM pagos p
                        LEFT JOIN transferencias t ON p.id = t.pago_id
                        WHERE p.id = @pagoId", connection);

                    commandGetPago.Parameters.AddWithValue("@pagoId", pagoId);

                    using var readerPagoActualizado = await commandGetPago.ExecuteReaderAsync();
                    if (await readerPagoActualizado.ReadAsync())
                    {
                        return new Pago
                        {
                            Id = readerPagoActualizado.GetInt32("id"),
                            PedidoId = readerPagoActualizado.GetInt32("pedido_id"),
                            Metodo = readerPagoActualizado.GetString("metodo"),
                            Monto = readerPagoActualizado.GetDecimal("monto"),
                            Estado = readerPagoActualizado.GetString("estado"),
                            FechaPago = readerPagoActualizado.GetDateTime("fecha_pago"),
                            ContadorId = readerPagoActualizado.GetInt32("contador_id"),
                            Notas = readerPagoActualizado.IsDBNull(readerPagoActualizado.GetOrdinal("notas")) ? null : readerPagoActualizado.GetString("notas")
                        };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Error al aprobar la transferencia: {ex.Message}");
                }
            }
            return null;
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
                        Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                        UrlRetorno = reader.IsDBNull(reader.GetOrdinal("url_retorno")) ? null : reader.GetString("url_retorno"),
                        ContadorId = reader.IsDBNull(reader.GetOrdinal("contador_id")) ? null : reader.GetInt32("contador_id"),
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
                    SELECT p.*, pe.estado as estado_pedido 
                    FROM pagos p
                    INNER JOIN pedidos pe ON p.pedido_id = pe.id
                    WHERE p.estado = 'pendiente'
                    ORDER BY p.fecha_pago DESC", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var pago = new Pago
                    {
                        Id = reader.GetInt32("id"),
                        PedidoId = reader.GetInt32("pedido_id"),
                        Metodo = reader.GetString("metodo"),
                        Monto = reader.GetDecimal("monto"),
                        Estado = reader.GetString("estado"),
                        FechaPago = reader.IsDBNull(reader.GetOrdinal("fecha_pago")) ? null : reader.GetDateTime("fecha_pago"),
                        Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                        UrlRetorno = reader.IsDBNull(reader.GetOrdinal("url_retorno")) ? null : reader.GetString("url_retorno"),
                        ContadorId = reader.IsDBNull(reader.GetOrdinal("contador_id")) ? null : reader.GetInt32("contador_id"),
                        Pedido = new Pedido
                        {
                            Id = reader.GetInt32("pedido_id"),
                            Estado = reader.GetString("estado_pedido")
                        }
                    };
                    pagos.Add(pago);
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
                        Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? null : reader.GetString("notas"),
                        UrlRetorno = reader.IsDBNull(reader.GetOrdinal("url_retorno")) ? null : reader.GetString("url_retorno"),
                        ContadorId = reader.IsDBNull(reader.GetOrdinal("contador_id")) ? null : reader.GetInt32("contador_id"),
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