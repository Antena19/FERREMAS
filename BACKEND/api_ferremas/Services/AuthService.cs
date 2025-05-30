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
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Ferremas.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IClienteRepository _clienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly string _connectionString;
        private readonly string _jwtSecret;

        public AuthService(
            IConfiguration configuration,
            IClienteRepository clienteRepository,
            IUsuarioRepository usuarioRepository)
        {
            _configuration = configuration;
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _jwtSecret = _configuration["Jwt:Secret"];
        }

        public async Task<(bool success, string token, string message, UsuarioDTO usuario)> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var usuario = await _usuarioRepository.ObtenerPorEmailAsync(loginDTO.Email);
                Console.WriteLine($"Usuario encontrado: {usuario != null}");
                if (usuario == null)
                {
                    return (false, null, "Credenciales inválidas", null);
                }

                Console.WriteLine($"Password en BD: {usuario.Password}");
                var hashedInput = HashPassword(loginDTO.Password);
                Console.WriteLine($"Password hasheado: {hashedInput}");
                if (!VerifyPassword(loginDTO.Password, usuario.Password))
                {
                    return (false, null, "Credenciales inválidas", null);
                }

                // Actualizar último acceso
                usuario.UltimoAcceso = DateTime.Now;
                await _usuarioRepository.ActualizarUsuarioAsync(usuario);

                // Mapear el rol antes de crear el DTO
                if (usuario.Rol == "administrator")
                {
                    usuario.Rol = "administrador";
                }

                var usuarioDTO = MapearAUsuarioDTO(usuario);
                var token = GenerateJwtToken(usuarioDTO);
                return (true, token, "Login exitoso", usuarioDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex.Message}");
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

        public async Task<(bool success, string message)> LogoutAsync(string token)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand("UPDATE sesiones SET activo = 0 WHERE token = @token", connection))
                    {
                        command.Parameters.AddWithValue("@token", token);
                        await command.ExecuteNonQueryAsync();
                        return (true, "Sesión cerrada exitosamente");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al cerrar sesión: {ex.Message}");
            }
        }

        public async Task<(bool success, string newToken, string message)> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand("SELECT usuario_id FROM refresh_tokens WHERE token = @token AND activo = 1 AND fecha_expiracion > NOW()", connection))
                    {
                        command.Parameters.AddWithValue("@token", refreshToken);
                        var usuarioId = await command.ExecuteScalarAsync();

                        if (usuarioId != null)
                        {
                            // Invalidar el refresh token actual
                            using (var updateCommand = new MySqlCommand("UPDATE refresh_tokens SET activo = 0 WHERE token = @token", connection))
                            {
                                updateCommand.Parameters.AddWithValue("@token", refreshToken);
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            // Obtener información del usuario
                            using (var userCommand = new MySqlCommand("SELECT id, email, nombre, apellido, rol FROM usuarios WHERE id = @usuarioId", connection))
                            {
                                userCommand.Parameters.AddWithValue("@usuarioId", usuarioId);
                                using (var reader = await userCommand.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        var usuario = new UsuarioDTO
                                        {
                                            Id = reader.GetInt32(0),
                                            Email = reader.GetString(1),
                                            Nombre = reader.GetString(2),
                                            Apellido = reader.GetString(3),
                                            Rol = reader.GetString(4)
                                        };

                                        var newToken = GenerateJwtToken(usuario);
                                        return (true, newToken, "Token refrescado exitosamente");
                                    }
                                }
                            }
                        }
                        return (false, null, "Refresh token inválido o expirado");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al refrescar token: {ex.Message}");
            }
        }

        private string GenerateJwtToken(UsuarioDTO usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            // Usar el rol tal cual está en la base de datos
            var rol = usuario.Rol;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, rol)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
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

        public async Task<UsuarioDTO> ObtenerUsuarioPorEmailAsync(string email)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(email);
            if (usuario == null) return null;

            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Rut = usuario.Rut,
                Telefono = usuario.Telefono
            };
        }
    }
} 
