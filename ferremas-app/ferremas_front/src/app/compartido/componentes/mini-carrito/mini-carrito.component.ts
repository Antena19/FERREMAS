import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CarritoService } from '../../../servicios/carrito.service';
import { Carrito } from '../../../modelos/carrito.model';

@Component({
  selector: 'app-mini-carrito',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mini-carrito.component.html',
  styleUrls: ['./mini-carrito.component.css']
})
export class MiniCarritoComponent implements OnInit {
  carrito: Carrito = {
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  };
  totalItems = 0;
  mostrarDropdown = false;

  constructor(private carritoService: CarritoService, private router: Router) { }

  ngOnInit(): void {
    // Suscribirse a los cambios del carrito
    this.carritoService.carrito$.subscribe(carrito => {
      console.log('Carrito actualizado:', carrito);
      this.carrito = carrito;
      this.calcularTotalItems();
    });
  }

  calcularTotalItems(): void {
    this.totalItems = this.carrito.items.reduce((sum, item) => sum + item.cantidad, 0);
    console.log("Total de items en el carrito:",this.totalItems);
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
}