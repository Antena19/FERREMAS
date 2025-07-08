import { Component, OnInit } from '@angular/core';
import { CarritoService } from 'src/app/services/carrito.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonButton,
  IonIcon,
  IonSpinner
} from '@ionic/angular/standalone';
import { AuthService } from 'src/app/services/auth.service';
import { ApiService } from 'src/app/services/api.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-carrito',
  templateUrl: './carrito.page.html',
  styleUrls: ['./carrito.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonButton,
    IonIcon,
    IonSpinner,
    FormsModule
  ]
})
export class CarritoPage implements OnInit {

  productos: any[] = [];           // 🛍️ Productos cargados del carrito
  cargando: boolean = true;        // ⏳ Muestra spinner mientras carga
  subtotal: number = 0;
  impuestos: number = 0;
  total: number = 0;
  usuarioLogueado: boolean = false;

  // Wizard de pago
  pasoActual: number = 1;
  mostrarWizardPago: boolean = false;
  sucursales: any[] = [];
  direcciones: any[] = [];
  cargandoSucursales: boolean = false;
  cargandoDirecciones: boolean = false;
  cargandoActualizacionPedido: boolean = false;

  // Datos de entrega
  tipoEntrega: string = '';
  sucursalId: number | null = null;
  direccionId: number | null = null;
  notas: string = '';
  pedidoCreado: any = null;
  errorPedido: string = '';

  // Paso 3: Webpay
  cargandoPago: boolean = false;
  errorPago: string = '';

  resultadoPago: 'exito' | 'error' | null = null;
  mensajePago: string = '';
  metodoPagoSeleccionado: string = 'webpay';

  constructor(
    private carritoService: CarritoService,
    private auth: AuthService,
    private api: ApiService // <-- Inyectar ApiService directamente
  ) {}

  ngOnInit(): void {
    this.carritoService.getCarrito$().subscribe(productos => {
      this.productos = productos;
      this.calcularTotales();
      this.cargando = false;
    });
    this.usuarioLogueado = this.auth.estaAutenticado();

    // Manejo de retorno Webpay
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token_ws');
    if (token) {
      this.mostrarWizardPago = true;
      this.pasoActual = 4;
      this.resultadoPago = null;
      this.mensajePago = '';
      this.cargandoPago = true;
      this.confirmarPagoWebpay(token);
      // Limpiar el token de la URL tras la confirmación
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }

  calcularTotales(): void {
    this.subtotal = this.productos.reduce((acc, p) => acc + this.getPrecioProducto(p) * (p.cantidad ?? 1), 0);
    this.impuestos = this.subtotal * 0.19;
    this.total = this.subtotal + this.impuestos;
  }

  eliminarProducto(productoId: number): void {
    this.carritoService.eliminarProducto(productoId).subscribe();
  }

  vaciarCarrito(): void {
    this.carritoService.vaciarCarrito().subscribe();
  }

  cambiarCantidad(producto: any, nuevaCantidad: number): void {
    if (nuevaCantidad < 1) return;
    this.carritoService.agregarProducto(producto, nuevaCantidad - producto.cantidad).subscribe();
  }

  onCantidadChange(event: Event, producto: any): void {
    const input = event.target as HTMLInputElement;
    const value = input ? Number(input.value) : producto.cantidad;
    if (!isNaN(value) && value > 0) {
      this.cambiarCantidad(producto, value);
    }
  }

  irAPagar(): void {
    this.mostrarWizard();
  }

  avanzarPaso() {
    this.pasoActual++;
  }
  retrocederPaso() {
    if (this.pasoActual > 1) this.pasoActual--;
  }
  irAPaso(paso: number) {
    this.pasoActual = paso;
  }

  // Crear pedido
  crearPedido() {
    this.errorPedido = '';
    if (!this.tipoEntrega) {
      this.errorPedido = 'Selecciona el tipo de entrega.';
      return;
    }
    const data: any = {
      tipoEntrega: this.tipoEntrega,
      sucursalId: this.tipoEntrega === 'retiro_tienda' ? this.sucursalId : null,
      direccionId: this.tipoEntrega === 'despacho_domicilio' ? this.direccionId : null,
      notas: this.notas
    };
    this.api.crearOActualizarPedidoDesdeCarrito(data).subscribe({
      next: (res: any) => {
        this.pedidoCreado = Array.isArray(res) ? res[0] : res;
        this.avanzarPaso();
      },
      error: (err: any) => {
        this.errorPedido = err?.error?.mensaje || 'Error al crear/actualizar el pedido.';
      }
    });
  }

  mostrarWizard() {
    this.mostrarWizardPago = true;
    this.cargarSucursales();
    this.cargarDirecciones();
    this.pasoActual = 1;

    this.api.getPedidoPendiente().subscribe({
      next: (pedido: any) => {
        if (pedido) {
          this.pedidoCreado = pedido;
          this.tipoEntrega = pedido.tipo_entrega;
          this.sucursalId = pedido.sucursal_id;
          this.direccionId = pedido.direccion_id;
          this.notas = pedido.notas;
        } else {
          this.pedidoCreado = null;
          this.tipoEntrega = '';
          this.sucursalId = null;
          this.direccionId = null;
          this.notas = '';
        }
      }
    });
  }

  cargarSucursales() {
    this.cargandoSucursales = true;
    this.api.getSucursalesActivas().subscribe({
      next: (res: any) => {
        this.sucursales = Array.isArray(res) ? res : (res?.sucursales || []);
        this.cargandoSucursales = false;
      },
      error: () => { this.cargandoSucursales = false; }
    });
  }

  cargarDirecciones() {
    this.cargandoDirecciones = true;
    this.api.getMisDirecciones().subscribe({
      next: (res: any) => {
        // Ajuste: busca el array en res.data.direcciones
        this.direcciones = Array.isArray(res) ? res : (Array.isArray(res?.data) ? res.data : []);
        this.cargandoDirecciones = false;
      },
      error: () => { this.cargandoDirecciones = false; }
    });
  }

  getImagenProducto(producto: any): string {
    // Unificar lógica: imagen remota válida > imagen local > default
    let url = '';
    if (producto.imagenUrl && producto.imagenUrl.match(/\.(jpg|jpeg|png|webp|gif)$/i)) {
      if (producto.imagenUrl.includes('-')) {
        url = 'https://localhost:7091/img/' + producto.imagenUrl;
      } else {
        url = 'assets/img/' + producto.imagenUrl;
      }
    } else if (producto.producto_imagen) {
      url = 'assets/img/' + producto.producto_imagen;
    } else {
      url = 'assets/img/default.png';
    }
    console.log('Imagen para producto', producto.nombre || producto.producto_nombre || producto.id, ':', url, producto);
    return url;
  }

  onImgError(event: Event) {
    (event.target as HTMLImageElement).src = 'assets/img/default.png';
  }

  getNombreProducto(producto: any): string {
    return producto.producto_nombre || producto.nombre || 'Producto';
  }

  getPrecioProducto(producto: any): number {
    return producto.precio_unitario != null ? producto.precio_unitario : (producto.precio != null ? producto.precio : 0);
  }

  getSubtotalProducto(producto: any): number {
    return this.getPrecioProducto(producto) * (producto.cantidad ?? 1);
  }

  getIdProducto(producto: any): number {
    return producto.producto_id != null ? producto.producto_id : producto.id;
  }

  pagarConWebpay() {
    this.cargandoPago = true;
    this.errorPago = '';
    if (!this.pedidoCreado || !this.pedidoCreado.id) {
      this.errorPago = 'No se encontró el pedido.';
      this.cargandoPago = false;
      return;
    }
    // 1. Crear el pago asociado al pedido
    this.api.crearPago({
      pedidoId: this.pedidoCreado.id,
      metodo: 'webpay',
      urlRetorno: window.location.origin + window.location.pathname // para volver aquí
    }).subscribe({
      next: (pago: any) => {
        // 2. Crear la transacción Webpay con el monto y el id del pedido
        this.api.crearTransaccionWebpay({
          amount: this.pedidoCreado.total,
          pedidoId: this.pedidoCreado.id
        }).subscribe({
          next: (res: any) => {
            this.cargandoPago = false;
            if (res && res.url && res.token) {
              // Redirigir a Webpay
              window.location.href = `${res.url}?token_ws=${res.token}`;
            } else {
              this.errorPago = 'No se pudo iniciar el pago Webpay.';
            }
          },
          error: () => {
            this.cargandoPago = false;
            this.errorPago = 'Error al crear la transacción Webpay.';
          }
        });
      },
      error: () => {
        this.cargandoPago = false;
        this.errorPago = 'Error al crear el pago.';
      }
    });
  }

  confirmarPagoWebpay(token: string) {
    this.mensajePago = '';
    this.resultadoPago = null;
    this.cargandoPago = true;
    this.api.confirmarTransaccionWebpay(token).subscribe({
      next: (res: any) => {
        this.resultadoPago = 'exito';
        this.mensajePago = '¡Pago realizado con éxito!';
        this.cargandoPago = false;
        // Limpiar el carrito si el pago fue exitoso
        this.carritoService.vaciarCarrito().subscribe();
      },
      error: (err: any) => {
        this.resultadoPago = 'error';
        this.mensajePago = err?.error || 'Error al confirmar el pago.';
        this.cargandoPago = false;
      }
    });
  }

  // Llamar este método cada vez que cambie tipoEntrega, sucursalId, direccionId o notas
  actualizarPedidoPendiente() {
    if (!this.tipoEntrega) return;
    this.cargandoActualizacionPedido = true;
    const data: any = {
      tipoEntrega: this.tipoEntrega,
      sucursalId: this.tipoEntrega === 'retiro_tienda' ? this.sucursalId : null,
      direccionId: this.tipoEntrega === 'despacho_domicilio' ? this.direccionId : null,
      notas: this.notas
    };
    this.api.crearOActualizarPedidoDesdeCarrito(data).subscribe({
      next: (res: any) => {
        this.pedidoCreado = Array.isArray(res) ? res[0] : res;
        this.cargandoActualizacionPedido = false;
      },
      error: () => {
        this.cargandoActualizacionPedido = false;
      }
    });
  }

  // Ejemplo de uso: en los eventos de cambio de los campos del wizard
  onTipoEntregaChange() {
    this.sucursalId = null;
    this.direccionId = null;
    this.actualizarPedidoPendiente();
  }
  onSucursalChange() {
    this.actualizarPedidoPendiente();
  }
  onDireccionChange() {
    this.actualizarPedidoPendiente();
  }
  onNotasChange() {
    this.actualizarPedidoPendiente();
  }

  cerrarWizard() {
    this.mostrarWizardPago = false;
    this.pasoActual = 1;
  }

  getNombreSucursal(id: number | string | null): string {
    if (id == null) return 'No seleccionada';
    const suc = this.sucursales.find(s => s.id == id);
    return suc ? suc.nombre : 'No seleccionada';
  }
}
