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
        private readonly MercadoPagoService _mercadoPagoService;

        public PagosService(IPagoRepository pagoRepository, IPedidoRepository pedidoRepository, MercadoPagoService mercadoPagoService)
        {
            _pagoRepository = pagoRepository;
            _pedidoRepository = pedidoRepository;
            _mercadoPagoService = mercadoPagoService;
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

            // Si el método es Mercado Pago, generar preferencia
            if (pagoCreateDTO.Metodo == "mercadopago")
            {
                // Crear preferencia en Mercado Pago
                var mpResponse = await _mercadoPagoService.CrearPreferenciaPago(pedido, pagoId);

                // Actualizar el pago con los datos de Mercado Pago
                nuevoPago.MercadoPagoPreferenceId = mpResponse.PreferenceId;
                await _pagoRepository.ActualizarPagoAsync(nuevoPago);

                // Agregar URL de pago a la respuesta
                respuesta.UrlPasarela = mpResponse.InitPoint; // o SandboxInitPoint para testing
                respuesta.MercadoPagoPreferenceId = mpResponse.PreferenceId;
            }

            return respuesta;
        }

        public async Task<PagoResponseDTO> ConfirmarPagoAsync(PagoConfirmacionDTO confirmacionDTO)
        {
            // Buscar el pago por el preference ID de MercadoPago
            var pago = await _pagoRepository.ObtenerPorTokenPasarelaAsync(confirmacionDTO.MercadoPagoPaymentId);
            if (pago == null)
                throw new KeyNotFoundException("No se encontró un pago con el ID de MercadoPago especificado");

            // Actualizar el pago con todos los datos de MercadoPago
            pago.MercadoPagoPaymentId = confirmacionDTO.MercadoPagoPaymentId;
            pago.Estado = confirmacionDTO.Estado;
            pago.MercadoPagoStatus = confirmacionDTO.MercadoPagoStatus;
            pago.MercadoPagoStatusDetail = confirmacionDTO.MercadoPagoStatusDetail;
            pago.MercadoPagoPaymentMethodId = confirmacionDTO.MercadoPagoPaymentMethodId;
            pago.MercadoPagoPaymentTypeId = confirmacionDTO.MercadoPagoPaymentTypeId;
            pago.MercadoPagoInstallments = confirmacionDTO.MercadoPagoInstallments;
            pago.MercadoPagoCardNumber = confirmacionDTO.MercadoPagoCardNumber;

            // Actualizar el pago
            await _pagoRepository.ActualizarPagoAsync(pago);

            // Si el pago fue exitoso, actualizar el estado del pedido
            if (confirmacionDTO.Estado == "COMPLETADO")
            {
                await _pedidoRepository.UpdatePedidoEstadoAsync(pago.PedidoId, "PAGADO");
            }
            else if (confirmacionDTO.Estado == "RECHAZADO")
            {
                // Mantener el pedido como pendiente si el pago fue rechazado
                await _pedidoRepository.UpdatePedidoEstadoAsync(pago.PedidoId, "PENDIENTE");
            }

            // Obtener el pago actualizado
            pago = await _pagoRepository.ObtenerPorIdAsync(pago.Id);

            return MapPagoToDTO(pago);
        }

        public async Task<bool> PagoExisteAsync(int id)
        {
            return await _pagoRepository.PagoExisteAsync(id);
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
                MercadoPagoPaymentId = pago.MercadoPagoPaymentId,
                MercadoPagoPreferenceId = pago.MercadoPagoPreferenceId,
                MercadoPagoStatus = pago.MercadoPagoStatus,
                MercadoPagoStatusDetail = pago.MercadoPagoStatusDetail,
                MercadoPagoPaymentMethodId = pago.MercadoPagoPaymentMethodId,
                MercadoPagoPaymentTypeId = pago.MercadoPagoPaymentTypeId,
                MercadoPagoInstallments = pago.MercadoPagoInstallments,
                MercadoPagoCardNumber = pago.MercadoPagoCardNumber,
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