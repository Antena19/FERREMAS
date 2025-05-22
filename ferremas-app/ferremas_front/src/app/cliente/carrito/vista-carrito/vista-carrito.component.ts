import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CarritoService } from '../../../servicios/carrito.service';
import { Carrito, ItemCarrito } from '../../../modelos/carrito.model';
import { AuthService } from '../../../servicios/auth.service';

@Component({
  selector: 'app-vista-carrito',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './vista-carrito.component.html',
  styleUrls: ['./vista-carrito.component.css']
})
export class VistaCarritoComponent implements OnInit {
  carrito: Carrito = {
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  };
  usuarioAutenticado = false;

  constructor(
    private carritoService: CarritoService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Suscribirse a los cambios de autenticación
    this.authService.usuario$.subscribe(usuario => {
      this.usuarioAutenticado = !!usuario;
    });

    // Suscribirse a los cambios del carrito
    this.carritoService.carrito$.subscribe(carrito => {
      this.carrito = carrito;
    });
  }

  actualizarCantidad(item: ItemCarrito, cantidad: number): void {
    if (cantidad > 0) {
      this.carritoService.actualizarCantidad(item.productoId, cantidad);
    }
  }

  eliminarItem(item: ItemCarrito): void {
    this.carritoService.eliminarProducto(item.productoId);
  }

  vaciarCarrito(): void {
    if (confirm('¿Estás seguro de que deseas vaciar el carrito?')) {
      this.carritoService.vaciarCarrito();
    }
  }

  continuarComprando(): void {
    this.router.navigate(['/catalogo']);
  }

  procederAlCheckout(): void {
    if (this.usuarioAutenticado) {
      this.router.navigate(['/cliente/checkout']);
    }
  }
}