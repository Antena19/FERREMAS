using Ferremas.Api.Modelos;

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
    }
} 