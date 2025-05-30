using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Ferremas.Api.Modelos;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Ferremas.Api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public CategoriasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = new List<Categoria>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new MySqlCommand("SELECT id, nombre, descripcion FROM categorias WHERE activo = 1", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categorias.Add(new Categoria
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return Ok(categorias);
        }
    }
} 