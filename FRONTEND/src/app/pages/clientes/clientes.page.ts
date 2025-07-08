import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { CommonModule } from '@angular/common'; // ✅ Necesario para *ngIf y *ngFor
import {
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonGrid,
  IonRow,
  IonCol,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardSubtitle,
  IonCardContent,
  IonIcon,
  IonButton
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-clientes',
  templateUrl: './clientes.page.html',
  styleUrls: ['./clientes.page.scss'],
  standalone: true,
  imports: [
    CommonModule, // ✅ Para directivas estructurales
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonList,
    IonItem,
    IonLabel,
    IonGrid,
    IonRow,
    IonCol,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardSubtitle,
    IonCardContent,
    IonIcon, // ✅ Para mostrar iconos en el HTML
    IonButton
  ]
})
export class ClientesPage implements OnInit {

  clientes: any[] = []; // ✅ Arreglo para almacenar los clientes desde el backend
  esAdmin: boolean = false; // 🛡️ Indica si el usuario es administrador
  clienteSeleccionado: any = null; // Cliente seleccionado para ver detalle
  historialPedidos: any[] = []; // Historial de pedidos del cliente seleccionado

  constructor(private api: ApiService, private auth: AuthService) {}

  /**
   * ✅ Al iniciar la vista, se cargan los clientes desde el backend
   */
  ngOnInit() {
    this.api.getClientes().subscribe({
      next: (data) => this.clientes = data,
      error: () => alert('❌ Error al cargar los clientes')
    });
    // Detectar si el usuario es admin
    const usuario = this.auth.obtenerUsuario();
    this.esAdmin = usuario && usuario.rol && usuario.rol.toLowerCase().includes('admin');
  }

  /**
   * Muestra el detalle e historial de compras del cliente seleccionado
   */
  verDetalleCliente(cliente: any) {
    this.clienteSeleccionado = cliente;
    this.api.getHistorialComprasCliente(cliente.id).subscribe({
      next: (historial) => this.historialPedidos = historial,
      error: () => this.historialPedidos = []
    });
    // Aquí puedes abrir un modal o mostrar una sección con el detalle
  }
}
