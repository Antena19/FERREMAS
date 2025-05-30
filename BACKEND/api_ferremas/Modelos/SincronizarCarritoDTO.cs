using System.Collections.Generic;

namespace Ferremas.Api.Modelos
{
    public class SincronizarCarritoDTO
    {
        public int UsuarioId { get; set; }
        public List<ItemSincronizarDTO> Items { get; set; }
    }
} 