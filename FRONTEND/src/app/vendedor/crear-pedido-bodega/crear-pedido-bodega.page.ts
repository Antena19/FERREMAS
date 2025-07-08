import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-crear-pedido-bodega',
  templateUrl: './crear-pedido-bodega.page.html',
  styleUrls: ['./crear-pedido-bodega.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class CrearPedidoBodegaPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
