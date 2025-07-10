using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using System;
using System.Collections.Generic;

namespace Ferremas.Tests
{
    public class PedidosServiceTests
    {
        private Mock<IPedidosService> _pedidosServiceMock;

        [SetUp]
        public void Setup()
        {
            _pedidosServiceMock = new Mock<IPedidosService>();
        }

        // PD-01: Crear pedido
        [Test]
        public async Task CrearPedido_DatosValidos_PedidoCreadoCorrectamente()
        {
            var pedido = new Pedido { Id = 1, UsuarioId = 2, FechaPedido = DateTime.Now, Estado = "Nuevo", TipoEntrega = "Retiro", Subtotal = 10000, Total = 12000 };
            var pedidoDTO = new PedidoDTO { Id = 1, UsuarioId = 2 };
            _pedidosServiceMock.Setup(s => s.CreatePedidoAsync(pedido)).ReturnsAsync(pedidoDTO);

            var result = await _pedidosServiceMock.Object.CreatePedidoAsync(pedido);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        // PD-02: Obtener pedido por ID
        [Test]
        public async Task ObtenerPedidoPorId_PedidoExistente_PedidoEncontrado()
        {
            var pedidoDTO = new PedidoDTO { Id = 1, UsuarioId = 2 };
            _pedidosServiceMock.Setup(s => s.GetPedidoByIdAsync(1)).ReturnsAsync(pedidoDTO);

            var result = await _pedidosServiceMock.Object.GetPedidoByIdAsync(1);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        // PD-03: Actualizar estado de pedido
        [Test]
        public async Task ActualizarEstadoPedido_DatosValidos_EstadoActualizado()
        {
            var pedidoDTO = new PedidoDTO { Id = 1, Estado = "Enviado" };
            _pedidosServiceMock.Setup(s => s.UpdatePedidoEstadoAsync(1, "Enviado")).ReturnsAsync(pedidoDTO);

            var result = await _pedidosServiceMock.Object.UpdatePedidoEstadoAsync(1, "Enviado");

            Assert.IsNotNull(result);
            Assert.That(result.Estado, Is.EqualTo("Enviado"));
        }

        // PD-04: Eliminar pedido
        [Test]
        public async Task EliminarPedido_PedidoExistente_PedidoEliminado()
        {
            _pedidosServiceMock.Setup(s => s.DeletePedidoAsync(1)).ReturnsAsync(true);

            var result = await _pedidosServiceMock.Object.DeletePedidoAsync(1);

            Assert.IsTrue(result);
        }

        // PD-05: Obtener pedidos por usuario
        [Test]
        public async Task ObtenerPedidosPorUsuario_UsuarioExistente_ListaPedidosObtenida()
        {
            var pedidos = new List<PedidoDTO> { new PedidoDTO { Id = 1, UsuarioId = 2 }, new PedidoDTO { Id = 2, UsuarioId = 2 } };
            _pedidosServiceMock.Setup(s => s.GetPedidosByUsuarioIdAsync(2)).ReturnsAsync(pedidos);

            var result = await _pedidosServiceMock.Object.GetPedidosByUsuarioIdAsync(2);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // PD-06: Obtener historial de compras de cliente
        [Test]
        public async Task ObtenerHistorialComprasCliente_ClienteExistente_HistorialObtenido()
        {
            var historial = new List<PedidoDTO> { new PedidoDTO { Id = 1, UsuarioId = 2 }, new PedidoDTO { Id = 2, UsuarioId = 2 } };
            _pedidosServiceMock.Setup(s => s.GetHistorialComprasClienteAsync(2)).ReturnsAsync(historial);

            var result = await _pedidosServiceMock.Object.GetHistorialComprasClienteAsync(2);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
} 