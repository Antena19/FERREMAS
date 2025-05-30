using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    [Authorize(Roles = "Contador")]
    [ApiController]
    [Route("api/[controller]")]
    public class ContadorController : ControllerBase
    {
        private readonly IContadorService _contadorService;

        public ContadorController(IContadorService contadorService)
        {
            _contadorService = contadorService;
        }

        [HttpPost("aprobar-transferencia")]
        public async Task<IActionResult> AprobarPagoTransferencia([FromBody] AprobarTransferenciaRequest request)
        {
            try
            {
                var pago = await _contadorService.AprobarPagoTransferencia(
                    request.PedidoId,
                    request.ContadorId,
                    request.BancoOrigen,
                    request.NumeroCuenta
                );
                return Ok(pago);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("historial-pagos")]
        public async Task<IActionResult> GetHistorialPagos([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var pagos = await _contadorService.GetHistorialPagos(fechaInicio, fechaFin);
            return Ok(pagos);
        }

        [HttpGet("pagos-pendientes")]
        public async Task<IActionResult> GetPagosPendientes()
        {
            var pagos = await _contadorService.GetPagosPendientes();
            return Ok(pagos);
        }

        [HttpGet("pago/{pagoId}")]
        public async Task<IActionResult> GetPagoById(int pagoId)
        {
            var pago = await _contadorService.GetPagoById(pagoId);
            if (pago == null)
                return NotFound();
            return Ok(pago);
        }

        [HttpGet("pedidos-pendientes-pago")]
        public async Task<IActionResult> GetPedidosPendientesPago()
        {
            var pedidos = await _contadorService.GetPedidosPendientesPago();
            return Ok(pedidos);
        }
    }

    public class AprobarTransferenciaRequest
    {
        public int PedidoId { get; set; }
        public int ContadorId { get; set; }
        public string BancoOrigen { get; set; }
        public string NumeroCuenta { get; set; }
    }
} 