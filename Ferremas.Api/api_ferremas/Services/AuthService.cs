using System;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Ferremas.Api.Repositories;

namespace Ferremas.Api.Services
{
    public interface IAuthService
    {
        Task<(bool success, string token, string message, UsuarioDTO usuario)> LoginAsync(LoginDTO loginDTO);
        Task<(bool success, string message)> RegistroAsync(RegistroDTO registroDTO);
        Task<(bool success, string message)> RecuperarContrasenaAsync(RecuperacionContrasenaDTO recuperacionDTO);
        Task<(bool success, string message)> CambiarContrasenaAsync(CambioContrasenaDTO cambioDTO);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;
        private readonly IClienteRepository _clienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public AuthService(
            IConfiguration configuration, 
            IClienteRepository clienteRepository,
            IUsuarioRepository usuarioRepository)
        {
            _configuration = configuration;
            _jwtSecret = _configuration["Jwt:Secret"];
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<(bool success, string token, string message, UsuarioDTO usuario)> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var usuario = await _usuarioRepository.ObtenerPorEmailAsync(loginDTO.Email);
                if (usuario == null)
                {
                    return (false, null, "Credenciales inválidas", null);
                }

                // Verificar contraseña
                if (!VerifyPassword(loginDTO.Password, usuario.Password))
                {
                    return (false, null, "Credenciales inválidas", null);
                }

                // Actualizar último acceso
                usuario.UltimoAcceso = DateTime.Now;
                await _usuarioRepository.ActualizarUsuarioAsync(usuario);

                var token = GenerateJwtToken(usuario);
                var usuarioDTO = MapearAUsuarioDTO(usuario);
                return (true, token, "Login exitoso", usuarioDTO);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error en el login: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> RegistroAsync(RegistroDTO registroDTO)
        {
            try
            {
                // Verificar si el correo ya existe
                if (await _usuarioRepository.EmailExisteAsync(registroDTO.Email))
                {
                    return (false, "El correo electrónico ya está registrado");
                }

                // Verificar si el RUT ya existe
                if (await _usuarioRepository.RutExisteAsync(registroDTO.Rut))
                {
                    return (false, "El RUT ya está registrado");
                }

                // Crear usuario
                var usuario = new Usuario
                {
                    Nombre = registroDTO.Nombre,
                    Apellido = registroDTO.Apellido,
                    Email = registroDTO.Email,
                    Password = HashPassword(registroDTO.Password),
                    Rut = registroDTO.Rut,
                    Telefono = registroDTO.Telefono,
                    Rol = "cliente",
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                await _usuarioRepository.CrearUsuarioAsync(usuario);

                // Crear cliente
                var cliente = new Cliente
                {
                    Nombre = registroDTO.Nombre,
                    Apellido = registroDTO.Apellido,
                    Rut = registroDTO.Rut,
                    CorreoElectronico = registroDTO.Email,
                    Telefono = registroDTO.Telefono,
                    FechaRegistro = DateTime.Now,
                    TipoCliente = "particular",
                    Estado = "activo",
                    Newsletter = false
                };

                await _clienteRepository.CrearClienteAsync(cliente);
                return (true, "Registro exitoso");
            }
            catch (Exception ex)
            {
                return (false, $"Error en el registro: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> RecuperarContrasenaAsync(RecuperacionContrasenaDTO recuperacionDTO)
        {
            try
            {
                // TODO: Implementar la lógica de recuperación de contraseña
                return (true, "Se ha enviado un correo con las instrucciones");
            }
            catch (Exception ex)
            {
                return (false, $"Error en la recuperación: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> CambiarContrasenaAsync(CambioContrasenaDTO cambioDTO)
        {
            try
            {
                // TODO: Implementar la lógica de cambio de contraseña
                return (true, "Contraseña actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al cambiar la contraseña: {ex.Message}");
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == hashedPassword;
        }

        private UsuarioDTO MapearAUsuarioDTO(Usuario usuario)
        {
            if (usuario == null) return null;
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rut = usuario.Rut,
                Telefono = usuario.Telefono,
                Rol = usuario.Rol,
                FechaRegistro = usuario.FechaRegistro,
                UltimoAcceso = usuario.UltimoAcceso,
                Activo = usuario.Activo
            };
        }
    }
} 