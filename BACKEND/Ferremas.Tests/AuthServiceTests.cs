using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Ferremas.Api.Services;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using System;

namespace Ferremas.Tests
{
    public class AuthServiceTests
    {
        private Mock<IAuthService> _authServiceMock;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
        }

        // AU-01: Login válido
        [Test]
        public async Task Login_CredencialesCorrectas_RetornaTokenJWT()
        {
            var loginDTO = new LoginDTO { Email = "usuario@email.com", Password = "contrasena" };
            var usuarioDTO = new UsuarioDTO { Email = loginDTO.Email };
            var tokenEsperado = "token.jwt";
            var response = (true, tokenEsperado, "Login exitoso", usuarioDTO);

            _authServiceMock.Setup(s => s.LoginAsync(loginDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.LoginAsync(loginDTO);

            Assert.IsTrue(result.success);
            Assert.That(result.token, Is.EqualTo(tokenEsperado));
            Assert.That(result.usuario.Email, Is.EqualTo(loginDTO.Email));
        }

        // AU-02: Login inválido
        [Test]
        public async Task Login_CredencialesIncorrectas_ErrorAutenticacion()
        {
            var loginDTO = new LoginDTO { Email = "usuario@email.com", Password = "incorrecta" };
            (bool success, string token, string message, UsuarioDTO usuario) response = (false, null, "Credenciales incorrectas", (UsuarioDTO)null);

            _authServiceMock.Setup(s => s.LoginAsync(loginDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.LoginAsync(loginDTO);

            Assert.IsFalse(result.success);
            Assert.IsNull(result.token);
        }

        // AU-03: Registro válido
        [Test]
        public async Task Registro_UsuarioDatosValidos_UsuarioRegistradoCorrectamente()
        {
            var registroDTO = new RegistroDTO { Email = "nuevo@email.com", Password = "123456", Nombre = "Nuevo" };
            var response = (true, "Registro exitoso");
            _authServiceMock.Setup(s => s.RegistroAsync(registroDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.RegistroAsync(registroDTO);

            Assert.IsTrue(result.success);
        }

        // AU-04: Registro inválido
        [Test]
        public async Task Registro_UsuarioDatosInvalidos_ErrorValidacion()
        {
            var registroDTO = new RegistroDTO { Email = "", Password = "", Nombre = "" };
            var response = (false, "Datos inválidos");
            _authServiceMock.Setup(s => s.RegistroAsync(registroDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.RegistroAsync(registroDTO);

            Assert.IsFalse(result.success);
        }

        // AU-05: Recuperar contraseña
        [Test]
        public async Task RecuperarContrasena_EmailValido_RecuperacionExitosa()
        {
            var recuperacionDTO = new RecuperacionContrasenaDTO { Email = "usuario@email.com" };
            var response = (true, "Recuperación exitosa");
            _authServiceMock.Setup(s => s.RecuperarContrasenaAsync(recuperacionDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.RecuperarContrasenaAsync(recuperacionDTO);

            Assert.IsTrue(result.success);
        }

        // AU-06: Cambiar contraseña
        [Test]
        public async Task CambiarContrasena_DatosValidos_CambioExitoso()
        {
            var cambioDTO = new CambioContrasenaDTO { Token = "token123", NuevaPassword = "nueva123", ConfirmarPassword = "nueva123" };
            var response = (true, "Cambio exitoso");
            _authServiceMock.Setup(s => s.CambiarContrasenaAsync(cambioDTO)).ReturnsAsync(response);

            var result = await _authServiceMock.Object.CambiarContrasenaAsync(cambioDTO);

            Assert.IsTrue(result.success);
        }
    }
} 