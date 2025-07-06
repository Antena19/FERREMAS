import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { ImageMappingService } from 'src/app/services/image-mapping.service';
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
export class HomePage implements OnInit, OnDestroy {
  productos: any[] = [];       // ✅ Lista de productos destacados (máx. 8)
  categorias: any[] = [];      // ✅ Lista de categorías visibles
  cargando: boolean = true;    // ⏳ Spinner mientras carga la vista

  // Carrusel
  productoActual = 0;
  carruselInterval: any;

  constructor(
    private api: ApiService,     // 📡 Servicio para llamar a la API de productos
    private imageMapping: ImageMappingService, // 🖼 Servicio para mapear imágenes
    private router: Router       // 🔁 Navegación a otras vistas
  ) {}

  ngOnInit(): void {
    // Obtener productos
    this.api.getProductos().subscribe({
      next: (res) => {
        this.productos = res.slice(0, 8);
        this.cargando = false;
        this.iniciarCarrusel();
      },
      error: (err) => {
        console.error('❌ Error al cargar productos', err);
        this.cargando = false;
      }
    });

    // Obtener categorías desde la API
    this.api.getCategorias().subscribe({
      next: (res) => {
        this.categorias = res;
      },
      error: (err) => {
        console.error('❌ Error al cargar categorías', err);
      }
    });
  }

  // Carrusel automático
  iniciarCarrusel() {
    if (this.carruselInterval) clearInterval(this.carruselInterval);
    this.carruselInterval = setInterval(() => {
      this.siguienteProducto();
    }, 5000); // 5 segundos
  }

  siguienteProducto() {
    if (this.productos.length === 0) return;
    this.productoActual = (this.productoActual + 1) % this.productos.length;
  }

  anteriorProducto() {
    if (this.productos.length === 0) return;
    this.productoActual = (this.productoActual - 1 + this.productos.length) % this.productos.length;
  }

  ngOnDestroy(): void {
    if (this.carruselInterval) clearInterval(this.carruselInterval);
  }

  // 🛒 Acción al hacer clic en "Agregar al carrito"
  agregarAlCarrito(producto: any): void {
    console.log('Agregar al carrito:', producto.nombre);
  }

  // 🔁 Acción cuando se hace clic en "Ver más"
  verMas(): void {
    this.router.navigate(['/productos']);
  }

  // Navegar a productos filtrados por categoría
  verCategoria(categoria: any): void {
    this.router.navigate(['/productos'], { queryParams: { categoriaId: categoria.id, categoria: categoria.nombre } });
  }

  // Ver todos los productos
  verTodos(): void {
    this.router.navigate(['/productos']);
  }

  // 🖼 Obtener la ruta correcta de la imagen
  getImagePath(producto: any): string {
    return 'assets/img/' + producto.imagenUrl;
  }

  // Ir al detalle del producto
  verDetalle(producto: any): void {
    this.router.navigate(['/detalle-producto', producto.id]);
  }

  // Devuelve el índice del producto anterior en el carrusel
  prevIndex(): number {
    if (this.productos.length === 0) return 0;
    return (this.productoActual - 1 + this.productos.length) % this.productos.length;
  }

  // Devuelve el índice del producto siguiente en el carrusel
  nextIndex(): number {
    if (this.productos.length === 0) return 0;
    return (this.productoActual + 1) % this.productos.length;
  }

  // Devuelve el estilo transform para el carrusel coverflow
  getCarruselTransform(i: number): string {
    const total = this.productos.length;
    if (total === 0) return '';
    let pos = i - this.productoActual;
    if (pos > total / 2) pos -= total;
    if (pos < -total / 2) pos += total;
    // Card principal
    if (pos === 0) {
      return 'translate(-50%, -50%) scale(1.05) perspective(600px) translateZ(10px)';
    }
    // Laterales inmediatas
    if (pos === -1) {
      return 'translate(-150%, -50%) scale(0.85) perspective(600px) translateZ(-20px)';
    }
    if (pos === 1) {
      return 'translate(50%, -50%) scale(0.85) perspective(600px) translateZ(-20px)';
    }
    // Más lejos
    return `translate(${pos * 110 - 50}%, -50%) scale(0.6) perspective(600px) translateZ(-60px)`;
  }

  // Devuelve el z-index para cada tarjeta
  getCarruselZIndex(i: number): number {
    const total = this.productos.length;
    if (total === 0) return 1;
    let pos = i - this.productoActual;
    if (pos > total / 2) pos -= total;
    if (pos < -total / 2) pos += total;
    if (pos === 0) return 3;
    if (Math.abs(pos) === 1) return 2;
    return 1;
  }

  // Devuelve la opacidad para el carrusel (solo principal y laterales inmediatas visibles)
  getCarruselOpacity(i: number): number {
    const total = this.productos.length;
    if (total === 0) return 1;
    let pos = i - this.productoActual;
    if (pos > total / 2) pos -= total;
    if (pos < -total / 2) pos += total;
    return Math.abs(pos) <= 1 ? 1 : 0;
  }
}
