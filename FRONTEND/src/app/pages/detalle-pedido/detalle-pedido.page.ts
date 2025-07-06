import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { CommonModule } from '@angular/common';

import {
  IonHeader, IonToolbar, IonTitle, IonContent, IonCard,
  IonCardHeader, IonCardTitle, IonCardContent, IonList,
  IonItem, IonLabel, IonSpinner } from '@ionic/angular/standalone';

@Component({
  selector: 'app-detalle-pedido',
  templateUrl: './detalle-pedido.page.html',
  styleUrls: ['./detalle-pedido.page.scss'],
  standalone: true,
  imports: [IonSpinner, 
    CommonModule,
    IonHeader, IonToolbar, IonTitle, IonContent, IonCard,
    IonCardHeader, IonCardTitle, IonCardContent,
    IonList, IonItem, IonLabel
  ]
})
export class DetallePedidoPage implements OnInit {

  pedido: any = null; // 🧾 Pedido con detalles

  constructor(
    private route: ActivatedRoute,
    private api: ApiService
  ) {}

  /**
   * ✅ Al iniciar, obtener el ID del pedido desde la URL y cargar los detalles
   */
  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.api.getDetallePedido(id).subscribe({
        next: (data) => this.pedido = data,
        error: () => alert('❌ Error al cargar detalle del pedido')
      });
    }
  }
}
