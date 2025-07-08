import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';       // 📡 Servicio de conexión al backend
import { AuthService } from 'src/app/services/auth.service';     // 🔐 Servicio para obtener info del usuario logueado
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonButton,
  IonIcon,
  IonSpinner
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-carrito',
  templateUrl: './carrito.page.html',
  styleUrls: ['./carrito.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonButton,
    IonIcon,
    IonSpinner
  ]
})
export class CarritoPage implements OnInit {

  productos: any[] = [];           // 🛍️ Productos cargados del carrito
  cargando: boolean = true;        // ⏳ Muestra spinner mientras carga
  usuarioLogeado: boolean = false; // 👤 Indica si el usuario está autenticado

  constructor(
    private api: ApiService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarCarrito();          // 🔄 Al cargar la vista, traemos los productos
  }

  /**
   * 🔄 Carga los productos del carrito desde el backend
   */
  cargarCarrito(): void {
    const idUsuario = this.auth.obtenerIdUsuario(); // ✅ Usamos sesión real
    this.usuarioLogeado = idUsuario !== null; // 👤 Actualizar estado de autenticación

    if (idUsuario !== null) {
      this.api.getCarritoPorUsuario(idUsuario).subscribe({
        next: (res: any) => {
          console.log('🛒 Carrito recibido:', res);

          // ✅ Detectamos estructura de respuesta
          if (Array.isArray(res.productos)) {
            this.productos = res.productos;
          } else if (Array.isArray(res.items)) {
            this.productos = res.items;
          } else if (Array.isArray(res.detalle)) {
            this.productos = res.detalle;
          } else if (Array.isArray(res)) {
            this.productos = res;
          } else {
            console.warn('⚠️ No se encontraron productos en el carrito');
            this.productos = [];
          }

          this.cargando = false;
        },
        error: (err) => {
          console.error('❌ Error al cargar carrito:', err);
          this.cargando = false;
        }
      });
    } else {
      console.warn('⚠️ Usuario no autenticado');
      this.productos = []; // 🛒 Carrito vacío para usuarios no logueados
      this.cargando = false;
    }
  }

  /**
   * 🗑 Elimina un producto del carrito (backend + frontend)
   * @param productoId ID del producto a eliminar
   */
  eliminarProducto(productoId: number): void {
    const idUsuario = this.auth.obtenerIdUsuario();

    if (idUsuario !== null) {
      this.api.eliminarDelCarrito(idUsuario, productoId).subscribe({
        next: () => {
          // ✅ Quitamos el producto del arreglo local usando producto_id
          this.productos = this.productos.filter(p => p.producto_id !== productoId);
        },
        error: (err) => {
          console.error('❌ Error al eliminar producto:', err);
        }
      });
    }
  }

  /**
   * 🧾 Confirma el pedido de todos los productos del carrito
   */
  confirmarPedido(): void {
    const idUsuario = this.auth.obtenerIdUsuario();

    if (idUsuario !== null) {
      this.api.confirmarPedido(idUsuario).subscribe({
        next: () => {
          this.productos = []; // 🧹 Limpiar el carrito
          alert('✅ Pedido confirmado exitosamente');
        },
        error: (err) => {
          console.error('❌ Error al confirmar pedido:', err);
        }
      });
    }
  }
}
