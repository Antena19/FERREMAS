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
    this.cargando = true;

    const data = localStorage.getItem(this.LOCAL_KEY);
    console.log("carrito cargado");
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