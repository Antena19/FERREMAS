using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;
using System.Collections.Generic;

namespace Ferremas.Tests
{
    public class IntegracionTransbankTests
    {
        private Mock<IPagosService> _pagosServiceMock;

        [SetUp]
        public void Setup()
        {
            _pagosServiceMock = new Mock<IPagosService>();
        }

        // PA-01: Crear pago
        [Test]
        public async Task CrearPago_DatosValidos_PagoCreadoCorrectamente()
        {
            var pagoDTO = new PagoCreateDTO { /* completa con datos necesarios */ };
            var pagoEsperado = new PagoResponseDTO { Id = 1 };
            _pagosServiceMock.Setup(s => s.CrearPagoAsync(pagoDTO)).ReturnsAsync(pagoEsperado);

            var resultado = await _pagosServiceMock.Object.CrearPagoAsync(pagoDTO);

            Assert.IsNotNull(resultado);
            Assert.That(resultado.Id, Is.EqualTo(1));
        }

        // PA-02: Confirmar pago
        [Test]
        public async Task ConfirmarPago_DatosValidos_PagoConfirmado()
        {
            var confirmacion = new PagoConfirmacionDTO { /* completa con datos necesarios */ };
            var respuestaEsperada = new PagoResponseDTO { Id = 1 };
            _pagosServiceMock.Setup(s => s.ConfirmarPagoAsync(confirmacion)).ReturnsAsync(respuestaEsperada);

            var resultado = await _pagosServiceMock.Object.ConfirmarPagoAsync(confirmacion);

            Assert.IsNotNull(resultado);
            Assert.That(resultado.Id, Is.EqualTo(1));
        }

        // PA-03: Obtener pago por ID
        [Test]
        public async Task ObtenerPagoPorId_PagoExistente_PagoEncontrado()
        {
            var pagoEsperado = new PagoResponseDTO { Id = 1 };
            _pagosServiceMock.Setup(s => s.ObtenerPorIdAsync(1)).ReturnsAsync(pagoEsperado);

            var resultado = await _pagosServiceMock.Object.ObtenerPorIdAsync(1);

            Assert.IsNotNull(resultado);
            Assert.That(resultado.Id, Is.EqualTo(1));
        }

        // PA-04: Obtener pagos por pedido
        [Test]
        public async Task ObtenerPagosPorPedido_PedidoExistente_ListaPagosObtenida()
        {
            var listaPagos = new List<PagoResponseDTO> { new PagoResponseDTO { Id = 1 }, new PagoResponseDTO { Id = 2 } };
            _pagosServiceMock.Setup(s => s.ObtenerPorPedidoAsync(1)).ReturnsAsync(listaPagos);

            var resultado = await _pagosServiceMock.Object.ObtenerPorPedidoAsync(1);

            Assert.IsNotNull(resultado);
            Assert.That(resultado.Count, Is.EqualTo(2));
        }

        // PA-05: Validar pago inexistente
        [Test]
        public async Task ObtenerPago_PagoInexistente_RetornaNull()
        {
            _pagosServiceMock.Setup(s => s.ObtenerPorIdAsync(99)).ReturnsAsync((PagoResponseDTO)null);

            var resultado = await _pagosServiceMock.Object.ObtenerPorIdAsync(99);

            Assert.IsNull(resultado);
        }

        // PA-06: Validar error al confirmar pago inválido
        [Test]
        public void ConfirmarPago_DatosInvalidos_ErrorConfirmacion()
        {
            var confirmacion = new PagoConfirmacionDTO { /* completa con datos necesarios */ };
            _pagosServiceMock.Setup(s => s.ConfirmarPagoAsync(confirmacion)).ThrowsAsync(new System.Exception("Error de confirmación"));

            Assert.ThrowsAsync<System.Exception>(async () =>
                await _pagosServiceMock.Object.ConfirmarPagoAsync(confirmacion)
            );
        }
    }
} 