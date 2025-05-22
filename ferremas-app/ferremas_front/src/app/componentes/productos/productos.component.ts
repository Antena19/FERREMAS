import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductoService } from '../../servicios/producto.service';
import { CategoriaService } from '../../servicios/categoria.service';
import { Producto } from '../../modelos/producto.model';
import { Categoria } from '../../modelos/categoria.model';
import { FiltrosProductosComponent } from './filtros-productos/filtros-productos.component';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, RouterModule, FiltrosProductosComponent],
  templateUrl: './productos.component.html',
  styleUrls: ['./productos.component.css']
})
export class ProductosComponent implements OnInit {
  productos: Producto[] = [];
  categorias: Categoria[] = [];
  cargando = true;
  error = '';

  // Filtros
  terminoBusqueda = '';
  categoriaSeleccionada?: number;
  precioMinimo?: number;
  precioMaximo?: number;

  constructor(
    private productoService: ProductoService,
    private categoriaService: CategoriaService
  ) {}

  ngOnInit(): void {
    this.cargarCategorias();
    this.cargarProductos();
  }

  cargarCategorias(): void {
    this.categoriaService.obtenerTodas().subscribe({
      next: (categorias: Categoria[]) => {
        this.categorias = categorias;
      },
      error: (error: Error) => {
        console.error('Error al cargar categorías:', error);
      }
    });
  }

  cargarProductos(): void {
    this.cargando = true;
    this.productoService.obtenerTodos().subscribe({
      next: (productos: Producto[]) => {
        this.productos = productos;
        this.cargando = false;
      },
      error: (error: Error) => {
        this.error = 'Error al cargar los productos';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  onCategoriaChange(categoriaId: number | null): void {
    this.categoriaSeleccionada = categoriaId || undefined;
    if (categoriaId) {
      this.cargarProductosPorCategoria(categoriaId);
    } else {
      this.cargarProductos();
    }
  }

  cargarProductosPorCategoria(categoriaId: number): void {
    this.cargando = true;
    this.productoService.obtenerPorCategoria(categoriaId).subscribe({
      next: (productos: Producto[]) => {
        this.productos = productos;
        this.cargando = false;
      },
      error: (error: Error) => {
        this.error = 'Error al cargar productos por categoría';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  onBuscar(termino: string): void {
    this.terminoBusqueda = termino;
    if (termino) {
      this.buscarProductos();
    } else {
      this.cargarProductos();
    }
  }

  buscarProductos(): void {
    this.cargando = true;
    this.productoService.buscar(this.terminoBusqueda).subscribe({
      next: (productos: Producto[]) => {
        this.productos = productos;
        this.cargando = false;
      },
      error: (error: Error) => {
        this.error = 'Error al buscar productos';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  aplicarFiltros(filtros: any): void {
    this.terminoBusqueda = filtros.termino || '';
    this.categoriaSeleccionada = filtros.categoriaId;
    this.precioMinimo = filtros.precioMin;
    this.precioMaximo = filtros.precioMax;
    this.buscarProductos();
  }
} 