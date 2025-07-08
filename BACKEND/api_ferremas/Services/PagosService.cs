using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using Ferremas.Api.Repositories;

namespace Ferremas.Api.Services
{
    public class PagosService : IPagosService
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        // Eliminado: private readonly MercadoPagoService _mercadoPagoService;

        public PagosService(IPagoRepository pagoRepository, IPedidoRepository pedidoRepository)
        {
            _pagoRepository = pagoRepository;
            _pedidoRepository = pedidoRepository;
        }

        public async Task<IEnumerable<PagoResponseDTO>> ObtenerTodosAsync()
        {
            var pagos = await _pagoRepository.ObtenerTodosAsync();
            return pagos.Select(MapPagoToDTO);
        }

        public async Task<PagoResponseDTO> ObtenerPorIdAsync(int id)
        {
            var pago = await _pagoRepository.ObtenerPorIdAsync(id);
            if (pago == null)
                return null;

            return MapPagoToDTO(pago);
        }

        public async Task<IEnumerable<PagoResponseDTO>> ObtenerPorPedidoAsync(int pedidoId)
        {
            var pagos = await _pagoRepository.ObtenerPorPedidoAsync(pedidoId);
            return pagos.Select(MapPagoToDTO);
        }

        public async Task<PagoResponseDTO> CrearPagoAsync(PagoCreateDTO pagoCreateDTO)
        {
            // Verificar que el pedido existe
            var pedido = await _pedidoRepository.GetPedidoByIdAsync(pagoCreateDTO.PedidoId);
            if (pedido == null)
                throw new KeyNotFoundException($"No se encontró el pedido con ID {pagoCreateDTO.PedidoId}");

            // Crear el nuevo pago
            var nuevoPago = new Pago
            {
                PedidoId = pagoCreateDTO.PedidoId,
                Monto = pedido.Total,
                FechaPago = DateTime.Now,
                Estado = "PENDIENTE",
                Metodo = pagoCreateDTO.Metodo,
                UrlRetorno = pagoCreateDTO.UrlRetorno
            };

            // Guardar el pago inicialmente
            var pagoId = await _pagoRepository.CrearPagoAsync(nuevoPago);
            nuevoPago.Id = pagoId;

            // Respuesta final
            var respuesta = MapPagoToDTO(nuevoPago);
            return respuesta;
        }

        public async Task<PagoResponseDTO> ConfirmarPagoAsync(PagoConfirmacionDTO confirmacionDTO)
        {
            // 1. Confirmar con Webpay (esto lo hace el controlador y pasa el resultado aquí)
            // 2. Buscar el pago por buyOrder o token
            var pago = await _pagoRepository.ObtenerPorWebpayTokenAsync(confirmacionDTO.Token);
            if (pago == null && !string.IsNullOrEmpty(confirmacionDTO.BuyOrder))
            {
                pago = await _pagoRepository.ObtenerPorWebpayBuyOrderAsync(confirmacionDTO.BuyOrder);
            }
            if (pago == null)
                throw new KeyNotFoundException("Pago no encontrado para el token o buyOrder proporcionado");

            // 3. Actualizar los campos de Webpay y el estado
            pago.WebpayToken = confirmacionDTO.Token;
            pago.WebpayBuyOrder = confirmacionDTO.BuyOrder;
            pago.WebpaySessionId = confirmacionDTO.SessionId;
            pago.WebpayAuthorizationCode = confirmacionDTO.AuthorizationCode;
            pago.WebpayPaymentTypeCode = confirmacionDTO.PaymentTypeCode;
            pago.WebpayResponseCode = confirmacionDTO.ResponseCode;
            pago.WebpayCardLastDigits = confirmacionDTO.CardLastDigits;
            pago.WebpayInstallmentsNumber = confirmacionDTO.InstallmentsNumber;
            pago.WebpayTransactionDate = confirmacionDTO.TransactionDate;
            pago.WebpayStatus = confirmacionDTO.Status;
            pago.WebpayVci = confirmacionDTO.Vci;
            pago.Estado = (confirmacionDTO.Status == "AUTHORIZED" && confirmacionDTO.ResponseCode == 0) ? "completado" : "fallido";
            pago.FechaPago = DateTime.Now;

            await _pagoRepository.ActualizarPagoAsync(pago);

            // 4. Si el pago fue exitoso, actualizar el estado del pedido
            if (pago.Estado == "completado")
            {
                await _pedidoRepository.ActualizarEstadoPedidoAsync(pago.PedidoId, "pagado");
            }

            return MapPagoToDTO(pago);
        }

        public async Task<bool> PagoExisteAsync(int id)
        {
            return await _pagoRepository.PagoExisteAsync(id);
        }

        public async Task ActualizarTokenYBuyOrderAsync(int pedidoId, string token, string buyOrder)
        {
            // Solo actualiza el pago pendiente asociado al pedido
            await _pagoRepository.ActualizarTokenYBuyOrderAsync(pedidoId, token, buyOrder);
        }

        private PagoResponseDTO MapPagoToDTO(Pago pago)
        {
            return new PagoResponseDTO
            {
                Id = pago.Id,
                PedidoId = pago.PedidoId,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago ?? DateTime.Now,
                Estado = pago.Estado,
                Metodo = pago.Metodo,
                UrlPasarela = pago.UrlRetorno
            };
        }
    

        private string GenerarTokenPasarela()
        {
            // En un entorno real, esto sería generado por la pasarela de pago
            // Para simular, generamos un token aleatorio
            return Guid.NewGuid().ToString("N");
        }


    }
}