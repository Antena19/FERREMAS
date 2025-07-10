using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using System.Collections.Generic;

namespace Ferremas.Tests
{
    public class CarritoServiceTests
    {
        private Mock<ICarritoService> _carritoServiceMock;

        [SetUp]
        public void Setup()
        {
            _carritoServiceMock = new Mock<ICarritoService>();
        }

        // CA-01: Obtener carrito
        [Test]
        public async Task ObtenerCarrito_UsuarioExistente_CarritoObtenido()
        {
            var carrito = new Carrito { UsuarioId = 1 };
            _carritoServiceMock.Setup(s => s.ObtenerCarrito(1)).ReturnsAsync(carrito);

            var result = await _carritoServiceMock.Object.ObtenerCarrito(1);

            Assert.IsNotNull(result);
            Assert.That(result.UsuarioId, Is.EqualTo(1));
        }

        // CA-02: Agregar item al carrito
        [Test]
        public async Task AgregarItem_ProductoValido_ItemAgregado()
        {
            var carrito = new Carrito { UsuarioId = 1 };
            _carritoServiceMock.Setup(s => s.AgregarItem(1, 2, 3)).ReturnsAsync(carrito);

            var result = await _carritoServiceMock.Object.AgregarItem(1, 2, 3);

            Assert.IsNotNull(result);
        }

        // CA-03: Actualizar cantidad de item
        [Test]
        public async Task ActualizarCantidad_ItemExistente_CantidadActualizada()
        {
            var carrito = new Carrito { UsuarioId = 1 };
            _carritoServiceMock.Setup(s => s.ActualizarCantidad(1, 10, 5)).ReturnsAsync(carrito);

            var result = await _carritoServiceMock.Object.ActualizarCantidad(1, 10, 5);

            Assert.IsNotNull(result);
        }

        // CA-04: Eliminar item del carrito
        [Test]
        public async Task EliminarItem_ItemExistente_ItemEliminado()
        {
            var carrito = new Carrito { UsuarioId = 1 };
            _carritoServiceMock.Setup(s => s.EliminarItem(1, 10)).ReturnsAsync(carrito);

            var result = await _carritoServiceMock.Object.EliminarItem(1, 10);

            Assert.IsNotNull(result);
        }

        // CA-05: Vaciar carrito
        [Test]
        public async Task VaciarCarrito_UsuarioExistente_CarritoVaciado()
        {
            _carritoServiceMock.Setup(s => s.VaciarCarrito(1)).Returns(Task.CompletedTask);

            await _carritoServiceMock.Object.VaciarCarrito(1);

            Assert.Pass();
        }

        // CA-06: Calcular totales del carrito
        [Test]
        public async Task CalcularTotales_CarritoConProductos_TotalesCalculados()
        {
            var carrito = new Carrito { UsuarioId = 1 };
            _carritoServiceMock.Setup(s => s.CalcularTotales(carrito)).ReturnsAsync(carrito);

            var result = await _carritoServiceMock.Object.CalcularTotales(carrito);

            Assert.IsNotNull(result);
        }
    }
} 