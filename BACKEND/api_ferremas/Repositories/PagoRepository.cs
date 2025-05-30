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
                    SELECT p.*, ped.fecha as FechaPedido
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
                    SELECT p.*, ped.fecha as FechaPedido
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
                    SELECT p.*, ped.fecha as FechaPedido
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
                        mercadopago_payment_id,
                        mercadopago_preference_id,
                        mercadopago_status,
                        mercadopago_status_detail,
                        mercadopago_payment_method_id,
                        mercadopago_payment_type_id,
                        mercadopago_installments,
                        mercadopago_card_number,
                        referencia_transaccion,
                        notas,
                        contador_id,
                        url_retorno
                    ) VALUES (
                        @PedidoId,
                        @Monto,
                        @FechaPago,
                        @Estado,
                        @Metodo,
                        @MercadoPagoPaymentId,
                        @MercadoPagoPreferenceId,
                        @MercadoPagoStatus,
                        @MercadoPagoStatusDetail,
                        @MercadoPagoPaymentMethodId,
                        @MercadoPagoPaymentTypeId,
                        @MercadoPagoInstallments,
                        @MercadoPagoCardNumber,
                        @ReferenciaTransaccion,
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
                    pago.MercadoPagoPaymentId,
                    pago.MercadoPagoPreferenceId,
                    pago.MercadoPagoStatus,
                    pago.MercadoPagoStatusDetail,
                    pago.MercadoPagoPaymentMethodId,
                    pago.MercadoPagoPaymentTypeId,
                    pago.MercadoPagoInstallments,
                    pago.MercadoPagoCardNumber,
                    pago.ReferenciaTransaccion,
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
                        mercadopago_payment_id = @MercadoPagoPaymentId,
                        mercadopago_preference_id = @MercadoPagoPreferenceId,
                        mercadopago_status = @MercadoPagoStatus,
                        mercadopago_status_detail = @MercadoPagoStatusDetail,
                        mercadopago_payment_method_id = @MercadoPagoPaymentMethodId,
                        mercadopago_payment_type_id = @MercadoPagoPaymentTypeId,
                        mercadopago_installments = @MercadoPagoInstallments,
                        mercadopago_card_number = @MercadoPagoCardNumber,
                        referencia_transaccion = @ReferenciaTransaccion,
                        notas = @Notas,
                        contador_id = @ContadorId,
                        url_retorno = @UrlRetorno
                    WHERE id = @Id";

                var filasAfectadas = await connection.ExecuteAsync(sql, new
                {
                    pago.Id,
                    pago.Estado,
                    pago.Metodo,
                    pago.MercadoPagoPaymentId,
                    pago.MercadoPagoPreferenceId,
                    pago.MercadoPagoStatus,
                    pago.MercadoPagoStatusDetail,
                    pago.MercadoPagoPaymentMethodId,
                    pago.MercadoPagoPaymentTypeId,
                    pago.MercadoPagoInstallments,
                    pago.MercadoPagoCardNumber,
                    pago.ReferenciaTransaccion,
                    pago.Notas,
                    pago.ContadorId,
                    pago.UrlRetorno
                });

                return filasAfectadas > 0;
            }
        }

        public async Task<bool> ActualizarEstadoPagoAsync(int id, string estado, string referenciaTransaccion, string notas)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE pagos 
                    SET 
                        estado = @Estado,
                        referencia_transaccion = @ReferenciaTransaccion,
                        notas = @Notas
                    WHERE id = @Id";

                var filasAfectadas = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    Estado = estado,
                    ReferenciaTransaccion = referenciaTransaccion,
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
                    SELECT p.*, ped.fecha as FechaPedido
                    FROM pagos p
                    JOIN pedidos ped ON p.pedido_id = ped.id
                    WHERE p.mercadopago_preference_id = @MercadoPagoPreferenceId";

                var pago = await connection.QueryFirstOrDefaultAsync<Pago>(sql, new { MercadoPagoPreferenceId = mercadoPagoPreferenceId });
                return pago;
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