import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';                // 📥 Capturar ID de la URL
import { ApiService } from 'src/app/services/api.service';      // 📡 Servicio para obtener el producto
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-detalle-producto',
  standalone: true,
  templateUrl: './detalle-producto.page.html',
  styleUrls: ['./detalle-producto.page.scss'],
  imports: [CommonModule, IonicModule, RouterModule, FormsModule]
})
export class DetalleProductoPage implements OnInit {
  producto: any = null;       // 🧩 Producto cargado
  cargando: boolean = true;   // ⏳ Spinner mientras carga
  esAdmin: boolean = false;    // Cambia esto por tu lógica real de autenticación
  sucursales: any[] = [];
  sucursalSeleccionada: any = null;
  inventario: any = null;       // Inventario del producto
  categorias: any[] = [];
  marcas: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const usuario = this.auth.obtenerUsuario();
    const rol = (usuario?.rol || '').toLowerCase();
    this.esAdmin = rol.includes('admin');
    console.log('Usuario:', usuario);
    console.log('esAdmin:', this.esAdmin);

    // Cargar categorías y marcas para los selectores
    this.api.getCategorias().subscribe(cats => this.categorias = cats);
    this.api.getMarcas().subscribe(marcas => this.marcas = marcas);

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.api.getProductoPorId(+id).subscribe({
        next: (res) => {
          this.producto = res;
          console.log('Producto detalle:', this.producto);
          if (this.esAdmin) {
            this.cargarSucursalesYInventario(+id);
          }
          this.cargando = false;
        },
        error: () => {
          console.error('❌ Error al obtener producto');
          this.cargando = false;
        }
      });
    }
  }

  cargarSucursalesYInventario(productoId: number) {
    this.api.getSucursales().subscribe({
      next: (res) => {
        this.sucursales = res.data;
        if (this.sucursales.length > 0) {
          this.sucursalSeleccionada = this.sucursales[0];
          this.cargarInventario(productoId, this.sucursales[0].id);
        }
      },
      error: () => {
        console.error('❌ Error al cargar sucursales');
      }
    });
  }

  onSucursalChange(productoId: number) {
    if (this.sucursalSeleccionada) {
      this.cargarInventario(productoId, this.sucursalSeleccionada.id);
    }
  }

  cargarInventario(productoId: number, sucursalId: number) {
    this.api.getInventarioPorProductoYSucursal(productoId, sucursalId).subscribe(inv => {
      this.inventario = inv;
    });
  }

  agregarAlCarrito(): void {
    console.log('🛒 Agregado al carrito:', this.producto?.nombre);
  }

  getImagePath(producto: any): string {
    return 'assets/img/' + (producto.imagenUrl || producto.imagen_url || 'default.png');
  }

  guardarCambios() {
    if (!this.producto) return;

    const productoActualizado = {
      nombre: this.producto.nombre,
      descripcion: this.producto.descripcion,
      precio: this.producto.precio,
      categoriaId: this.producto.categoriaId,
      marcaId: this.producto.marcaId,
      imagenUrl: this.producto.imagenUrl,
      especificaciones: this.producto.especificaciones,
      activo: this.producto.activo
    };

    this.api.actualizarProducto(this.producto.id, productoActualizado).subscribe({
      next: () => {
        console.log('✅ Producto actualizado exitosamente');
        // Aquí podrías mostrar un toast o alert de éxito
        alert('Producto actualizado exitosamente');
      },
      error: (error) => {
        console.error('❌ Error al actualizar producto:', error);
        alert('Error al actualizar el producto');
      }
    });
  }
}
  