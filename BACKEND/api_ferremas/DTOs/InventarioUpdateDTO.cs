using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class InventarioUpdateDTO
    {
        [Required]
        public int SucursalId { get; set; }

        [Required]
        public int Stock { get; set; }
    }
} 