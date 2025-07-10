using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferremas.Api.DTOs;
using Ferremas.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IO;
using System.Linq;

namespace Ferremas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // GET: api/Productos
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductoDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos([FromQuery] bool todos = false)
        {
            bool esAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                var rolClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == "role" ||
                    c.Type == "roles" ||
                    c.Type == "Role" ||
                    c.Type.EndsWith("/role") ||
                    c.Type.EndsWith("/roles")
                )?.Value?.ToLower();
                Console.WriteLine($"ROL JWT: {rolClaim}");
                esAdmin = rolClaim != null && rolClaim.Contains("admin");
                Console.WriteLine($"esAdmin: {esAdmin}, todos: {todos}");
            }
            var productos = await _productoService.ObtenerTodosAsync(todos && esAdmin);
            return Ok(productos);
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        // GET: api/Productos/categoria/5
        [HttpGet("categoria/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ProductoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorCategoria(int id)
        {
            var productos = await _productoService.ObtenerPorCategoriaAsync(id);
            return Ok(productos);
        }

        // GET: api/Productos/buscar?termino=taladro&categoriaId=2&precioMin=1000&precioMax=5000
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(IEnumerable<ProductoDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarProductos(
            [FromQuery] string termino,
            [FromQuery] int? categoriaId,
            [FromQuery] decimal? precioMin,
            [FromQuery] decimal? precioMax)
        {
            var productos = await _productoService.BuscarProductosAsync(termino, categoriaId, precioMin, precioMax);
            return Ok(productos);
        }

        // POST: api/Productos
        [HttpPost]
        [Authorize(Roles = "Administrador,administrador")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCreateDTO productoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var id = await _productoService.CrearProductoAsync(productoDTO);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el producto");
            }
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoUpdateDTO productoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _productoService.ActualizarProductoAsync(id, productoDTO);

            if (!resultado)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var resultado = await _productoService.EliminarProductoAsync(id);

            if (!resultado)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/Productos/5/stock/10
        [HttpPatch("{id}/stock/{cantidad}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarStock(int id, int cantidad)
        {
            var resultado = await _productoService.ActualizarStockAsync(id, cantidad);

            if (!resultado)
            {
                return NotFound();
            }

            return NoContent();
        }
        // PUT: api/Productos/1/inventario
        [HttpPut("{id}/inventario")]
        [Authorize(Roles = "Administrador,administrador,vendedor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarInventario(int id, [FromBody] InventarioUpdateDTO inventarioDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _productoService.ActualizarInventarioAsync(id, inventarioDTO);

                if (!resultado)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Productos/{id}/subir-imagen
        [HttpPost("{id}/subir-imagen")]
        [Authorize(Roles = "Administrador,administrador")]
        public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest("No se envió ninguna imagen.");

            var nombreArchivo = $"{Guid.NewGuid()}_{imagen.FileName}";
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);
            var rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            // Actualiza el producto en la base de datos
            var actualizado = await _productoService.ActualizarImagenAsync(id, nombreArchivo);
            if (!actualizado)
                return NotFound();

            return Ok(new { nombreArchivo });
        }

        // POST: api/Productos/carga-masiva
        [HttpPost("carga-masiva")]
        [Authorize(Roles = "Administrador,administrador")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CargaMasiva([FromForm] IFormFile archivoCsv)
        {
            if (archivoCsv == null || archivoCsv.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            var productosCreados = new List<string>();
            var errores = new List<string>();
            var lineas = new List<string>();
            using (var reader = new StreamReader(archivoCsv.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var linea = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(linea))
                        lineas.Add(linea);
                }
            }

            // Detectar y saltar encabezado si la primera línea no es válida
            int fila = 1;
            if (lineas.Count > 0)
            {
                var primerLinea = lineas[0];
                var partes = primerLinea.Split(',');
                if (partes.Length < 6)
                    partes = primerLinea.Split(';');
                if (partes.Length >= 4 && !decimal.TryParse(partes[3], out _))
                {
                    // Es encabezado, saltar
                    lineas.RemoveAt(0);
                    fila++;
                }
            }

            foreach (var linea in lineas)
            {
                var partes = linea.Split(',');
                if (partes.Length < 6)
                    partes = linea.Split(';');
                if (partes.Length < 6)
                {
                    errores.Add($"Fila {fila}: Formato incorrecto");
                    fila++;
                    continue;
                }
                try
                {
                    // Validar precio
                    if (!decimal.TryParse(partes[3], out decimal precio) || precio <= 0)
                        throw new Exception("Precio inválido o menor o igual a cero");
                    // Validar IDs
                    if (!int.TryParse(partes[4], out int categoriaId))
                        throw new Exception("ID de categoría inválido");
                    if (!int.TryParse(partes[5], out int marcaId))
                        throw new Exception("ID de marca inválido");

                    // Validar existencia de categoría y marca
                    // (esto requiere acceso a los servicios, aquí se asume que existen métodos para validar)
                    // Puedes reemplazar por el servicio real si existe
                    var categoriaExiste = await _productoService.CategoriaExisteAsync(categoriaId);
                    var marcaExiste = await _productoService.MarcaExisteAsync(marcaId);
                    if (!categoriaExiste)
                        throw new Exception($"La categoría con ID {categoriaId} no existe");
                    if (!marcaExiste)
                        throw new Exception($"La marca con ID {marcaId} no existe");

                    // Validar código único
                    if (await _productoService.ProductoCodigoExisteAsync(partes[0]))
                        throw new Exception($"Ya existe un producto con el código '{partes[0]}'");

                    var productoDTO = new ProductoCreateDTO
                    {
                        Codigo = partes[0],
                        Nombre = partes[1],
                        Descripcion = partes[2],
                        Precio = precio,
                        CategoriaId = categoriaId,
                        MarcaId = marcaId,
                        ImagenUrl = partes.Length > 6 ? partes[6] : null,
                        Especificaciones = partes.Length > 7 ? partes[7] : null
                    };
                    await _productoService.CrearProductoAsync(productoDTO);
                    productosCreados.Add(productoDTO.Nombre);
                }
                catch (Exception ex)
                {
                    errores.Add($"Fila {fila}: {ex.Message}");
                }
                fila++;
            }
            return Ok(new { productosCreados, errores });
        }
    }
}
