using System.Threading.Tasks;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> ObtenerPorIdAsync(int id);
        Task<Usuario> ObtenerPorEmailAsync(string email);
        Task<Usuario> ObtenerPorRutAsync(string rut);
        Task<bool> EmailExisteAsync(string email);
        Task<bool> RutExisteAsync(string rut);
        Task<Usuario> CrearUsuarioAsync(Usuario usuario);
        Task<bool> ActualizarUsuarioAsync(Usuario usuario);
        Task<bool> EliminarUsuarioAsync(int id);
        Task<bool> CambiarEstadoAsync(int id, bool activo);
        Task<bool> ActualizarUltimoAccesoAsync(int id);
    }
} 