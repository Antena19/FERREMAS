import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pedido-detalle',
  templateUrl: './pedido-detalle.page.html',
  styleUrls: ['./pedido-detalle.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class PedidoDetallePage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
