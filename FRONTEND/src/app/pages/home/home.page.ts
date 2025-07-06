import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router'; // ✅ Asegúrate de importar esto
// 📦 Importaciones necesarias para la vista
import { CommonModule } from '@angular/common';
import { IonSpinner } from '@ionic/angular/standalone';

@Component({
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
  standalone: true,
  imports: [CommonModule, IonSpinner,RouterModule], // ✅ Necesarios para usar *ngIf, *ngFor y <ion-spinner>
})
export class HomePage implements OnInit {
  productos: any[] = [];       // ✅ Lista de productos destacados (máx. 8)
  categorias: any[] = [];      // ✅ Lista de categorías visibles
  cargando: boolean = true;    // ⏳ Spinner mientras carga la vista

  constructor(
    private api: ApiService,     // 📡 Servicio para llamar a la API de productos
    private router: Router       // 🔁 Navegación a otras vistas
  ) {}

  ngOnInit(): void {
    // 🔄 Obtener los productos desde la API
    this.api.getProductos().subscribe({
      next: (res) => {
        this.productos = res.slice(0, 8); // ✅ Mostrar los primeros 8 productos
        this.cargando = false;
      },
      error: (err) => {
        console.error('❌ Error al cargar productos', err);
        this.cargando = false;
      }
    });

    // 📚 Cargar categorías reales basadas en tus productos actuales
    this.categorias = [
      { nombre: 'Herramientas Eléctricas', icono: 'bi bi-lightning-charge-fill' },
      { nombre: 'Herramientas Manuales', icono: 'bi bi-hammer' },
      { nombre: 'Materiales de Construcción', icono: 'bi bi-bricks' },
      { nombre: 'Pinturas y Acabados', icono: 'bi bi-palette-fill' },
      { nombre: 'Seguridad Industrial', icono: 'bi bi-shield-check' },
    ];
  }

  // 🛒 Acción al hacer clic en "Agregar al carrito"
  agregarAlCarrito(producto: any): void {
    console.log('Agregar al carrito:', producto.nombre);
  }

  // 🔁 Acción cuando se hace clic en "Ver más"
  verMas(): void {
    this.router.navigate(['/productos']);
  }
}
