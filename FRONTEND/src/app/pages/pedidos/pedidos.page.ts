import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
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

  constructor(
    private api: ApiService,
    private auth: AuthService
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
    this.esCliente = this.usuario?.rol === 'cliente';
    
    if (this.esCliente) {
      this.cargarHistorialCompras();
    } else {
      this.cargarPedidosUsuario();
    }
  }

  cargarHistorialCompras() {
    this.api.getMiHistorialCompras().subscribe({
      next: (res) => {
        this.pedidos = res;
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
        this.pedidos = res;
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        console.error('❌ Error al obtener pedidos');
      }
    });
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
