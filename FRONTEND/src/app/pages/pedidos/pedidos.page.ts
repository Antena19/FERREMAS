import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonButton,
  IonSpinner, 
  IonHeader, 
  IonToolbar, 
  IonTitle,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardTitle,
  IonIcon,
  IonFab,
  IonFabButton,
  IonBadge,
  IonChip,
  IonAvatar,
  IonThumbnail,
  IonGrid,
  IonRow,
  IonCol,
  IonText,
  IonButtons,
  IonBackButton
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { 
  cartOutline, 
  homeOutline, 
  timeOutline, 
  checkmarkCircleOutline,
  closeCircleOutline,
  alertCircleOutline,
  bagOutline,
  carOutline,
  storefrontOutline
} from 'ionicons/icons';

@Component({
  selector: 'app-pedidos',
  templateUrl: './pedidos.page.html',
  styleUrls: ['./pedidos.page.scss'],
  standalone: true,
  imports: [
    IonTitle, 
    IonToolbar, 
    IonHeader,
    IonBackButton,
    IonButtons,
    CommonModule,
    RouterModule,
    IonContent,
    IonList,
    IonItem,
    IonLabel,
    IonButton,
    IonSpinner,
    IonCard,
    IonCardContent,
    IonCardHeader,
    IonCardTitle,
    IonIcon,
    IonFab,
    IonFabButton,
    IonBadge,
    IonChip,
    IonAvatar,
    IonThumbnail,
    IonGrid,
    IonRow,
    IonCol,
    IonText
  ]
})
export class PedidosPage implements OnInit {

  pedidos: any[] = [];
  cargando = true;
  usuario: any = null;
  esCliente = false;
  sucursales: any[] = [];
  direcciones: any[] = [];
  pedidosExpandido: { [id: number]: boolean } = {};
  pedidoCreado: any; // Nuevo campo para almacenar el pedido pendiente

  constructor(
    private api: ApiService,
    private auth: AuthService,
    private route: ActivatedRoute
  ) {
    addIcons({
      cartOutline,
      homeOutline,
      timeOutline,
      checkmarkCircleOutline,
      closeCircleOutline,
      alertCircleOutline,
      bagOutline,
      carOutline,
      storefrontOutline
    });
  }

  ngOnInit(): void {
    this.usuario = this.auth.obtenerUsuario();
    this.esCliente = this.usuario?.rol?.toLowerCase() === 'cliente';
    
    this.cargarSucursales();
    this.cargarDirecciones();
    
    if (this.usuario?.rol?.toLowerCase() === 'administrador') {
      this.cargarTodosLosPedidos();
    } else if (this.esCliente) {
      this.cargarHistorialCompras();
    } else {
      this.cargarPedidosUsuario();
    }

    this.route.queryParams.subscribe(params => {
      const pedidoId = params['pedidoId'];
      if (pedidoId) {
        // Lógica para cargar el pedido pendiente por ID y sincronizar el flujo de pago
        this.api.getDetallePedido(pedidoId).subscribe(pedido => {
          this.pedidoCreado = pedido;
          // ...asigna los datos necesarios para el wizard de pago
        });
      }
    });
  }

  cargarSucursales() {
    this.api.getSucursalesActivas().subscribe({
      next: (res: any) => {
        this.sucursales = Array.isArray(res) ? res : (res?.sucursales || []);
      }
    });
  }

  cargarDirecciones() {
    this.api.getMisDirecciones().subscribe({
      next: (res: any) => {
        this.direcciones = Array.isArray(res) ? res : (Array.isArray(res?.data) ? res.data : []);
      }
    });
  }

  getNombreSucursal(id: number | string | null): string {
    if (id == null) return '-';
    const suc = this.sucursales.find(s => s.id == id);
    return suc ? suc.nombre : '-';
  }

  getTextoDireccion(id: number | string | null): string {
    if (id == null) return '-';
    const dir = this.direcciones.find(d => d.id == id);
    if (!dir) return '-';
    return `${dir.calle} ${dir.numero}${dir.departamento ? ', ' + dir.departamento : ''}, ${dir.comuna}, ${dir.region}`;
  }

  cargarHistorialCompras() {
    this.api.getMiHistorialCompras().subscribe({
      next: (res) => {
        this.pedidos = res.map(p => ({
          ...p,
          usuarioNombre: p.usuarioNombre || (this.usuario?.nombre ? this.usuario.nombre + (this.usuario.apellido ? ' ' + this.usuario.apellido : '') : null)
        }));
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        console.error('❌ Error al obtener historial de compras');
      }
    });
  }

  cargarPedidosUsuario() {
    const idUsuario = this.auth.obtenerIdUsuario();
    if (!idUsuario) return;

    this.api.getPedidosPorUsuario(idUsuario).subscribe({
      next: (res) => {
        this.pedidos = res.map(p => ({
          ...p,
          usuarioNombre: p.usuarioNombre || (this.usuario?.nombre ? this.usuario.nombre + (this.usuario.apellido ? ' ' + this.usuario.apellido : '') : null)
        }));
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        console.error('❌ Error al obtener pedidos');
      }
    });
  }

  cargarTodosLosPedidos() {
    this.api.getTodosLosPedidos().subscribe({
      next: (res) => {
        this.pedidos = res.map(p => ({
          ...p,
          usuarioNombre: p.usuarioNombre || p.nombreUsuario || (p.usuario?.nombre ? p.usuario.nombre + (p.usuario.apellido ? ' ' + p.usuario.apellido : '') : null)
        }));
        this.cargando = false;
      },
      error: (err) => {
        console.error('❌ Error al obtener todos los pedidos:', err);
        this.cargando = false;
      }
    });
  }

  obtenerSucursalODireccion(pedido: any): string {
    if (pedido.sucursalId || pedido.sucursal_id) {
      return this.getNombreSucursal(pedido.sucursalId ?? pedido.sucursal_id);
    }
    if (pedido.direccionId || pedido.direccion_id) {
      return this.getTextoDireccion(pedido.direccionId ?? pedido.direccion_id);
    }
    // Fallback: si solo hay una dirección y no hay id, mostrarla
    if ((!pedido.direccionId && !pedido.direccion_id) && this.direcciones.length === 1) {
      return this.getTextoDireccion(this.direcciones[0].id);
    }
    return pedido.sucursalNombre || pedido.direccionTexto || '-';
  }

  toggleExpandido(id: number) {
    this.pedidosExpandido[id] = !this.pedidosExpandido[id];
  }

  obtenerIconoEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'entregado':
        return 'checkmark-circle-outline';
      case 'cancelado':
        return 'close-circle-outline';
      case 'pendiente':
      case 'confirmado':
      case 'asignado_vendedor':
      case 'en_bodega':
      case 'preparado':
      case 'en_entrega':
        return 'time-outline';
      default:
        return 'alert-circle-outline';
    }
  }

  obtenerColorEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'entregado':
        return 'success';
      case 'cancelado':
        return 'danger';
      case 'pendiente':
      case 'confirmado':
      case 'asignado_vendedor':
      case 'en_bodega':
      case 'preparado':
      case 'en_entrega':
        return 'warning';
      default:
        return 'medium';
    }
  }

  formatearFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-CL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  obtenerIconoTipoEntrega(tipoEntrega: string): string {
    switch (tipoEntrega?.toLowerCase()) {
      case 'retiro_tienda':
        return 'storefront-outline';
      case 'despacho_domicilio':
        return 'car-outline';
      default:
        return 'bag-outline';
    }
  }
}
