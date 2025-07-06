import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';     // 📡 Servicio que interactúa con la API
import { Router, ActivatedRoute } from '@angular/router';      // 🔁 Navegación y lectura de parámetros
import { CommonModule } from '@angular/common';                // ✅ Para usar *ngIf, *ngFor, etc.
import { IonicModule } from '@ionic/angular';                  // ✅ Componentes de Ionic
import { RouterModule } from '@angular/router';                // ✅ Para usar routerLink
import { FormsModule } from '@angular/forms';                  // ✅ Para ngModel en el buscador

@Component({
  selector: 'app-productos',
  standalone: true,  // 🚀 Componente independiente (sin módulo)
  templateUrl: './productos.page.html',
  styleUrls: ['./productos.page.scss'],
  imports: [CommonModule, IonicModule, RouterModule, FormsModule] // 🧩 Módulos necesarios
})
export class ProductosPage implements OnInit {

  productos: any[] = [];               // 📦 Todos los productos desde la API
  productosFiltrados: any[] = [];     // 🔍 Productos mostrados según filtro/búsqueda
  terminoBusqueda: string = '';       // 🔠 Texto de búsqueda
  categoriaSeleccionada: string | null = null; // 🧩 Categoría actual desde la URL (si existe)

  constructor(
    private api: ApiService,              // 📡 Servicio de productos
    private router: Router,               // 🔁 Para ir al detalle
    private route: ActivatedRoute         // 📥 Para leer parámetros como ?categoria=
  ) {}

  ngOnInit(): void {
    // 📥 Leer la categoría desde la URL (si viene)
    this.categoriaSeleccionada = this.route.snapshot.queryParamMap.get('categoria');

    // 🔄 Obtener productos desde el backend
    this.api.getProductos().subscribe({
      next: (data) => {
        this.productos = data;

        // 🔍 Aplicar filtro por categoría si viene en la URL
        if (this.categoriaSeleccionada) {
          this.productosFiltrados = data.filter(p =>
            p.categoria?.toLowerCase() === this.categoriaSeleccionada!.toLowerCase()
          );
        } else {
          this.productosFiltrados = data; // ✅ Mostrar todos
        }
      },
      error: (err) => {
        console.error('❌ Error al obtener productos:', err);
      }
    });
  }

  /**
   * 🔍 Filtra productos según el texto ingresado
   */
  filtrarProductos(): void {
    const termino = this.terminoBusqueda.trim().toLowerCase();

    // 🧩 Buscar solo dentro de los productos originales
    const base = this.categoriaSeleccionada
      ? this.productos.filter(p =>
          p.categoria?.toLowerCase() === this.categoriaSeleccionada!.toLowerCase())
      : this.productos;

    if (termino === '') {
      this.productosFiltrados = base;
    } else {
      this.productosFiltrados = base.filter(p =>
        p.nombre.toLowerCase().includes(termino) ||
        p.descripcion?.toLowerCase().includes(termino)
      );
    }
  }

  /**
   * 🧭 Navega al detalle del producto seleccionado
   * @param id ID del producto
   */
  verDetalle(id: number): void {
    this.router.navigate(['/detalle-producto', id]);
  }
}
