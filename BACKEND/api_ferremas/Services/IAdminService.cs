using Ferremas.Api.Modelos;
using Ferremas.Api.DTOs;

namespace Ferremas.Api.Services
{
    public interface IAdminService
    {
        // Gestión de usuarios por rol
        Task<IEnumerable<Usuario>> GetAllUsuarios();
        Task<IEnumerable<Cliente>> GetAllClientes();
        Task<IEnumerable<Vendedor>> GetAllVendedores();
        Task<IEnumerable<Bodeguero>> GetAllBodegueros();
        Task<IEnumerable<Contador>> GetAllContadores();

        // Obtener usuario específico
        Task<Usuario> GetUsuarioById(int id);
        Task<Cliente> GetClienteById(int id);
        Task<Vendedor> GetVendedorById(int id);
        Task<Bodeguero> GetBodegueroById(int id);
        Task<Contador> GetContadorById(int id);

        // Obtener perfil completo del usuario
        Task<PerfilUsuarioDTO> GetPerfilUsuario(int id);

        // Crear usuarios
        Task<Usuario> CreateUsuario(Usuario usuario, string rol);
        Task<Cliente> CreateCliente(Cliente cliente);
        Task<Vendedor> CreateVendedor(Vendedor vendedor);
        Task<Bodeguero> CreateBodeguero(Bodeguero bodeguero);
        Task<Contador> CreateContador(Contador contador);

        // Actualizar usuarios
        Task<Usuario> UpdateUsuario(Usuario usuario);
        Task<Cliente> UpdateCliente(Cliente cliente);
        Task<Vendedor> UpdateVendedor(Vendedor vendedor);
        Task<Bodeguero> UpdateBodeguero(Bodeguero bodeguero);
        Task<Contador> UpdateContador(Contador contador);

        // Eliminar usuarios
        Task<bool> DeleteUsuario(int id);
        Task<bool> DeleteCliente(int id);
        Task<bool> DeleteVendedor(int id);
        Task<bool> DeleteBodeguero(int id);
        Task<bool> DeleteContador(int id);

        // Cambiar estado de usuarios
        Task<bool> ActivarUsuario(int id);
        Task<bool> DesactivarUsuario(int id);

        // ============================
        // 🏠 GESTIÓN DE DIRECCIONES
        // ============================
        Task<IEnumerable<DireccionDTO>> GetDireccionesUsuario(int usuarioId);
        Task<DireccionDTO> GetDireccionById(int id);
        Task<DireccionDTO> CrearDireccion(DireccionDTO direccion);
        Task<DireccionDTO> ActualizarDireccion(int id, DireccionDTO direccion);
        Task<bool> EliminarDireccion(int id);

        // ============================
        // 👤 GESTIÓN DE DATOS PERSONALES
        // ============================
        Task<bool> ActualizarDatosPersonales(int usuarioId, DatosPersonalesDTO datos);
    }
} 