import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CarritoService } from '../../servicios/carrito.service';
import { Carrito, ItemCarrito } from '../../modelos/carrito.model';
import { NotificacionService } from '../../servicios/notificacion.service';


@Component({
  selector: 'app-carrito',
  templateUrl: './carrito.component.html',
  styleUrls: ['./carrito.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule]
})
export class CarritoComponent implements OnInit, OnDestroy {
  carrito: Carrito = {
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  };
  cargando = true;
  error = '';
  private carritoSubscription: any;
  private LOCAL_KEY = 'carrito_local';

  constructor(
    private carritoService: CarritoService,
    private notificacionService: NotificacionService,
    private router: Router
  ) { }

  ngOnInit(): void {
    console.log('Inicializando componente de carrito...');
    this.cargando = true;

    // Suscribirse a los cambios del carrito
    this.carritoSubscription = this.carritoService.carrito$.subscribe({
      next: (carrito) => {
        console.log('Carrito actualizado en componente:', carrito);
        this.carrito = carrito;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error en suscripción del carrito:', error);
        this.error = 'Error al cargar el carrito';
        this.cargando = false;
        this.notificacionService.error(this.error);
      }
    });

    // Cargar carrito inicial
    console.log('Solicitando carga inicial del carrito...');
    this.carritoService.obtenerCarrito().subscribe({
      next: (carrito) => {
        console.log('Carrito inicial cargado:', carrito);
      },
      error: (error) => {
        console.error('Error al cargar carrito inicial:', error);
        this.error = 'Error al cargar el carrito';
        this.notificacionService.error(this.error);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.carritoSubscription) {
      this.carritoSubscription.unsubscribe();
    }
  }

  actualizarCantidad(item: ItemCarrito, cantidad: number): void {
    if (cantidad <= 0) {
      this.eliminarProducto(item.productoId);
      return;
    }
    this.carritoService.actualizarCantidad(item.productoId, cantidad).subscribe({
      next: () => {
        this.notificacionService.exito('Cantidad actualizada');
      },
      error: (error: Error) => {
        this.notificacionService.error('Error al actualizar la cantidad');
      }
    });
  }

  eliminarProducto(productoId: number): void {
    this.carritoService.eliminarProducto(productoId).subscribe({
      next: () => {
        this.notificacionService.exito('Producto eliminado del carrito');
      },
      error: (error: Error) => {
        this.notificacionService.error('Error al eliminar el producto');
      }
    });
  }

  eliminarItem(productoId: number): void {
    this.eliminarProducto(productoId);
  }

  limpiarCarrito(): void {
    this.vaciarCarrito();
  }

  vaciarCarrito(): void {
    this.carritoService.vaciarCarrito().subscribe({
      next: () => {
        this.notificacionService.exito('Carrito vaciado correctamente');
      },
      error: (error: Error) => {
        this.notificacionService.error('Error al vaciar el carrito');
      }
    });
  }

  irAProductos(): void {
    this.router.navigate(['/catalogo']);
  }

  procederAlPago(): void {
    this.router.navigate(['/cliente/checkout']);
  }
} 