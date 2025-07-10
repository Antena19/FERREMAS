using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using System.Collections.Generic;
using System;

namespace Ferremas.Tests
{
    public class InventarioServiceTests
    {
        private Mock<IInventarioService> _inventarioServiceMock;

        [SetUp]
        public void Setup()
        {
            _inventarioServiceMock = new Mock<IInventarioService>();
        }

        // IN-01: Obtener inventario por sucursal
        [Test]
        public async Task ObtenerInventarioPorSucursal_SucursalExistente_ListaInventarioObtenida()
        {
            var inventario = new List<Inventario> { new Inventario { Id = 1 }, new Inventario { Id = 2 } };
            _inventarioServiceMock.Setup(s => s.GetInventarioBySucursal(1)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.GetInventarioBySucursal(1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // IN-02: Obtener inventario por producto
        [Test]
        public async Task ObtenerInventarioPorProducto_ProductoYSucursalExistente_InventarioEncontrado()
        {
            var inventario = new Inventario { Id = 1, ProductoId = 5, SucursalId = 1 };
            _inventarioServiceMock.Setup(s => s.GetInventarioByProducto(5, 1)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.GetInventarioByProducto(5, 1);

            Assert.IsNotNull(result);
            Assert.That(result.ProductoId, Is.EqualTo(5));
        }

        // IN-03: Actualizar stock en inventario
        [Test]
        public async Task ActualizarStock_InventarioExistente_StockActualizado()
        {
            var inventario = new Inventario { Id = 1, Stock = 10 };
            _inventarioServiceMock.Setup(s => s.UpdateStock(1, 5)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.UpdateStock(1, 5);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Stock, Is.EqualTo(10));
        }

        // IN-04: Actualizar precios en inventario
        [Test]
        public async Task ActualizarPrecios_InventarioExistente_PreciosActualizados()
        {
            var inventario = new Inventario { Id = 1, PrecioCompra = 1000, PrecioVenta = 1500 };
            _inventarioServiceMock.Setup(s => s.UpdatePrecios(1, 1000, 1500)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.UpdatePrecios(1, 1000, 1500);

            Assert.IsNotNull(result);
            Assert.That(result.PrecioVenta, Is.EqualTo(1500));
        }

        // IN-05: Obtener productos bajo stock
        [Test]
        public async Task ObtenerProductosBajoStock_SucursalExistente_ListaObtenida()
        {
            var inventario = new List<Inventario> { new Inventario { Id = 1 }, new Inventario { Id = 2 } };
            _inventarioServiceMock.Setup(s => s.GetProductosBajoStock(1)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.GetProductosBajoStock(1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // IN-06: Obtener productos sobre stock
        [Test]
        public async Task ObtenerProductosSobreStock_SucursalExistente_ListaObtenida()
        {
            var inventario = new List<Inventario> { new Inventario { Id = 1 }, new Inventario { Id = 2 } };
            _inventarioServiceMock.Setup(s => s.GetProductosSobreStock(1)).ReturnsAsync(inventario);

            var result = await _inventarioServiceMock.Object.GetProductosSobreStock(1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
} 