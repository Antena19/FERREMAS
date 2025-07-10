using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Repositories;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using System;

namespace Ferremas.Tests
{
    public class ProductosServiceTests
    {
        private Mock<IProductoRepository> _productoRepositoryMock;
        private ProductoService _productoService;

        [SetUp]
        public void Setup()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _productoService = new ProductoService(_productoRepositoryMock.Object);
        }

        // PR-01: Crear producto con datos válidos
        [Test]
        public async Task CrearProducto_DatosValidos_ProductoCreadoCorrectamente()
        {
            var productoDTO = new ProductoCreateDTO
            {
                Codigo = "P001",
                Nombre = "Martillo",
                Descripcion = "Martillo de acero",
                Precio = 5000,
                CategoriaId = 1,
                MarcaId = 1,
                ImagenUrl = "martillo.png",
                Especificaciones = "Peso: 500g"
            };
            _productoRepositoryMock.Setup(r => r.ProductoExisteAsync(productoDTO.Codigo)).ReturnsAsync(false);
            _productoRepositoryMock.Setup(r => r.CrearProductoAsync(It.IsAny<Producto>())).ReturnsAsync(1);

            var id = await _productoService.CrearProductoAsync(productoDTO);

            Assert.That(id, Is.EqualTo(1));
            _productoRepositoryMock.Verify(r => r.CrearProductoAsync(It.IsAny<Producto>()), Times.Once);
        }

        // PR-02: Validar código duplicado
        [Test]
        public void CrearProducto_CodigoDuplicado_ErrorValidacion()
        {
            var productoDTO = new ProductoCreateDTO
            {
                Codigo = "P001",
                Nombre = "Martillo",
                Precio = 5000
            };
            _productoRepositoryMock.Setup(r => r.ProductoExisteAsync(productoDTO.Codigo)).ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _productoService.CrearProductoAsync(productoDTO)
            );
            Assert.That(ex.Message, Does.Contain("código"));
        }

        // PR-03: Obtener producto por ID
        [Test]
        public async Task ObtenerProductoPorId_ProductoExistente_ProductoEncontrado()
        {
            var producto = new Producto { Id = 1, Codigo = "P001", Nombre = "Martillo", Precio = 5000 };
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(producto);

            var result = await _productoService.ObtenerPorIdAsync(1);

            Assert.IsNotNull(result);
            Assert.That(result.Nombre, Is.EqualTo("Martillo"));
        }

        // PR-04: Actualizar producto
        [Test]
        public async Task ModificarProducto_DatosValidos_ProductoActualizado()
        {
            var productoDTO = new ProductoUpdateDTO
            {
                Nombre = "Martillo Pro",
                Descripcion = "Martillo profesional",
                Precio = 6000,
                CategoriaId = 1,
                MarcaId = 1,
                ImagenUrl = "martillo_pro.png",
                Especificaciones = "Peso: 600g",
                Activo = true
            };
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(new Producto());
            _productoRepositoryMock.Setup(r => r.ActualizarProductoAsync(It.IsAny<Producto>())).ReturnsAsync(true);

            var resultado = await _productoService.ActualizarProductoAsync(1, productoDTO);

            Assert.IsTrue(resultado);
        }

        // PR-05: Eliminar producto
        [Test]
        public async Task EliminarProducto_ProductoExistente_ProductoEliminado()
        {
            _productoRepositoryMock.Setup(r => r.ProductoExisteAsync(1)).ReturnsAsync(true);
            _productoRepositoryMock.Setup(r => r.EliminarProductoAsync(1)).ReturnsAsync(true);

            var resultado = await _productoService.EliminarProductoAsync(1);

            Assert.IsTrue(resultado);
        }

        // PR-06: Actualizar stock de producto
        [Test]
        public async Task ActualizarStock_ProductoExistente_StockActualizadoCorrectamente()
        {
            int productoId = 1;
            int cantidad = 5;
            _productoRepositoryMock.Setup(r => r.ProductoExisteAsync(productoId)).ReturnsAsync(true);
            _productoRepositoryMock.Setup(r => r.ActualizarStockAsync(productoId, cantidad)).ReturnsAsync(true);

            var resultado = await _productoService.ActualizarStockAsync(productoId, cantidad);

            Assert.IsTrue(resultado);
            _productoRepositoryMock.Verify(r => r.ActualizarStockAsync(productoId, cantidad), Times.Once);
        }
    }
} 