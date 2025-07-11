﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferremas.Api.Modelos
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        public DateTime FechaPedido { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoEntrega { get; set; }

        public int? SucursalId { get; set; }

        [ForeignKey("SucursalId")]
        public Sucursal Sucursal { get; set; }

        public int? DireccionId { get; set; }

        [ForeignKey("DireccionId")]
        public Direccion Direccion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostoEnvio { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Impuestos { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }

        public string Notas { get; set; }

        public int? VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Usuario Vendedor { get; set; }

        public int? BodegueroId { get; set; }

        [ForeignKey("BodegueroId")]
        public Usuario Bodeguero { get; set; }

        // Propiedades adicionales para pedidos_vendedor
        public DateTime? FechaAsignacion { get; set; }
        public string EstadoAsignacion { get; set; }
        public decimal? ComisionCalculada { get; set; }

        // Propiedades para nombres (no mapeadas a la base de datos)
        [NotMapped]
        public string UsuarioNombre { get; set; }

        [NotMapped]
        public string SucursalNombre { get; set; }

        [NotMapped]
        public string VendedorNombre { get; set; }

        [NotMapped]
        public string BodegueroNombre { get; set; }

        // Propiedades de navegación
        public virtual Cliente Cliente { get; set; }
        public virtual Direccion DireccionEntrega { get; set; }
        public virtual ICollection<PedidoItem> Items { get; set; }
    }
}