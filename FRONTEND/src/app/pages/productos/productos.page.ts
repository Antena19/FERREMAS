import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';     // 📡 Servicio que interactúa con la API
import { Router, ActivatedRoute } from '@angular/router';      // 🔁 Navegación y lectura de parámetros
import { CommonModule } from '@angular/common';                // ✅ Para usar *ngIf, *ngFor, etc.
import { IonicModule } from '@ionic/angular';                  // ✅ Componentes de Ionic
import { RouterModule } from '@angular/router';                // ✅ Para usar routerLink
import { FormsModule } from '@angular/forms';                  // ✅ Para ngModel en el buscador
import { AuthService } from 'src/app/services/auth.service';

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
  sidebarAbierto: boolean = false;
  categoriaFiltro: string = '';
  marcaFiltro: string = '';
  categorias: any[] = [];
  marcas: any[] = [];
  productosCargados = false;
  categoriasCargadas = false;
  marcasCargadas = false;
  esAdmin: boolean = false;
  mensajeCarga: string = '';
  erroresCarga: string[] = [];

  constructor(
    private api: ApiService,              // 📡 Servicio de productos
    private router: Router,               // 🔁 Para ir al detalle
    private route: ActivatedRoute,         // 📥 Para leer parámetros como ?categoria=
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.categoriaSeleccionada = this.route.snapshot.queryParamMap.get('categoria');
    this.api.getCategorias().subscribe(cats => {
      this.categorias = cats;
      this.categoriasCargadas = true;
      this.intentarFiltrar();
    });
    this.api.getMarcas().subscribe(marcas => {
      this.marcas = marcas;
      this.marcasCargadas = true;
      this.intentarFiltrar();
    });
    const usuario = this.auth.obtenerUsuario();
    this.esAdmin = usuario && usuario.rol && usuario.rol.toLowerCase().includes('admin');
    this.api.getProductos(this.esAdmin).subscribe({
      next: (data) => {
        this.productos = data;
        this.productosCargados = true;
        this.intentarFiltrar();
      },
      error: (err) => {
        console.error('❌ Error al obtener productos:', err);
      }
    });
  }

  intentarFiltrar() {
    if (this.productosCargados && this.categoriasCargadas && this.marcasCargadas) {
      this.filtrarProductos();
    }
  }

  /**
   * 🔍 Filtra productos según el texto ingresado
   */
  filtrarProductos(): void {
    let base = this.productos;
    if (this.categoriaFiltro) {
      base = base.filter(p => Number(p.categoriaId) === Number(this.categoriaFiltro));
    }
    if (this.marcaFiltro) {
      base = base.filter(p => Number(p.marcaId) === Number(this.marcaFiltro));
    }
    const termino = this.terminoBusqueda.trim().toLowerCase();
    if (termino) {
      base = base.filter(p =>
        p.nombre.toLowerCase().includes(termino) ||
        (p.descripcion?.toLowerCase().includes(termino))
      );
    }
    this.productosFiltrados = base;
  }

  /**
   * 🧭 Navega al detalle del producto seleccionado
   * @param id ID del producto
   */
  verDetalle(id: number): void {
    this.router.navigate(['/detalle-producto', id]);
  }

  getImagePath(producto: any): string {
    if (producto.imagenUrl && producto.imagenUrl.includes('-') && producto.imagenUrl.match(/\.(jpg|jpeg|png|webp|gif)$/i)) {
      return 'https://localhost:7091/img/' + producto.imagenUrl;
    }
    return 'assets/img/' + (producto.imagenUrl || producto.imagen_url || 'default.png');
  }

  getCategoriaNombre(id: number): string {
    const cat = this.categorias.find(c => c.id === id);
    return cat ? cat.nombre : 'N/A';
  }

  getMarcaNombre(id: number): string {
    const marca = this.marcas.find(m => m.id === id);
    return marca ? marca.nombre : 'N/A';
  }

  agregarAlCarrito(producto: any): void {
    console.log('Agregar al carrito:', producto);
  }

  cargarCsv(event: any) {
    const archivo = event.target.files[0];
    if (!archivo) return;
    this.mensajeCarga = '';
    this.erroresCarga = [];
    this.api.cargarProductosCsv(archivo).subscribe({
      next: (res) => {
        if (res.productosCreados && res.productosCreados.length > 0) {
          this.mensajeCarga = `Productos creados: ${res.productosCreados.join(', ')}`;
        }
        if (res.errores && res.errores.length > 0) {
          this.erroresCarga = res.errores;
        }
        // Refrescar productos
        this.api.getProductos(this.esAdmin).subscribe(data => {
          this.productos = data;
          this.intentarFiltrar();
        });
      },
      error: (err) => {
        this.mensajeCarga = 'Error al cargar el archivo.';
      }
    });
  }

  irACrearProducto() {
    this.router.navigate(['/detalle-producto', 'nuevo']);
  }

  eliminarProducto(producto: any) {
    if (!confirm('¿Estás seguro de que deseas eliminar este producto? Se marcará como inactivo y no podrá ser comprado.')) return;
    const data = { ...producto, activo: false };
    this.api.actualizarProducto(producto.id, data).subscribe({
      next: () => {
        producto.activo = false;
        this.filtrarProductos();
      },
      error: () => {
        alert('Error al eliminar el producto');
      }
    });
  }
}
