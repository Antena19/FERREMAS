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
    [Route("api/marcas")]
    public class MarcasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public MarcasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/marcas
        [HttpGet]
        public async Task<IActionResult> GetMarcas()
        {
            var marcas = new List<Marca>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new MySqlCommand("SELECT id, nombre, descripcion, logo_url FROM marcas WHERE activo = 1", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        marcas.Add(new Marca
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LogoUrl = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            return Ok(marcas);
        }
    }
} 