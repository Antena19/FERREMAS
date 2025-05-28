import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CarritoService } from '../../../servicios/carrito.service';
import { Carrito } from '../../../modelos/carrito.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-mini-carrito',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mini-carrito.component.html',
  styleUrls: ['./mini-carrito.component.css']
})
export class MiniCarritoComponent implements OnInit, OnDestroy {
  carrito: Carrito = {
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  };
  totalItems = 0;
  mostrarDropdown = false;
  private carritoSubscription: Subscription = new Subscription();

  constructor(private carritoService: CarritoService, private router: Router) { }

  ngOnInit(): void {
    console.log('Inicializando mini-carrito...');
    
    // Suscribirse a los cambios del carrito
    this.carritoSubscription = this.carritoService.carrito$.subscribe({
      next: (carrito) => {
        console.log('Mini-carrito actualizado:', carrito);
        this.carrito = carrito;
        this.calcularTotalItems();
      },
      error: (error) => {
        console.error('Error en mini-carrito:', error);
      }
    });

    // Cargar carrito inicial
    console.log('Solicitando carga inicial del carrito en mini-carrito...');
    this.carritoService.obtenerCarrito().subscribe({
      next: (carrito) => {
        console.log('Carrito inicial cargado en mini-carrito:', carrito);
      },
      error: (error) => {
        console.error('Error al cargar carrito inicial en mini-carrito:', error);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.carritoSubscription) {
      this.carritoSubscription.unsubscribe();
    }
  }

  calcularTotalItems(): void {
    this.totalItems = this.carrito.items.reduce((sum, item) => sum + item.cantidad, 0);
    console.log("Total de items en el mini-carrito:", this.totalItems);
  }

  toggleDropdown(): void {
    this.mostrarDropdown = !this.mostrarDropdown;
  }

  cerrarDropdown(): void {
    this.mostrarDropdown = false;
  }

  irAlCarrito() {
    this.cerrarDropdown();
    this.router.navigate(['/cliente/carrito']);
  }

  irAlCheckout() {
    this.cerrarDropdown();
    this.router.navigate(['/cliente/checkout']);
  }

  onImgError(event: Event) {
    const img = event.target as HTMLImageElement;
    if (!img.src.endsWith('no-imagen.png')) {
      img.src = 'assets/no-imagen.png';
    }
  }
}