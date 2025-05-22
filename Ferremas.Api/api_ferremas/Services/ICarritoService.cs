using System.Threading.Tasks;
using Ferremas.Api.Modelos;

namespace Ferremas.Api.Services
{
    public interface ICarritoService
    {
        Task<Carrito> ObtenerCarrito(int usuarioId);
        Task<Carrito> AgregarItem(int usuarioId, int productoId, int cantidad);
        Task<Carrito> ActualizarCantidad(int usuarioId, int itemId, int cantidad);
        Task<Carrito> EliminarItem(int usuarioId, int itemId);
        Task VaciarCarrito(int usuarioId);
        Task<Carrito> CalcularTotales(Carrito carrito);
    }
} 