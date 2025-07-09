using Ferremas.Api.Services;
using Ferremas.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferremas.Api.Controllers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }

    [Authorize(Roles = "Administrador,administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Endpoints para usuarios
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetAllUsuarios()
        {
            var usuarios = await _adminService.GetAllUsuarios();
            return Ok(ApiResponse<IEnumerable<Usuario>>.Ok(usuarios, "Lista de usuarios obtenida exitosamente"));
        }

        [HttpGet("usuarios/{id}")]
        public async Task<IActionResult> GetUsuarioById(int id)
        {
            var usuario = await _adminService.GetUsuarioById(id);
            if (usuario == null)
                return NotFound(ApiResponse<Usuario>.Error($"No se encontró el usuario con ID {id}"));
            return Ok(ApiResponse<Usuario>.Ok(usuario, "Usuario encontrado exitosamente"));
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioRequest request)
        {
            try
            {
                var usuario = await _adminService.CreateUsuario(request.Usuario, request.Rol);
                return Ok(ApiResponse<Usuario>.Ok(usuario, "Usuario creado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Usuario>.Error($"Error al crear usuario: {ex.Message}"));
            }
        }

        [HttpPut("usuarios")]
        public async Task<IActionResult> UpdateUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var usuarioActualizado = await _adminService.UpdateUsuario(usuario);
                return Ok(ApiResponse<Usuario>.Ok(usuarioActualizado, "Usuario actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Usuario>.Error($"Error al actualizar usuario: {ex.Message}"));
            }
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var resultado = await _adminService.DeleteUsuario(id);
                if (!resultado)
                    return NotFound(ApiResponse<bool>.Error($"No se encontró el usuario con ID {id} para eliminar"));
                return Ok(ApiResponse<bool>.Ok(true, "Usuario eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Error($"Error al eliminar usuario: {ex.Message}"));
            }
        }

        // Endpoints para clientes
        [HttpGet("clientes")]
        public async Task<IActionResult> GetAllClientes()
        {
            var clientes = await _adminService.GetAllClientes();
            return Ok(ApiResponse<IEnumerable<Cliente>>.Ok(clientes, "Lista de clientes obtenida exitosamente"));
        }

        [HttpGet("clientes/{id}")]
        public async Task<IActionResult> GetClienteById(int id)
        {
            var cliente = await _adminService.GetClienteById(id);
            if (cliente == null)
                return NotFound(ApiResponse<Cliente>.Error($"No se encontró el cliente con ID {id}"));
            return Ok(ApiResponse<Cliente>.Ok(cliente, "Cliente encontrado exitosamente"));
        }

        [HttpPost("clientes")]
        public async Task<IActionResult> CreateCliente([FromBody] Cliente cliente)
        {
            try
            {
                var clienteCreado = await _adminService.CreateCliente(cliente);
                return Ok(ApiResponse<Cliente>.Ok(clienteCreado, "Cliente creado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Cliente>.Error($"Error al crear cliente: {ex.Message}"));
            }
        }

        [HttpPut("clientes")]
        public async Task<IActionResult> UpdateCliente([FromBody] Cliente cliente)
        {
            try
            {
                var clienteActualizado = await _adminService.UpdateCliente(cliente);
                return Ok(ApiResponse<Cliente>.Ok(clienteActualizado, "Cliente actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Cliente>.Error($"Error al actualizar cliente: {ex.Message}"));
            }
        }

        [HttpDelete("clientes/{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var resultado = await _adminService.DeleteCliente(id);
                if (!resultado)
                    return NotFound(ApiResponse<bool>.Error($"No se encontró el cliente con ID {id} para eliminar"));
                return Ok(ApiResponse<bool>.Ok(true, "Cliente eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Error($"Error al eliminar cliente: {ex.Message}"));
            }
        }

        // Endpoint para obtener perfil completo del usuario
        [HttpGet("perfil/{id}")]
        public async Task<IActionResult> GetPerfilUsuario(int id)
        {
            try
            {
                var perfil = await _adminService.GetPerfilUsuario(id);
                if (perfil == null)
                    return NotFound(ApiResponse<object>.Error($"No se encontró el perfil del usuario con ID {id}"));
                return Ok(ApiResponse<object>.Ok(perfil, "Perfil obtenido exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error($"Error al obtener perfil: {ex.Message}"));
            }
        }

        // Endpoint público para obtener perfil del usuario autenticado
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> GetMiPerfil()
        {
            try
            {
                // Obtener el ID del usuario desde el token JWT
                var userIdClaim = User.FindFirst("userId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Error("No se pudo identificar al usuario"));
                }

                var perfil = await _adminService.GetPerfilUsuario(userId);
                if (perfil == null)
                    return NotFound(ApiResponse<object>.Error($"No se encontró el perfil del usuario"));
                return Ok(ApiResponse<object>.Ok(perfil, "Perfil obtenido exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error($"Error al obtener perfil: {ex.Message}"));
            }
        }

        // Endpoints para vendedores
        [HttpGet("vendedores")]
        public async Task<IActionResult> GetAllVendedores()
        {
            var vendedores = await _adminService.GetAllVendedores();
            return Ok(vendedores);
        }

        [HttpGet("vendedores/{id}")]
        public async Task<IActionResult> GetVendedorById(int id)
        {
            var vendedor = await _adminService.GetVendedorById(id);
            if (vendedor == null)
                return NotFound();
            return Ok(vendedor);
        }

        [HttpPost("vendedores")]
        public async Task<IActionResult> CreateVendedor([FromBody] Vendedor vendedor)
        {
            try
            {
                var vendedorCreado = await _adminService.CreateVendedor(vendedor);
                return Ok(vendedorCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("vendedores")]
        public async Task<IActionResult> UpdateVendedor([FromBody] Vendedor vendedor)
        {
            try
            {
                var vendedorActualizado = await _adminService.UpdateVendedor(vendedor);
                return Ok(vendedorActualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("vendedores/{id}")]
        public async Task<IActionResult> DeleteVendedor(int id)
        {
            var resultado = await _adminService.DeleteVendedor(id);
            if (!resultado)
                return NotFound();
            return Ok();
        }

        // Endpoints para bodegueros
        [HttpGet("bodegueros")]
        public async Task<IActionResult> GetAllBodegueros()
        {
            var bodegueros = await _adminService.GetAllBodegueros();
            return Ok(bodegueros);
        }

        [HttpGet("bodegueros/{id}")]
        public async Task<IActionResult> GetBodegueroById(int id)
        {
            var bodeguero = await _adminService.GetBodegueroById(id);
            if (bodeguero == null)
                return NotFound();
            return Ok(bodeguero);
        }

        [HttpPost("bodegueros")]
        public async Task<IActionResult> CreateBodeguero([FromBody] Bodeguero bodeguero)
        {
            try
            {
                var bodegueroCreado = await _adminService.CreateBodeguero(bodeguero);
                return Ok(bodegueroCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("bodegueros")]
        public async Task<IActionResult> UpdateBodeguero([FromBody] Bodeguero bodeguero)
        {
            try
            {
                var bodegueroActualizado = await _adminService.UpdateBodeguero(bodeguero);
                return Ok(bodegueroActualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("bodegueros/{id}")]
        public async Task<IActionResult> DeleteBodeguero(int id)
        {
            var resultado = await _adminService.DeleteBodeguero(id);
            if (!resultado)
                return NotFound();
            return Ok();
        }

        // Endpoints para contadores
        [HttpGet("contadores")]
        public async Task<IActionResult> GetAllContadores()
        {
            var contadores = await _adminService.GetAllContadores();
            return Ok(contadores);
        }

        [HttpGet("contadores/{id}")]
        public async Task<IActionResult> GetContadorById(int id)
        {
            var contador = await _adminService.GetContadorById(id);
            if (contador == null)
                return NotFound();
            return Ok(contador);
        }

        [HttpPost("contadores")]
        public async Task<IActionResult> CreateContador([FromBody] Contador contador)
        {
            try
            {
                var contadorCreado = await _adminService.CreateContador(contador);
                return Ok(contadorCreado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("contadores")]
        public async Task<IActionResult> UpdateContador([FromBody] Contador contador)
        {
            try
            {
                var contadorActualizado = await _adminService.UpdateContador(contador);
                return Ok(contadorActualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("contadores/{id}")]
        public async Task<IActionResult> DeleteContador(int id)
        {
            var resultado = await _adminService.DeleteContador(id);
            if (!resultado)
                return NotFound();
            return Ok();
        }

        // Endpoints para activar/desactivar usuarios
        [HttpPut("usuarios/{id}/activar")]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            try
            {
                var resultado = await _adminService.ActivarUsuario(id);
                if (!resultado)
                    return NotFound(ApiResponse<bool>.Error($"No se encontró el usuario con ID {id} para activar"));
                return Ok(ApiResponse<bool>.Ok(true, "Usuario activado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Error($"Error al activar usuario: {ex.Message}"));
            }
        }

        [HttpPut("usuarios/{id}/desactivar")]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            try
            {
                var resultado = await _adminService.DesactivarUsuario(id);
                if (!resultado)
                    return NotFound(ApiResponse<bool>.Error($"No se encontró el usuario con ID {id} para desactivar"));
                return Ok(ApiResponse<bool>.Ok(true, "Usuario desactivado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.Error($"Error al desactivar usuario: {ex.Message}"));
            }
        }

        // Endpoint para obtener todas las sucursales
        [HttpGet("sucursales")]
        public async Task<IActionResult> GetAllSucursales()
        {
            try
            {
                var sucursales = await _adminService.GetAllSucursales();
                return Ok(ApiResponse<IEnumerable<Sucursal>>.Ok(sucursales, "Lista de sucursales obtenida exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<Sucursal>>.Error($"Error al obtener sucursales: {ex.Message}"));
            }
        }

        // Endpoint público para obtener sucursales activas
        [AllowAnonymous]
        [HttpGet("sucursales/activas")]
        public async Task<IActionResult> GetSucursalesActivas()
        {
            var sucursales = await _adminService.GetAllSucursales(); // O ajusta para filtrar solo activas si tienes ese método
            return Ok(sucursales);
        }
    }

    public class CreateUsuarioRequest
    {
        public Usuario Usuario { get; set; }
        public string Rol { get; set; }
    }
} 