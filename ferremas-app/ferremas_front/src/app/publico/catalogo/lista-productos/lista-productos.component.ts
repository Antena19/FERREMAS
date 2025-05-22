// src/app/publico/catalogo/lista-productos/lista-productos.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductoService } from '../../../servicios/producto.service';
import { CategoriaService } from '../../../servicios/categoria.service';
import { MarcaService } from '../../../servicios/marca.service';
import { Producto } from '../../../modelos/producto.model';
import { Categoria } from '../../../modelos/categoria.model';
import { Marca } from '../../../modelos/marca.model';
import { FiltrosProductoComponent } from '../filtros-producto/filtros-producto.component';
import { CarritoService } from '../../../servicios/carrito.service';
import { NotificacionService } from '../../../servicios/notificacion.service';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { Observable, of } from 'rxjs';
import { Carrito } from '../../../modelos/carrito.model';
import { AuthService } from '../../../servicios/auth.service';

@Component({
  selector: 'app-lista-productos',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, FiltrosProductoComponent],
  templateUrl: './lista-productos.component.html',
  styleUrls: ['./lista-productos.component.css']
})
export class ListaProductosComponent implements OnInit, OnDestroy {
  productos: Producto[] = [];
  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  productosFiltrados: Producto[] = [];
  cargando: boolean = true;
  error: string = '';

  // Filtros
  terminoBusqueda: string = '';
  categoriaSeleccionada: number | null = null;
  marcaSeleccionada: number | null = null;
  precioMinimo?: number;
  precioMaximo?: number;
  ordenSeleccionado: string = 'nombre';
  private destroy$ = new Subject<void>();

  // **Agrega estas dos líneas:**
  usuarioAutenticado: boolean = false;
  carrito: Carrito = { items: [], subtotal: 0, impuestos: 0, descuentos: 0, total: 0 };

  constructor(
    private productoService: ProductoService,
    private categoriaService: CategoriaService,
    private marcaService: MarcaService,
    private carritoService: CarritoService,
    private notificacionService: NotificacionService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarProductos();
    this.cargarCategorias();
    this.cargarMarcas();

    // Suscribirse a los cambios de autenticación
    this.authService.usuario$.subscribe(usuario => {
      this.usuarioAutenticado = !!usuario;
    });

    // Suscribirse a los cambios del carrito
    this.carritoService.carrito$.subscribe(carrito => {
      this.carrito = carrito;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  cargarProductos(): void {
    this.cargando = true;
    this.productoService.obtenerTodos()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (productos) => {
          this.productos = productos;
          this.aplicarFiltros();
          this.cargando = false;
        },
        error: (error) => {
          this.error = 'Error al cargar los productos';
          console.error('Error:', error);
          this.cargando = false;
        }
      });
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

  cargarMarcas(): void {
    this.marcaService.obtenerTodas().subscribe({
      next: (marcas: Marca[]) => {
        this.marcas = marcas;
      },
      error: (error: Error) => {
        console.error('Error al cargar marcas:', error);
      }
    });
  }

  onCategoriaChange(categoriaId: number): void {
    this.categoriaSeleccionada = categoriaId;
    this.filtrarProductos();
  }

  onMarcaChange(marcaId: number | null): void {
    this.marcaSeleccionada = marcaId;
    this.filtrarProductos();
  }

  onBuscar(termino: string): void {
    this.terminoBusqueda = termino;
    this.filtrarProductos();
  }

  aplicarFiltros(filtros?: { termino: string; categoriaId?: number; precioMin?: number; precioMax?: number }): void {
    if (filtros) {
      this.terminoBusqueda = filtros.termino;
      this.categoriaSeleccionada = filtros.categoriaId || null;
      this.precioMinimo = filtros.precioMin;
      this.precioMaximo = filtros.precioMax;
    }

    let resultado = [...this.productos];

    // Filtrar por término de búsqueda
    if (this.terminoBusqueda) {
      const termino = this.terminoBusqueda.toLowerCase();
      resultado = resultado.filter(p => 
        p.nombre.toLowerCase().includes(termino) || 
        p.descripcion.toLowerCase().includes(termino)
      );
    }

    // Filtrar por categoría
    if (this.categoriaSeleccionada) {
      resultado = resultado.filter(p => p.categoriaId === this.categoriaSeleccionada);
    }

    // Filtrar por marca
    if (this.marcaSeleccionada) {
      resultado = resultado.filter(p => p.marcaId === this.marcaSeleccionada);
    }

    // Filtrar por precio
    if (this.precioMinimo !== undefined && this.precioMaximo !== undefined) {
      resultado = resultado.filter(p => 
        p.precio >= (this.precioMinimo ?? 0) && 
        p.precio <= (this.precioMaximo ?? Number.MAX_VALUE)
      );
    }

    // Ordenar productos
    switch (this.ordenSeleccionado) {
      case 'nombre':
        resultado.sort((a, b) => a.nombre.localeCompare(b.nombre));
        break;
      case 'precio-asc':
        resultado.sort((a, b) => a.precio - b.precio);
        break;
      case 'precio-desc':
        resultado.sort((a, b) => b.precio - a.precio);
        break;
    }

    this.productosFiltrados = resultado;
  }

  private filtrarProductos(): void {
    this.cargando = true;
    let observable;

    if (this.terminoBusqueda) {
      observable = this.productoService.buscar(this.terminoBusqueda);
    } else if (this.categoriaSeleccionada) {
      observable = this.productoService.obtenerPorCategoria(this.categoriaSeleccionada);
    } else if (this.marcaSeleccionada) {
      observable = this.productoService.obtenerPorMarca(this.marcaSeleccionada);
    } else {
      observable = this.productoService.obtenerTodos();
    }

    observable.subscribe({
      next: (productos: Producto[]) => {
        this.productos = productos;
        this.cargando = false;
        this.aplicarFiltros();
      },
      error: (error: Error) => {
        this.error = 'Error al filtrar productos';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  agregarAlCarrito(producto: Producto): void {
    console.log('Agregando al carrito:', producto);
    this.carritoService.agregarProducto(producto.id, 1, producto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificacionService.exito(`${producto.nombre} agregado al carrito`);
        },
        error: (error) => {
          this.notificacionService.error('Error al agregar el producto al carrito');
          console.error('Error:', error);
        }
      });
  }

  limpiarFiltros(): void {
    this.terminoBusqueda = '';
    this.categoriaSeleccionada = null;
    this.marcaSeleccionada = null;
    this.precioMinimo = undefined;
    this.precioMaximo = undefined;
    this.ordenSeleccionado = 'nombre';
    this.filtrarProductos();
  }

  getCategorias(): number[] {
    return [...new Set(this.productos.map(p => p.categoriaId))];
  }
}