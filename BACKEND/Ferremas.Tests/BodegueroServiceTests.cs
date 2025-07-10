using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using System.Collections.Generic;

namespace Ferremas.Tests
{
    public class BodegueroServiceTests
    {
        private Mock<IBodegueroService> _bodegueroServiceMock;

        [SetUp]
        public void Setup()
        {
            _bodegueroServiceMock = new Mock<IBodegueroService>();
        }

        // BO-01: Obtener inventario de sucursal
        [Test]
        public async Task ObtenerInventarioSucursal_SucursalExistente_ListaObtenida()
        {
            var inventario = new List<Inventario> { new Inventario { Id = 1 }, new Inventario { Id = 2 } };
            _bodegueroServiceMock.Setup(s => s.GetInventarioSucursal(1)).ReturnsAsync(inventario);

            var result = await _bodegueroServiceMock.Object.GetInventarioSucursal(1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // BO-02: Obtener todos los inventarios
        [Test]
        public async Task ObtenerTodosInventarios_ListaObtenida()
        {
            var inventario = new List<Inventario> { new Inventario { Id = 1 }, new Inventario { Id = 2 } };
            _bodegueroServiceMock.Setup(s => s.GetAllInventario()).ReturnsAsync(inventario);

            var result = await _bodegueroServiceMock.Object.GetAllInventario();

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // BO-03: Obtener pedidos asignados
        [Test]
        public async Task ObtenerPedidosAsignados_BodegueroExistente_ListaObtenida()
        {
            var pedidos = new List<PedidoBodega> { new PedidoBodega { Id = 1 }, new PedidoBodega { Id = 2 } };
            _bodegueroServiceMock.Setup(s => s.GetPedidosBodegaAsignados(1)).ReturnsAsync(pedidos);

            var result = await _bodegueroServiceMock.Object.GetPedidosBodegaAsignados(1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // BO-04: Obtener pedido de bodega por ID
        [Test]
        public async Task ObtenerPedidoBodegaPorId_PedidoExistente_PedidoObtenido()
        {
            var pedido = new PedidoBodega { Id = 1 };
            _bodegueroServiceMock.Setup(s => s.GetPedidoBodegaById(1)).ReturnsAsync(pedido);

            var result = await _bodegueroServiceMock.Object.GetPedidoBodegaById(1);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        // BO-05: Crear entrega de bodega
        [Test]
        public async Task CrearEntregaBodega_PedidoExistente_EntregaCreada()
        {
            var entrega = new EntregaBodega { Id = 1 };
            _bodegueroServiceMock.Setup(s => s.CrearEntregaBodega(1)).ReturnsAsync(entrega);

            var result = await _bodegueroServiceMock.Object.CrearEntregaBodega(1);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        // BO-06: Actualizar estado de pedido de bodega
        [Test]
        public async Task ActualizarEstadoPedidoBodega_DatosValidos_EstadoActualizado()
        {
            var pedido = new PedidoBodega { Id = 1, Estado = "Entregado" };
            _bodegueroServiceMock.Setup(s => s.ActualizarEstadoPedidoBodega(1, "Entregado")).ReturnsAsync(pedido);

            var result = await _bodegueroServiceMock.Object.ActualizarEstadoPedidoBodega(1, "Entregado");

            Assert.IsNotNull(result);
            Assert.That(result.Estado, Is.EqualTo("Entregado"));
        }
    }
} 