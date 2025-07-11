import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';                // 📥 Capturar ID de la URL
import { ApiService } from 'src/app/services/api.service';      // 📡 Servicio para obtener el producto
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from 'src/app/services/auth.service';
import { CarritoService } from 'src/app/services/carrito.service';

@Component({
  selector: 'app-detalle-producto',
  standalone: true,
  templateUrl: './detalle-producto.page.html',
  styleUrls: ['./detalle-producto.page.scss'],
  imports: [CommonModule, IonicModule, RouterModule, FormsModule]
})
export class DetalleProductoPage implements OnInit {
  producto: any = null;       // 🧩 Producto cargado
  cargando: boolean = true;   // ⏳ Spinner mientras carga
  esAdmin: boolean = false;    // Cambia esto por tu lógica real de autenticación
  sucursales: any[] = [];     // Lista de sucursales
  sucursalSeleccionada: any = null;
  inventario: any = null;       // Inventario del producto
  categorias: any[] = [];
  marcas: any[] = [];
  cantidad: number = 1; // <--- Nueva variable para la cantidad
  modoCreacion: boolean = false;
  imagenSeleccionada: File | null = null;
  puedeEliminar: boolean = false; // Si el producto puede eliminarse (stock=0 en todas las sucursales)
  advertenciaStock: string = '';
  inventarioGlobal: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private auth: AuthService,
    private carritoService: CarritoService // <--- Inyectar el servicio
  ) {}

  ngOnInit(): void {
    const usuario = this.auth.obtenerUsuario();
    const rol = (usuario?.rol || '').toLowerCase();
    this.esAdmin = rol.includes('admin');
    console.log('Usuario:', usuario);
    console.log('esAdmin:', this.esAdmin);

    // Cargar categorías y marcas para los selectores
    this.api.getCategorias().subscribe(cats => this.categorias = cats);
    this.api.getMarcas().subscribe(marcas => this.marcas = marcas);

    const id = this.route.snapshot.paramMap.get('id');
    if (id === 'nuevo') {
      this.modoCreacion = true;
      this.cargando = false;
      this.producto = {
        codigo: '',
        nombre: '',
        descripcion: '',
        precio: 0,
        categoriaId: '',
        marcaId: '',
        imagenUrl: '',
        especificaciones: '',
        activo: true
      };
    } else if (id) {
      this.api.getProductoPorId(+id).subscribe({
        next: (res) => {
          this.producto = res;
          console.log('Producto detalle:', this.producto);
          if (this.esAdmin) {
            this.cargarSucursalesYInventario(+id);
            this.verificarStockGlobal(+id);
          }
          this.cargando = false;
        },
        error: () => {
          console.error('❌ Error al obtener producto');
          this.cargando = false;
        }
      });
    }
  }

  cargarSucursalesYInventario(productoId: number) {
    this.api.getSucursales().subscribe({
      next: (sucursales) => {
        this.sucursales = sucursales.data;
        if (this.sucursales.length > 0) {
          this.sucursalSeleccionada = this.sucursales[0];
          this.cargarInventario(productoId, this.sucursales[0].id);
        }
      },
      error: () => {
        console.error('❌ Error al cargar sucursales');
      }
    });
  }

  onSucursalChange(productoId: number) {
    if (this.sucursalSeleccionada) {
      this.cargarInventario(productoId, this.sucursalSeleccionada.id);
    }
  }

  cargarInventario(productoId: number, sucursalId: number) {
    this.api.getInventarioPorProductoYSucursal(productoId, sucursalId).subscribe(inv => {
      this.inventario = inv;
    });
  }

  agregarAlCarrito(): void {
    if (this.cantidad < 1) return;
    if (!this.producto) return;
    this.carritoService.agregarProducto(this.producto, this.cantidad).subscribe({
      next: () => {
        alert('Producto agregado al carrito');
      },
      error: () => {
        alert('Error al agregar al carrito');
      }
    });
  }

  sumarCantidad() {
    this.cantidad++;
  }

  restarCantidad() {
    if (this.cantidad > 1) this.cantidad--;
  }

  getImagePath(producto: any): string {
    // Si la imagen es una subida al backend (nombre generado con guion y extensión)
    if (producto.imagenUrl && producto.imagenUrl.includes('-') && producto.imagenUrl.match(/\.(jpg|jpeg|png|webp|gif)$/i)) {
      return 'https://localhost:7091/img/' + producto.imagenUrl;
    }
    // Si es una imagen antigua o por defecto
    return 'assets/img/' + (producto.imagenUrl || producto.imagen_url || 'default.png');
  }

  guardarCambios() {
    if (!this.producto) return;
    const productoData = {
      codigo: this.producto.codigo,
      nombre: this.producto.nombre,
      descripcion: this.producto.descripcion,
      precio: this.producto.precio,
      categoriaId: this.producto.categoriaId,
      marcaId: this.producto.marcaId,
      imagenUrl: this.producto.imagenUrl,
      especificaciones: this.producto.especificaciones ? this.producto.especificaciones : "Sin especificaciones",
      activo: this.producto.activo
    };
    if (this.modoCreacion) {
      this.api.crearProducto(productoData).subscribe({
        next: (res) => {
          const nuevoId = res.id || res;
          if (this.imagenSeleccionada) {
            const formData = new FormData();
            formData.append('imagen', this.imagenSeleccionada);
            this.api.subirImagenProducto(nuevoId, formData).subscribe({
              next: () => {
                alert('Producto creado exitosamente');
                window.location.href = '/productos';
              },
              error: () => {
                alert('Producto creado, pero error al subir la imagen');
                window.location.href = '/productos';
              }
            });
          } else {
            alert('Producto creado exitosamente');
            window.location.href = '/productos';
          }
        },
        error: () => {
          alert('Error al crear el producto');
        }
      });
    } else {
      this.api.actualizarProducto(this.producto.id, productoData).subscribe({
        next: () => {
          alert('Producto actualizado exitosamente');
        },
        error: () => {
          alert('Error al actualizar el producto');
        }
      });
    }
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (this.modoCreacion) {
        this.imagenSeleccionada = file;
      } else {
        this.subirImagen(file);
      }
    }
  }

  subirImagen(file: File) {
    const formData = new FormData();
    formData.append('imagen', file);

    this.api.subirImagenProducto(this.producto.id, formData).subscribe({
      next: (res) => {
        // res debe contener el nombre de la imagen guardada
        this.producto.imagenUrl = res.nombreArchivo || res.imagenUrl;
        // Ya no se llama a this.guardarCambios() aquí
      },
      error: (err) => {
        alert('Error al subir la imagen');
        console.error(err);
      }
    });
  }

  verificarStockGlobal(productoId: number) {
    // Trae sucursales y luego inventario global
    this.api.getSucursales().subscribe({
      next: (sucursalesRes) => {
        const sucursales = sucursalesRes.data || sucursalesRes; // Ajusta según tu backend
        this.api.getInventarioGlobal().subscribe({
          next: (data) => {
            console.log('productoId:', productoId, typeof productoId);
            this.inventarioGlobal = data.filter((inv: any) =>
              Number(inv.productoId) === Number(productoId)
            );
            console.log('Inventario global filtrado:', this.inventarioGlobal);
            // Log de depuración: mostrar el stock del producto para cada sucursal
            sucursales.forEach((suc: any) => {
              const sucId = suc.id || suc.sucursalId || suc.sucursal_id;
              const inv = this.inventarioGlobal.find((i: any) =>
                i.sucursalId == sucId || i.sucursal_id == sucId
              );
              console.log(`Sucursal ${sucId}:`, inv ? inv.stock : 'SIN INVENTARIO');
            });
            // Solo se puede eliminar si hay inventario en TODAS las sucursales y el stock es 0 en todas
            const todosCero = sucursales.every((suc: any) => {
              const sucId = suc.id || suc.sucursalId || suc.sucursal_id;
              const inv = this.inventarioGlobal.find((i: any) =>
                i.sucursalId == sucId || i.sucursal_id == sucId
              );
              return inv && inv.stock === 0;
            });
            // Además, debe haber inventario para todas las sucursales
            const hayInventarioEnTodas = sucursales.every((suc: any) => {
              const sucId = suc.id || suc.sucursalId || suc.sucursal_id;
              return this.inventarioGlobal.some((i: any) => i.sucursalId == sucId || i.sucursal_id == sucId);
            });
            this.puedeEliminar = todosCero && hayInventarioEnTodas && sucursales.length > 0;
            this.advertenciaStock = this.puedeEliminar
              ? ''
              : 'No se puede eliminar el producto mientras exista stock en alguna sucursal.';
          },
          error: () => {
            this.puedeEliminar = false;
            this.advertenciaStock = 'No se pudo verificar el stock global.';
          }
        });
      },
      error: () => {
        this.puedeEliminar = false;
        this.advertenciaStock = 'No se pudo obtener la lista de sucursales.';
      }
    });
  }

  eliminarProducto() {
    if (!this.puedeEliminar || !this.producto) return;
    if (!confirm('¿Estás seguro de que deseas eliminar este producto? Esta acción no se puede deshacer.')) return;
    this.api.eliminarProducto(this.producto.id).subscribe({
      next: () => {
        alert('Producto eliminado exitosamente');
        window.location.href = '/productos';
      },
      error: () => {
        alert('Error al eliminar el producto');
      }
    });
  }
}
  