using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.Repositories;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace Ferremas.Tests
{
    public class ClientesServiceTests
    {
        private Mock<IClienteRepository> _clienteRepositoryMock;
        private ClienteService _clienteService;

        [SetUp]
        public void Setup()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clienteService = new ClienteService(_clienteRepositoryMock.Object);
        }

        public void ValidarModelo(object modelo)
        {
            var context = new ValidationContext(modelo, null, null);
            Validator.ValidateObject(modelo, context, validateAllProperties: true);
        }

        // CL-01: Crear cliente con datos válidos
        [Test]
        public async Task CrearCliente_DatosValidos_ClienteCreadoSinErrores()
        {
            var clienteDTO = new ClienteCreateDTO
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Rut = "12345678-9",
                CorreoElectronico = "juan@email.com",
                Telefono = "987654321",
                TipoCliente = "particular",
                Newsletter = true
            };
            _clienteRepositoryMock.Setup(r => r.ClienteExisteAsync(clienteDTO.Rut)).ReturnsAsync(false);
            _clienteRepositoryMock.Setup(r => r.CorreoExisteAsync(clienteDTO.CorreoElectronico)).ReturnsAsync(false);
            _clienteRepositoryMock.Setup(r => r.CrearClienteAsync(It.IsAny<Cliente>())).ReturnsAsync(1);

            var id = await _clienteService.CrearClienteAsync(clienteDTO);

            Assert.That(id, Is.EqualTo(1));
            _clienteRepositoryMock.Verify(r => r.CrearClienteAsync(It.IsAny<Cliente>()), Times.Once);
        }

        // CL-02: Intentar crear cliente con correo duplicado
        [Test]
        public void CrearCliente_CorreoDuplicado_ErrorValidacion()
        {
            var clienteDTO = new ClienteCreateDTO
            {
                Nombre = "Ana",
                Apellido = "Gómez",
                Rut = "98765432-1",
                CorreoElectronico = "ana@email.com",
                Telefono = "123456789",
                TipoCliente = "particular",
                Newsletter = false
            };
            _clienteRepositoryMock.Setup(r => r.ClienteExisteAsync(clienteDTO.Rut)).ReturnsAsync(false);
            _clienteRepositoryMock.Setup(r => r.CorreoExisteAsync(clienteDTO.CorreoElectronico)).ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _clienteService.CrearClienteAsync(clienteDTO)
            );
            Assert.That(ex.Message, Does.Contain("correo"));
        }

        // CL-03: Obtener cliente por ID
        [Test]
        public async Task ObtenerClientePorId_ClienteExistente_ClienteEncontrado()
        {
            var cliente = new Cliente { Id = 1, Nombre = "Juan", Apellido = "Pérez", Rut = "12345678-9", CorreoElectronico = "juan@email.com" };
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(cliente);

            var result = await _clienteService.ObtenerPorIdAsync(1);

            Assert.IsNotNull(result);
            Assert.That(result.Nombre, Is.EqualTo("Juan"));
        }

        // CL-04: Actualizar cliente
        [Test]
        public async Task ActualizarCliente_DatosValidos_ClienteActualizado()
        {
            var clienteExistente = new Cliente { Id = 1, Nombre = "Juan", Apellido = "Pérez" };
            var updateDTO = new ClienteUpdateDTO { Nombre = "Juanito", Apellido = "Pérez", Telefono = "123", TipoCliente = "empresa", Estado = "activo", Newsletter = true };
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(clienteExistente);
            _clienteRepositoryMock.Setup(r => r.ActualizarClienteAsync(It.IsAny<Cliente>())).ReturnsAsync(true);

            var result = await _clienteService.ActualizarClienteAsync(1, updateDTO);

            Assert.IsTrue(result);
        }

        // CL-05: Eliminar cliente
        [Test]
        public async Task EliminarCliente_ClienteExistente_ClienteEliminado()
        {
            _clienteRepositoryMock.Setup(r => r.ClienteExisteAsync(1)).ReturnsAsync(true);
            _clienteRepositoryMock.Setup(r => r.EliminarClienteAsync(1)).ReturnsAsync(true);

            var result = await _clienteService.EliminarClienteAsync(1);

            Assert.IsTrue(result);
        }

        // CL-06: Crear dirección para cliente
        [Test]
        public async Task CrearDireccion_ClienteExistente_DireccionCreada()
        {
            int clienteId = 1;
            var direccionDTO = new DireccionCreateDTO
            {
                Calle = "Calle Falsa",
                Numero = "123",
                Departamento = "A",
                Comuna = "Centro",
                Region = "RM",
                CodigoPostal = "1234567",
                EsPrincipal = true
            };
            _clienteRepositoryMock.Setup(r => r.ClienteExisteAsync(clienteId)).ReturnsAsync(true);
            _clienteRepositoryMock.Setup(r => r.CrearDireccionAsync(It.IsAny<Direccion>())).ReturnsAsync(10);

            var id = await _clienteService.CrearDireccionAsync(clienteId, direccionDTO);

            Assert.That(id, Is.EqualTo(10));
            _clienteRepositoryMock.Verify(r => r.CrearDireccionAsync(It.IsAny<Direccion>()), Times.Once);
        }
    }
} 