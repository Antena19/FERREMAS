import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
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
  IonIcon
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
    IonIcon // ✅ Para mostrar iconos en el HTML
  ]
})
export class ClientesPage implements OnInit {

  clientes: any[] = []; // ✅ Arreglo para almacenar los clientes desde el backend

  constructor(private api: ApiService) {}

  /**
   * ✅ Al iniciar la vista, se cargan los clientes desde el backend
   */
  ngOnInit() {
    this.api.getClientes().subscribe({
      next: (data) => this.clientes = data,
      error: () => alert('❌ Error al cargar los clientes')
    });
  }
}
