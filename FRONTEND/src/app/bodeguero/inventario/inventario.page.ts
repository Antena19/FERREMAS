import { Component, OnInit } from '@angular/core';
import { IonicModule, ModalController, AlertController } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-inventario',
  templateUrl: './inventario.page.html',
  styleUrls: ['./inventario.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class InventarioPage implements OnInit {
  productos: any[] = [];
  sucursales: any[] = [];
  inventarioPorSucursal: { [sucursalId: number]: any[] } = {};
  esAdmin: boolean = false;
  cargando: boolean = true;
  mensaje: string = '';
  mensajeTipo: 'exito' | 'error' | '' = '';
  sucursalExpandida: number | null = null; // Para el acordeón
  inventarioEditando: { [key: string]: boolean } = {}; // clave: sucursalId-productoId
  stockEditado: { [key: string]: number } = {}; // clave: sucursalId-productoId
  minStockEditado: { [key: string]: number } = {};

  constructor(
    private api: ApiService,
    private auth: AuthService,
    private alertCtrl: AlertController,
    private router: Router
  ) { }

  async ngOnInit() {
    this.cargando = true;
    this.mensaje = '';
    const usuario = this.auth.obtenerUsuario();
    this.esAdmin = usuario && usuario.rol && usuario.rol.toLowerCase().includes('admin');
    (this.esAdmin ? this.api.getSucursales() : this.api.getSucursalesActivas()).subscribe({
      next: (sucs: any) => {
        this.sucursales = Array.isArray(sucs.data) ? sucs.data : [];
        this.sucursalExpandida = this.sucursales.length > 0 ? this.sucursales[0].id : null;
        this.cargarProductosYInventario();
      },
      error: (err) => {
        this.mensaje = 'Error al cargar sucursales';
        this.sucursales = [];
        this.productos = [];
        this.cargando = false;
      }
    });
  }

  toggleSucursal(sucursalId: number) {
    this.sucursalExpandida = this.sucursalExpandida === sucursalId ? null : sucursalId;
  }

  cargarProductosYInventario() {
    this.api.getProductos(true).subscribe({
      next: (prods) => {
        this.productos = prods || [];
        this.cargarInventario();
      },
      error: (err) => {
        this.mensaje = 'Error al cargar productos';
        this.productos = [];
        this.cargando = false;
      }
    });
  }

  cargarInventario() {
    let pendientes = this.sucursales.length;
    this.inventarioPorSucursal = {};
    if (pendientes === 0) {
      this.cargando = false;
      return;
    }
    this.sucursales.forEach(suc => {
      this.api.getInventarioPorSucursal ?
        this.api.getInventarioPorSucursal(suc.id).subscribe({
          next: (inv) => {
            this.inventarioPorSucursal[suc.id] = inv || [];
            pendientes--;
            if (pendientes === 0) this.cargando = false;
          },
          error: () => {
            this.inventarioPorSucursal[suc.id] = [];
            pendientes--;
            if (pendientes === 0) this.cargando = false;
          }
        }) : pendientes--;
    });
  }

  obtenerStock(productoId: number, sucursalId: number): any {
    const inventario = this.inventarioPorSucursal[sucursalId] || [];
    return inventario.find((i: any) => i.productoId === productoId) || { stock: 0, id: null };
  }

  // Devuelve el stock de un producto en una sucursal
  getStock(productoId: number, sucursalId: number): number {
    const item = this.inventarioPorSucursal[sucursalId]?.find(
      inv => inv.productoId === productoId
    );
    return item ? item.stock : 0;
  }

  // Devuelve el stock mínimo de un producto en una sucursal
  getStockMinimo(productoId: number, sucursalId: number): number {
    const item = this.inventarioPorSucursal[sucursalId]?.find(
      inv => inv.productoId === productoId
    );
    return item && item.stockMinimo !== undefined ? item.stockMinimo : 0;
  }

  // Inicia la edición de stock
  editarStockInline(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    this.inventarioEditando[key] = true;
    this.stockEditado[key] = this.getStock(productoId, sucursalId);
    this.minStockEditado[key] = this.getStockMinimo(productoId, sucursalId);
  }

  // Cancela la edición
  cancelarEdicionStock(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    this.inventarioEditando[key] = false;
  }

  // Guarda el nuevo stock
  guardarStock(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    const inventario = this.inventarioPorSucursal[sucursalId]?.find(
      inv => inv.productoId === productoId
    );
    if (!inventario) return;
    const nuevoStock = this.stockEditado[key];
    const nuevoMinStock = this.minStockEditado[key];
    // Construye el objeto inventario actualizado SOLO con campos primitivos requeridos
    const inventarioActualizado = {
      id: inventario.id,
      productoId: inventario.productoId,
      sucursalId: inventario.sucursalId,
      stock: nuevoStock,
      stockMinimo: nuevoMinStock,
      stockMaximo: inventario.stockMaximo ?? 0,
      precioCompra: inventario.precioCompra ?? 0,
      precioVenta: inventario.precioVenta ?? 0,
      activo: inventario.activo ?? true,
      producto: { id: inventario.productoId },
      sucursal: { id: inventario.sucursalId }
    };
    this.api.actualizarInventarioCompleto(inventarioActualizado).subscribe({
      next: () => {
        this.api.getInventarioPorSucursal(sucursalId).subscribe(inv => {
          this.inventarioPorSucursal[sucursalId] = inv;
        });
        this.inventarioEditando[key] = false;
        this.mostrarMensaje('Stock actualizado correctamente', 'exito');
      },
      error: () => {
        this.mostrarMensaje('Error al actualizar el stock', 'error');
      }
    });
  }

  async editarStock(productoId: number, sucursalId: number) {
    const inventario = this.obtenerStock(productoId, sucursalId);
    const alert = await this.alertCtrl.create({
      header: 'Editar stock',
      inputs: [
        {
          name: 'cantidad',
          type: 'number',
          value: inventario.stock,
          min: 0,
          placeholder: 'Nuevo stock'
        }
      ],
      buttons: [
        { text: 'Cancelar', role: 'cancel' },
        {
          text: 'Guardar',
          handler: (data) => {
            this.actualizarStock(inventario.id, data.cantidad, sucursalId);
          }
        }
      ]
    });
    await alert.present();
  }

  actualizarStock(inventarioId: number, cantidad: number, sucursalId: number) {
    if (!inventarioId) {
      this.mensaje = 'No se puede actualizar el stock porque no existe el registro de inventario.';
      return;
    }
    this.api.actualizarStockInventario(inventarioId, cantidad).subscribe({
      next: (res) => {
        // Refrescar inventario de la sucursal
        this.api.getInventarioPorSucursal(sucursalId).subscribe(inv => {
          this.inventarioPorSucursal[sucursalId] = inv;
        });
        this.mensaje = 'Stock actualizado correctamente';
      },
      error: () => {
        this.mensaje = 'Error al actualizar el stock';
      }
    });
  }

  // Al eliminar, actualiza la tabla inmediatamente
  eliminarProductoDeInventario(productoId: number, sucursalId: number) {
    const inventario = this.inventarioPorSucursal[sucursalId]?.find(
      inv => inv.productoId === productoId
    );
    if (!inventario || inventario.stock !== 0) return;
    if (window.confirm('¿Estás seguro de eliminar este producto del inventario? Esta acción no se puede deshacer.')) {
      this.api.actualizarStockInventario(inventario.id, 0).subscribe({
        next: () => {
          // Elimina del frontend inmediatamente
          this.inventarioPorSucursal[sucursalId] = (this.inventarioPorSucursal[sucursalId] || []).filter(inv => inv.productoId !== productoId);
          this.mostrarMensaje('Producto eliminado del inventario', 'exito');
          // Luego recarga desde backend para asegurar consistencia
          this.api.getInventarioPorSucursal(sucursalId).subscribe(inv => {
            this.inventarioPorSucursal[sucursalId] = inv;
          });
        },
        error: () => {
          this.mostrarMensaje('Error al eliminar el producto del inventario', 'error');
        }
      });
    }
  }

  // Devuelve los productos que no existen en el inventario de la sucursal
  productosFaltantesEnSucursal(sucursalId: number): any[] {
    const inventario = this.inventarioPorSucursal[sucursalId] || [];
    const idsEnInventario = new Set(inventario.map(i => i.productoId));
    return this.productos.filter(p => !idsEnInventario.has(p.id));
  }

  // Estado para agregar producto faltante
  agregarStockEditado: { [key: string]: number } = {};
  agregarMinStockEditado: { [key: string]: number } = {};
  agregandoProducto: { [key: string]: boolean } = {};

  iniciarAgregarProducto(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    this.agregandoProducto[key] = true;
    this.agregarStockEditado[key] = 1;
    this.agregarMinStockEditado[key] = 1;
  }

  cancelarAgregarProducto(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    this.agregandoProducto[key] = false;
  }

  guardarAgregarProducto(productoId: number, sucursalId: number) {
    const key = `${sucursalId}-${productoId}`;
    const stock = this.agregarStockEditado[key];
    const minStock = this.agregarMinStockEditado[key];
    // Aquí deberías llamar a un endpoint para crear el inventario
    // Simulación: solo mostrar mensaje de éxito y recargar inventario
    // Reemplaza esto por la llamada real a la API
    this.mostrarMensaje('Producto agregado al inventario (simulado)', 'exito');
    this.agregandoProducto[key] = false;
    // Recarga inventario real
    this.api.getInventarioPorSucursal(sucursalId).subscribe(inv => {
      this.inventarioPorSucursal[sucursalId] = inv;
    });
  }

  // Saber si el stock fue modificado
  stockModificado(productoId: number, sucursalId: number): boolean {
    const key = `${sucursalId}-${productoId}`;
    return this.stockEditado[key] !== undefined && this.stockEditado[key] !== this.getStock(productoId, sucursalId);
  }

  minStockModificado(productoId: number, sucursalId: number): boolean {
    const key = `${sucursalId}-${productoId}`;
    return this.minStockEditado[key] !== undefined && this.minStockEditado[key] !== this.getStockMinimo(productoId, sucursalId);
  }

  // Navegar a la ficha técnica del producto
  irAFichaTecnica(productoId: number) {
    this.router.navigate(['/detalle-producto', productoId]);
  }

  max(a: number, b: number): number {
    return Math.max(a, b);
  }

  mostrarMensaje(mensaje: string, tipo: 'exito' | 'error') {
    this.mensaje = mensaje;
    this.mensajeTipo = tipo;
    setTimeout(() => {
      this.mensaje = '';
      this.mensajeTipo = '';
    }, 3000);
  }
}
