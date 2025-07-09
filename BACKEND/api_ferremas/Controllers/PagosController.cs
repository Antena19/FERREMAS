using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Transbank.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank;

namespace Ferremas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly IPagosService _pagosService;

        public PagosController(IPagosService pagosService)
        {
            _pagosService = pagosService;
        }



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PagoResponseDTO>>> ObtenerPagos()
        {
            var pagos = await _pagosService.ObtenerTodosAsync();
            return Ok(pagos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagoResponseDTO>> ObtenerPago(int id)
        {
            var pago = await _pagosService.ObtenerPorIdAsync(id);

            if (pago == null)
                return NotFound($"No se encontró el pago con ID {id}");

            return Ok(pago);
        }

        [HttpGet("pedido/{pedidoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PagoResponseDTO>>> ObtenerPagosPorPedido(int pedidoId)
        {
            var pagos = await _pagosService.ObtenerPorPedidoAsync(pedidoId);
            return Ok(pagos);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagoResponseDTO>> CrearPago(PagoCreateDTO pagoCreateDTO)
        {
            try
            {
                var nuevoPago = await _pagosService.CrearPagoAsync(pagoCreateDTO);
                return CreatedAtAction(nameof(ObtenerPago), new { id = nuevoPago.Id }, nuevoPago);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("confirmar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagoResponseDTO>> ConfirmarPago(PagoConfirmacionDTO confirmacionDTO)
        {
            try
            {
                var pagoConfirmado = await _pagosService.ConfirmarPagoAsync(confirmacionDTO);
                return Ok(pagoConfirmado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // --- INICIO INTEGRACIÓN WEBPAY ---
        // Endpoint público para crear la transacción Webpay
        [HttpPost("webpay/crear")]
        [AllowAnonymous]
        public async Task<IActionResult> CrearTransaccionWebpay([FromBody] WebpayCrearRequestDTO request)
        {
            // Usa el ID del pedido como buyOrder
            var buyOrder = request.PedidoId.ToString();
            var sessionId = $"session-{Guid.NewGuid()}";
            var amount = (int)Math.Round(request.Amount); // Webpay solo acepta enteros
            var returnUrl = "http://localhost:8100/carrito"; // <-- AQUÍ

            var options = new Options(
                "597055555532",
                "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C",
                IntegrationType.Test
            );
            var transaction = new Transaction(options);
            var response = transaction.Create(buyOrder, sessionId, amount, returnUrl);

            // *** NUEVO: Actualizar el registro de pago con el token y buyOrder ***
            await _pagosService.ActualizarTokenYBuyOrderAsync(request.PedidoId, response.Token, buyOrder);

            return Ok(new { url = response.Url, token = response.Token });
        }

        // Endpoint público para confirmar la transacción Webpay
        [HttpPost("webpay/confirmar")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarTransaccionWebpay([FromBody] WebpayConfirmarRequestDTO request)
        {
            var options = new Options(
                "597055555532",
                "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C",
                IntegrationType.Test
            );
            var transaction = new Transaction(options);
            var result = transaction.Commit(request.Token);

            // Mapear el resultado de Webpay a un DTO de confirmación
            var confirmacionDTO = new PagoConfirmacionDTO
            {
                Token = request.Token,
                BuyOrder = result.BuyOrder,
                SessionId = result.SessionId,
                AuthorizationCode = result.AuthorizationCode,
                PaymentTypeCode = result.PaymentTypeCode,
                ResponseCode = result.ResponseCode,
                CardLastDigits = result.CardDetail?.CardNumber,
                InstallmentsNumber = result.InstallmentsNumber,
                TransactionDate = result.TransactionDate,
                Status = result.Status,
                Vci = result.Vci
            };

            var pagoConfirmado = await _pagosService.ConfirmarPagoAsync(confirmacionDTO);
            return Ok(pagoConfirmado);
        }
        // --- FIN INTEGRACIÓN WEBPAY ---

        // DTOs para Webpay
        public class WebpayCrearRequestDTO
        {
            public decimal Amount { get; set; }
            public int PedidoId { get; set; }
        }
        public class WebpayConfirmarRequestDTO
        {
            public string Token { get; set; }
        }
    }
}