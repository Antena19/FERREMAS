﻿using System.ComponentModel.DataAnnotations;

namespace Ferremas.Api.DTOs
{
    public class ProductoUpdateDTO
    {
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        [Range(1, 1000000000, ErrorMessage = "El precio debe ser mayor a $1 y menor a mil millones.")]
        public decimal Precio { get; set; }

        public int? CategoriaId { get; set; }

        public int? MarcaId { get; set; }

        public string ImagenUrl { get; set; }

        public string Especificaciones { get; set; }

        public bool Activo { get; set; }
    }
}