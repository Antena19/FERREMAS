using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Ferremas.Api.Repositories
{
    public class PagoRepository : IPagoRepository
    {
        private readonly string _connectionString;

        public PagoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Pago>> ObtenerTodosAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, ped.fecha_pedido as FechaPedido
                    FROM pagos p
                    JOIN pedidos ped ON p.pedido_id = ped.id
                    ORDER BY p.fecha_pago DESC";

                var pagos = await connection.QueryAsync<Pago>(sql);
                return pagos;
            }
        }

        public async Task<Pago> ObtenerPorIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, ped.fecha_pedido as FechaPedido
                    FROM pagos p
                    JOIN pedidos ped ON p.pedido_id = ped.id
                    WHERE p.id = @Id";

                var pago = await connection.QueryFirstOrDefaultAsync<Pago>(sql, new { Id = id });
                return pago;
            }
        }

        public async Task<IEnumerable<Pago>> ObtenerPorPedidoAsync(int pedidoId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, ped.fecha_pedido as FechaPedido
                    FROM pagos p
                    JOIN pedidos ped ON p.pedido_id = ped.id
                    WHERE p.pedido_id = @PedidoId
                    ORDER BY p.fecha_pago DESC";

                var pagos = await connection.QueryAsync<Pago>(sql, new { PedidoId = pedidoId });
                return pagos;
            }
        }

        public async Task<int> CrearPagoAsync(Pago pago)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO pagos (
                        pedido_id,
                        monto,
                        fecha_pago,
                        estado,
                        metodo,
                        webpay_token,
                        webpay_buyorder,
                        webpay_sessionid,
                        webpay_authorization_code,
                        webpay_payment_type_code,
                        webpay_response_code,
                        webpay_card_last_digits,
                        webpay_installments_number,
                        webpay_transaction_date,
                        webpay_status,
                        webpay_vci,
                        notas,
                        contador_id,
                        url_retorno
                    ) VALUES (
                        @PedidoId,
                        @Monto,
                        @FechaPago,
                        @Estado,
                        @Metodo,
                        @WebpayToken,
                        @WebpayBuyOrder,
                        @WebpaySessionId,
                        @WebpayAuthorizationCode,
                        @WebpayPaymentTypeCode,
                        @WebpayResponseCode,
                        @WebpayCardLastDigits,
                        @WebpayInstallmentsNumber,
                        @WebpayTransactionDate,
                        @WebpayStatus,
                        @WebpayVci,
                        @Notas,
                        @ContadorId,
                        @UrlRetorno
                    );
                    SELECT LAST_INSERT_ID();";

                var id = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    pago.PedidoId,
                    pago.Monto,
                    pago.FechaPago,
                    pago.Estado,
                    pago.Metodo,
                    pago.WebpayToken,
                    pago.WebpayBuyOrder,
                    pago.WebpaySessionId,
                    pago.WebpayAuthorizationCode,
                    pago.WebpayPaymentTypeCode,
                    pago.WebpayResponseCode,
                    pago.WebpayCardLastDigits,
                    pago.WebpayInstallmentsNumber,
                    pago.WebpayTransactionDate,
                    pago.WebpayStatus,
                    pago.WebpayVci,
                    pago.Notas,
                    pago.ContadorId,
                    pago.UrlRetorno
                });

                return id;
            }
        }

        public async Task<bool> ActualizarPagoAsync(Pago pago)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE pagos 
                    SET 
                        estado = @Estado,
                        metodo = @Metodo,
                        webpay_token = @WebpayToken,
                        webpay_buyorder = @WebpayBuyOrder,
                        webpay_sessionid = @WebpaySessionId,
                        webpay_authorization_code = @WebpayAuthorizationCode,
                        webpay_payment_type_code = @WebpayPaymentTypeCode,
                        webpay_response_code = @WebpayResponseCode,
                        webpay_card_last_digits = @WebpayCardLastDigits,
                        webpay_installments_number = @WebpayInstallmentsNumber,
                        webpay_transaction_date = @WebpayTransactionDate,
                        webpay_status = @WebpayStatus,
                        webpay_vci = @WebpayVci,
                        notas = @Notas,
                        contador_id = @ContadorId,
                        url_retorno = @UrlRetorno
                    WHERE id = @Id";

                var filasAfectadas = await connection.ExecuteAsync(sql, new
                {
                    pago.Id,
                    pago.Estado,
                    pago.Metodo,
                    pago.WebpayToken,
                    pago.WebpayBuyOrder,
                    pago.WebpaySessionId,
                    pago.WebpayAuthorizationCode,
                    pago.WebpayPaymentTypeCode,
                    pago.WebpayResponseCode,
                    pago.WebpayCardLastDigits,
                    pago.WebpayInstallmentsNumber,
                    pago.WebpayTransactionDate,
                    pago.WebpayStatus,
                    pago.WebpayVci,
                    pago.Notas,
                    pago.ContadorId,
                    pago.UrlRetorno
                });

                return filasAfectadas > 0;
            }
        }

        public async Task<bool> ActualizarEstadoPagoAsync(int id, string estado, string notas)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE pagos 
                    SET 
                        estado = @Estado,
                        notas = @Notas
                    WHERE id = @Id";

                var filasAfectadas = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    Estado = estado,
                    Notas = notas
                });

                return filasAfectadas > 0;
            }
        }

        public async Task<Pago> ObtenerPorTokenPasarelaAsync(string mercadoPagoPreferenceId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT p.*, ped.fecha_pedido as FechaPedido
                    FROM pagos p
                    JOIN pedidos ped ON p.pedido_id = ped.id
                    WHERE p.mercadopago_preference_id = @MercadoPagoPreferenceId";

                var pago = await connection.QueryFirstOrDefaultAsync<Pago>(sql, new { MercadoPagoPreferenceId = mercadoPagoPreferenceId });
                return pago;
            }
        }

        public async Task<Pago> ObtenerPorWebpayTokenAsync(string webpayToken)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM pagos WHERE webpay_token = @WebpayToken LIMIT 1";
                var pago = await connection.QueryFirstOrDefaultAsync<Pago>(sql, new { WebpayToken = webpayToken });
                return pago;
            }
        }

        public async Task<Pago> ObtenerPorWebpayBuyOrderAsync(string webpayBuyOrder)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM pagos WHERE webpay_buyorder = @WebpayBuyOrder LIMIT 1";
                var pago = await connection.QueryFirstOrDefaultAsync<Pago>(sql, new { WebpayBuyOrder = webpayBuyOrder });
                return pago;
            }
        }

        public async Task<bool> ActualizarTokenYBuyOrderAsync(int pedidoId, string token, string buyOrder)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"UPDATE pagos SET webpay_token = @Token, webpay_buyorder = @BuyOrder WHERE pedido_id = @PedidoId AND estado = 'pendiente'";
                var filasAfectadas = await connection.ExecuteAsync(sql, new { Token = token, BuyOrder = buyOrder, PedidoId = pedidoId });
                return filasAfectadas > 0;
            }
        }

        public async Task<bool> PagoExisteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT COUNT(1) FROM pagos WHERE id = @Id";

                var existe = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
                return existe > 0;
            }
        }
    }
}