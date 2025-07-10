using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Repositories;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;

namespace Ferremas.Tests
{
    public class ClienteServiceTests
    {
        private Mock<IClienteRepository> _clienteRepositoryMock;
        private ClienteService _clienteService;

        [SetUp]
        public void Setup()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clienteService = new ClienteService(_clienteRepositoryMock.Object);
        }

        [Test]
        public async Task CrearCliente_DatosValidos_ClienteCreadoSinErrores()
        {
            // Arrange
            var clienteDTO = new ClienteCreateDTO
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Rut = "12345678-9",
                CorreoElectronico = "juan.perez@email.com",
                Telefono = "987654321",
                TipoCliente = "particular",
                Newsletter = true
            };

            _clienteRepositoryMock.Setup(r => r.ClienteExisteAsync(clienteDTO.Rut)).ReturnsAsync(false);
            _clienteRepositoryMock.Setup(r => r.CorreoExisteAsync(clienteDTO.CorreoElectronico)).ReturnsAsync(false);
            _clienteRepositoryMock.Setup(r => r.CrearClienteAsync(It.IsAny<Cliente>())).ReturnsAsync(1);

            // Act
            var id = await _clienteService.CrearClienteAsync(clienteDTO);

            // Assert
            Assert.That(id, Is.EqualTo(1));
            _clienteRepositoryMock.Verify(r => r.CrearClienteAsync(It.IsAny<Cliente>()), Times.Once);
        }
    }
}