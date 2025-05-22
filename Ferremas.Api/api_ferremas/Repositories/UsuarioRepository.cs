using System;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace Ferremas.Api.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Usuario> ObtenerPorIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM usuarios WHERE id = @Id";
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
            }
        }

        public async Task<Usuario> ObtenerPorEmailAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM usuarios WHERE email = @Email";
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
            }
        }

        public async Task<Usuario> ObtenerPorRutAsync(string rut)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM usuarios WHERE rut = @Rut";
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Rut = rut });
            }
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT COUNT(*) FROM usuarios WHERE email = @Email";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
                return count > 0;
            }
        }

        public async Task<bool> RutExisteAsync(string rut)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT COUNT(*) FROM usuarios WHERE rut = @Rut";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { Rut = rut });
                return count > 0;
            }
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
                    INSERT INTO usuarios (
                        nombre, apellido, email, password, rut, telefono, 
                        rol, fecha_registro, ultimo_acceso, activo
                    ) VALUES (
                        @Nombre, @Apellido, @Email, @Password, @Rut, @Telefono,
                        @Rol, @FechaRegistro, @UltimoAcceso, @Activo
                    );
                    SELECT LAST_INSERT_ID();";

                var id = await connection.ExecuteScalarAsync<int>(sql, usuario);
                usuario.Id = id;
                return usuario;
            }
        }

        public async Task<bool> ActualizarUsuarioAsync(Usuario usuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"
                    UPDATE usuarios 
                    SET nombre = @Nombre,
                        apellido = @Apellido,
                        email = @Email,
                        password = @Password,
                        rut = @Rut,
                        telefono = @Telefono,
                        rol = @Rol,
                        ultimo_acceso = @UltimoAcceso,
                        activo = @Activo
                    WHERE id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, usuario);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "DELETE FROM usuarios WHERE id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "UPDATE usuarios SET activo = @Activo WHERE id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Activo = activo });
                return rowsAffected > 0;
            }
        }

        public async Task<bool> ActualizarUltimoAccesoAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "UPDATE usuarios SET ultimo_acceso = @UltimoAcceso WHERE id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UltimoAcceso = DateTime.Now });
                return rowsAffected > 0;
            }
        }
    }
} 