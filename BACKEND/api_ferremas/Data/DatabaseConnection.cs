using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Ferremas.Api.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<T> ExecuteQueryAsync<T>(string storedProcedure, Func<MySqlDataReader, T> mapper, MySqlParameter[] parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            return mapper(reader);
        }

        public async Task<int> ExecuteNonQueryAsync(string storedProcedure, MySqlParameter[] parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string storedProcedure, MySqlParameter[] parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }
    }
} 